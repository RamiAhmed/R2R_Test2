﻿using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MiniJSON;

[ExecuteInEditMode] 
public class ResultsDownloader : EditorWindow {

	public string DatabaseURL = "www.alphastagestudios.com/test/results";

	//public IDictionary resultsDict = null;

	private WWW resultsWWW;

	// Add menu item named "Download Results" to the Window menu
	[MenuItem("Window/Download Results")]
	public static void ShowWindow()
	{
		//Show existing window instance. If one doesn't exist, make one.
		EditorWindow.GetWindow(typeof(ResultsDownloader));
	}

	void OnGUI() {
		GUILayout.Label("Download Test Results from Server");
		DatabaseURL = EditorGUILayout.TextField("Database URL", DatabaseURL);

		if (GUILayout.Button(new GUIContent("Download Results", "Press this button to download results from the database located at DatabaseURL"))) {

			GameObject resultsGetterGO = GameObject.FindGameObjectWithTag("ResultsGetter");

			if (resultsGetterGO == null) {

				UnityEngine.Object prefab = Resources.Load("EditorPrefabs/ResultsGetter");

				if (prefab != null) {
					UnityEngine.Object resultsGetter = PrefabUtility.InstantiatePrefab(prefab);
					if (resultsGetter != null) {
						resultsGetterGO = (GameObject)resultsGetter;
					}
					else {
						Debug.LogError("resultsGetter is null");
					}
				}
				else {
					Debug.LogWarning("Could not instantiate prefab");
				}
			}

			if (resultsGetterGO != null) {
				resultsGetterGO.GetComponent<DummyScript>().StartCoroutine(GetResults());
			}
			else {
				Debug.LogError("ResultsGetterGO is still null");
			}
		}
		
	}
	/*
	private Action.ActionState actionDownloadResults() {
		resultsWWW = new WWW(DatabaseURL);
		
		float elapsedTime = 0f;
		
		while (!resultsWWW.isDone) {
			elapsedTime += Time.deltaTime;
			
			if (elapsedTime >= 10.0f) {
				Debug.LogError("WWW request to URL: " + DatabaseURL + "\n Timed out.");
				return Action.ActionState.ACTION_ABORTED;
			}
		}
		
		if (!resultsWWW.isDone || !string.IsNullOrEmpty(resultsWWW.error)) {
			Debug.LogError("WWW request to URL: " + DatabaseURL + " failed.\n" + resultsWWW.error);
			return Action.ActionState.ACTION_ABORTED;
		}
		
		string response = resultsWWW.text;
		Debug.Log("Received text: " + response);
		Debug.Log("WWW request (loading results) took: " + elapsedTime.ToString() + " seconds.");
		
		IDictionary responseDict = (IDictionary) Json.Deserialize(response);
		
		resultsDict = responseDict;	
		
		return Action.ActionState.ACTION_DONE;
	}*/

	IEnumerator GetResults() {
		resultsWWW = new WWW(DatabaseURL);

		float elapsedTime = 0f;

		while (!resultsWWW.isDone) {
			elapsedTime += Time.fixedDeltaTime;
			Debug.Log("elapsedTime: " + elapsedTime);
			
			if (elapsedTime >= 10.0f) {
				Debug.LogError("WWW request to URL: " + DatabaseURL + "\n Timed out.");
				yield break;
			}
		}
		
		if (!resultsWWW.isDone || !string.IsNullOrEmpty(resultsWWW.error)) {
			Debug.LogError("WWW request to URL: " + DatabaseURL + " failed.\n" + resultsWWW.error);
			yield break;
		}
		
		string response = (resultsWWW.text).ToString();
		response = response.Substring(1, response.Length-1);
		response = "{" + response + "}";

		Debug.Log("Received text: " + response);
		Debug.Log("WWW request (loading results) took: " + elapsedTime.ToString() + " seconds.");

		StringBuilder stringBuilder = new StringBuilder();

		IDictionary dict = (IDictionary)MiniJSON.Json.Deserialize(response);

		int index = 0;
		foreach (DictionaryEntry entry in dict) {
			if (entry.Value != null) {
				IList list = (IList)dict[index.ToString()];
				index++;

				foreach (object s in list) {
					IDictionary iDict = (IDictionary)s;

					foreach (DictionaryEntry el in iDict) {
						if (el.Value != null) {
							string elStr = el.Value.ToString().Replace(";", "|");
							stringBuilder.Append(string.Format("{0};", elStr));
						}
						else {
							stringBuilder.Append("NaN;");
						}
					}

					stringBuilder.Append(Environment.NewLine);
				}				
			}
          	else
				Debug.Log(string.Format("{0} is null", entry.Key));				 
		}


		string dirPath = Application.dataPath + "/Results/";
		string filePath = dirPath + "results.csv";
		
		if (!Directory.Exists(dirPath))
			Directory.CreateDirectory(dirPath);
		
		int i = 2;
		while (File.Exists(filePath)) {
			filePath = dirPath + "results" + i.ToString() + ".csv";
			i++;
		}
		
		File.WriteAllText(filePath, stringBuilder.ToString());
		
		if (File.Exists(filePath)) 
			Debug.Log("Successfully wrote out to file at: " + filePath);
		else 
			Debug.LogError("Failed to write out to file at path: " + filePath);


	}


}