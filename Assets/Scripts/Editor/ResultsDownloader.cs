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


	private bool bResultsHeatmapExists = false;
	private bool bShow3DHeatmapColors = false;
	private bool bShow2DHeatmapColors = false;

	// Add menu item named "Download Results" to the Window menu
	[MenuItem("Window/Download Results")]
	public static void ShowWindow()
	{
		//Show existing window instance. If one doesn't exist, make one.
		EditorWindow.GetWindow(typeof(ResultsDownloader));
	}

	private void downloadResults() {
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

	void OnGUI() {

		if (resultsHeatmapRef == null)  {
			GameObject resultsHeatmapGO = GameObject.FindGameObjectWithTag("ResultsGetter");
			if (resultsHeatmapGO != null) 
				resultsHeatmapRef = resultsHeatmapGO.GetComponent<ResultsHeatmapGenerator>();			
		}


		if (Event.current.type == EventType.Layout) {
			bResultsHeatmapExists = resultsHeatmapRef != null;
			bShow3DHeatmapColors = bResultsHeatmapExists ? resultsHeatmapRef.Heatmap3DColors.Length == 4 : false;
			bShow2DHeatmapColors = bResultsHeatmapExists ? resultsHeatmapRef.Heatmap2DColors.Length == 4 : false;
		} 



		EditorGUILayout.Space();

		GUILayout.Label("Download Test Results from Server");
		DatabaseURL = EditorGUILayout.TextField("Database URL", DatabaseURL);

		EditorGUILayout.Separator();
		EditorGUILayout.Space();

		/**** DOWNLOAD RESULTS ****/

		if (GUILayout.Button(new GUIContent("Download Results", "Press this button to download results from the database located at DatabaseURL"))) {
			downloadResults();
		}

		if (bResultsHeatmapExists) {
			EditorGUILayout.Separator();

			if (GUILayout.Button(new GUIContent("Cleanup everything", "Press this button to permanently delete all the contents of the 'Results' folder and the 3D heatmap objects"))) {
				resultsHeatmapRef.CleanupAll();
			}
		}


		/**** 3D HEATMAPS ****/

		if (bResultsHeatmapExists) {
			EditorGUILayout.Separator();
			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
				resultsHeatmapRef.bGenerateMouse3DHeatmap = EditorGUILayout.ToggleLeft(new GUIContent("Include 3D Mouse Positions"), resultsHeatmapRef.bGenerateMouse3DHeatmap);
				resultsHeatmapRef.bGenerateEyes3DHeatmap = EditorGUILayout.ToggleLeft(new GUIContent("Include 3D Gaze Positions"), resultsHeatmapRef.bGenerateEyes3DHeatmap);
				resultsHeatmapRef.bGenerateClicks3DHeatmap = EditorGUILayout.ToggleLeft(new GUIContent("Include 3D Click Positions"), resultsHeatmapRef.bGenerateClicks3DHeatmap);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Separator();
			
			if (bShow3DHeatmapColors) {
				EditorGUILayout.BeginHorizontal();
					resultsHeatmapRef.Heatmap3DColors[0] = EditorGUILayout.ColorField("3D Mouse Color", resultsHeatmapRef.Heatmap3DColors[0]);
					resultsHeatmapRef.Heatmap3DColors[1] = EditorGUILayout.ColorField("3D Gaze Color", resultsHeatmapRef.Heatmap3DColors[1]);
					resultsHeatmapRef.Heatmap3DColors[2] = EditorGUILayout.ColorField("3D Right Click Color", resultsHeatmapRef.Heatmap3DColors[2]);
					resultsHeatmapRef.Heatmap3DColors[3] = EditorGUILayout.ColorField("3D Left Click Color", resultsHeatmapRef.Heatmap3DColors[3]);				
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.Separator();

			resultsHeatmapRef.Heatmap3DObjectSize = EditorGUILayout.FloatField("3D Heatmap Object Scale (Size)", resultsHeatmapRef.Heatmap3DObjectSize);

			EditorGUILayout.Separator();
		
			if (GUILayout.Button(new GUIContent("Generate 3D Heatmaps", "Press this button to generate game objects to serve as the chosen 3D heatmap points"))) {
				resultsHeatmapRef.Render3DHeatmaps();
			}

			EditorGUILayout.Separator();
			
			if (GUILayout.Button(new GUIContent("Cleanup 3D Heatmaps", "Press this button to delete all 3D heatmap objects permanently"))) {
				resultsHeatmapRef.Cleanup3DHeatmap();
			}
		}
		



		/**** 2D HEATMAPS ****/

		if (bResultsHeatmapExists) {

			EditorGUILayout.Separator();
			EditorGUILayout.Space();
			
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

			if (bShow2DHeatmapColors) {
				EditorGUILayout.BeginHorizontal();
					resultsHeatmapRef.Heatmap2DColors[0] = EditorGUILayout.ColorField("2D Mouse Color", resultsHeatmapRef.Heatmap2DColors[0]);
					resultsHeatmapRef.Heatmap2DColors[1] = EditorGUILayout.ColorField("2D Gaze Color", resultsHeatmapRef.Heatmap2DColors[1]);
					resultsHeatmapRef.Heatmap2DColors[2] = EditorGUILayout.ColorField("2D Right Click Color", resultsHeatmapRef.Heatmap2DColors[2]);
					resultsHeatmapRef.Heatmap2DColors[3] = EditorGUILayout.ColorField("2D Left Click Color", resultsHeatmapRef.Heatmap2DColors[3]);				
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.Separator();

			if (GUILayout.Button(new GUIContent("Generate 2D Heatmaps", "Press this button to generate .png images with the chosen 2D heatmap points"))) {
				resultsHeatmapRef.Render2DHeatmaps();
			}

			EditorGUILayout.Separator();

			if (GUILayout.Button(new GUIContent("Cleanup 2D Heatmaps", "Press this button to delete the 2D heatmaps folder permanently, including all 2D heatmaps within"))) {
				resultsHeatmapRef.Cleanup2DHeatmap();
			}
		}
		
	}
	


}
