using UnityEngine;
using System.Collections;

public class Unit : Entity {
	
	public int GoldCost = 1;
	public float SellGoldPercentage = 0.5f;
	public bool isHealer = false;
	public float HealThreshold = 0.75f;

	// Use this for initialization
	protected override void Start () {
		Debug.Log (_gameController.GameTime + ": Unit created");

		this.CurrentHitPoints = this.MaxHitPoints;
	}
	
	
}
