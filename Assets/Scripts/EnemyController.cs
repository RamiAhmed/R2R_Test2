using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyController : Enemy {

	private enum EnemyState {
		SPAWNING,
		MOVING,
		ATTACKING,
		FLEEING,
		DEAD
	};
	
	private EnemyState currentEnemyState = EnemyState.SPAWNING;
	
	private PlayerController counterPlayer;	

	protected override void Start () {
		base.Start();
		if (currentEnemyState == EnemyState.SPAWNING) {
			
			GameObject redDot = Instantiate(Resources.Load("Misc Objects/RedDot")) as GameObject;
			redDot.transform.parent = this.transform;
			redDot.transform.localPosition = Vector3.zero;
			
			GameObject[] points = GameObject.FindGameObjectsWithTag("Waypoint");
			foreach (GameObject point in points) {
				if (point.transform.name.Contains("Start")) {
					this.transform.position = point.transform.position;
					break;
				}
			}

			counterPlayer = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>().players[0].GetComponent<PlayerController>();
		
			currentEnemyState = EnemyState.MOVING;	
			
			lookAtTarget(gateRef.transform.position);			
		}
		
	}
	
	protected override void Update() {
		base.Update();
		
		if (_gameController.CurrentGameState != GameController.GameState.PLAY) {
			return;
		}
		
		if (IsDead) {
			this.currentEnemyState = EnemyState.DEAD;
		}
	}

	protected override void FixedUpdate () {
		base.FixedUpdate();
		
		if (_gameController.CurrentGameState != GameController.GameState.PLAY) {
			return;
		}
		
		if (currentEnemyState == EnemyState.MOVING) {			
			Entity nearest = GetNearestUnit(counterPlayer.unitsList);
			if (nearest != null) {
				attackTarget = nearest;
				currentEnemyState = EnemyState.ATTACKING;
			}
			else if (gateRef != null) {
				if (GetIsWithinAttackingRange(gateRef)) {
					attackTarget = gateRef;
					currentEnemyState = EnemyState.ATTACKING;
				}
				else {
					MoveTo(gateRef.transform);
				}
			}
		}
		else if (currentEnemyState == EnemyState.ATTACKING) {
			if (this.GetShouldFlee()) {
				this.currentEnemyState = EnemyState.FLEEING;
			}			
			else if (attackTarget != null) {
				if (GetIsWithinAttackingRange(attackTarget)) {
					Attack(attackTarget);	
				}
				else {
					MoveTo(attackTarget.transform);
				}
			}
			else {
				currentEnemyState = EnemyState.MOVING;	
			}
		}
		else if (currentEnemyState == EnemyState.FLEEING) {
			if (attackTarget != null) {
				if (GetIsWithinPerceptionRange(attackTarget)) {
					FleeFrom(attackTarget.transform);	
				}
				else {
					StopMoving();
					this.currentEnemyState = EnemyState.MOVING;
					this.FleeThreshold /= 2f;
				}
			}
			else {
				StopMoving();
				this.currentEnemyState = EnemyState.MOVING;
				this.FleeThreshold /= 2f;
			}
		}
	}
	
	protected override void LateUpdate() {
		base.LateUpdate();
		
		if (_gameController.CurrentGameState != GameController.GameState.PLAY) {
			return;
		}
		
		if (currentEnemyState == EnemyState.DEAD) {
			if (IsDead) {
				counterPlayer.PlayerGold += this.GoldReward;
				StatsCollector.TotalGoldEarned += this.GoldReward;
				lastAttacker.killCount++;
				StatsCollector.TotalEnemiesKilled++;
			}
			Deselect(counterPlayer.SelectedUnits);
			_gameController.enemies.Remove(this);
			Destroy(this.gameObject);	
		}
	}
	
	protected override void RemoveSelf() {
		base.RemoveSelf();

		Deselect(counterPlayer.SelectedUnits);
		_gameController.enemies.Remove(this);
		Destroy(this.gameObject);
	}

}
