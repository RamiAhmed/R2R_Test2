using UnityEngine;
using System.Collections;

public class GateOfLife : Entity {

	// Use this for initialization
	protected override void Start () {
		base.Start();

		CurrentHitPoints = MaxHitPoints;
	}
	
	// Update is called once per frame
	protected override void Update () {
		base.Update();
		if (IsDead) {
			PlayerController player = _gameController.players[0].GetComponent<PlayerController>();
			player.PlayerLives = 0;	
		}
		else {
			if (_gameController.CurrentPlayState == GameController.PlayState.COMBAT) {
				if (attackTarget != null) {
					if (GetIsWithinAttackingRange(attackTarget)) {
						Attack(attackTarget);	
					}
					else {
						attackTarget = null;
					}
				}
				else {
					Entity enemy = GetNearestUnit(_gameController.enemies);
					if (enemy != null && GetIsWithinAttackingRange(enemy)) {
						attackTarget = enemy;
					}
				}
			}
		}
	}
	
}
