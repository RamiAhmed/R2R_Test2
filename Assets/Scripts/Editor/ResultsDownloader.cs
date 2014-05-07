using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using MiniJSON;

[ExecuteInEditMode] 
public class ResultsDownloader : MonoBehaviour {

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
			//getResults();
			StartCoroutine(GetResults());
		}

	}

	IEnumerator GetResults() {
	//private void getResults() {

		resultsWWW = new WWW(DatabaseURL);

		float elapsedTime = 0f;

		while (!resultsWWW.isDone) {
			elapsedTime += Time.deltaTime;
			
			if (elapsedTime >= 10.0f) {
				Debug.LogError("WWW request to URL: " + DatabaseURL + "\n Timed out.");
				break;
			}
			else
				Debug.Log("Getting results");
			
			yield return null;
			//return;
		}
		
		if (!resultsWWW.isDone || !string.IsNullOrEmpty(resultsWWW.error)) {
			Debug.LogError("WWW request to URL: " + DatabaseURL + " failed.\n" + resultsWWW.error);
			yield break;
			//return;
		}
		
		string response = resultsWWW.text;
		Debug.Log("Received text: " + response);
		Debug.Log("WWW request (loading results) took: " + elapsedTime.ToString() + " seconds.");
		
		IDictionary responseDict = (IDictionary) Json.Deserialize(response);
		
		resultsDict = responseDict;			
	}


}
