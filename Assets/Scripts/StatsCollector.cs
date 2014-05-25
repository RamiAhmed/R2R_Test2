using UnityEngine;
using System.IO;
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

	private GameController _gameController = null;
	private ScenarioHandler scenarioHandler = null;
	private static PlayerController _playerRef = null;
	
	void Start() {
		_gameController = this.GetComponent<GameController>();
		if (_gameController == null) {
			_gameController = this.GetComponentInChildren<GameController>();
		}

		scenarioHandler = GameObject.FindGameObjectWithTag("ScenarioHandler").GetComponent<ScenarioHandler>();
		_playerRef = _gameController.players[0].GetComponent<PlayerController>();
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
		/*if (scenarioHandler != null && scenarioHandler.CurrentScenario != ScenarioHandler.ScenarioState.NONE) {
			if (Scenario != scenarioHandler.CurrentScenario.ToString()) {
				Scenario = scenarioHandler.CurrentScenario.ToString();
			}
		}
		else {
			if (Scenario != "TAIS")
				Scenario = "TAIS";
		}*/
		TotalTimeSpent += Time.deltaTime;

		if (Input.GetKeyDown(KeyCode.Print) || Input.GetKeyDown(KeyCode.SysReq) || Input.GetKeyDown(KeyCode.F10)) {
			TakeScreenshot();
		}
	}

	public static void TakeScreenshot() {
		string path = ScreenShotName();
		Debug.Log("Saving Screenshot at path: " + path);
		Application.CaptureScreenshot(path);
		
		//_playerRef.StartCoroutine(ScreenshotRoutine());
	}

	private static string ScreenShotName() {
		return string.Format("{0}/ScreenShots/ScreenShot_{1}.png", 
		                     Application.dataPath, 
		                     System.DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss"));
	}
	private static IEnumerator ScreenshotRoutine() {
		yield return new WaitForEndOfFrame();

		Camera cam = _playerRef.PlayerCam;
		if (cam != null) {
			/*RenderTexture currentRT = RenderTexture.active;
			
			RenderTexture.active = cam.targetTexture;
			cam.Render();
			Texture2D imageOverview = new Texture2D(cam.targetTexture.width, cam.targetTexture.height, TextureFormat.RGB24, false);
			imageOverview.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
			imageOverview.Apply();
			
			RenderTexture.active = currentRT;	*/

			Texture2D tex = new Texture2D(Screen.width, Screen.height);
			tex.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);
			tex.Apply();
			
			// Encode texture into PNG
			//byte[] bytes = imageOverview.EncodeToPNG();
			byte[] bytes = tex.EncodeToPNG();
			
			// save in memory
			string filename = ScreenShotName();
			File.WriteAllBytes(filename, bytes);
		}
		else {
			Debug.LogError("Could not find player camera");
		}
	}
}
