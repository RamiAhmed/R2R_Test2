using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniJSON;

public class DatabaseHandler : MonoBehaviour {

	public string getURL = "";
	public string postURL = "";
	private WWW www, requestWWW;

	public IDictionary questionsDict = null;

	public Dictionary<string,string> DemographicsDict = null;
	public bool SavedDemographics = false;

	private WWWForm answersForm = null;

	private MouseTracker mouseTracker = null;

	//private ScenarioHandler scenarioHandler = null;

	void Start () {
		/*scenarioHandler = this.GetComponent<ScenarioHandler>();
		if (scenarioHandler == null) {
			scenarioHandler = this.GetComponentInChildren<ScenarioHandler>();
		}*/

		//if (!scenarioHandler.DoneTesting) {
			if (questionsDict == null) {
				StartCoroutine(LoadQuestions());
			}

			answersForm = new WWWForm();
		//}

		mouseTracker = GameObject.FindGameObjectWithTag("GameController").GetComponent<MouseTracker>();
		if (mouseTracker == null)
			Debug.LogError("Could not locate MouseTracker component on GameController");
	}

	public void ReadyData(Dictionary<string,string> dict) {
		ReadyData(dict, false);
	}

	public void ReadyData(Dictionary<string,string> dict, bool bDemographics) {
		if (bDemographics) {
			if (!SavedDemographics) {
				SavedDemographics = true;
				DemographicsDict = dict;
				//Debug.Log("Saving demographics: " + DemographicsDict.ToString());
			}
			else {
				//Debug.Log("Load previous demographics: " + DemographicsDict.ToString());
				dict = DemographicsDict;
			}
		}

		if (dict != null) {
			foreach (KeyValuePair<string,string> pair in dict) {
				if (!pair.Key.Equals("") && !pair.Value.Equals("")) {
					int outValue = 0;
					if (int.TryParse(pair.Value, out outValue)) {
						answersForm.AddField(pair.Key, outValue);
					}
					else {
						answersForm.AddField(pair.Key, pair.Value);
					}
				}
			}
		}
		else {
			Debug.LogWarning("Could not find dictionary in ReadyData. bDemographics: " + bDemographics);
		}

	}

	private string convertListToString(List<Vector2> list) {
		string result = "";

		foreach (Vector2 pos in list) {
			result += String.Format("({0},{1});", pos.x.ToString("F2"), pos.y.ToString("F2"));
		}

		return result;
	}

	private string convertListToString(List<Vector3> list) {
		string result = "";

		foreach (Vector3 pos in list) {
			result += String.Format("({0},{1},{2});", pos.x.ToString("F2"), pos.y.ToString("F2"), pos.z.ToString("F2"));
		}

		return result;
	}

