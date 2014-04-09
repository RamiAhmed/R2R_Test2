using UnityEngine;
using System.Collections;

public class Enemy : Entity {

	[Range(1, 10)]
	public int GoldReward = 1;

	[Range(1, 20)]
	public int MaxGoldReward = 10;
	
	public float HitPointsScaleFactor = 10f,
				DamageScaleFactor = 1f,
				AccuracyScaleFactor = 1f,
				EvasionScaleFactor = 1f,
				ArmorScaleFactor = 1f,
				MovementSpeedScaleFactor = 1.5f,
				PerceptionRangeScaleFactor = 0.5f,
				AttackingRangeScaleFactor = 0.3f,
				AttacksPerSecondScaleFactor = 0.1f,
				FleeThresholdScaleFactor = 0.01f,
				GoldScaleFactor = 0.0f;

	// Use this for initialization
	protected override void Start () {
		Debug.Log (_gameController.GameTime + ": Enemy created");
		
		float height = Terrain.activeTerrain.SampleHeight(this.transform.position);
		height += this.transform.collider.bounds.size.y/2f + 0.1f;
		this.transform.position = new Vector3(this.transform.position.x, height, this.transform.position.z);
		
		int wave = _gameController.WaveCount-1;
				
		GoldReward = Mathf.RoundToInt(Mathf.Clamp(wave * GoldScaleFactor, (float)GoldReward, (float)MaxGoldReward));
		
		MaxHitPoints += (wave*HitPointsScaleFactor) + Random.Range(0f, 5f);
		CurrentHitPoints = MaxHitPoints;
		
		MinimumDamage += wave*DamageScaleFactor + Random.value;
		MaximumDamage += wave*DamageScaleFactor + Random.value;
		Accuracy += wave*AccuracyScaleFactor;
		if (Accuracy > 1f) 
			Accuracy = 1f;

		Evasion += wave*EvasionScaleFactor;
		if (Evasion > 1f) 
			Evasion = 1f;

		Armor += wave*ArmorScaleFactor;
		
		MovementSpeed += (wave*MovementSpeedScaleFactor) + Random.value;
		
		PerceptionRange += (wave*PerceptionRangeScaleFactor);
		AttackingRange += (wave*AttackingRangeScaleFactor);
		
		AttacksPerSecond += (wave*AttacksPerSecondScaleFactor);
		FleeThreshold -= (wave*FleeThresholdScaleFactor);
	}
}
