using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

public class EyeTribeClient : MonoBehaviour {

	public Vector3 gazePosNormalY = new Vector3(Screen.width/2f, Screen.height/2f, 0f);
	public Vector3 gazePosInvertY = new Vector3(Screen.width/2f, Screen.height/2f, 0f);

	public float LastPupilSize = 0f;
	public bool LastFixated = false;
	
	private ETListener listener;

	private Rect guiRect = new Rect();
	
	// Use this for initialization
	void Start () {
		try {
			listener = new ETListener();			
		}
		catch(SocketException e) {
			Debug.LogError("EyeTribeClient listener instantiation error: " + e.Message.ToString());
		}

		float width = 250f,
		height = 75f;
		guiRect = new Rect(Screen.width/2f - width/2f, Screen.height/2f - height/2f, width, height);
	}

	void OnGUI() {
		if (!listener.bReady) {
			guiRect = GUI.Window(1, guiRect, showListenerNotReady, "Eye Tracker Not Ready");
			GUI.BringWindowToFront(1);
		}
	}

	private void showListenerNotReady(int windowID) {
		if (!listener.bReady) {
			Color origColor = GUI.color;
			if (GUI.color != Color.red)
				GUI.color = Color.red;
			GUI.Box(new Rect(5f, 25f, guiRect.width-10f, guiRect.height-30f), "EyeTribe Eye Tracker is not ready!!");
			GUI.color = origColor;
		}
	}
	
	public void startCalibration() {	
		Debug.Log("Sending Start calibration");
		
		try {
			listener.startCalibration();
		}
		catch {
			Debug.LogError("startCalibration failed");
		}
		
	}
	
	// Update is called once per frame
	void Update () {
		if (listener != null) {
			Vector3 lastGazePoint = listener.lastGazePoint;	
			gazePosNormalY = lastGazePoint;
			gazePosInvertY = new Vector3(lastGazePoint.x, Screen.height - lastGazePoint.y, lastGazePoint.z);

			LastFixated = listener.LastFixated;
			LastPupilSize = listener.LastPupilSize;
		}
		else {
			Debug.LogWarning("ETListener is null, cannot listen to eye tracking");
		}
	}
}
