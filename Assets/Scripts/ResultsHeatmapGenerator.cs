using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

[System.Serializable]
public class ResultsHeatmapGenerator : MonoBehaviour {

	public HeatmapDictionary HeatmapDict = new HeatmapDictionary();

	private WWW resultsWWW;


	public void SetHeatmapToggles(bool[] heatmapToggles) {
		if (HeatmapDict.Count > 0) {
			int index = 0;
			foreach (KeyValuePair<string, List<string>> pair in HeatmapDict) {
				if (pair.Key.Contains("Mouse") && pair.Key.Contains("3D")) {
					/*
					Debug.Log("index: " + HeatmapDict.GetIndex(pair.Key));
					string strIndex = strIndex = pair.Key.Substring(0, 2);

					int index;
					bool bIndex = int.TryParse(strIndex, out index);
					if (bIndex && index < heatmapToggles.Length) {
						heatmapToggles[index] = false;
						heatmapToggles[index] = EditorGUILayout.ToggleLeft(new GUIContent(pair.Key.ToString()), heatmapToggles[index]);
					}
					else {
						Debug.LogError("Failed to parse index from heatmap dict key: " + pair.Key + ", strIndex: " + strIndex);
					}*/
					/*
					if (index < heatmapToggles.Length) {
						heatmapToggles[index] = false;
						heatmapToggles[index] = EditorGUILayout.ToggleLeft(new GUIContent(pair.Key.ToString()), heatmapToggles[index]);
					}


					index++;*/
				}
			}
			/*
			for (int i = 0; i < heatmapToggles.Length; i++) {
				//GameObject go = GameObject.Find(HeatmapDict.GetKey(i));
				GameObject go = findChildByName(HeatmapDict.GetKey(i));
				if (go != null) {
					go.SetActive(heatmapToggles[i]);
				}
			}*/
		}
	}

	private GameObject findChildByName(string name) {
		for (int i = 0; i < this.transform.childCount; i++) {
			if (this.transform.GetChild(i).name == name)
				return this.transform.GetChild(i).gameObject;
		}

		return null;
	}


	public void AddToDict(string key, List<string> value) {
		if (HeatmapDict.ContainsKey(key)) {
			HeatmapDict[key] = value;
			Debug.LogWarning("Replacing earlier set heatmap dict key: " + key);
		}
		else {
			HeatmapDict.Add(key, value);
		}
	}

	public void Render3DMouseHeatmap() {
		if (this.transform.childCount > 0) {
			while (this.transform.childCount > 0) {
				Transform child = this.transform.GetChild(0);
				if (child.transform.childCount > 0) {
					DestroyImmediate(child.transform.GetChild(0).gameObject);
				}

				DestroyImmediate(child.gameObject);
			}
		}

		if (HeatmapDict.Count > 0) {
			foreach (KeyValuePair<string, List<string>> pair in HeatmapDict) {
				if (pair.Key.Contains("Mouse") && pair.Key.Contains("3D")) {
					GameObject parent = Instantiate(Resources.Load("EditorPrefabs/HeatmapParent")) as GameObject;
					parent.transform.parent = this.transform;
					parent.name = pair.Key;

					if (HeatmapDict.GetIndex(pair.Key) < 10)
						parent.name = "0" + parent.name;

					if (parent != null) {
						renderHeatmapList(convertStringListToVector3(pair.Value), parent.transform);
					}
					else {
						Debug.LogError("Could not instantiate heatmap parent");
						break;
					}
				}
			}

			Debug.Log("Rendering 3D mouse position heatmap");
		}
		else {
			Debug.LogError("Heatmap dictionary has not been populated");
		}
	}

	private void renderHeatmapList(List<Vector3> list, Transform parent) {
		if (list == null || list.Count <= 0) {
			Debug.LogError("3D Heatmap list is null or has length 0");
		}
		else {
			foreach (Vector3 pos in list) {
				createHeatmapPoint(parent, pos);
			}
		}
	}

	private void createHeatmapPoint(Transform parent, Vector3 pos) {
		UnityEngine.Object heatmapObj = Resources.Load("EditorPrefabs/HeatmapPoint");
		if (heatmapObj != null) {
			GameObject heatmapGO = Instantiate(heatmapObj) as GameObject;
			if (heatmapGO != null) {
				heatmapGO.transform.position = pos;
				heatmapGO.transform.parent = parent;
				Debug.Log("Created 3D heatmap game object at: " + pos.ToString());
			}
			else {
				Debug.LogError("Could not instantiate 3D heatmap object as game object");
			}
		}
		else {
			Debug.LogError("Could not load 3D heatmap object from resources");
		}
	}

