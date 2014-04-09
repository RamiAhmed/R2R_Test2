using UnityEngine;
using System.Collections;

public class MouseTracker : MonoBehaviour {

	public float MouseTrackFrequencyPerSecond = 2f;
	float lastMouse = 0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.time - lastMouse > 1f/MouseTrackFrequencyPerSecond) {
			lastMouse = Time.time;

			TrackMouse();
		}
	}

	public void TrackMouse() {
		Vector2 mousePos = Input.mousePosition;
		GA.API.Design.NewEvent("MousePosition:" + Time.time.ToString("F1"), new Vector3(mousePos.x, mousePos.y, 0f));
	}
}
