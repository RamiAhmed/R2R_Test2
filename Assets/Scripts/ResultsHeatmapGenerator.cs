using UnityEngine;
//using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

[System.Serializable, ExecuteInEditMode]
public class ResultsHeatmapGenerator : MonoBehaviour {
	
	public HeatmapDictionary HeatmapDict = new HeatmapDictionary();
	
	public bool bGenerateMouse3DHeatmap = true;
	public bool bGenerateEyes3DHeatmap = true;
	public bool bGenerateClicks3DHeatmap = true;
	
	public bool bGenerateMouse2DHeatmap = true;
	public bool bGenerateEyes2DHeatmap = true;
	public bool bGenerateClicks2DHeatmap = true;
	
	public int Heatmap2DWidth = 1920;
	public int Heatmap2DHeight = 1080;
	
	public int Heatmap2DPixelSize = 2;
	public float Heatmap2DColorMultiplicationFactor = 1.5f;
	
	public float Heatmap3DObjectSize = 1f;
	
	public Color[] Heatmap2DColors = new Color[4];
	public Color[] Heatmap3DColors = new Color[4];
	private Color transparentColor = new Color(1f, 1f, 1f, 0f);

	[SerializeField]
	private IDictionary cachedResults;

	private WWW resultsWWW;
	
	private const string Heatmaps2DFolder = "2D Heatmaps";
	private const string ResultsFolder = "Results";

	private const int ResultsListsStartIndex = 47;
	
