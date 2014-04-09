using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Faction : MonoBehaviour {
	
	public string FactionName = "Divine";
	public List<Unit> FactionUnits = new List<Unit>();

	public Texture2D TAISAttack, TAISAssist, TAISGuard, TAISStandGuard, 
					TAISNearest_ALLY, TAISStrongest_ALLY, TAISWeakest_ALLY, TAISMostDamaged_ALLY, TAISLeastDamaged_ALLY,
					TAISNearest_ENEMY, TAISStrongest_ENEMY, TAISWeakest_ENEMY, TAISMostDamaged_ENEMY, TAISLeastDamaged_ENEMY,
					TAISSelf,
					TAISAlways, TAIS75HP, TAIS50HP, TAIS25HP, TAISLessHP;

}
