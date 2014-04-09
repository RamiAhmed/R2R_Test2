using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour {
	
	public List<Entity> enemies;
	public List<PlayerController> players;
	
	public float GameTime = 0f;
	public float BuildTime = 0f;
	public int WaveCount = 0;

	[Range(1, 10)]
	public int WaveAdditionFactor = 1;

	[Range(5, 50)]
	public int MaxWaveSize = 20;

	[Range(9, 100)]
	public int MaximumWaveCount = 10;
	
	[HideInInspector]
	public bool ForceSpawn = false;
	
	public int MaxUnitCount = 50;
	
	public float MaxBuildTime = 30f;
	public int WaveSize = 10;
	
	public float StartYPosition = 30f;
	
	public bool GameEnded = false;
	public bool GameWon = false;

	public AudioClip BuildMusic, CombatMusic, BackgroundMusic;
	private Dictionary<string, AudioSource> audioSources;

	public bool bQuittingGame = false;

	private QuestionnaireHandler qHandler = null;

	private ScenarioHandler scenarioHandler = null;

	private bool bIntroductionShown = false;

	public enum GameState {
		INTRODUCTION,
		MENU,
		PLAY,
		PAUSED,
		ENDING,
		QUESTIONNAIRE
	};
	
	public GameState CurrentGameState = GameState.QUESTIONNAIRE;
	
	public enum PlayState {
		BUILD,
		COMBAT,
		NONE
	};
	
	public PlayState CurrentPlayState = PlayState.NONE;
	
	private bool hasSpawnedThisWave = false;
	
	private GameObject miniMapCam;
	
	private bool isRestarting = false;

	private MenuController menuController = null;

	void Start () {
		enemies = new List<Entity>();
		players = new List<PlayerController>();
		
		PlayerController player = (Instantiate(Resources.Load("Player/PlayerObject")) as GameObject).GetComponent<PlayerController>();
		GameObject[] points = GameObject.FindGameObjectsWithTag("Waypoint");
		foreach (GameObject point in points) {
			if (point.transform.name.Contains("End")) {
				Vector3 targetPos = point.transform.position;
				targetPos.y += StartYPosition;
				player.transform.position = targetPos;
				break;
			}
		}
		players.Add(player);
	
		miniMapCam = GameObject.FindGameObjectWithTag("MiniMapCam");

		audioSources = new Dictionary<string, AudioSource>();

		addAudioSource("Background", BackgroundMusic);
		addAudioSource("Build", BuildMusic, 0.5f, true);
		addAudioSource("Combat", CombatMusic, 0.1f, true);

		qHandler = this.GetComponent<QuestionnaireHandler>();
		if (qHandler == null) {
			qHandler = this.GetComponentInChildren<QuestionnaireHandler>();
		}

		if (!qHandler.QuestionnaireEnabled) {
			CurrentGameState = GameState.MENU;
		}

		scenarioHandler = GameObject.FindGameObjectWithTag("ScenarioHandler").GetComponent<ScenarioHandler>();

		menuController = this.GetComponent<MenuController>();
		if (menuController == null) {
			menuController = this.GetComponentInChildren<MenuController>();
		}
	}

	private void stopBuildMusic() {
		stopAudioSource("Build");
	}

	private void stopCombatMusic() {
		stopAudioSource("Combat");
	}

	private void stopAudioSource(string type) {
		if (audioSources.ContainsKey(type)) {
			audioSources[type].Stop();
		}
	}

	private void addAudioSource(string type, AudioClip audioClip) {
		addAudioSource(type, audioClip, 1.0f);
	}

	private void addAudioSource(string type, AudioClip audioClip, float volume) {
		addAudioSource(type, audioClip, volume, false);
	}

	private void addAudioSource(string type, AudioClip audioClip, float volume, bool looping) {
		if (audioClip != null) {
			audioSources.Add(type, this.gameObject.AddComponent<AudioSource>());
			audioSources[type].playOnAwake = false;
			audioSources[type].clip = audioClip;
			audioSources[type].volume = volume;
			audioSources[type].loop = looping;
		}
	}

	private void playBackgroundMusic() {
		playAudioClip("Background");
	}

	private void playCombatMusic() {
		playAudioClip("Combat");
	}

	private void playBuildMusic() {
		playAudioClip("Build");
	}

	private void playAudioClip(string type) {
		if (audioSources != null) {
			if (audioSources.ContainsKey(type)) {
				if (audioSources[type].clip != null) {
					if (!audioSources[type].isPlaying) {
						audioSources[type].Play();
					}
				}
			}
		}
	}
	
	public float GetMaxBuildTime() {
		return WaveCount <= 0 ? MaxBuildTime * 2f : MaxBuildTime;	
	}

	void Update () {
		if (CurrentGameState == GameState.PLAY || CurrentGameState == GameState.MENU) {
			/*if (Input.GetKeyUp(KeyCode.Pause) || Input.GetKeyUp(KeyCode.P)) {
				if (CurrentGameState == GameState.PAUSED) {
					CurrentGameState = GameState.PLAY;
				}
				else if (CurrentGameState == GameState.PLAY) {
					CurrentGameState = GameState.PAUSED;					
				}
			}*/
			
			if (Input.GetKeyUp(KeyCode.Escape)) {
				if (menuController.ShowingInstructions > 0) {
					menuController.ShowingInstructions = 0;
				}
				else if (players[0].GetCurrentlyPlacingUnit() != null) {
					players[0].ClearPlacingUnit();
				}
				else {
					if (CurrentGameState == GameState.PLAY || CurrentGameState == GameState.PAUSED) {
						CurrentGameState = GameState.MENU;	
					}
					else if (CurrentGameState == GameState.MENU && GameTime > 0f) {
						CurrentGameState = GameState.PLAY;
					}					
				}
			}
		}

		if (CurrentGameState == GameState.PLAY) {
			if (!bIntroductionShown && qHandler.QuestionnaireEnabled) {
				CurrentGameState = GameState.INTRODUCTION;
				bIntroductionShown = true;
				return;
			}

			GameTime += Time.deltaTime;
			
			if (!miniMapCam.activeSelf) {
				miniMapCam.SetActive(true);
			}
			
			if (CurrentPlayState == PlayState.BUILD) {
				playBuildMusic();

				if (qHandler.QuestionnaireEnabled && qHandler.CurrentState == QuestionnaireHandler.QuestionnaireState.STARTING) { 
					CurrentGameState = GameState.QUESTIONNAIRE;
				}
				else {
					BuildTime += Time.deltaTime;

					if (BuildTime >= GetMaxBuildTime() || ForceSpawn) {
						OnCombatStart();
					}
				}

			}
			else if (CurrentPlayState == PlayState.COMBAT) {
				playCombatMusic();

				if (!hasSpawnedThisWave) {
					hasSpawnedThisWave = true;
					SpawnWave();
				}				
				else if (CheckForWaveEnd()) {
					OnBuildStart();
				}
			}
			else {
				//CurrentGameState = GameState.MENU;
				CurrentPlayState = PlayState.BUILD;
			}
		}
		else if (CurrentGameState == GameState.ENDING) {
			if (qHandler.QuestionnaireEnabled && !isRestarting) {
				//Debug.Log("Game ending; set questionnaireState to AFTER");
				qHandler.CurrentState = QuestionnaireHandler.QuestionnaireState.AFTER;
				this.CurrentGameState = GameState.QUESTIONNAIRE;
			}
			else {
				EndGame(isRestarting);
			}
		}
		else if (CurrentGameState == GameState.QUESTIONNAIRE && !qHandler.QuestionnaireEnabled) {
			CurrentGameState = GameState.MENU;
		}
		else {
			if (miniMapCam.activeSelf) {
				miniMapCam.SetActive(false);
			}
		}
	}

	private void OnBuildStart() {
		if (WaveCount < MaximumWaveCount) {
			playBuildMusic();

			if (qHandler.QuestionnaireEnabled) {
				if ((qHandler.CurrentState == QuestionnaireHandler.QuestionnaireState.DURING || qHandler.CurrentState == QuestionnaireHandler.QuestionnaireState.AFTER) 
				    && ((WaveCount+1) % qHandler.QuestionnaireWaveFrequency) == 0) {
					CurrentGameState = GameState.QUESTIONNAIRE;
				}
			}

			CurrentPlayState = PlayState.BUILD;
			hasSpawnedThisWave = false;
			stopCombatMusic();

			foreach (PlayerController player in players) {
				player.DisplayFeedbackMessage("Build Phase is starting.", Color.white);
			}
		}
		else {
			GameWon = true;
			CurrentGameState = GameState.ENDING;
		}
	}

	private void OnCombatStart() {
		WaveCount++;
		ForceSpawn = false;

		if (WaveCount > 1) {
			WaveSize += WaveAdditionFactor;
			WaveSize = WaveSize > MaxWaveSize ? MaxWaveSize : WaveSize;
		}

		BuildTime = 0f;
		CurrentPlayState = PlayState.COMBAT;
		stopBuildMusic();

		foreach (PlayerController player in players) {
			player.DisplayFeedbackMessage("Combat Phase is starting.", Color.white);
		}
	}
	
	private bool CheckForWaveEnd() {
		if (enemies.Count <= 0) {	
			return true;
		}
		else {
			return false;
		}
	}
	
	private void SpawnWave() {
		for (int i = 0; i < WaveSize; i++) {
			Invoke("SpawnEnemy", (float)i/2f);	
		}
	}
	
	private void SpawnEnemy() {
		GameObject enemy = Instantiate(Resources.Load("Enemies/Enemy")) as GameObject;
		enemies.Add(enemy.GetComponent<Entity>());
	}
	
	public void RestartGame() {
		isRestarting = true;
		this.CurrentGameState = GameState.ENDING;
	}
	
	public void EndGame(bool bRestarting) {
		if (!bRestarting && !GameEnded) {
			GameEnded = true;
			scenarioHandler.IterateScenario();
		}
		else {
			Application.LoadLevel(0);	
		}
	}
	
	public void QuitGame() {
		if (qHandler.QuestionnaireEnabled && (qHandler.CurrentState == QuestionnaireHandler.QuestionnaireState.DURING || qHandler.CurrentState == QuestionnaireHandler.QuestionnaireState.AFTER)) {
			if (qHandler.CurrentState == QuestionnaireHandler.QuestionnaireState.DURING) {
				qHandler.CurrentState = QuestionnaireHandler.QuestionnaireState.AFTER;
			}
			bQuittingGame = true;
			CurrentGameState = GameState.QUESTIONNAIRE;
		}
		else {
			Application.Quit();	
		}
	}
	
}