	public void SubmitAllData() {
//		answersForm.AddField("scenario", StatsCollector.Scenario);
		answersForm.AddField("raw_time_played", Mathf.RoundToInt(StatsCollector.TotalTimePlayed));
		answersForm.AddField("raw_time_spent", Mathf.RoundToInt(StatsCollector.TotalTimeSpent));
		answersForm.AddField("raw_wave_count", StatsCollector.WaveCount);
		answersForm.AddField("raw_total_tactics_changes", StatsCollector.TotalTacticalChanges);
		answersForm.AddField("raw_tactics_changes", StatsCollector.AmountOfTacticsChanges);
		answersForm.AddField("raw_targets_changes", StatsCollector.AmountOfTargetsChanges);
		answersForm.AddField("raw_condition_changes", StatsCollector.AmountOfConditionChanges);
		answersForm.AddField("raw_gold_spent", StatsCollector.TotalGoldSpent);
		answersForm.AddField("raw_gold_earned", StatsCollector.TotalGoldEarned);
		answersForm.AddField("raw_units_died", StatsCollector.TotalUnitsDied);
		answersForm.AddField("raw_enemies_killed", StatsCollector.TotalEnemiesKilled);
		answersForm.AddField("raw_gold_deposit_left", StatsCollector.GoldDepositLeft);
		answersForm.AddField("raw_units_bought", StatsCollector.AmountOfUnitsBought);
		answersForm.AddField("raw_unit_upgrades", StatsCollector.AmountOfUnitUpgrades);
		answersForm.AddField("raw_units_sold", StatsCollector.AmountOfUnitsSold);
		answersForm.AddField("raw_units_moved", StatsCollector.AmountOfUnitsMoved);
		answersForm.AddField("raw_total_selections", StatsCollector.AmountOfSelections);
		answersForm.AddField("raw_units_selected", StatsCollector.AmountOfUnitSelections);
		answersForm.AddField("raw_enemies_selected", StatsCollector.AmountOfEnemySelections);
		answersForm.AddField("raw_force_spawns", StatsCollector.AmountOfForceSpawns);


		string eyesPos2D = convertListToString(mouseTracker.EyesPoints2D);
		string eyesPos3D = convertListToString(mouseTracker.EyesPoints3D);
		string mousePos2D = convertListToString(mouseTracker.MousePoints2D);
		string mousePos3D = convertListToString(mouseTracker.MousePoints3D);
		string leftClickPos2D = convertListToString(mouseTracker.LeftClickPoints2D);
		string leftClickPos3D = convertListToString(mouseTracker.LeftClickPoints3D);
		string rightClickPos2D = convertListToString(mouseTracker.RightClickPoints2D);
		string rightClickPos3D = convertListToString(mouseTracker.RightClickPoints3D);

		string tais_eyesPos2D = convertListToString(mouseTracker.TAISEyesPoints2D);
		string tais_eyesPos3D = convertListToString(mouseTracker.TAISEyesPoints3D);
		string tais_mousePos2D = convertListToString(mouseTracker.TAISMousePoints2D);
		string tais_mousePos3D = convertListToString(mouseTracker.TAISMousePoints3D);
		string tais_leftClickPos2D = convertListToString(mouseTracker.TAISLeftClickPoints2D);
		string tais_leftClickPos3D = convertListToString(mouseTracker.TAISLeftClickPoints3D);
		string tais_rightClickPos2D = convertListToString(mouseTracker.TAISRightClickPoints2D);
		string tais_rightClickPos3D = convertListToString(mouseTracker.TAISRightClickPoints3D);
	

		answersForm.AddField("raw_eyes_pos_2D", eyesPos2D);
		answersForm.AddField("raw_eyes_pos_3D", eyesPos3D);
		answersForm.AddField("raw_mouse_pos_2D", mousePos2D);
		answersForm.AddField("raw_mouse_pos_3D", mousePos3D);
		answersForm.AddField("raw_lclick_pos_2D", leftClickPos2D);
		answersForm.AddField("raw_lclick_pos_3D", leftClickPos3D);
		answersForm.AddField("raw_rclick_pos_2D", rightClickPos2D);
		answersForm.AddField("raw_rclick_pos_3D", rightClickPos3D);

		answersForm.AddField("tais_eyes_pos_2D", tais_eyesPos2D);
		answersForm.AddField("tais_eyes_pos_3D", tais_eyesPos3D);
		answersForm.AddField("tais_mouse_pos_2D", tais_mousePos2D);
		answersForm.AddField("tais_mouse_pos_3D", tais_mousePos3D);
		answersForm.AddField("tais_lclick_pos_2D", tais_leftClickPos2D);
		answersForm.AddField("tais_lclick_pos_3D", tais_leftClickPos3D);
		answersForm.AddField("tais_rclick_pos_2D", tais_rightClickPos2D);
		answersForm.AddField("tais_rclick_pos_3D", tais_rightClickPos3D);


		StatsCollector.TotalTimePlayed = 0;
		StatsCollector.TotalTimeSpent = 0;
		StatsCollector.WaveCount = 0;
		StatsCollector.TotalTacticalChanges = 0;
		StatsCollector.AmountOfTacticsChanges = 0;
		StatsCollector.AmountOfConditionChanges = 0;
		StatsCollector.AmountOfTargetsChanges = 0;
		StatsCollector.AmountOfConditionChanges = 0;
		StatsCollector.TotalGoldSpent = 0;
		StatsCollector.TotalGoldEarned = 0;
		StatsCollector.TotalUnitsDied = 0;
		StatsCollector.TotalEnemiesKilled = 0;
		StatsCollector.GoldDepositLeft = 0;
		StatsCollector.AmountOfUnitsBought = 0;
		StatsCollector.AmountOfUnitUpgrades = 0;
		StatsCollector.AmountOfUnitsMoved = 0;
		StatsCollector.AmountOfSelections = 0;
		StatsCollector.AmountOfUnitSelections = 0;
		StatsCollector.AmountOfEnemySelections = 0;
		StatsCollector.AmountOfForceSpawns = 0;

		StartCoroutine(SendForm());
	}

	IEnumerator SendForm() {
		requestWWW = new WWW(postURL, answersForm);
		
		yield return requestWWW;
		
		// Print the error to the console		
		if (!string.IsNullOrEmpty(requestWWW.error)) {			
			Debug.LogWarning("WWW request error: " + requestWWW.error);
			yield return null;
		}		
		else {				
			Debug.Log("WWW returned text: " + requestWWW.text);	
			yield return requestWWW.text;
		}
	}

	IEnumerator LoadQuestions() {
		www = new WWW(getURL);

		float elapsedTime = 0f;

		while (!www.isDone) {
			elapsedTime += Time.deltaTime;

			if (elapsedTime >= 10.0f) {
				Debug.LogError("WWW request to URL: " + getURL + "\n Timed out.");
				break;
			}

			yield return null;
		}

		if (!www.isDone || !string.IsNullOrEmpty(www.error)) {
			Debug.LogError("WWW request to URL: " + getURL + " failed.\n" + www.error);
			yield break;
		}

		string response = www.text;
		Debug.Log("Received text: " + response);
		Debug.Log("WWW request (loading questions) took: " + elapsedTime.ToString() + " seconds.");

		IDictionary responseDict = (IDictionary) Json.Deserialize(response);

		questionsDict = responseDict;
	}
}
