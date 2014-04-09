using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {
	
	public List<Entity> unitsList = null, deadUnitsList = null, SelectedUnits = null;

	public UnitController currentlyPlacingUnit = null;
	
	public bool isDebugging = false;
	
	public int PlayerLives = 1;
	public int PlayerGold = 20;
	
	public float ShowFeedbackTime = 3f; // show feedback messages for 3 seconds
	
	public Faction playerFaction;
	
	public Texture marqueeGraphics;	
	
	public Texture2D swordHUD, bootHUD, shieldHUD, healthContainerHUD, healthBarHUD, TacticsCircleHUD, GoldIconHUD, UnitCountIcon;

	public GUISkin PlayerHUDSkin = null, IntroductionSkin = null, TAISSkin = null, CountdownSkin = null;

	private float screenWidth = 0f,
				screenHeight = 0f;
	
	private Camera playerCam;
	
	private GameController _gameController;
	
	private string feedbackText = "";
	
	private Vector2 marqueeOrigin, marqueeSize;
	private Rect marqueeRect, backupRect;
	
	private Color feedbackColor = Color.white;

	private bool hasRespawnedUnits = true;

	public bool bSelectingTactics = false;
	 
	private ScenarioHandler scenarioHandler = null;

	private GateOfLife gateRef = null;

	private bool bShowingHealthbars = false;

	void Start () {
		screenWidth = Screen.width;
		screenHeight = Screen.height;
		unitsList = new List<Entity>();
		deadUnitsList = new List<Entity>();
		playerCam = this.GetComponentInChildren<Camera>();
		_gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
		SelectedUnits = new List<Entity>();		

		scenarioHandler = GameObject.FindGameObjectWithTag("ScenarioHandler").GetComponent<ScenarioHandler>();

		gateRef = GameObject.FindGameObjectWithTag("GateOfLife").GetComponent<GateOfLife>();
	}

	void Update () {		
		if (_gameController.CurrentGameState != GameController.GameState.PLAY) {
			return;
		}		
		
		if (PlayerLives <= 0) {
			_gameController.CurrentGameState = GameController.GameState.ENDING;
			return;
		}
		
		if (_gameController.CurrentPlayState == GameController.PlayState.BUILD) {
			if (!hasRespawnedUnits) {
				respawnUnits();
			}
			else {
				createSpawnShortcuts();

				if (Input.GetKeyDown(KeyCode.Space)) {
					if (!_gameController.ForceSpawn) {
						_gameController.ForceSpawn = true;
						StatsCollector.AmountOfForceSpawns++;
					}
				}
			}
		}
		else if (_gameController.CurrentPlayState == GameController.PlayState.COMBAT) {
			if (bSelectingTactics) {
				bSelectingTactics = false;
			}

			hasRespawnedUnits = false;
		}

		if (isDebugging) {
			if (Input.GetKeyDown(KeyCode.End)) {
				PlayerLives = 0;
			}
		}

		if (Input.GetKeyDown(KeyCode.AltGr) || Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt)) {
			bShowingHealthbars = !bShowingHealthbars;
		}
		
		if (SelectedUnits.Count > 0) {
			if (Input.GetMouseButtonDown(1)) {
				if (_gameController.CurrentPlayState == GameController.PlayState.BUILD) {
					if (!ClearPlacingUnit()) {
						moveUnit();
					}
				}
				else if (SelectedUnits[0].GetIsUnit()) {
					DisplayFeedbackMessage("You cannot move units, unless in the Build Phase.");
				}
			}

			if (scenarioHandler.CurrentScenario == ScenarioHandler.ScenarioState.WITH_TAIS) {
				if (Input.GetKeyUp(KeyCode.Q)) {
					bSelectingTactics = !bSelectingTactics;	
				}
			}
			else {
				if (bSelectingTactics)
					bSelectingTactics = false;
			}
		}
		else {
			if (Input.GetMouseButtonDown(0)) {
				UnitController currentlySelectedUnit = GetCurrentlyPlacingUnit();
				if (currentlySelectedUnit != null) {
					currentlySelectedUnit.BuildUnit();
				}
			}

			if (Input.GetMouseButtonDown(1)) {
				ClearPlacingUnit();
			}
		}
		
		float yMarginFactor = 0.25f;
		Rect disallowedRect = new Rect(0f, screenHeight - (screenHeight * yMarginFactor), screenWidth, screenHeight * yMarginFactor);

		Vector2 mousePos = new Vector2(Input.mousePosition.x, screenHeight - Input.mousePosition.y);

		bool selectionBool = 
			SelectedUnits.Count == 0 || 
			//_gameController.CurrentPlayState == GameController.PlayState.COMBAT || 
			GetCurrentlyPlacingUnit() != null || 
			(!disallowedRect.Contains(mousePos) && !bSelectingTactics);

		if (selectionBool) {
			handleUnitSelection();
		}
		else {
			clearMarqueeRect();
		}
	}

	public bool ClearPlacingUnit() {
		UnitController currentlyPlacing = GetCurrentlyPlacingUnit();
		if (currentlyPlacing != null) {
			if (currentlyPlacing.GetIsPlacing()) {
				Destroy(currentlyPlacing.gameObject);
			}
			SetCurrentlyPlacingUnit(null);

			return true;
		}

		return false;
	}
	
	private void respawnUnits() {
		if (deadUnitsList.Count > 0) {
			Debug.Log(_gameController.GameTime + ": " + "Respawn units");
			foreach (Entity go in deadUnitsList) {				
				UnitController unit = go.GetComponent<UnitController>();
				
				unitsList.Add(go);
				unit.SetIsNotDead();
				
				go.gameObject.SetActive(true);
			}
			deadUnitsList.Clear();
		}			
		
		if (unitsList.Count > 0) {
			foreach (Entity go in unitsList) {
				UnitController unit = go.GetComponent<UnitController>();
				unit.StopMoving();
				unit.StopAllAnimations();
				unit.transform.position = unit.LastBuildLocation;	
				unit.currentUnitState = UnitController.UnitState.PLACED;
				unit.SetIsNotDead();
			}
		}

		hasRespawnedUnits = true;
	}
					
	private void moveUnit() {
		if (SelectedUnits.Count > 0) {			
			Ray mouseRay = playerCam.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
			RaycastHit[] hits = Physics.RaycastAll(mouseRay);
			foreach (RaycastHit hit in hits) {
				if (hit.collider.GetType() == typeof(TerrainCollider)) {
					Vector3 clickedPos = new Vector3(hit.point.x, hit.point.y, hit.point.z);

					if (SelectedUnits[0].GetIsPosWalkable(clickedPos)) {
						foreach (Entity ent in SelectedUnits) {
							if (!ent.IsDead && ent.GetIsUnit() && ent.GetComponent<UnitController>().playerOwner == this) {
								ent.MoveTo(clickedPos);
								StatsCollector.AmountOfUnitsMoved++;
							}
						}
					}
					else {
						DisplayFeedbackMessage("You cannot move units to that location.");
					}

					break;
				}
			}
		}
	}
	
	private void clearSelection() {		
		if (SelectedUnits.Count > 0) {
			//Debug.Log("clearSelection");
			while (SelectedUnits.Count > 0) {
				SelectedUnits[0].Deselect(SelectedUnits);
			}
		}
	}
	
	private void handleUnitSelection() {		
		if (Input.GetMouseButtonDown(0)) {
			clearSelection();
			
			float _invertedY = screenHeight - Input.mousePosition.y;
			marqueeOrigin = new Vector2(Input.mousePosition.x, _invertedY);
			
			selectUnit();

			if (SelectedUnits.Count > 0) {
				StatsCollector.AmountOfSelections++;

				if (SelectedUnits[0].GetIsUnit()) {
					StatsCollector.AmountOfUnitSelections++;
				}
				else if (SelectedUnits[0].GetIsEnemy()) {
					StatsCollector.AmountOfEnemySelections++;
				}
			}
		}
		
		if (Input.GetMouseButton(0)) {
			float _invertedY = screenHeight - Input.mousePosition.y;
									
			marqueeSize = new Vector2(Input.mousePosition.x - marqueeOrigin.x, (marqueeOrigin.y - _invertedY) * -1);
			//FIX FOR RECT.CONTAINS NOT ACCEPTING NEGATIVE VALUES
			if (marqueeRect.width < 0) {
			    backupRect = new Rect(marqueeRect.x - Mathf.Abs(marqueeRect.width), marqueeRect.y, Mathf.Abs(marqueeRect.width), marqueeRect.height);
			}
			else if (marqueeRect.height < 0) {
			    backupRect = new Rect(marqueeRect.x, marqueeRect.y - Mathf.Abs(marqueeRect.height), marqueeRect.width, Mathf.Abs(marqueeRect.height));
			}
			if (marqueeRect.width < 0 && marqueeRect.height < 0) {
			    backupRect = new Rect(marqueeRect.x - Mathf.Abs(marqueeRect.width), marqueeRect.y - Mathf.Abs(marqueeRect.height), Mathf.Abs(marqueeRect.width), Mathf.Abs(marqueeRect.height));
			}
			
			if ((marqueeRect.width > 0f || backupRect.width > 0f) && (marqueeRect.height > 0f || backupRect.height > 0f)) {	
				bool unitFound = false;
				foreach (Entity unit in unitsList) {
				    //Convert the world position of the unit to a screen position and then to a GUI point
				    Vector3 _screenPos = playerCam.WorldToScreenPoint(unit.transform.position);
				    Vector2 _screenPoint = new Vector2(_screenPos.x, screenHeight - _screenPos.y);
				    //Ensure that any units not within the marquee are currently unselected
				    if (!marqueeRect.Contains(_screenPoint) || !backupRect.Contains(_screenPoint)) {
						unit.Deselect(SelectedUnits);
				    }
				    
					if (marqueeRect.Contains(_screenPoint) || backupRect.Contains(_screenPoint)) {
						unit.Select(SelectedUnits);
						StatsCollector.AmountOfUnitSelections++;
						StatsCollector.AmountOfSelections++;
						if (!unitFound) {
							unitFound = true;
						}
				    }
				}
				
				if (!unitFound) {
					foreach (Entity enemy in _gameController.enemies) {
					    //Convert the world position of the unit to a screen position and then to a GUI point
					    Vector3 _screenPos = playerCam.WorldToScreenPoint(enemy.transform.position);
					    Vector2 _screenPoint = new Vector2(_screenPos.x, screenHeight - _screenPos.y);
					    //Ensure that any units not within the marquee are currently unselected
					    if (!marqueeRect.Contains(_screenPoint) || !backupRect.Contains(_screenPoint)) {
							enemy.Deselect(SelectedUnits);
					    }
					    
						if (marqueeRect.Contains(_screenPoint) || backupRect.Contains(_screenPoint)) {
							enemy.Select(SelectedUnits);
							StatsCollector.AmountOfEnemySelections++;
							StatsCollector.AmountOfSelections++;
					    }							
					}
				}
				else {
					foreach (Entity entity in SelectedUnits) {
						if (entity.GetIsEnemy() || (entity.GetIsUnit() && entity.IsDead)) {
							entity.Deselect(SelectedUnits);
						}
					}
				}
				
			}
		}
		
		if (Input.GetMouseButtonUp(0)) {
			clearMarqueeRect();
		}
	}
	
	private void clearMarqueeRect() {
		marqueeRect.width = 0;
		marqueeRect.height = 0;
		backupRect.width = 0;
		backupRect.height = 0;
		marqueeSize = Vector2.zero;		
	}
	
	private void selectUnit() {
		Ray mouseRay = playerCam.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
		
		RaycastHit[] hits = Physics.RaycastAll(mouseRay);
		foreach (RaycastHit hit in hits) {			
			if (hit.transform.GetComponent<Entity>() != null) {
				Entity selectedUnit = hit.transform.GetComponent<Entity>();
				selectedUnit.Select(SelectedUnits);
				break;			
			}			
		}			
	}
	
	/* GUI & UNIT SPAWNING */
	void OnGUI() {
		if (_gameController.CurrentGameState == GameController.GameState.INTRODUCTION) {
			renderIntroductionInstructions();
		}
		else if (_gameController.CurrentGameState == GameController.GameState.PLAY) {			
			renderTopHUD();
			renderBottomHUD();		

			renderFeedbackMessage();			
			renderMarqueeSelection();

			if (isDebugging) {
				renderSelectedDebugFeedback();	
			}

			if (bShowingHealthbars) {
				renderAllUnitsHealthbar();
			}
			else {
				renderSelectedUnitsHealthbar();
			}

			if (_gameController.CurrentPlayState == GameController.PlayState.BUILD) {
				int buildTimeLeft = Mathf.RoundToInt(_gameController.GetMaxBuildTime() - _gameController.BuildTime);

				if (buildTimeLeft == 10 || buildTimeLeft <= 5) {
					if (CountdownSkin != null && GUI.skin != CountdownSkin) {
						GUI.skin = CountdownSkin;
					}

					float width = 100f, height = 100f;

					GUI.Label(new Rect((screenWidth/2f) - (width/2f), (screenHeight/2f) - (height/2f), width, height), buildTimeLeft.ToString());	
				}
			}
		}
	}

	private void renderIntroductionInstructions() {
		if (IntroductionSkin != null && GUI.skin != IntroductionSkin) {
			GUI.skin = IntroductionSkin;
		}
		
		float width = screenWidth * 0.3f,
		height = screenHeight * 0.6f;
		float x = (screenWidth/2f) - (width/2f),
		y = (screenHeight/2f) - (height/2f);
		
		string boxString = "";
		boxString += "Welcome to Right to Rule Prototype 0.0.2!\n\n";
		
		if (scenarioHandler.LastScenario == ScenarioHandler.ScenarioState.NONE) {
			boxString += "This game is inspired by Tower Defense games, thus you must defend your Gate of Life by buying and placing units to defend against incoming waves of creeps (see arrows in-game). ";
			boxString += "Your units respawn if they die as soon as the next build phase starts. ";
			boxString += "Since this is a prototype game meant for academic testing, the game features two scenarios - one with tactics and one without. ";
			boxString += "This means that after you lose or win the first scenario, a second scenario will be automatically launched. Please play through both scenarios. ";
		}
		else if (!scenarioHandler.DoneTesting) {
			boxString += "Congratulations on finishing the first scenario and thank you for your feedback. The second scenario will commence as soon as you click 'continue'. ";
		}
		else {
			boxString += "Congratulations on finishing both scenarios and thereby concluding the formal test, and thank you very much for your feedback. ";
			boxString += "Now you may play freely without being interrupted by questions or you may quit the game at will. ";
		}
		
		boxString += "\n\nGood luck and have fun!";
		
		GUILayout.BeginArea(new Rect(x, y, width, height));
		
		GUILayout.Box(boxString);
		
		//GUILayout.FlexibleSpace();
		
		if (GUILayout.Button("Continue", GUILayout.Height(40f))) {
			if (scenarioHandler.LastScenario == ScenarioHandler.ScenarioState.NONE) {
				_gameController.CurrentGameState = GameController.GameState.MENU;
			}
			else {
				_gameController.CurrentGameState = GameController.GameState.PLAY;
			}
		}
		
		GUILayout.EndArea();
	}

	private void renderAllUnitsHealthbar() {
		foreach (Entity unit in unitsList) {
			renderHealthbar(unit);
		}

		foreach (Entity enemy in _gameController.enemies) {
			renderHealthbar(enemy);
		}

		if (gateRef != null) {
			renderHealthbar(gateRef);
		}
	}

	private void renderSelectedUnitsHealthbar() {
		if (SelectedUnits.Count > 0) {
			foreach (Entity selected in SelectedUnits) {
				renderHealthbar(selected);
			}
		}
	}

	private void renderHealthbar(Entity entity) {
		if (entity != null && !entity.IsDead) {
			float width = 100f, height = 20f;

			Vector3 healthBarPos = playerCam.WorldToScreenPoint(entity.transform.position);
			float barWidth = width * (entity.CurrentHitPoints / entity.MaxHitPoints);
			
			GUI.BeginGroup(new Rect(healthBarPos.x - (width/2f), screenHeight - healthBarPos.y - (width/2f), barWidth, height));
			GUI.DrawTexture(new Rect(0f, 0f, width, height), healthBarHUD, ScaleMode.StretchToFill);
			GUI.EndGroup();
		}
	}
	
	private void renderSelectedDebugFeedback() {
		if (SelectedUnits.Count > 0) {
			if (SelectedUnits[0].GetIsUnit() || SelectedUnits[0].GetIsEnemy()) {
				Entity selectedUnit = SelectedUnits[0];
				
				string debugLabel = "DEBUG FEEDBACK\n";
				
				debugLabel += "\nSelected: " + selectedUnit.Class + ": " + selectedUnit.Name;
	
				if (selectedUnit.lastAttacker != null) {
					debugLabel += "\nLast attacker: " + selectedUnit.lastAttacker.Name;
				}
				else {
					debugLabel += "\nLast attacker: None";
				}
				if (selectedUnit.attackTarget != null) {
					debugLabel += "\nAttack target: " + selectedUnit.attackTarget.Name;
				}
				else {
					debugLabel += "\nAttack target: None"; 
				}

				debugLabel += "\nAttack count: " + selectedUnit.attackCount;
				debugLabel += "\nKill count: " + selectedUnit.killCount;
				debugLabel += "\nAttacked count: " + selectedUnit.attackedCount;
				
				float unitScoreSum = 0f;
				foreach (Entity unit in unitsList) {
					unitScoreSum += unit.GetTotalScore();
				}
				debugLabel += "\n\nTotal Unit Score: " + unitScoreSum;
				
				debugLabel += "\nCurrently Selected Units Count: " + SelectedUnits.Count;
				
				unitScoreSum = 0f;
				foreach(Entity unit in SelectedUnits) {
					unitScoreSum += unit.GetTotalScore();
				}
				debugLabel += "\nCurrently Selected Units Total Score: " + unitScoreSum;
				
				float x = 10f, y = 50f, width = 300f, height = 300f;
				GUI.Box(new Rect(x, y, width, height), "");
				GUI.Label(new Rect(x+5f, y+5f, width-10f, height-6f), debugLabel);
			}
		}			
	}
	
	private void renderMarqueeSelection() {
		marqueeRect = new Rect(marqueeOrigin.x, marqueeOrigin.y, marqueeSize.x, marqueeSize.y);	
		GUI.color = new Color(0, 0, 0, .3f);
		GUI.DrawTexture(marqueeRect, marqueeGraphics);
		GUI.color = Color.white;
	}
	
	private void renderTopHUD() {
		if (PlayerHUDSkin != null && GUI.skin != PlayerHUDSkin) {
			GUI.skin = PlayerHUDSkin;
		}

		float width = screenWidth * 0.99f,
			height = screenHeight * 0.05f;
		float x = 1f,
			y = 5f;
		
		GUILayout.BeginArea(new Rect(x, y, width, height));		
		GUILayout.BeginHorizontal();
		
		if (GUILayout.Button("Main Menu (ESC)", GUILayout.Height(height))) {
			_gameController.CurrentGameState = GameController.GameState.MENU;
		}
		
		GUILayout.FlexibleSpace();
		

		
		if (_gameController.CurrentPlayState == GameController.PlayState.BUILD) {
			GUILayout.Box("Next Wave: " + (_gameController.WaveCount+1) + " / " + _gameController.MaximumWaveCount, GUILayout.Height(height));

			float maxBuildTime = _gameController.GetMaxBuildTime();
			GUILayout.Box("Build time left: " + Mathf.Round(_gameController.BuildTime) + " / " + Mathf.Round(maxBuildTime), GUILayout.Height(height));	
			
			if (GUILayout.Button(new GUIContent("Spawn Now (Space)"), GUILayout.Height(height))) {
				_gameController.ForceSpawn = true;	
				StatsCollector.AmountOfForceSpawns++;
			}
		}
		else if (_gameController.CurrentPlayState == GameController.PlayState.COMBAT) {
			GUILayout.Box("Current Wave: " + _gameController.WaveCount + " / " + _gameController.MaximumWaveCount, GUILayout.Height(height));

			GUI.color = Color.red;
			GUILayout.Box("Combat! Creeps: " + _gameController.enemies.Count + " / " + _gameController.WaveSize, GUILayout.Height(height));	
			GUI.color = Color.white;
		}
		
		GUILayout.FlexibleSpace();
		
		GUILayout.Box(new GUIContent("Unit count: " + unitsList.Count + " / " + _gameController.MaxUnitCount, UnitCountIcon), GUILayout.Height(height), GUILayout.Width(width*0.1f));
		GUILayout.Box(new GUIContent("Gold: " + PlayerGold + "g", GoldIconHUD), GUILayout.Height(height), GUILayout.Width(width*0.075f));	

		
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}

	
	private Texture2D GetTacticsIcon(UnitController.Tactics tactic) {
		Texture2D icon = null;
		switch (tactic) {
			case UnitController.Tactics.Attack: icon = playerFaction.TAISAttack; break;
			case UnitController.Tactics.Follow: icon = playerFaction.TAISAssist; break;
			case UnitController.Tactics.Guard: icon = playerFaction.TAISGuard; break;
			case UnitController.Tactics.HoldTheLine: icon = playerFaction.TAISStandGuard; break;
		}	
		
		return icon;
	}
	
	private Texture2D GetTargetIcon(UnitController.Tactics tactic, UnitController.Target target) {
		Texture2D icon = null;
		bool bAlly = (tactic == UnitController.Tactics.Follow || tactic == UnitController.Tactics.Guard);
		
		if (bAlly) {
			switch (target) {
				case UnitController.Target.Nearest: icon = playerFaction.TAISNearest_ALLY; break;
				case UnitController.Target.Strongest: icon = playerFaction.TAISStrongest_ALLY; break;
				case UnitController.Target.Weakest: icon = playerFaction.TAISWeakest_ALLY; break;
				case UnitController.Target.HighestHP: icon = playerFaction.TAISLeastDamaged_ALLY; break;
				case UnitController.Target.LowestHP: icon = playerFaction.TAISMostDamaged_ALLY; break;
			}
		}
		else {
			switch (target) {
				case UnitController.Target.Nearest: icon = playerFaction.TAISNearest_ENEMY; break;
				case UnitController.Target.Strongest: icon = playerFaction.TAISStrongest_ENEMY; break;
				case UnitController.Target.Weakest: icon = playerFaction.TAISWeakest_ENEMY; break;
				case UnitController.Target.HighestHP: icon = playerFaction.TAISLeastDamaged_ENEMY; break;
				case UnitController.Target.LowestHP: icon = playerFaction.TAISMostDamaged_ENEMY; break;
			}
		}
		
		return icon;
	}
	
	private Texture2D GetConditionIcon(UnitController.Condition condition) {
		Texture2D icon = null;
		switch (condition) {
			case UnitController.Condition.Always: icon = playerFaction.TAISAlways; break;
			case UnitController.Condition.HP_75: icon = playerFaction.TAIS75HP; break;
			case UnitController.Condition.HP_50: icon = playerFaction.TAIS50HP; break;
			case UnitController.Condition.HP_25: icon = playerFaction.TAIS25HP; break;
			case UnitController.Condition.HP_less: icon = playerFaction.TAISLessHP; break;
		}
		
		return icon;
	}

	
	private void renderBottomHUD() {
		if (PlayerHUDSkin != null && GUI.skin != PlayerHUDSkin) {
			GUI.skin = PlayerHUDSkin;
		}

		float width = (screenWidth * (1f - 0.13f - 0.01f)),
			height = screenHeight * 0.25f;
		float x = screenWidth - width,
			y = screenHeight - height;
		
		float elementWidth = width/3f,
			elementHeight = height,
			elementX = 0f;
				
		GUI.BeginGroup(new Rect(x, y, width, height)); 
		// Start Bottom HUD
		
		float unitButtonsHeight = elementHeight * 0.2f;
		float healthBarHeight = elementHeight * 0.25f;
		
		GUI.BeginGroup(new Rect(0, 0, elementWidth, elementHeight)); 
		// Unit details
		if (SelectedUnits.Count > 0) {
			Entity selectedUnit = SelectedUnits[0];
			if (selectedUnit.GetIsUnit() && _gameController.CurrentPlayState == GameController.PlayState.BUILD) {
				UnitController selectedUnitController = selectedUnit.GetComponent<UnitController>();	
				
				if (selectedUnitController != null) { 
					// Sell & Upgrade buttons if unit
					string sellTip = "Sell this unit for 50% of the cost.",
					sellLabel = "Sell (Value: " + selectedUnitController.GetSellAmount() + "g)";
					sellTip += "\nSelling will remove the unit permanently.";
					if (GUI.Button(new Rect(0f, 0f, elementWidth/2f, unitButtonsHeight), new GUIContent(sellLabel, sellTip))) {
						selectedUnitController.SellUnit();	
					}
					
					if (selectedUnitController.CanUpgrade()) {
						string upgradeTip = "Upgrade Cost: " + selectedUnitController.UpgradesInto.GoldCost + "g",
								upgradeLabel = "Upgrade (Cost: " + selectedUnitController.UpgradesInto.GoldCost + "g)"; 
						upgradeTip += "\n Upgraded Unit Score: " + selectedUnitController.UpgradesInto.GetTotalScore();
						if (GUI.Button(new Rect(elementWidth/2f, 0f, elementWidth/2f, unitButtonsHeight), new GUIContent(upgradeLabel, upgradeTip))) {
							selectedUnitController.UpgradeUnit();	
						}
					}
				}
			}
			
			// Health bar
			float hpWidth = elementWidth * (selectedUnit.CurrentHitPoints / selectedUnit.MaxHitPoints);		
			string hpLabel = selectedUnit.CurrentHitPoints.ToString("F0") + " / " + selectedUnit.MaxHitPoints.ToString("F0");
			GUI.BeginGroup(new Rect(elementX, unitButtonsHeight, hpWidth, healthBarHeight));				
				GUI.DrawTexture(new Rect(0f, 0f, elementWidth, healthBarHeight), healthBarHUD, ScaleMode.StretchToFill);
			GUI.EndGroup();

			GUI.DrawTexture(new Rect(elementX, unitButtonsHeight, elementWidth, healthBarHeight), healthContainerHUD, ScaleMode.StretchToFill);
			GUI.Label(new Rect(elementWidth/2f, unitButtonsHeight+healthBarHeight/3f, elementWidth, healthBarHeight), new GUIContent(hpLabel));
			
			
			// Class: Name
			float unitTitleHeight = elementHeight * 0.15f;			
			GUI.Box(new Rect(elementX+1f, unitButtonsHeight + healthBarHeight, elementWidth, unitTitleHeight), selectedUnit.Class + ": " + selectedUnit.Name);

			
			// Unit details
			float detailsHeight = elementHeight - healthBarHeight - unitButtonsHeight - unitTitleHeight;
			float detailsWidth = elementWidth/3f;
			GUI.BeginGroup(new Rect(elementX, healthBarHeight + unitButtonsHeight + unitTitleHeight, elementWidth, detailsHeight));
				// Profile picture
				if (selectedUnit.ProfilePicture != null) {
					GUI.Box(new Rect(0f, 0f, detailsWidth, detailsHeight), "");
					GUI.DrawTexture(new Rect(0f, 0f, detailsWidth, detailsHeight), selectedUnit.ProfilePicture, ScaleMode.ScaleToFit);
				}
				else {
					GUI.Box(new Rect(0f, 0f, detailsWidth, detailsHeight), "No picture");
				}			
			
				// Sword
				string swordTip = "Damage: " + selectedUnit.MinimumDamage.ToString("F2") + "-" + selectedUnit.MaximumDamage.ToString("F2") + "\n";
				swordTip += "Accuracy: " + selectedUnit.Accuracy.ToString("F2") + "\n";
				swordTip += "Attacks per Second: " + selectedUnit.AttacksPerSecond.ToString("F2") + "\n";
				swordTip += "Attacking Range: " + selectedUnit.AttackingRange;
				GUI.BeginGroup(new Rect(detailsWidth, 0f, elementWidth, detailsHeight));
					GUI.Box(new Rect(0f, 0f, detailsWidth, detailsHeight), new GUIContent(swordHUD, swordTip));
					
					string dpsLabel = "DPS: " + selectedUnit.GetDamagePerSecond().ToString("F1");
					GUI.Label(new Rect(5f, detailsHeight/3f, detailsWidth, detailsHeight), new GUIContent(dpsLabel));
				GUI.EndGroup();		
			
				// Shield
				string shieldTip = "Armor: " + selectedUnit.Armor.ToString("F2") + "\n";
				shieldTip += "Evasion: " + selectedUnit.Evasion.ToString("F2");
				GUI.BeginGroup(new Rect(detailsWidth*2f, 0f, elementWidth, detailsHeight/2f));
					GUI.Box(new Rect(0f, 0f, detailsWidth, detailsHeight/2f), new GUIContent(shieldHUD, shieldTip));
			
					string defenseLabel = "Armor: " + selectedUnit.Armor.ToString("F0");
					GUI.Label(new Rect(5f, 5f, detailsWidth, detailsHeight), new GUIContent(defenseLabel));				
				GUI.EndGroup();
				
				// Boot
				string bootTip = "Movement Speed: " + selectedUnit.MovementSpeed.ToString("F0") + "\n";
				bootTip += "Flee Chance: " + (selectedUnit.FleeThreshold*100f).ToString("F0") + "%\n";
				bootTip += "Perception Range: " + selectedUnit.PerceptionRange.ToString("F2");
				GUI.BeginGroup(new Rect(detailsWidth*2f, detailsHeight/2f, detailsWidth, detailsHeight/2f));
					GUI.Box(new Rect(0f, 0f, detailsWidth, detailsHeight/2f), new GUIContent(bootHUD, bootTip));
			
					string moveLabel = "Speed: " + selectedUnit.MovementSpeed.ToString("F0");
					GUI.Label(new Rect(5f, 5f, detailsWidth, detailsHeight), new GUIContent(moveLabel));
				GUI.EndGroup();
				
			GUI.EndGroup();
		}
		else {
			GUI.Box(new Rect(0f, unitButtonsHeight + healthBarHeight, elementWidth, elementHeight - healthBarHeight - unitButtonsHeight), "No unit selected");	
		}
				
		GUI.EndGroup(); // End unit details

		elementX += elementWidth;
		elementHeight -= unitButtonsHeight + healthBarHeight;		
		elementWidth = width * 0.466f;

		// Tactical AI System
		GUI.BeginGroup(new Rect(elementX, unitButtonsHeight + healthBarHeight, elementWidth-5f, elementHeight)); 
		if (scenarioHandler.CurrentScenario == ScenarioHandler.ScenarioState.WITH_TAIS) {
			if (SelectedUnits.Count > 0 && SelectedUnits[0].GetIsUnit()) {			
				UnitController selectedUnitController = SelectedUnits[0].GetComponent<UnitController>();
				
				float columnWidth = (elementWidth-5f)/2f, ///3f, // removed due to removal of conditions
					rowHeight = elementHeight * 0.25f;
				
				GUI.BeginGroup(new Rect(0f, 0f, columnWidth, elementHeight)); // Tactics
				
					GUI.Box(new Rect(0f, 0f, columnWidth, rowHeight), "Tactics");
				
					GUI.BeginGroup(new Rect(0f, rowHeight+5f, columnWidth, elementHeight-rowHeight+5f));
				
						if (selectedUnitController != null) {
							string tacticsString = selectedUnitController.GetTacticsName(selectedUnitController.currentTactic);
							string tacticsTip = "Set tactical orders for this unit. Changing the tactics will affect the units behaviour.";
							
							GUI.DrawTexture(new Rect(0f, 0f, columnWidth/6f, rowHeight), GetTacticsIcon(selectedUnitController.currentTactic), ScaleMode.ScaleToFit);

							if (GUI.Button(new Rect(columnWidth/6f, 0f, columnWidth-(columnWidth/6f), rowHeight), new GUIContent(tacticsString, tacticsTip))) {
								if (_gameController.CurrentPlayState == GameController.PlayState.BUILD) {
									if (!bSelectingTactics) 
										bSelectingTactics = true;
								}
								else {
									DisplayFeedbackMessage("You can only set Tactics in the Build phase.");
								}
							}
						}
				
					GUI.EndGroup();
				GUI.EndGroup();
				
				GUI.BeginGroup(new Rect(columnWidth, 0f, columnWidth, elementHeight)); // Target
					
					GUI.Box(new Rect(0f, 0f, columnWidth, rowHeight), "Target");
					
					GUI.BeginGroup(new Rect(0f, rowHeight+5f, columnWidth, elementHeight-rowHeight+5f));
				
						if (selectedUnitController != null) {
							string targetString = selectedUnitController.GetTargetName(selectedUnitController.currentTarget);
							Texture2D icon = GetTargetIcon(selectedUnitController.currentTactic, selectedUnitController.currentTarget);
							if (selectedUnitController.currentTactic == UnitController.Tactics.HoldTheLine) {
								targetString = "Self";
								icon = playerFaction.TAISSelf;
							}
							
							string targetTip = "Set the tactical target for this unit. Unit's tactics will be applied to the chosen target.";

							GUI.DrawTexture(new Rect(0f, 0f, columnWidth/6f, rowHeight), icon, ScaleMode.ScaleToFit);

							if (GUI.Button(new Rect(columnWidth/6f, 0f, columnWidth-(columnWidth/6f), rowHeight), new GUIContent(targetString, targetTip))) {
								if (_gameController.CurrentPlayState == GameController.PlayState.BUILD) {
									if (!bSelectingTactics) 
										bSelectingTactics = true;
								}
								else {
									DisplayFeedbackMessage("You can only set Targets in the Build phase.");
								}
							}						
						}
				
					GUI.EndGroup();
				
				GUI.EndGroup();
				/*
				GUI.BeginGroup(new Rect(columnWidth*2f, 0f, columnWidth, elementHeight)); // Condition
				
					GUI.Box(new Rect(0f, 0f, columnWidth, rowHeight), "Condition");
				
					GUI.BeginGroup(new Rect(0f, rowHeight+5f, columnWidth, elementHeight-rowHeight+5f));
				
						if (selectedUnitController != null) {
							string conditionString = selectedUnitController.GetConditionName(selectedUnitController.currentCondition);
							string conditionTip = "Set the tactical condition for this unit. The condition will affect when the unit's tactic is executed.";

							GUI.DrawTexture(new Rect(0f, 0f, columnWidth/6f, rowHeight), GetConditionIcon(selectedUnitController.currentCondition), ScaleMode.ScaleToFit);

							if (GUI.Button(new Rect(columnWidth/6f, 0f, columnWidth-(columnWidth/6f), rowHeight), new GUIContent(conditionString, conditionTip))) {
								if (_gameController.CurrentPlayState == GameController.PlayState.BUILD) {
									if (!bSelectingTactics) 
										bSelectingTactics = true;
								}
								else {
									DisplayFeedbackMessage("You can only set Conditions in the Build phase.");
								}
							}
						}
					GUI.EndGroup();

				GUI.EndGroup();
				*/
			}
			else if (SelectedUnits.Count > 0 && !SelectedUnits[0].GetIsUnit()) {
				GUI.Box(new Rect(0f, 0f, elementWidth, elementHeight), "You can only set Tactics on your own units.");
			}
			else {
				GUI.Box(new Rect(0f, 0f, elementWidth, elementHeight), "No unit selected");	
			}
		}
		else {
			GUI.Box(new Rect(0f, 0f, elementWidth, elementHeight), "You cannot set Tactics in this scenario.");
		}
				
		GUI.EndGroup(); // End Tactics

		elementX += elementWidth;
		elementWidth = width * 0.2f;

		GUI.BeginGroup(new Rect(elementX, unitButtonsHeight + healthBarHeight, elementWidth, elementHeight)); // Spawn Grid	
		if (playerFaction.FactionUnits.Count > 0) {
			if (!GUI.skin.button.wordWrap)
				GUI.skin.button.wordWrap = true;

			GUI.BeginGroup(new Rect(0f, 0f, elementWidth, elementHeight));
				createSpawnButton(0, elementWidth, elementHeight);
			GUI.EndGroup();
			
			GUI.BeginGroup(new Rect(0f, elementHeight/2f, elementWidth, elementHeight));
				createSpawnButton(2, elementWidth, elementHeight);
			GUI.EndGroup();
			
			GUI.BeginGroup(new Rect(elementWidth/2f, 0f, elementWidth, elementHeight));
				createSpawnButton(1, elementWidth, elementHeight);
			GUI.EndGroup();
			
			GUI.BeginGroup(new Rect(elementWidth/2f, elementHeight/2f, elementWidth, elementHeight));
				createSpawnButton(3, elementWidth, elementHeight);
			GUI.EndGroup();
		}
		else {
			GUI.Box(new Rect(0f, 0f, elementWidth, elementHeight), "ERROR: No spawnable units");	
		}
		
		GUI.EndGroup(); // end spawn grid
		
		
		GUI.EndGroup(); // End Bottom HUD

		if (scenarioHandler.CurrentScenario == ScenarioHandler.ScenarioState.WITH_TAIS) {
			if (SelectedUnits.Count > 0 && SelectedUnits[0].GetIsUnit()) {
				renderTacticsInterface();
			}
		}

		if (GUI.tooltip != "") {
			GUI.skin.box.wordWrap = true;
			Vector2 mousePos = Input.mousePosition;
			float tipWidth = 250f, tipHeight = 70f;
			GUI.Box(new Rect(mousePos.x - tipWidth, screenHeight - mousePos.y - tipHeight, tipWidth, tipHeight), GUI.tooltip);
		}		
	}

	private void renderTacticsInterface() {
		UnitController selectedUnit = SelectedUnits[0].GetComponent<UnitController>();

		if (selectedUnit == null) {
			Debug.LogWarning("Could not find selected unit in renderTacticsInterface");
		}		
		else if (bSelectingTactics && scenarioHandler.CurrentScenario == ScenarioHandler.ScenarioState.WITH_TAIS) {
			if (TAISSkin != null && GUI.skin != TAISSkin) {
				GUI.skin = TAISSkin;
			}

			float width = screenWidth * 0.6f,
			height = screenHeight * 0.4f;
			float x = (screenWidth - width)/2f,
			y = height/2f;

			float elementWidth = width/2f, //width/3f, // removed 3 because of removal of conditions
			elementHeight = height/8f;

			System.Array arr;
			int count;

			GUI.Box(new Rect(x, y, width, height), "");

			GUILayout.BeginArea(new Rect(x, y, width, height));	
			GUILayout.BeginHorizontal();

			// Tactics
			GUILayout.BeginVertical(GUILayout.Width(elementWidth));

				GUILayout.BeginHorizontal();
					Texture2D tacticsIcon = GetTacticsIcon(selectedUnit.currentTactic);
					if (tacticsIcon != null) {
						GUILayout.Space(elementWidth/5f);
						GUI.DrawTexture(new Rect(0f, 2f, elementWidth/5f, elementHeight), tacticsIcon, ScaleMode.ScaleToFit);
					}
					
					GUILayout.Box("Current Tactic: " + selectedUnit.GetTacticsName(selectedUnit.currentTactic), GUILayout.Height(elementHeight));

				GUILayout.EndHorizontal();
				GUILayout.Space(5f);

				arr = selectedUnit.GetTacticsValues();;				
				count = arr.Length;				

				for (int i = 0; i < count; i++) {
					UnitController.Tactics tactic = (UnitController.Tactics) i;
					string tacticName = selectedUnit.GetTacticsName(tactic);

					Texture2D icon = GetTacticsIcon(tactic);

					GUILayout.BeginHorizontal();
						if (icon != null) {
							GUILayout.Space(elementWidth/5f);
							GUI.DrawTexture(new Rect(0f,((i+1)*elementHeight+(5f*i)+10f), elementWidth/5f, elementHeight), icon, ScaleMode.ScaleToFit);
						}
						if (GUILayout.Button(new GUIContent(tacticName, selectedUnit.GetTacticsTip(tactic)), GUILayout.Height(elementHeight))) {
							if (selectedUnit.currentTactic != tactic) {
								selectedUnit.currentTactic = tactic;
								StatsCollector.TotalTacticalChanges++;
								StatsCollector.AmountOfTacticsChanges++;
							}
						}
					GUILayout.EndHorizontal();
				}

			GUILayout.EndVertical();

			// Targets
			GUILayout.BeginVertical(GUILayout.Width(elementWidth));

				arr = selectedUnit.GetTargetsValues();						

				if (arr != null) {
					GUILayout.BeginHorizontal();
						Texture2D targetIcon = GetTargetIcon(selectedUnit.currentTactic, selectedUnit.currentTarget);
						if (targetIcon != null) {
							GUILayout.Space(elementWidth/5f);
							GUI.DrawTexture(new Rect(elementWidth, 2f, elementWidth/5f, elementHeight), targetIcon, ScaleMode.ScaleToFit);
						}
						GUILayout.Box("Current Target: " + selectedUnit.GetTargetName(selectedUnit.currentTarget), GUILayout.Height(elementHeight));
					GUILayout.EndHorizontal();
					GUILayout.Space(5f);

					count = arr.Length;
					for (int i = 0; i < count; i++) {
						UnitController.Target target = (UnitController.Target) i;
						string targetName = selectedUnit.GetTargetName(target);

						Texture2D icon = GetTargetIcon(selectedUnit.currentTactic, target);

						GUILayout.BeginHorizontal();
							if (icon != null) {
								GUILayout.Space(elementWidth/5f);
								GUI.DrawTexture(new Rect(elementWidth, ((i+1)*elementHeight+(5f*i)+10f), elementWidth/5f, elementHeight), icon, ScaleMode.ScaleToFit);
							}
							if (GUILayout.Button(new GUIContent(targetName, selectedUnit.GetTargetTip(target)), GUILayout.Height(elementHeight))) {
								if (selectedUnit.currentTarget != target) {
									selectedUnit.currentTarget = target;
									StatsCollector.TotalTacticalChanges++;
									StatsCollector.AmountOfTargetsChanges++;
								}
							}
						GUILayout.EndHorizontal();
					}
				}
				else {
					GUILayout.BeginHorizontal();
						Texture2D icon = playerFaction.TAISSelf;
						if (icon != null) {
							GUILayout.Space(elementWidth/5f);
							GUI.DrawTexture(new Rect(elementWidth, 2f, elementWidth/5f, elementHeight), icon, ScaleMode.ScaleToFit);
						}
						GUILayout.Box("Current Target: Self", GUILayout.Height(elementHeight));
					GUILayout.EndHorizontal();
					GUILayout.Space(5f);

					GUILayout.BeginHorizontal();						
						if (icon != null) {
							GUILayout.Space(elementWidth/5f);
							GUI.DrawTexture(new Rect(elementWidth, elementHeight+5f, elementWidth/5f, elementHeight), icon, ScaleMode.ScaleToFit);
						}
						GUILayout.Box(new GUIContent("Self", "Stand Guard tactic can only target self."), GUILayout.Height(elementHeight));				

					GUILayout.EndHorizontal();
				}
			GUILayout.EndVertical();

			/*
			// Conditions
			GUILayout.BeginVertical(GUILayout.Width(elementWidth));

				GUILayout.BeginHorizontal();
					Texture2D conditionIcon = GetConditionIcon(selectedUnit.currentCondition);
					if (conditionIcon != null) {
						GUILayout.Space(elementWidth/5f);
						GUI.DrawTexture(new Rect(elementWidth*2f, 2f, elementWidth/5f, elementHeight), conditionIcon, ScaleMode.ScaleToFit);
					}
					GUILayout.Box("Current Condition: " + selectedUnit.GetConditionName(selectedUnit.currentCondition), GUILayout.Height(elementHeight));
				GUILayout.EndHorizontal();
				GUILayout.Space(5f);

				arr = selectedUnit.GetConditionsValues();
				count = arr.Length;

				for (int i = 0; i < count; i++) {
					UnitController.Condition condition = (UnitController.Condition) i;
					string conditionName = selectedUnit.GetConditionName(condition);

					Texture2D icon = GetConditionIcon(condition);

					GUILayout.BeginHorizontal();
						if (icon != null) {
							GUILayout.Space(elementWidth/5f);
							GUI.DrawTexture(new Rect(elementWidth*2f, ((i+1)*elementHeight+(5f*i)+10f), elementWidth/5f, elementHeight), icon, ScaleMode.ScaleToFit);
						}
						if (GUILayout.Button(new GUIContent(conditionName), GUILayout.Height(elementHeight))) {
							if (selectedUnit.currentCondition != condition) {
								selectedUnit.currentCondition = condition;
								StatsCollector.TotalTacticalChanges++;
								StatsCollector.AmountOfConditionChanges++;
							}	
					}
					GUILayout.EndHorizontal();
				}

			GUILayout.EndVertical();
*/
			GUILayout.EndHorizontal();

			GUILayout.FlexibleSpace();

			if (GUILayout.Button("Confirm ('Q' key)", GUILayout.Width(width), GUILayout.Height(elementHeight))) {
				if (bSelectingTactics)
					bSelectingTactics = false;	
			}

			GUILayout.EndArea();
		}
	}
	
	private void createSpawnButton(int index, float elementWidth, float elementHeight) {
		UnitController unit = playerFaction.FactionUnits[index].GetComponent<UnitController>();
		string tip = "Gold Cost: " + unit.GoldCost + "g\n"; 
		tip += "Unit Class: " + unit.Class + "\n";
		tip += "Unit Score: " + unit.GetTotalScore();
		
		GUIContent btn = new GUIContent((index+1) + " : " + unit.Name + " (" + unit.Class + ")", tip);
		if (GUI.Button(new Rect(0f, 0f, elementWidth/2f, elementHeight/2f), btn)) {
			spawnUnit(index);
		}
	}
	
	private void createSpawnShortcuts() {
		for (int i = 0; i < playerFaction.FactionUnits.Count; i++) {
			if (Input.GetKeyUp((KeyCode)(49 + i))) {
				spawnUnit(i);	
			}
		}
	}
	
	public UnitController GetCurrentlyPlacingUnit() {
		return currentlyPlacingUnit;
	}

	public void SetCurrentlyPlacingUnit(UnitController unit) {
		currentlyPlacingUnit = unit;
	}
	
	private void spawnUnit(int index) {
		ClearPlacingUnit();
		
		GameObject newUnit = Instantiate(playerFaction.FactionUnits[index].gameObject) as GameObject;
		newUnit.SetActive(true);
		
		int cost = newUnit.GetComponent<Unit>().GoldCost;
		if (_gameController.CurrentPlayState != GameController.PlayState.BUILD) {
			Destroy(newUnit);
			DisplayFeedbackMessage("You may only buy units in the Build phase.");
		}
		else if (_gameController.MaxUnitCount <= unitsList.Count) {
			Destroy(newUnit);
			DisplayFeedbackMessage("You cannot build more units, you have reached the maximum.");
		}
		else if (PlayerGold >= cost) {
			SetCurrentlyPlacingUnit(newUnit.GetComponent<UnitController>());
			GetCurrentlyPlacingUnit().playerOwner = this;
		}
		else {
			Destroy(newUnit);
			DisplayFeedbackMessage("You do not have enough gold.");
		}
	}
	
	private void renderFeedbackMessage() {
		if (feedbackText != "") {
			float width = screenWidth * 0.5f,
				height = 30f;
			float x = (screenWidth - width)/2f,
				y = (screenHeight/2f) - (height/2f);
			
			GUILayout.BeginArea(new Rect(x, y, width, height));
			
			Color guiColor = GUI.color;
			GUI.color = feedbackColor;
			GUILayout.Box(feedbackText, GUILayout.Width(width), GUILayout.Height(height));
			GUI.color = guiColor;
			
			GUILayout.EndArea();			
		}
	}
	
	public void DisplayFeedbackMessage(string text) {
		DisplayFeedbackMessage(text, Color.red);
	}
	
	public void DisplayFeedbackMessage(string text, Color color) {
		feedbackText = text;
		Invoke("stopDisplayFeedback", ShowFeedbackTime);
		feedbackColor = color;
	}
	
	private void stopDisplayFeedback() {
		feedbackText = "";	
	}
}
