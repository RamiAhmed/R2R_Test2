using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

public class Entity : MonoBehaviour {

	public string Name = "Entity",
				Class = "Entity";

	[Range(0, 100)]
	public float MinimumDamage = 0f;

	[Range(1, 100)]
	public float MaximumDamage = 1f;

	[Range(0, 100)]
	public float MinimumHealPower = 0f;

	[Range(1, 100)]
	public float MaximumHealPower = 0f;

	[Range(0f, 1f)]
	public float Accuracy = 0.5f;

	[Range(0f, 1f)]
	public float Evasion = 0.5f;

	[Range(0, 100)]
	public float Armor = 1f;

	[Range(10, 1000)]
	public float MaxHitPoints = 100f;

	[HideInInspector]
	public float CurrentHitPoints = 100f;

	[Range(0, 1000)]
	public float MovementSpeed = 2f;

	[Range(0, 100)]
	public float PerceptionRange = 10f;

	[Range(0, 100)]
	public float AttackingRange = 2f;

	[Range(0f, 10f)]
	public float AttacksPerSecond = 1f;

	[Range(0f, 1f)]
	public float FleeThreshold = 0.1f;

	[Range(0f, 1f)]
	public float MoralePointsPerSecond = 0.01f; 

	public Texture2D ProfilePicture = null;

	public GameObject Bullet = null,
					AlternateBullet = null;
	
	public int attackCount = 0, killCount = 0, attackedCount = 0;
	
	public string WalkAnimation = "", 
				  AttackAnimation = "";

	public List<AudioClip> 
		AttackSounds = new List<AudioClip>(), 
		DeathSounds = new List<AudioClip>(), 
		BeingHitSounds = new List<AudioClip>();

	[HideInInspector]
	public bool Selected = false,
				IsDead = false;

	[HideInInspector]
	public Entity lastAttacker = null,
				attackTarget = null;
	
	protected GameController _gameController = null;	
	protected GateOfLife gateRef = null;
	protected bool isMoving = false;
	protected Color originalMaterialColor = Color.white;
	protected float lastAttack = 0f;
	protected float moraleLevel = 100f;

	protected float meleeDistance = 5f;

	private float lastMoraleRegenerate = 0f;

	private float killY = -100f;

	private Vector3 targetPosition = Vector3.zero;

	private Seeker seeker;
	private Path path;
	private CharacterController controller;

	private int currentWaypoint = 0;
	private float nextWaypointDistance = 1.5f;

	private float repathRate = 1.5f,
				  lastRepath = -1f;
	
	private Animation animation;	

	private Dictionary<string, AudioSource> audioSources;
	
	void Awake() {
		_gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
		gateRef = GameObject.FindGameObjectWithTag("GateOfLife").GetComponent<GateOfLife>();
		seeker = this.GetComponent<Seeker>();
		controller = this.GetComponent<CharacterController>();
		originalMaterialColor = this.renderer.material.color;
		
		animation = this.GetComponent<Animation>();
		if (animation == null) {
			animation = this.GetComponentInChildren<Animation>();
		}

		audioSources = new Dictionary<string, AudioSource>();

		addAudioSource("Attack", AttackSounds);
		addAudioSource("BeingHit", BeingHitSounds);
	}

	private void addAudioSource(string type, List<AudioClip> sounds) {
		if (sounds.Count > 0) {
			audioSources.Add(type, this.gameObject.AddComponent<AudioSource>());
			audioSources[type].playOnAwake = false;
		}
	}

	protected virtual void Start() {}

	protected virtual void Update() {
		if (animation != null) {
			if (isMoving) {				
				if (!animation.IsPlaying(GetWalkAnimation())) {
					animation.Play(GetWalkAnimation());
				}
			}
		}

		if (_gameController.CurrentGameState == GameController.GameState.PLAY) {
			if (this.moraleLevel < 100f) {
				if (_gameController.GameTime - this.lastMoraleRegenerate > 1f) {
					this.lastMoraleRegenerate = _gameController.GameTime;
					this.moraleLevel += this.MoralePointsPerSecond;
				}
			}
		}
	}
	
