using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestionnaireHandler : MonoBehaviour {

	public bool QuestionnaireEnabled = true;

	public enum QuestionnaireState {
		NONE = 0,
		DEMOGRAPHICS,
		STARTING,
		DURING,
		AFTER
	};

	public QuestionnaireState CurrentState = QuestionnaireState.NONE;

	public int QuestionnaireWaveFrequency = 3;

	public float VerticalSpacing = 15f,
				TextAreaHeight = 50f,
				GUIElementHeight = 60f;
	 
	public int MaxDuringInterrupts = 3;

	public GUISkin QuestionnaireSkin = null;

	private int currentDuring = 1;

	private DatabaseHandler dbHandler = null;
	private GameController _gameController = null;

	private bool showingQuestionnaire = false; 

	private IDictionary questionsDict = null;
	private Dictionary<string,string> answersDict = null;

	private Dictionary<string,int> selectionDict = null;
	private Dictionary<string,string> textDict = null;

	private Rect questionnaireRect;

	private ScenarioHandler scenarioHandler = null;

	private Vector2 scrollPosition;
	
	void Start () {
		if (QuestionnaireEnabled) {
			dbHandler = GameObject.FindGameObjectWithTag("ScenarioHandler").GetComponent<DatabaseHandler>();
			if (dbHandler == null) {
				dbHandler = GameObject.FindGameObjectWithTag("ScenarioHandler").GetComponentInChildren<DatabaseHandler>();
			}

			_gameController = this.GetComponent<GameController>();
			if (_gameController == null) {
				_gameController = this.GetComponentInChildren<GameController>();
			}

			answersDict = new Dictionary<string,string>();
			selectionDict = new Dictionary<string,int>();
			textDict = new Dictionary<string,string>();

			float width = 300f, height = 150f,
				screenWidth = Screen.width * 0.9f,
				screenHeight = Screen.height * 0.9f;

			width = screenWidth > width ? screenWidth : width;
			height = screenHeight > height ? screenHeight : height;

			questionnaireRect = new Rect((Screen.width/2f) - (width/2f), (Screen.height/2f) - (height/2f), width, height);

			CurrentState++;

			scenarioHandler = GameObject.FindGameObjectWithTag("ScenarioHandler").GetComponent<ScenarioHandler>();
		}

		if (scenarioHandler != null && scenarioHandler.DoneTesting) {
			QuestionnaireEnabled = false;
		}
	}

	void Update () {
		if (QuestionnaireEnabled) {
			if (_gameController.CurrentGameState == GameController.GameState.QUESTIONNAIRE) {
				if (!showingQuestionnaire) {
					showingQuestionnaire = true;

					if (questionsDict == null && dbHandler.questionsDict != null) {
						questionsDict = dbHandler.questionsDict;
					}

					if (questionsDict == null) {
						showingQuestionnaire = false;
					}
				}
			}
			else {
				if (showingQuestionnaire) {
					showingQuestionnaire = false;
				}
			}
		}
	}

	void OnGUI() {
		if (QuestionnaireEnabled) {
			if (showingQuestionnaire) {
				if (QuestionnaireSkin != null && GUI.skin != QuestionnaireSkin) {
					GUI.skin = QuestionnaireSkin;
				}

				questionnaireRect = GUILayout.Window(0, questionnaireRect, DrawQuestionnaire, "");
				GUI.BringWindowToFront(0);
			}
		}
	}

	private void DrawQuestionnaire(int windowID) {
		if (!GUI.skin.box.wordWrap) {
			GUI.skin.box.wordWrap = true;
		}

		GUILayout.BeginVertical();
		scrollPosition = GUILayout.BeginScrollView(scrollPosition);

		switch (CurrentState) {
			case QuestionnaireState.DEMOGRAPHICS: buildDemographics(); break;
			case QuestionnaireState.STARTING: buildStarting(); break;
			case QuestionnaireState.DURING: buildDuring(); break;
			case QuestionnaireState.AFTER: buildAfter(); break;
		}

		if (_gameController.CurrentGameState == GameController.GameState.QUESTIONNAIRE) {
			if (GetQuestionsAnswered()) {
				GUILayout.FlexibleSpace();

				if (CurrentState != QuestionnaireState.AFTER) {
					if (GUILayout.Button("Continue", GUILayout.Height(GUIElementHeight))) {
						dbHandler.ReadyData(answersDict, (CurrentState == QuestionnaireState.DEMOGRAPHICS));
						answersDict.Clear();

						if (CurrentState == QuestionnaireState.DURING) {
							currentDuring++;
							if (currentDuring > MaxDuringInterrupts) {
								//Debug.Log("Iterate scenario: " + CurrentState.ToString() + " => " + (CurrentState+1).ToString());
								CurrentState++;
							}
						}
						else {
							//Debug.Log("Iterate scenario: " + CurrentState.ToString() + " => " + (CurrentState+1).ToString());
							CurrentState++;
						}

						_gameController.CurrentGameState = GameController.GameState.PLAY;
					}
				}
				else {
					if (GUILayout.Button("Submit Answers", GUILayout.Height(GUIElementHeight))) {
						dbHandler.ReadyData(answersDict);
						answersDict.Clear();

						dbHandler.SubmitAllData();
						QuestionnaireEnabled = false;

						if (!_gameController.bQuittingGame) {
							_gameController.CurrentGameState = GameController.GameState.PLAY;
						}
						else {
							_gameController.QuitGame();
						}
					}
				}
			}
		}
		else {
			Debug.Log("Return to game, no time for questionnaire");
		}

		GUILayout.EndScrollView();
		GUILayout.EndVertical();
	}

	private void buildDemographics() {
		//Debug.Log("build demographics");
		if (!dbHandler.SavedDemographics) {
			GUILayout.Box("DEMOGRAPHICS", GUILayout.Height(GUIElementHeight));

			buildSection("Demographics");
		}
		else {
			//Debug.Log("Loading previous demographics");
			dbHandler.ReadyData(null, true);
			//Debug.Log("Iterate scenario: " + CurrentState.ToString() + " => " + (CurrentState+1).ToString());
			CurrentState++;
			_gameController.CurrentGameState = GameController.GameState.PLAY;
		}
	}

	private void buildStarting() {
		GUILayout.Box("BEFORE STARTING", GUILayout.Height(GUIElementHeight));

		buildSection("Starting");
	}

	private void buildDuring() {
		GUILayout.Box("DURING " + currentDuring.ToString(), GUILayout.Height(GUIElementHeight));

		buildSection("During");
	}

	private void buildAfter() {
		GUILayout.Box("AFTER", GUILayout.Height(GUIElementHeight));

		buildSection("After");

		if (scenarioHandler.LastScenario != ScenarioHandler.ScenarioState.NONE) {
			string id = "preferred_scenario";
			if (!selectionDict.ContainsKey(id)) {
				selectionDict.Add(id, -1);
			}

			string[] options = new string[]{"With Tactics", "Without Tactics"};
			selectionDict[id] = addMultipleChoices(id, "Preferred Scenario", options, "Please choose which scenario you preferred.", selectionDict[id]);
		}
	}

	private void buildSection(string section) {
		IList sectionList = (IList) questionsDict[section];
		foreach (IDictionary dict in sectionList) {
			string id = dict["ID"].ToString();
			string question = dict["Question"].ToString();
			List<string> options = convertFromIList((IList)dict["Options"]);
			string helperText = dict["HelperText"].ToString();

			if (CurrentState == QuestionnaireState.DURING) {
				id += "_" + currentDuring.ToString();
			}

			if (options != null && options.Count > 0) {
				if (!selectionDict.ContainsKey(id)) {
					selectionDict.Add(id, -1);
				}

				selectionDict[id] = addMultipleChoices(id, question, options.ToArray(), helperText, selectionDict[id]);
			}
			else {
				if (!textDict.ContainsKey(id)) {
					textDict.Add(id, "");
				}

				textDict[id] = addTextQuestion(id, question, helperText, textDict[id]);
			}
		}
	}

	private List<string> convertFromIList(IList list) {
		List<string> newList = new List<string>();
		foreach (object item in list) {
			newList.Add(item.ToString());
		}

		return newList;
	}

	private int addMultipleChoices(string id, string question, string[] options, string helperText, int selection) {
		GUILayout.Box(question + "\n" + helperText, GUILayout.Height(GUIElementHeight));

		selection = GUILayout.Toolbar(selection, options, GUILayout.Height(GUIElementHeight));
		if (selection > -1) {
			AddOrReplaceToDict(id, options[selection]);
		}

		GUILayout.Space(VerticalSpacing);

		return selection;
	}

	private string addTextQuestion(string id, string question, string helperText, string returnText) {
		GUILayout.Box(question + "\n" + helperText, GUILayout.Height(GUIElementHeight));

		returnText = GUILayout.TextArea(returnText, GUILayout.Height(TextAreaHeight));
		if (returnText.Length > 0) {
			AddOrReplaceToDict(id, returnText);
		}

		GUILayout.Space(VerticalSpacing);

		return returnText;
	}

	private void AddOrReplaceToDict(string key, string value) {
		AddOrReplaceToDict(answersDict, key, value);
	}

	private void AddOrReplaceToDict(Dictionary<string,string> dict, string key, string value) {
		if (dict.ContainsKey(key)) {
			dict[key] = value;
		}
		else {
			dict.Add(key, value);
		}
	}	

	private void AddOrReplaceToDict(Dictionary<string,int> dict, string key, int value) {
		if (dict.ContainsKey(key)) {
			dict[key] = value;
		}
		else {
			dict.Add(key, value);
		}
	}	

	private bool GetQuestionsAnswered() {
		bool bAnswered = true;
		foreach (KeyValuePair<string,string> answer in answersDict)	{
			if ((answer.Value == "" || answer.Key == "") && !answer.Key.Contains("comment")) {
				bAnswered = false;
				break;
			}
		}

		if (bAnswered) {
			foreach (KeyValuePair<string,string> text in textDict) {
				if ((text.Value == "" || text.Key == "") && !text.Key.Contains("comment")) {
					bAnswered = false;
					break;
				}
			}
		}

		if (bAnswered) {
			foreach (KeyValuePair<string,int> selection in selectionDict) {
				if (selection.Value < 0 || selection.Key == "") {
					bAnswered = false;
					break;
				}
			}
		}

		return bAnswered;
	}
}
