using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class HeatmapDictionary : IEnumerable<KeyValuePair<string, List<string>>> {

	[SerializeField]
	private List<string> _keys = null;

	[SerializeField]
	private List<string> _values = null;

	private string _delimiter = ";";

	public HeatmapDictionary() {
		_keys = new List<string>();
		_values = new List<string>();
	}

	public void Add(string key, List<string> list) {
		if (ContainsKey(key)) {
			Debug.LogWarning("HeatmapDictionary Add overwriting previously set key: " + key);

			_values.RemoveAt(GetIndex(key));
			_keys.Remove(key);
		}

		_keys.Add(key);
		_values.Add(string.Join(_delimiter, list.ToArray()));
	}

	public List<string> GetValue(string key) {
		string values = _values[GetIndex(key)];
		if (!string.IsNullOrEmpty(values)) {
			List<string> valuesList = new List<string>();
			valuesList.AddRange(values.Split(_delimiter.ToCharArray()[0]));

			return valuesList;
		}
		else {
			Debug.LogError("HeatmapDictionary GetValue Error: No value found for key: " + key);
			return null;
		}
	}

	public int Count {
		get { return _keys.Count; }
	}

	public int GetIndex(string key) {
		if (!ContainsKey(key)) {
			return -1;
		}
		else {
			return _keys.IndexOf(key);
		}
	}

	public string GetKey(int index) {
		string key = _keys[index];
		if (!ContainsKey(key)) {
			return "";
		}
		else {
			return key;
		}
	}

	public bool ContainsKey(string key) {
		return _keys.Contains(key);
	}

	public List<string> this[string key] {
		get {
			if (this.ContainsKey(key)) {
				return this.GetValue(key);
			}
			else {
				return null;
			}
		}
		set {
			if (value != null) 
				Add(key, value);
			else 
				Debug.LogError("Cannot add null to HeatmapDictionary");
		}
	}

	public List<string> this[int index] {
		get {
			string key = _keys[index];
			if (ContainsKey(key)) {
				return this.GetValue(key);
			}
			else {
				return null;
			}
		}
		set {
			if (_values != null) {
				string key = _keys[index];
				Add(key, value);
			}
			else 
				Debug.LogError("Cannot add null to HeatmapDictionary");
		}
	}

	public void Clear() {
		_keys.Clear();
		_values.Clear();
	}

	public IEnumerator<KeyValuePair<string, List<string>>> GetEnumerator() {
		for (int i = 0; i < this.Count; i++) {
			yield return new KeyValuePair<string, List<string>>(this._keys[i], this.GetValue(_keys[i]));
		}
	}

	IEnumerator IEnumerable.GetEnumerator() {
		return GetEnumerator();
	}
}