	public bool GetIsWithinPerceptionRange(Entity target) {
		return GetIsWithinRange(target, PerceptionRange);	
	}
	
	public bool GetIsWithinAttackingRange(Entity target) {
		return GetIsWithinRange(target, AttackingRange);
	}
	
	public bool GetIsWithinRange(Entity target, float range) {
		return target != null && Vector3.Distance(target.transform.position, this.transform.position) < range;
	}
			
	protected string GetWalkAnimation() {
		return WalkAnimation;
	}
	
	protected string GetAttackAnimation() {
		return AttackAnimation;	
	}

	protected virtual void FixedUpdate() {
		MoveEntity();
	}

	protected virtual void LateUpdate() {
		if (this.transform.position.y <= killY) {
			RemoveSelf();
		}
		else if (_gameController.CurrentGameState != GameController.GameState.PLAY) {
			StopMoving();
		}
	}

	protected virtual void RemoveSelf() {
		StopMoving();
	}

	protected bool GetIsMelee() {
		return AttackingRange <= meleeDistance;
	}

	public virtual void Select(List<Entity> list) {
		if (!this.Selected && !this.IsDead) {
			this.Selected = true;
			if (!list.Contains(this)) {
				list.Add(this);
			}
		}
	}

	public virtual void Deselect(List<Entity> list) {
		if (this.Selected || this.IsDead) {
			this.Selected = false;
			if (list.Contains(this)) {
				list.Remove(this);
			}
		}
	}

	public float GetDamage(bool bAverage) {
		return bAverage ? (MaximumDamage - MinimumDamage)/2f + MinimumDamage : GetDamage();
	}

	public float GetDamage() {
		return Random.Range(MinimumDamage, MaximumDamage);
	}

	public float GetHealPower() {
		return Random.Range(MinimumHealPower, MaximumHealPower);
	}

	public float GetTotalScore() {
		return (GetDamage(true) + Accuracy + Evasion + Armor + (MaxHitPoints/10f) + (MovementSpeed/10f) + PerceptionRange + AttackingRange);
	}

	public bool GetIsAlly(Entity target) {
		return (this.GetIsUnit() && target.GetIsUnit()) || (this.GetIsEnemy() && target.GetIsEnemy());
	}
	
	public void Heal(Entity target, float healAmount) {
		if (target != null) {
			float currentTime = Time.time;
			if (currentTime - lastAttack > 1f/AttacksPerSecond) {
				lastAttack = currentTime;

				if (GetIsAlly(target)) {
					target.CurrentHitPoints = target.CurrentHitPoints + healAmount > target.MaxHitPoints ? target.MaxHitPoints : target.CurrentHitPoints + healAmount;
					ShootBullet(target, true);
					target.moraleLevel += healAmount;
					if (target.moraleLevel > 100) 
						target.moraleLevel = 100;

					if (animation != null) {
						animation.Play(GetAttackAnimation());	
					}
					Debug.Log(_gameController.GameTime + ": " + this.Name + " healed " + target.Name + " for " + healAmount + " hitpoints");
				}
				else {
					Debug.LogWarning(_gameController.GameTime + ": " + this.Name + " tried to heal non-ally : " + target.Name);	
				}
			}
		}

	}

	public Entity GuardOther(Entity target) {
		Entity newTarget = null;
		if (target != null) {
			Entity nearestEnemy = GetNearestUnit(_gameController.enemies);
			if (target.lastAttacker != null) {
				newTarget = target.lastAttacker;
			}
			else if (nearestEnemy != null && GetIsWithinAttackingRange(nearestEnemy)) {
				newTarget = nearestEnemy;
			}
			else {
				if (!GetIsWithinAttackingRange(target)) {
					MoveTo(target.transform);
				}
			}
		}

		return newTarget;
	}
	
