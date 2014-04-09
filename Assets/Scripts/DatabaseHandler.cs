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

	private ScenarioHandler scenarioHandler = null;

	void Start () {
		scenarioHandler = this.GetComponent<ScenarioHandler>();
		if (scenarioHandler == null) {
			scenarioHandler = this.GetComponentInChildren<ScenarioHandler>();
		}

		if (!scenarioHandler.DoneTesting) {
			if (questionsDict == null) {
				StartCoroutine(LoadQuestions());
			}

			answersForm = new WWWForm();
		}
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

	public void SubmitAllData() {
		answersForm.AddField("scenario", StatsCollector.Scenario);
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

		float elapsedTime = 0.0f;

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
