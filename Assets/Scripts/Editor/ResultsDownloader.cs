using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[ExecuteInEditMode] 
public class ResultsDownloader : EditorWindow {

	public string DatabaseURL = "www.alphastagestudios.com/test/results";
	
	private ResultsHeatmapGenerator resultsHeatmapRef = null;

	private Dictionary<string, bool> heatmapToggles = new Dictionary<string, bool>();

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
				resultsHeatmapRef = resultsGetterGO.GetComponent<ResultsHeatmapGenerator>();

				EditorUtility.SetDirty(resultsHeatmapRef.gameObject);
				resultsHeatmapRef.StartGetResults(this.DatabaseURL);
			}
			else {
				Debug.LogError("ResultsGetterGO is still null");
			}
		}

		if (GUILayout.Button(new GUIContent("Generate 3D Heatmap - Mouse", "Press this button to generate game objects to serve as 3D heatmap points for the mouse cursor"))) {
			if (resultsHeatmapRef == null) 
				resultsHeatmapRef = GameObject.FindGameObjectWithTag("ResultsGetter").GetComponent<ResultsHeatmapGenerator>();

			if (resultsHeatmapRef == null) {
				Debug.LogError("Could not generate heatmap as results have not been downloaded (could not find results getter game object)");
			}
			else {
				EditorUtility.SetDirty(resultsHeatmapRef.gameObject);
				resultsHeatmapRef.Render3DMouseHeatmap();
			}
		}

		if (resultsHeatmapRef != null) {
			//EditorGUILayout.BeginToggleGroup();

			if (resultsHeatmapRef.HeatmapDict.Count > 0) {
				foreach (KeyValuePair<string, List<string>> pair in resultsHeatmapRef.HeatmapDict) {
					if (pair.Key.Contains("Mouse") && pair.Key.Contains("3D")) {

						heatmapToggles[pair.Key] = false;

						heatmapToggles[pair.Key] = EditorGUILayout.ToggleLeft(new GUIContent(pair.Key.ToString()), heatmapToggles[pair.Key]);
					}
				}
			}

			//EditorGUILayout.EndVertical();
		}
	}

}
