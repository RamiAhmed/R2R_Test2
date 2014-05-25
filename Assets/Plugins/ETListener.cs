using UnityEngine;
using System;

using TETCSharpClient;
using TETCSharpClient.Data;

public class ETListener : IGazeUpdateListener
{
	public GazeData lastGazeData;
	
	public float LastPupilSize = 0f;
	public bool LastFixated = false;
	
	// z value for lastgaze point indicates a valid gaze location is present (0 no, 1 yes);
	public Vector3 lastGazePoint = Vector3.zero;

	public bool bReady = false;

	public ETListener () {
		Debug.Log("Launch ET listener");
		
		bool connectedOk = true;
		GazeManager.Instance.Activate(1, GazeManager.ClientMode.Push);
		GazeManager.Instance.AddGazeListener(this);
		
		if (!GazeManager.Instance.IsConnected) {
			Debug.LogWarning("Eyetracking Server not started");
			connectedOk = false;
		}
		else if (!GazeManager.Instance.IsCalibrated) {
			Debug.LogWarning("User is not calibrated");
			connectedOk = false;
		}

		if (!connectedOk) {
			Debug.LogWarning("Connection not ready");	
		}
		else {
			bReady = true;
		}
		
	}	

	public void OnCalibrationStateChanged(bool state) {}
	public void OnScreenIndexChanged(int index) {}

	public void OnGazeUpdate(GazeData gazeData) {
		int x = Mathf.RoundToInt((float)gazeData.SmoothedCoordinates.X);
		int y = Mathf.RoundToInt((float)gazeData.SmoothedCoordinates.Y);
				
		if (x == 0 & y == 0) {   
			lastGazePoint = new Vector3((float)x,(float)y,0f);
		}
		else {
			lastGazeData = gazeData;
			lastGazePoint = new Vector3((float)x,(float)y,1f);
			
			double avgPupilSize = 0.0;
			if (gazeData.LeftEye != null) {
				avgPupilSize += gazeData.LeftEye.PupilSize;
				
				if (gazeData.RightEye != null) {
					avgPupilSize += gazeData.RightEye.PupilSize;
					avgPupilSize /= 2.0;
				}
			}
			
			LastPupilSize = (float)avgPupilSize;
			LastFixated = gazeData.IsFixated;
		}
	}	
}