	void Start() {
		Heatmap2DColors = new Color[] {
			new Color(1f, 0f, 0f, 0.2f),
			new Color(0f, 0.1f, 1f, 0.2f),
			new Color(1f, 0.975f, 0f, 0.2f),
			new Color(0f, 0.54f, 0.04f, 0.2f)
		};
		
		Heatmap3DColors = new Color[] {
			new Color(1f, 0f, 0f, 0.2f),
			new Color(0f, 0.1f, 1f, 0.2f),
			new Color(1f, 0.975f, 0f, 0.2f),
			new Color(0f, 0.54f, 0.04f, 0.2f)
		};
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
	
	public void CleanupAll() {
		string dirPath = Path.Combine(Application.dataPath, ResultsFolder);
		
		if (Directory.Exists(dirPath)) {
			try {
				Directory.Delete(dirPath, true);
			}
			catch (System.Exception e) {
				Debug.LogWarning("CleanupAll() exception: " + e.Message.ToString());
			}
			
			if (!Directory.Exists(dirPath))
				Debug.Log(string.Format("Deleted folder contents successfully at: {0}", dirPath));
			else
				Debug.LogError(string.Format("Could not delete folder at: {0}", dirPath));
		}
		else
			Debug.LogWarning(string.Format("No folder found at: {0}", dirPath));
		
		Cleanup3DHeatmap();

		if (this.cachedResults != null && this.cachedResults.Count > 0)
			this.cachedResults.Clear();
	}
	
	public void Cleanup2DHeatmap() {
		string dirPath = Path.Combine(Application.dataPath, ResultsFolder);
		dirPath = Path.Combine(dirPath, Heatmaps2DFolder);
		
		if (Directory.Exists(dirPath)) {
			try {
				Directory.Delete(dirPath, true);
			}
			catch (System.Exception e) {
				Debug.LogWarning("Cleanup2D Heatmap error: " + e.Message.ToString());
			}
			
			if (!Directory.Exists(dirPath))
				Debug.Log(string.Format("Deleted folder contents successfully at: {0}", dirPath));
			else
				Debug.LogError(string.Format("Could not delete folder at: {0}", dirPath));
		}
		else
			Debug.LogWarning(string.Format("No folder found at: {0}", dirPath));
	}
	
	public void Cleanup3DHeatmap() {
		if (this.transform.childCount > 0) {
			while (this.transform.childCount > 0) {
				DestroyImmediate(this.transform.GetChild(0).gameObject);
			}
			
			Debug.Log("Deleted 3D Heatmap objects successfully");
		}
		else {
			Debug.LogWarning("No heatmap objects found, cannot cleanup 3D heatmap");
		}

		HeatmapDict.Clear();
	}
	
	public void Render2DHeatmaps() {
		StartCoroutine(render2DHeatmaps());
	}

	private IEnumerator render2DHeatmaps() {
		bool bRendered = false;
		
		if (HeatmapDict.Count > 0) {			
			foreach (KeyValuePair<string, List<string>> pair in HeatmapDict) {
				if (pair.Key.Contains("2D")) {
					if ((bGenerateMouse2DHeatmap && pair.Key.Contains("Mouse")) ||
					    (bGenerateEyes2DHeatmap && pair.Key.Contains("Eyes")) ||
					    (bGenerateClicks2DHeatmap && pair.Key.Contains("Click"))) {
						
						Color pixelColor = Color.white;
						if (pair.Key.Contains("Mouse"))
							pixelColor = Heatmap2DColors[0];
						else if (pair.Key.Contains("Eyes"))
							pixelColor = Heatmap2DColors[1];
						else if (pair.Key.Contains("Right"))
							pixelColor = Heatmap2DColors[2];
						else if (pair.Key.Contains("Left"))
							pixelColor = Heatmap2DColors[3];

						try {
							byte[] texBytes = render2DHeatmapTexture(convertStringListToVector2(pair.Value), pixelColor);
							if (texBytes != null) {
								if (createNew2DHeatmapPNG(texBytes, pair.Key))
									bRendered = true;
							}
							else {
								Debug.LogError("2D Heatmap Texture byte array is null");
							}
						}
						catch (Exception e) {
							Debug.LogWarning("Render2DHeatmaps error: " + e.Message.ToString());
						}
					}
				}
			}
		}
		
		if (bRendered)
			Debug.Log("Rendered 2D Heatmap succesfully");
		else {
			if (!bGenerateMouse2DHeatmap && !bGenerateEyes2DHeatmap && !bGenerateClicks2DHeatmap)
				Debug.LogWarning("Please include mouse, gaze and/or clicks to the 2D heatmap");
		}

		yield return bRendered;
	}
	
	private bool createNew2DHeatmapPNG(byte[] texBytes, string key) {
		bool result = false;
		
		if (texBytes != null && texBytes.Length > 0) {
			key = key.Replace(":", "_");

			string dirPath = Path.Combine(Application.dataPath, ResultsFolder);
			dirPath = Path.Combine(dirPath, Heatmaps2DFolder);
			string filePath = Path.Combine(dirPath, string.Format("{0}.png", key));
			
			int i = 2;
			while (File.Exists(filePath)) {
				filePath = Path.Combine(dirPath, string.Format("{0}-{1}.png", key, i.ToString()));
				
				i++;
			}
			
			if (!Directory.Exists(dirPath))
				Directory.CreateDirectory(dirPath);

			try {
				FileStream file = File.Create(filePath);
				BinaryWriter bw = new BinaryWriter(file);
				
				bw.Write(texBytes);

				file.Close();
				bw.Close();
			}
			catch (Exception e) {
				Debug.LogWarning("Create new 2D heatmap png error: " + e.Message.ToString());
			}
			
			if (File.Exists(filePath)) {
				result = true;
				Debug.Log("Sucessfully wrote out 2D heatmap to: " + filePath);
			}
			else
				Debug.LogError("Could not write out 2D heatmap to: " + filePath);
		}
		
		return result;
	}
	
	private byte[] render2DHeatmapTexture(List<Vector2> posList, Color pixelColor) {
		if (posList != null && posList.Count > 0) {
			Texture2D tex2D = new Texture2D(Heatmap2DWidth, Heatmap2DHeight);
			
			for (int x = 0; x < tex2D.width; x++) {
				for (int y = 0; y < tex2D.height; y++) {
					tex2D.SetPixel(x, y, transparentColor);
				}
			}

			Vector2 recordedScreenSize = new Vector2(1920f, 1080f);
			
			foreach (Vector2 pos in posList) {
				int x = recordedScreenSize.x != Heatmap2DWidth ? Mathf.RoundToInt((pos.x/recordedScreenSize.x) * Heatmap2DWidth) : Mathf.RoundToInt(pos.x);
				int y = recordedScreenSize.y != Heatmap2DHeight ? Mathf.RoundToInt((pos.y/recordedScreenSize.y) * Heatmap2DHeight) : Mathf.RoundToInt(pos.y);

				render2DHeatmapPoint(tex2D, x, y, pixelColor);
			}
			
			tex2D.Apply(false);
			
			return tex2D.EncodeToPNG();
		}
		else {
			return null;
		}
	}
	
	private void render2DHeatmapPoint(Texture2D tex2D, int x, int y, Color pixelColor) {
		int pixelSize = Heatmap2DPixelSize;
		float colorFator = Heatmap2DColorMultiplicationFactor;
		
		for (int tx = x-pixelSize; tx < x+pixelSize; tx++) {
			for (int ty = y-pixelSize; ty < y+pixelSize; ty++) {
				if (tx >= 0 && tx < tex2D.width && ty >= 0 && ty < tex2D.height) {
					Color oldColor = tex2D.GetPixel(tx, ty);
					if (oldColor == transparentColor)
						tex2D.SetPixel(tx, ty, pixelColor);
					else
						tex2D.SetPixel(tx, ty, oldColor * colorFator);
				}
			}
		}
	}
	
	private Transform findInChild(string name) {
		for (int i = 0; i < this.transform.childCount; i++) {
			if (this.transform.GetChild(i).name == name) {
				return this.transform.GetChild(i);
			}
		}
		
		return null;
	}
	
	
	public void Render3DHeatmaps() {
		bool bRendered = false;
		if (HeatmapDict.Count > 0) {
			if (this.transform.childCount > 0) {
				while (this.transform.childCount > 0) {
					Transform child = this.transform.GetChild(0);
					if (child.transform.childCount > 0) {
						DestroyImmediate(child.transform.GetChild(0).gameObject);
					}
					
					DestroyImmediate(child.gameObject);
				}
			}

			UnityEngine.Object parentPrefab = Resources.Load("EditorPrefabs/HeatmapParent");
			if (parentPrefab == null) {
				Debug.LogError("Could not find any parent prefab for 3D heatmap");
				return;
			}
			
			foreach (KeyValuePair<string, List<string>> pair in HeatmapDict) {
				if (pair.Key.Contains("3D")) {
					if ((bGenerateMouse3DHeatmap && pair.Key.Contains("Mouse")) ||
					    (bGenerateEyes3DHeatmap && pair.Key.Contains("Eyes")) ||
					    (bGenerateClicks3DHeatmap && pair.Key.Contains("Click"))) {
						
						string name = string.Format("Row {0}", pair.Key.Substring(0, pair.Key.IndexOf(":")));						
						GameObject parent = (findInChild(name) != null) ? findInChild(name).gameObject : Instantiate(parentPrefab) as GameObject;

						if (parent != null) {
							if (parent.name != name) {
								parent.transform.parent = this.transform;
								parent.name = name;
							}

							GameObject subparent = Instantiate(parentPrefab) as GameObject;
							subparent.transform.parent = parent.transform;
							subparent.name = pair.Key.ToString();
							
							Color color = Color.white;
							if (pair.Key.Contains("Mouse"))
								color = Heatmap3DColors[0];
							else if (pair.Key.Contains("Eyes"))
								color = Heatmap3DColors[1];
							else if (pair.Key.Contains("Right"))
								color = Heatmap3DColors[2];
							else if (pair.Key.Contains("Left"))
								color = Heatmap3DColors[3];
							
							if (renderHeatmapList(convertStringListToVector3(pair.Value), subparent.transform, color))
								bRendered = true;
						}
						else {
							Debug.LogError("Could not instantiate heatmap parent");
							break;
						}
					}
				}
			}
		}
		else {
			Debug.LogError("Heatmap dictionary has not been populated");
		}
		
		if (bRendered)
			Debug.Log("Rendered 3D Heatmap succesfully");
		else
			if (!bGenerateMouse3DHeatmap && !bGenerateEyes3DHeatmap && !bGenerateClicks3DHeatmap)
				Debug.LogWarning("Please include mouse, gaze and/or clicks to the 3D heatmap");
	}
	
	private bool renderHeatmapList(List<Vector3> list, Transform parent, Color color) {
		bool result = false;
		if (list == null || list.Count <= 0) {
			Debug.LogError("3D Heatmap list is null or has length 0");
		}
		else {
			foreach (Vector3 pos in list) {
				result = createHeatmapPoint(parent, pos, color);
			}
		}
		
		return result;
	}
	
	private bool createHeatmapPoint(Transform parent, Vector3 pos, Color color) {
		bool result = false;
		
		UnityEngine.Object heatmapObj = Resources.Load("EditorPrefabs/HeatmapPoint");
		if (heatmapObj != null) {
			GameObject heatmapGO = Instantiate(heatmapObj) as GameObject;
			if (heatmapGO != null) {
				heatmapGO.transform.position = pos;
				heatmapGO.transform.parent = parent;
				
				Material heatmapMat = new Material(heatmapGO.renderer.sharedMaterial);
				heatmapMat.color = color;
				heatmapGO.renderer.sharedMaterial = heatmapMat;
				
				heatmapGO.transform.localScale = Vector3.one * Heatmap3DObjectSize;
				//Debug.Log("Created 3D heatmap game object at: " + pos.ToString());
				
				result = true;
			}
			else {
				Debug.LogError("Could not instantiate 3D heatmap prefab as game object");
			}
		}
		else {
			Debug.LogError("Could not load 3D heatmap prefab from Resources");
		}
		
		return result;
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
						newList.Add(new Vector3(x, y, z));
					}
				}
				else {
					Debug.LogWarning("Convert string list to vector3 list error. Split string does not have length 3. Length: " + strPosSplit.Length + ", string: " + strPos);
				}
			}
		}
		
		return newList;
	}
	
	private List<Vector2> convertStringListToVector2(List<string> list) {
		List<Vector2> newList = new List<Vector2>();
		
		foreach (string strPos in list) {
			if (!string.IsNullOrEmpty(strPos)) {
				string cleanStrPos = strPos;
				
				cleanStrPos = cleanStrPos.Replace("(", string.Empty);
				cleanStrPos = cleanStrPos.Replace(")", string.Empty);
				cleanStrPos = cleanStrPos.Replace(";", string.Empty);
				
				float x,y;
				string[] strPosSplit = cleanStrPos.Split(',');
				
				if (strPosSplit.Length == 2) {
					bool bx = float.TryParse(strPosSplit[0], out x);
					bool by = float.TryParse(strPosSplit[1], out y);
					
					if (!bx || !by) {
						Debug.LogError(string.Format("Convert string list to vector2 list could not parse. x: {0}, y: {1}", strPosSplit[0], strPosSplit[1]));
					}
					else {
						newList.Add(new Vector2(x, y));
					}
				}
				else {
					Debug.LogError("Convert string list to vector2 list error. Split string does not have length 2. Length: " + strPosSplit.Length + ", string: " + strPos);
				}
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
		float maxAllowedTime = 30f;
		
		while (!resultsWWW.isDone) {
			elapsedTime += Time.fixedDeltaTime;
			Debug.Log("elapsedTime: " + elapsedTime);
			
			if (elapsedTime >= maxAllowedTime) {
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
		
		if (dict == null || dict.Count <= 0) {
			Debug.LogError("Downloaded and json deserialized response has no elements");
			yield break;
		}
		else {
			this.cachedResults = dict;
			Debug.Log("Cached response in dictionary with count: " + dict.Count.ToString());
			
			string[] columns = getDataColumns();

			int rowIndex = 0;
			int index = 0;
			foreach (DictionaryEntry entry in dict) {
				if (entry.Value != null) {
					IList list = (IList)dict[index.ToString()];
					index++;
					
					foreach (object s in list) {
						IDictionary iDict = (IDictionary)s;
						
						foreach (DictionaryEntry el in iDict) {
							string key = el.Key.ToString();
							if (el.Value != null) {								
								int intKey = 0;
								bool parseResult = int.TryParse(key, out intKey);
								if (parseResult && intKey > ResultsListsStartIndex) {
									addToHeatmapList(intKey, rowIndex, el.Value.ToString(), columns, key.Contains("tais"));
								}
							}
							
						}
						
						rowIndex++;
					}
				}
				else {
					Debug.Log(string.Format("{0} is null", entry.Key));
				}
			}
		}
	}

	public void GenerateResultsCSV() {
		if (this.cachedResults != null && this.cachedResults.Count > 0) {
			generateResultsCSV(this.cachedResults);
		}
		else {
			Debug.LogWarning("Could not generate results CSV because no results have been cached");
		}
	}

	private bool generateResultsCSV(IDictionary dict) {
		bool result = false;

		StringBuilder stringBuilder = new StringBuilder();
		
		string[] columns = getDataColumns();
		
		foreach (string col in columns) {
			if (!col.Contains("2D") && !col.Contains("3D") && !col.Contains("Fixations") && !col.Contains("Pupil") || col.Contains("Comments"))
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
						string key = el.Key.ToString();
						if (el.Value != null) {
							string elStr = el.Value.ToString().Replace(";", "|");
							
							int intKey = 0;
							bool parseResult = int.TryParse(key, out intKey);
							if (intKey <= ResultsListsStartIndex || !parseResult) {
								stringBuilder.Append(elStr + ";");
							}
						}
						else {
							stringBuilder.Append(";");
						}
					}
					
					stringBuilder.AppendLine();
					rowIndex++;
				}
			}
			else {
				Debug.Log(string.Format("{0} is null", entry.Key));
			}
		}

		string dirPath = Path.Combine(Application.dataPath, ResultsFolder);
		
		if (!Directory.Exists(dirPath))
			Directory.CreateDirectory(dirPath);

		string filePath = Path.Combine(dirPath, "results.csv");
		
		int j = 2;
		while (File.Exists(filePath)) {
			filePath = Path.Combine(dirPath, string.Format("results-{0}.csv", j.ToString()));
			j++;
		}

		try {
			File.WriteAllText(filePath, stringBuilder.ToString());
		}
		catch (System.Exception e) {
			Debug.LogWarning("Generate results csv file error: " + e.Message.ToString());
		}
		
		if (File.Exists(filePath)) {
			Debug.Log("Successfully wrote out to file at: " + filePath);
			result = true;
		}
		else {
			Debug.LogError("Failed to write out to file at path: " + filePath);
		}

		return result;
	}

	public void GenerateCoordinateResultsCSV() {
		if (this.cachedResults != null && this.cachedResults.Count > 0) {
			generateCoordinateResultsCSV(this.cachedResults);
		}
		else {
			Debug.LogWarning("Could not generate coordinate results CSV because no results have been cached");
		}
	}

	private bool generateCoordinateResultsCSV(IDictionary dict) {
		bool result = false;

		if (HeatmapDict.Count > 0) {
			StringBuilder coordinatesStringBuilder = new StringBuilder();

			int row = 1;
			bool b3D = false;
			bool bTais = false;
			string lastKey = "";
			foreach (KeyValuePair<string, List<string>> pair in HeatmapDict) {
				int calcRow;
				bool bParsed = int.TryParse(pair.Key.Substring(0, pair.Key.IndexOf(":")), out calcRow);
				if (bParsed) {
					if (row != calcRow) {
						coordinatesStringBuilder.AppendLine();
						row++;
					}					
					else if (b3D != pair.Key.Contains("3D")) {
						b3D = pair.Key.Contains("3D");
						coordinatesStringBuilder.AppendLine();
					}
					else if (bTais != pair.Key.Contains("TAIS")) {
						bTais = pair.Key.Contains("TAIS");
						coordinatesStringBuilder.AppendLine();
					}
					else if (lastKey != pair.Key) {
						lastKey = pair.Key;
						coordinatesStringBuilder.AppendLine();
					}
					
					coordinatesStringBuilder.Append(pair.Key + ";");
					
					if (pair.Key.Contains("2D")) {
						List<Vector2> list = convertStringListToVector2(pair.Value);
						foreach (Vector2 point in list) {
							coordinatesStringBuilder.Append(point.ToString() + ";");
						}
					}
					else if (pair.Key.Contains("3D")) {
						List<Vector3> list = convertStringListToVector3(pair.Value);
						foreach (Vector3 point in list) {
							coordinatesStringBuilder.Append(point.ToString() + ";");
						}
					}
					else {
						foreach (string str in pair.Value) {
							coordinatesStringBuilder.Append(str + ";");
						}
					}
				}
				else {
					Debug.LogError("Could not parse key from: " + pair.Key);
				}
			}	

			string dirPath = Path.Combine(Application.dataPath, ResultsFolder);
			
			if (!Directory.Exists(dirPath))
				Directory.CreateDirectory(dirPath);
			
			
			
			string coordinatesFilePath = Path.Combine(dirPath, "results-coords.csv");
			int i = 2;
			while (File.Exists(coordinatesFilePath)) {
				coordinatesFilePath = Path.Combine(dirPath, string.Format("results-coords-{0}.csv", i.ToString()));
				
				i++;
			}

			try {
				File.WriteAllText(coordinatesFilePath, coordinatesStringBuilder.ToString());
			}
			catch (System.Exception e) {
				Debug.LogWarning("Generate coordinate results csv file error: " + e.Message.ToString());
			}
			
			if (File.Exists(coordinatesFilePath)) {
				Debug.Log("Succesfully wrote out coordinates to file at: " + coordinatesFilePath);
				result = true;
			}
			else {
				Debug.LogError("Failed to write out coordinates to file at path: " + coordinatesFilePath);
			}
		}
		else {
			Debug.LogWarning("Heatmap Dictionary has not been populated (Count is 0)");
		}

		return result;
	}

	
	private void addToHeatmapList(int index, int rowIndex, string list, string[] columns, bool bTais) {
		if (index >= 0 && rowIndex >= 0 && !string.IsNullOrEmpty(list) && columns != null && columns.Length > 0) {
			List<string> coords = new List<string>();
			coords.AddRange((list.Replace(";", "|").Split('|')));
			
			string key = string.Format("{0}:{1}", rowIndex.ToString(), columns[index]);

			if (bTais)
				key = string.Format("{0}-TAIS", key);

			this.AddToDict(key, coords);
			//Debug.Log(string.Format("adding to heatmap results ref, key: {0}, value: {1}", key, coords));
		}
		else {
			Debug.LogWarning("Could not add coordinates list to heatmap dict: " + list);
		}
	}

	private string[] getDataColumns() {
		return new string[] {
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
			"Influenced by Eye Tracker?",
			"Annoyed by Eye Tracker?",
			"Eye Tracker Comments",
			"Influenced by Mouse Tracker?",
			"Annoyed by Mouse Tracker?",
			"Mouse Tracker Comments",
			"Influenced by Game Metrics?",
			"Annoyed by Game Metrics?",
			"Game Metrics Comments",
			"Influenced by Questionnaire?",
			"Annoyed by Questionnaire?",
			"Questionnaire Comments",
			"Time Played",
			"Time Spent",
			"Wave Count",
			"Total Tactics Changes",
			"Tactics Changes",
			"Target Changes",
			"Condition Changes",
			"Gold Spent",
			"Gold Earned",
			"Units Died",
			"Enemies Killed",
			"Gold Deposit Left",
			"Units Bought",
			"Unit Upgrades",
			"Units Sold",
			"Units Moved",
			"Total Selections",
			"Units Selected",
			"Enemies Selected",
			"Force Spawns",
			"Eyes Pos 2D",
			"Eyes Pos 3D",
			"Mouse Pos 2D",
			"Mouse Pos 3D",
			"Left Click Pos 2D",
			"Left Click Pos 3D",
			"Right Click Pos 2D",
			"Right Click Pos 3D",
			"TAIS Eyes Pos 2D",
			"TAIS Eyes Pos 3D",
			"TAIS Mouse Pos 2D",
			"TAIS Mouse Pos 3D",
			"TAIS Left Click Pos 2D",
			"TAIS Left Click Pos 3D",
			"TAIS Right Click Pos 2D",
			"TAIS Right Click Pos 3D",
			"Fixations List",
			"Average Pupil Size",
		};
	}
}