	public Entity FollowOther(Entity target) {
		Entity newTarget = null;
		if (target != null) {
			Entity nearestEnemy = GetNearestUnit(_gameController.enemies);
			if (target.attackTarget != null) {
				newTarget = target.attackTarget;	
			}
			else if (nearestEnemy != null && GetIsWithinAttackingRange(nearestEnemy)) {
				newTarget = nearestEnemy;
			}
			else {
				if (!GetIsWithinAttackingRange(target)) {
					MoveTo(target.transform);
				}
			}
		}

		return newTarget;
	}

	public Entity StandGround(List<Entity> list) {
		Entity target = GetNearestUnit(_gameController.enemies), 
				newTarget = null;
		if (target != null) {
			if (GetIsWithinAttackingRange(target)) {
				newTarget = target; 
			}
			else if (GetIsWithinAttackingRange(GetNearestUnit(list).lastAttacker)) {
				newTarget = GetNearestUnit(list).lastAttacker;
			}
		}

		return newTarget;
	}

	public void FleeFrom(Transform target) {
		if (target != null) {
			FleeFrom(target.position);
		}
	}

	public void FleeFrom(Vector3 target) {
		if (target.sqrMagnitude > 0f) {
			Vector3 direction = (this.transform.position - target).normalized * MovementSpeed;
			MoveTo(direction);
		}
	}

	public bool GetIsPosWalkable(Vector3 pos) {
		Node node = (Node)AstarPath.active.GetNearest(pos);
		return node.walkable && Vector3.Distance((Vector3)node.position, pos) < meleeDistance;
	}

	public void MoveTo(Transform target) {
		if (target != null) {
			MoveTo(target.position);
		}
	}

	public void MoveTo(Vector3 position) {
		if (position.sqrMagnitude > 0f) {
			if (Time.time - lastRepath > repathRate) {
				if (!seeker.IsDone()) {
					StopMoving();
				}

				lastRepath = Time.time + Random.value * repathRate * 0.5f;
				targetPosition = position;
				
				seeker.StartPath(this.transform.position, targetPosition, OnPathComplete);
				
				lookAtTarget(position);
			}
		}

	}

	private void OnPathComplete(Path p) {
		p.Claim(this);
		if (!p.error) {
			StopMoving();
			isMoving = true;

			path = p;
			currentWaypoint = 0;
		}
		else {
			p.Release(this);
			isMoving = false;
		}
	}

	private void MoveEntity() {
		if (path == null) {
			return;
		}

		List<Vector3> vectorPath = path.vectorPath;
        if (currentWaypoint >= vectorPath.Count) {
            StopMoving();
        }
		else {
	        Vector3 direction = (path.vectorPath[currentWaypoint] - this.transform.position).normalized;
	        direction *= MovementSpeed * Time.deltaTime * 2f;
	        controller.SimpleMove(direction);

			lookAtTarget(path.vectorPath[currentWaypoint]);

			if ((this.transform.position - vectorPath[currentWaypoint]).sqrMagnitude < nextWaypointDistance * nextWaypointDistance) {
	            currentWaypoint++;
	        }
		}
	}

	public void StopMoving() {
		if (path != null) {
			path.Release(this);
			path = null;
		}

		if (isMoving) {
			isMoving = false;
		}
	}

	protected int GetD20() {
		return Random.Range(1, 21);
	}

	protected float fGetD20() {
		return Random.Range(1f, 20f);
	}
	
	public float GetDamagePerSecond() {
		return this.GetDamage(true) * AttacksPerSecond * Accuracy;
	}

	public void ReceiveDamage(float damage) {
		if (damage <= 0f) {
			Debug.LogWarning("Receive Damage cannot damage 0 or less");
			return;
		}

		this.moraleLevel -= (this.GetD20()/20f) < FleeThreshold ? damage : 0f;
		if (this.moraleLevel < 0f) {
			this.moraleLevel = 0f;
		}

		this.CurrentHitPoints -= damage;
		if (this.CurrentHitPoints <= 0f) {
			PlayRandomDeathSound();
			this.IsDead = true;
		}
	}