	private List<Vector3> convertStringListToVector3(List<string> list) {
		List<Vector3> newList = new List<Vector3>();

		foreach (string strPos in list) {
			if (!string.IsNullOrEmpty(strPos)) {
				string cleanStrPos = strPos;

				cleanStrPos = cleanStrPos.Replace("(", string.Empty);
				cleanStrPos = cleanStrPos.Replace(")", string.Empty);
				cleanStrPos = cleanStrPos.Replace(";", string.Empty);

				float x,y,z;
				string[] strPosSplit = cleanStrPos.Split(',');
				if (strPosSplit.Length == 3) {
					bool bx = float.TryParse(strPosSplit[0], out x);
					bool by = float.TryParse(strPosSplit[1], out y);
					bool bz = float.TryParse(strPosSplit[2], out z);

					if (!bx || !by || !bz) {
						Debug.LogError(string.Format("Convert string list to vector3 list could not parse. x: {0}, y: {1}, z: {2}", strPosSplit[0], strPosSplit[1], strPosSplit[2]));
					}
					else {
						//Debug.Log(string.Format("adding pos. x: {0}, y: {1}, z: {2}", x, y, z));
						Vector3 pos = new Vector3(x, y, z);
						newList.Add(pos);
					}
				}
				else {
					Debug.LogWarning("Convert string list to vector3 list error. Split string does not have length 3. Length: " + strPosSplit.Length); 
				}
			}
		}

		return newList;
	}

	private List<Vector2> convertStringListToVector2(List<string> list) {
		List<Vector2> newList = new List<Vector2>();

		foreach (string strPos in list) {
			float x,y;
			string[] strPosSplit = strPos.Split(',');

			bool bx = float.TryParse(strPosSplit[0], out x);
			bool by = float.TryParse(strPosSplit[1], out y);

			if (!bx || !by) {
				Debug.LogError(string.Format("Convert string list to vector2 list could not parse. x: {0}, y: {1}", strPosSplit[0], strPosSplit[1]));
			}
			else {
				Vector2 pos = new Vector2(x, y);
				newList.Add(pos);
			}
		}

		return newList;
	}


	public void StartGetResults(string dbURL) {
		StartCoroutine(GetResults(dbURL));
	}

	public IEnumerator GetResults(string DatabaseURL) {
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
		
		
		IDictionary dict = (IDictionary)MiniJSON.Json.Deserialize(response);
		
		if (dict.Count <= 0) {
			Debug.LogError("Downloaded and json deserialized response has no elements");
			yield break;
		}
		
		
		StringBuilder stringBuilder = new StringBuilder();
		
		string[] columns = new string[] {
			"P id",
			"Timestamp",
			"Gender",
			"Age",
			"Playing Frequency",
			"Playing AmounT",
			"Favourite Genres/Games",
			"Starting Desire",
			"Starting Reasons",
			"Starting Comments",
			"During 1 Desire",
			"During 1 Reasons",
			"During 1 Comments",
			"After Desire",
			"After Reasons",
			"After Comments",
			"Intrusive Questionnaire",
			"Intrusive Eye tracking",
			"Intrusive Mouse tracking",
			"Intrusive Game metrics",
			"Raw Time Played",
			"Raw Time Spent",
			"Raw Wave Count",
			"Raw Total Tactics Changes",
			"Raw Tactics Changes",
			"Raw Target Changes",
			"Raw Condition Changes",
			"Raw Gold Spent",
			"Raw Gold Earned",
			"Raw Units Died",
			"Raw Enemies Killed",
			"Raw Gold Deposit Left",
			"Raw Units Bought",
			"Raw Unit Upgrades",
			"Raw Units Sold",
			"Raw Units Moved",
			"Raw Total Selections",
			"Raw Units Selected",
			"Raw Enemies Selected",
			"Raw Force Spawns",
			"Raw Eyes Pos 2D",
			"Raw Eyes Pos 3D",
			"Raw Mouse Pos 2D",
			"Raw Mouse Pos 3D",
			"Raw Left Click Pos 2D",
			"Raw Left Click Pos 3D",
			"Raw Right Click Pos 2D",
			"Raw Right Click Pos 3D"
		};
		
		foreach (string col in columns) {
			stringBuilder.Append(string.Format("{0};", col));
		}
		stringBuilder.AppendLine();
		
		
		int rowIndex = 0;
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
							stringBuilder.Append(elStr);
							
							int intKey = 0;
							bool result = int.TryParse(el.Key.ToString(), out intKey);
							if (result && intKey > 39) {
								addToHeatmapList(intKey, rowIndex, el.Value.ToString(), columns);
							}
						}
						else {
							stringBuilder.Append("NaN");
						}
						
						stringBuilder.Append(";");
					}
					
					stringBuilder.AppendLine();
					rowIndex++;
				}				
			}
			else {
				Debug.Log(string.Format("{0} is null", entry.Key));				 
			}
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
	
	private void addToHeatmapList(int index, int rowIndex, string list, string[] columns) {
		List<string> coords = new List<string>();
		coords.AddRange((list.Replace(";", "|").Split('|')));
		
		string key = string.Format("{0}:{1}", rowIndex.ToString(), columns[index]);
		this.AddToDict(key, coords);
		
		//Debug.Log(string.Format("adding to heatmap results ref, key: {0}, value: {1}", key, coords));
	}
}
