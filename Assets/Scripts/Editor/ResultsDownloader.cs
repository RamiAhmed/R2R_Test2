using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using MiniJSON;

[ExecuteInEditMode] 
public class ResultsDownloader : EditorWindow {

	public string DatabaseURL = "www.alphastagestudios.com/test/results";

	public IDictionary resultsDict = null;

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
		
		string response = resultsWWW.text;
		//Debug.Log("Received text: " + response);
		Debug.Log("WWW request (loading results) took: " + elapsedTime.ToString() + " seconds.");

		object jsonResponse = MiniJSON.Json.Deserialize(response);
		Debug.Log("jsonResponse: " + jsonResponse.ToString());


		IDictionary responseDict = (IDictionary) MiniJSON.Json.Deserialize(response);
		
		resultsDict = responseDict;			
	}


}