	public bool GetShouldFlee() {
		return this.moraleLevel / 100f < FleeThreshold;
	}
	
	public void StopAllAnimations() {
		if (animation != null) {
			animation.Stop();
			// TODO Find nicer solution
		}
	}

	public void SetIsNotDead() {
		SetIsNotDead(true);
	}

	public void SetIsNotDead(bool fullHealth) {
		this.IsDead = false;
		if (fullHealth) {
			this.CurrentHitPoints = this.MaxHitPoints;
		}
		else {
			this.CurrentHitPoints = 1f;
		}
	}

	protected void lookAtTarget(Vector3 target) {
		lookAtTarget(target, false);
	}
	
	protected void lookAtTarget(Vector3 target, bool bInstantenous) {
		if (!this.GetIsGate()) {
			Quaternion newRot = Quaternion.LookRotation(this.transform.position - target);
			newRot.z = 0f;
			newRot.x = 0f;
			if (!bInstantenous) {
				this.transform.rotation = Quaternion.Slerp(this.transform.rotation, newRot, Time.deltaTime * 4f);
			}
			else {
				this.transform.rotation = newRot;
			}
		}		
	}

	public void PlayRandomAttackSound() {
		playRandomSound(AttackSounds, "Attack");
	}

	public void PlayRandomBeingHitSound() {
		playRandomSound(BeingHitSounds, "BeingHit");
	}

	public void PlayRandomDeathSound() {
		if (DeathSounds.Count > 0) {
			AudioClip sound = DeathSounds.Count > 1 ? DeathSounds[Random.Range(0, DeathSounds.Count)] : DeathSounds[0];
			AudioSource.PlayClipAtPoint(sound, this.transform.position);
		}
	}

	private void playRandomSound(List<AudioClip> sounds, string type) {
		if (audioSources.ContainsKey(type)) {
			if (sounds.Count > 0) {
				if (!audioSources[type].isPlaying) {
					AudioClip sound = sounds.Count > 1 ? sounds[Random.Range(0, sounds.Count)] : sounds[0];
					audioSources[type].clip = sound;
					audioSources[type].Play();
				}
			}
		}
	}

	protected virtual bool Attack(Entity opponent) {
		bool hitResult = false;

		if (opponent.IsDead || opponent == null) {
			attackTarget = null;
		}
		else if (GetIsAlly(opponent)) {
			attackTarget = null;
			Debug.LogWarning(this.Name + " tried to attack ally " + opponent);
		}
		else {
			StopMoving();
			lookAtTarget(opponent.transform.position);

			float currentTime = Time.time;
			if (currentTime - lastAttack > 1f/AttacksPerSecond) {
				lastAttack = currentTime;

				if (Bullet != null) {
					ShootBullet(opponent);
				}

				float accuracy = this.Accuracy + Random.value;
				accuracy = accuracy > 1f ? 1f : accuracy;

				float evasion = opponent.Evasion + Random.value;
				evasion = evasion > 1f ? 1f : evasion;

				if (accuracy > evasion) {
					float damage = (GetDamage() - opponent.Armor);
					damage = damage < 1f ? 1f : damage;
					opponent.ReceiveDamage(damage);
					opponent.PlayRandomBeingHitSound();
					hitResult = true;
					Debug.Log(_gameController.GameTime + ": " + this.Name + " hit " + opponent.Name + " with " + damage.ToString() + " damage");
				}
				else {
					Debug.Log(_gameController.GameTime + ": " + this.Name + " missed " + opponent.Name);
				}
				
				this.attackCount++;
				opponent.attackedCount++;

				if (opponent.lastAttacker == null) {
					opponent.lastAttacker = this;
				}
				
				if (animation != null) {
					animation.Play(GetAttackAnimation());
				}		
								
				PlayRandomAttackSound();
			}
		}
		return hitResult;
	}
	
