using UnityEngine;
using System.Collections;

public class StatsCollector : MonoBehaviour {
	public static float TotalTimePlayed = 0f,
						TotalTimeSpent = 0f;

	public static int WaveCount = 0,
		TotalTacticalChanges = 0,
		AmountOfTacticsChanges = 0,
		AmountOfTargetsChanges = 0,
		AmountOfConditionChanges = 0,
		TotalGoldSpent = 0,
		TotalGoldEarned = 0,
		TotalEnemiesKilled = 0,
		TotalUnitsDied = 0,
		GoldDepositLeft = 0,
		AmountOfUnitsBought = 0,
		AmountOfUnitUpgrades = 0,
		AmountOfUnitsSold = 0,
		AmountOfUnitsMoved = 0,
		AmountOfSelections = 0,
		AmountOfUnitSelections = 0,
		AmountOfEnemySelections = 0,
		AmountOfForceSpawns = 0;

	public static string Scenario = "";

	private GameController _gameController = null;
	private ScenarioHandler scenarioHandler = null;
	
	void Start() {
		_gameController = this.GetComponent<GameController>();
		if (_gameController == null) {
			_gameController = this.GetComponentInChildren<GameController>();
		}

		scenarioHandler = GameObject.FindGameObjectWithTag("ScenarioHandler").GetComponent<ScenarioHandler>();
	}

	void Update() {
		if (WaveCount != _gameController.WaveCount) {
			WaveCount = _gameController.WaveCount;
		}
		if (TotalTimePlayed != _gameController.GameTime) {
			TotalTimePlayed = _gameController.GameTime;
		}
		if (_gameController.players[0].PlayerGold != GoldDepositLeft) {
			GoldDepositLeft = _gameController.players[0].PlayerGold;
		}
		if (scenarioHandler.CurrentScenario != ScenarioHandler.ScenarioState.NONE) {
			if (Scenario != scenarioHandler.CurrentScenario.ToString()) {
				Scenario = scenarioHandler.CurrentScenario.ToString();
			}
		}
		TotalTimeSpent += Time.deltaTime;

		if (Input.GetKeyDown(KeyCode.Print) || Input.GetKeyDown(KeyCode.SysReq) || Input.GetKeyDown(KeyCode.F10)) {
			string time = System.DateTime.Now.Hour + "-" + System.DateTime.Now.Minute;
			string date = System.DateTime.Now.Day + "-" + System.DateTime.Now.Month + "-" + System.DateTime.Now.Year + " " + time;
			string path = Application.dataPath + "/ScreenShot_" + date + ".png";
			Debug.Log("Saving Screenshot at path: " + path);
			Application.CaptureScreenshot(path);
		}
	}

}
