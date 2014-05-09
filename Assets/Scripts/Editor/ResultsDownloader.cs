using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//[ExecuteInEditMode] 
[System.Serializable]
public class ResultsDownloader : EditorWindow {

	public string DatabaseURL = "www.alphastagestudios.com/test/results";

	[SerializeField]
	private ResultsHeatmapGenerator resultsHeatmapRef = null;
	

	// Add menu item named "Download Results" to the Window menu
	[MenuItem("Window/Download Results")]
	public static void ShowWindow()
	{
		//Show existing window instance. If one doesn't exist, make one.
		EditorWindow.GetWindow(typeof(ResultsDownloader));
	}

	void OnGUI() {
		EditorGUILayout.Space();

		GUILayout.Label("Download Test Results from Server");
		DatabaseURL = EditorGUILayout.TextField("Database URL", DatabaseURL);

		EditorGUILayout.Separator();
		EditorGUILayout.Space();

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

				resultsHeatmapRef.StartGetResults(this.DatabaseURL);
			}
			else {
				Debug.LogError("ResultsGetterGO is still null");
			}
		}

		EditorGUILayout.Separator();
		EditorGUILayout.Space();

		if (resultsHeatmapRef == null)  {
			GameObject resultsHeatmapGO = GameObject.FindGameObjectWithTag("ResultsGetter");
			if (resultsHeatmapGO != null) 
				resultsHeatmapRef = resultsHeatmapGO.GetComponent<ResultsHeatmapGenerator>();

		}

		if (GUILayout.Button(new GUIContent("Generate 3D Heatmaps", "Press this button to generate game objects to serve as 3D heatmap points"))) {
			if (resultsHeatmapRef == null) {
				Debug.LogError("Could not generate 3D heatmap as results have not been downloaded (could not find results getter game object)");
			}
			else {
				resultsHeatmapRef.Render3DHeatmaps();
			}
		}

		if (resultsHeatmapRef != null) {
			EditorGUILayout.BeginHorizontal();
				resultsHeatmapRef.bGenerateMouse3DHeatmap = EditorGUILayout.ToggleLeft(new GUIContent("Include 3D Mouse Positions"), resultsHeatmapRef.bGenerateMouse3DHeatmap);
				resultsHeatmapRef.bGenerateEyes3DHeatmap = EditorGUILayout.ToggleLeft(new GUIContent("Include 3D Gaze Positions"), resultsHeatmapRef.bGenerateEyes3DHeatmap);
				resultsHeatmapRef.bGenerateClicks3DHeatmap = EditorGUILayout.ToggleLeft(new GUIContent("Include 3D Click Positions"), resultsHeatmapRef.bGenerateClicks3DHeatmap);
			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.Separator();
		EditorGUILayout.Space();

		if (GUILayout.Button(new GUIContent("Generate 2D Heatmap", "Press this button to generate a .png image with the 2D heatmap points"))) {
			if (resultsHeatmapRef == null) {
				Debug.LogError("Could not generate 2D heatmap as results have not been downloaded (could not find results getter game object)");
			}
			else {
				resultsHeatmapRef.Render2DHeatmaps();
			}
		}

		if (resultsHeatmapRef != null) {
			
			EditorGUILayout.BeginHorizontal();
				resultsHeatmapRef.Heatmap2DWidth = EditorGUILayout.IntSlider("2D Heatmap Width", resultsHeatmapRef.Heatmap2DWidth, 320, 2048);
				resultsHeatmapRef.Heatmap2DHeight = EditorGUILayout.IntSlider("2D Heatmap Height", resultsHeatmapRef.Heatmap2DHeight, 320, 2048);			
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Separator();

			EditorGUILayout.BeginHorizontal();
				resultsHeatmapRef.bGenerateMouse2DHeatmap = EditorGUILayout.ToggleLeft("Include 2D Mouse Positions", resultsHeatmapRef.bGenerateMouse2DHeatmap);
				resultsHeatmapRef.bGenerateEyes2DHeatmap = EditorGUILayout.ToggleLeft("Include 2D Gaze Positions", resultsHeatmapRef.bGenerateEyes2DHeatmap);
				resultsHeatmapRef.bGenerateClicks2DHeatmap = EditorGUILayout.ToggleLeft("Include 2D Click Positions", resultsHeatmapRef.bGenerateClicks2DHeatmap);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Separator();

			EditorGUILayout.BeginHorizontal();

				resultsHeatmapRef.Heatmap2DPixelSize = EditorGUILayout.IntSlider("2D Heatmap Pixel Size", resultsHeatmapRef.Heatmap2DPixelSize, 1, 25);
				resultsHeatmapRef.Heatmap2DColorMultiplicationFactor = EditorGUILayout.FloatField("2D  Heatmap Color Multiplication Factor", resultsHeatmapRef.Heatmap2DColorMultiplicationFactor);

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Separator();

			if (resultsHeatmapRef.Heatmap2DColors.Length == 4) {
				EditorGUILayout.BeginHorizontal();

					resultsHeatmapRef.Heatmap2DColors[0] = EditorGUILayout.ColorField("2D Mouse Color", resultsHeatmapRef.Heatmap2DColors[0]);
					resultsHeatmapRef.Heatmap2DColors[1] = EditorGUILayout.ColorField("2D Eyes Color", resultsHeatmapRef.Heatmap2DColors[1]);
					resultsHeatmapRef.Heatmap2DColors[2] = EditorGUILayout.ColorField("2D Right Click Color", resultsHeatmapRef.Heatmap2DColors[2]);
					resultsHeatmapRef.Heatmap2DColors[3] = EditorGUILayout.ColorField("2D Left Click Color", resultsHeatmapRef.Heatmap2DColors[3]);				
				EditorGUILayout.EndHorizontal();
			}

		}
		
	}
	


}