	protected virtual void ShootBullet(Entity opponent) {
		ShootBullet(opponent, false);	
	}

	protected virtual void ShootBullet(Entity opponent, bool bAlternate) {
		GameObject newBullet = null;
		if (!bAlternate) {
			if (Bullet != null) {
				newBullet = Instantiate(Bullet) as GameObject;
			}
			else {
				Debug.LogWarning("Could not find Bullet");
			}
		}
		else {
			if (AlternateBullet != null) {
				newBullet = Instantiate(AlternateBullet) as GameObject;	
			}
			else {
				Debug.LogWarning("Could not find Alternate Bullet");
			}
		}
			
		if (newBullet.collider != null) {
			Physics.IgnoreCollision(newBullet.collider, this.transform.collider);
		}
		Bullet bullet = newBullet.GetComponent<Bullet>();
		bullet.Target = opponent.transform.position;
		bullet.Owner = this.gameObject;
	}

	public bool GetIsUnit() {
		return this != null && this.transform.GetComponent<UnitController>() != null;
	}

	public bool GetIsEnemy() {
		return this != null && this.transform.GetComponent<EnemyController>() != null;
	}

	public bool GetIsGate() {
		return this != null && this.transform.GetComponent<GateOfLife>() != null;
	}

	protected Entity GetNearestUnit(List<Entity> list) {
		if (list.Count <= 0)
			return null;

		Entity nearest = null;
		float shortestDistance = PerceptionRange;
		foreach (Entity unit in list) {
			float distance = Vector3.Distance(unit.transform.position, this.transform.position);
			if (distance < shortestDistance && unit.gameObject != this.gameObject && GetIsWithinPerceptionRange(unit)) {
				nearest = unit;
				shortestDistance = distance;
			}
		}

		return nearest != null ? nearest : null;
	}

	protected Entity GetWeakestUnit(List<Entity> list) {
		if (list.Count <= 0)
			return null;

		Entity weakest = null;
		float weakestScore = list[0].GetTotalScore();
		foreach (Entity unit in list) {
			float score = unit.GetTotalScore();
			if (score < weakestScore && unit.gameObject != this.gameObject && GetIsWithinPerceptionRange(unit))	{
				weakest = unit;
				weakestScore = score;
			}
		}

		return weakest != null ? weakest : null;
	}

	protected Entity GetStrongestUnit(List<Entity> list) {
		if (list.Count <= 0)
			return null;

		Entity strongest = null;
		float strongestScore = 0;
		foreach (Entity unit in list) {
			float score = unit.GetTotalScore();
			if (score > strongestScore && unit.gameObject != this.gameObject && GetIsWithinPerceptionRange(unit)) {
				strongest = unit;
				strongestScore = score;
			}
		}

		return strongest != null ? strongest : null;
	}

	protected Entity GetLeastDamagedUnit(List <Entity> list) {
		if (list.Count <= 0) {
			return null;
		}

		Entity leastDamaged = null;
		float damage = 100;
		foreach (Entity unit in list) {
			float hpDiff = unit.MaxHitPoints - unit.CurrentHitPoints;
			if (hpDiff < damage || hpDiff <= 0f && unit.gameObject != this.gameObject && GetIsWithinPerceptionRange(unit)) {
				leastDamaged = unit;
				damage = hpDiff;
			}
		}

		return leastDamaged != null ? leastDamaged : null;
	}

	protected Entity GetMostDamagedUnit(List<Entity> list) {
		if (list.Count <= 0)
			return null;

		Entity mostDamaged = null;
		float damage = 0;
		foreach (Entity unit in list) {
			float hpDiff = unit.MaxHitPoints - unit.CurrentHitPoints;
			if (hpDiff > damage && hpDiff >= 0f && unit.gameObject != this.gameObject && GetIsWithinPerceptionRange(unit)) {
				mostDamaged = unit;
				damage = hpDiff;
			}
		}

		return mostDamaged != null ? mostDamaged : null;
	}
}
