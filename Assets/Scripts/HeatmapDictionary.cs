using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class HeatmapDictionary : IEnumerable<KeyValuePair<string, List<string>>> {

	[SerializeField]
	private List<string> _keys = null;

	[SerializeField]
	private List<string> _values = null;

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
		_values.Add(string.Join(";", list.ToArray()));
	}

	public List<string> GetValue(string key) {
		string values = _values[GetIndex(key)];
		if (!string.IsNullOrEmpty(values)) {
			List<string> valuesList = new List<string>();
			valuesList.AddRange(values.Split(';'));

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
		if (!_keys.Contains(key)) {
			return -1;
		}
		else {
			return _keys.IndexOf(key);
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

	public IEnumerator<KeyValuePair<string, List<string>>> GetEnumerator() {
		for (int i = 0; i < this.Count; i++) {
			yield return new KeyValuePair<string, List<string>>(this._keys[i], this.GetValue(_keys[i]));
		}
	}

	IEnumerator IEnumerable.GetEnumerator() {
		return GetEnumerator();
	}
}
