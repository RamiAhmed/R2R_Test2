using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class ResultsHeatmapGenerator : MonoBehaviour {

	public Dictionary<string, List<string>> HeatmapDict = new Dictionary<string, List<string>>();

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		/*
		while (this.transform.childCount > 0) {
			Destroy(this.transform.GetChild(0).gameObject, 0.01f);
		}

		foreach (KeyValuePair<string, List<string>> pair in HeatmapDict) {
			if (pair.Key.Contains("Mouse")) {
				if (pair.Key.Contains("3D")) {
					foreach (string point in pair.Value) {
						string[] strPos = point.Split(',');

						float x,y,z;

						float.TryParse(strPos[0], out x);
						float.TryParse(strPos[1], out y);
						float.TryParse(strPos[2], out z);

						Vector3 pos = new Vector3(x, y, z);

						GameObject hPoint = Instantiate(Resources.Load("EditorPrefabs/HeatmapPoint")) as GameObject;
						hPoint.transform.position = pos;
						hPoint.transform.parent = this.transform;
					}
				}
			}
			//Debug.Log(pair.Key + " : " + pair.Value.ToString());
		}*/
	}

	public void AddToDict(KeyValuePair<string, List<string>> pair) {
		AddToDict(pair.Key, pair.Value);
	}

	public void AddToDict(string key, List<string> value) {
		if (HeatmapDict.ContainsKey(key)) {
			HeatmapDict[key] = value;
		}
		else {
			HeatmapDict.Add(key, value);
		}
	}
}
