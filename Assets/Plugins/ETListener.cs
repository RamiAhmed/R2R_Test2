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
	public Vector3 lastGazePoint = new Vector3(0,0,0);

	public bool bReady = false;

	public ETListener () {
		Debug.Log("Launch ET listener");
		
		bool connectedOk = true;
		GazeManager.Instance.Activate(1, GazeManager.ClientMode.Push);
		GazeManager.Instance.AddGazeListener(this);
		
		if (!GazeManager.Instance.IsConnected) {
			Debug.LogWarning("Eyetracking Server not started");

			//Dispatcher.BeginInvoke(new Action(() => MessageBox.Show("EyeTracking Server not started")));
			connectedOk = false;
		}
		else if (!GazeManager.Instance.IsCalibrated) {
			Debug.LogWarning("User is not calibrated");
			
			//Dispatcher.BeginInvoke(new Action(() => MessageBox.Show("User is not calibrated")));
			connectedOk = false;
		}

		if (!connectedOk) {
			Debug.LogWarning("Connection not ready");	
		}
		else {
			bReady = true;
		}
		
	}
	
	#region Undefined methods 
	
	public void startCalibration() {
		//GazeManager.Instance.CalibrationStart();
	}
	
	
	#endregion
	
	
	#region Listener methods	
	public void OnScreenIndexChanged(int number) {}
	
	public void OnCalibrationStateChanged(bool val) {}
	
	public void OnGazeUpdate(GazeData gazeData) {
		int x = (int) Math.Round(gazeData.SmoothedCoordinates.X, 0);
		int y = (int) Math.Round(gazeData.SmoothedCoordinates.Y, 0);
		
		
		if (x == 0 & y == 0)
		{   
			lastGazePoint = new Vector3((float)x,(float)y,0f);
			return;
		}
		
		lastGazeData = gazeData;
		lastGazePoint = new Vector3((float)x,(float)y,1f);
		
		double pupilSize = 0.0;
		if (gazeData.LeftEye != null) {
			pupilSize += gazeData.LeftEye.PupilSize;
			
			if (gazeData.RightEye != null) {
				pupilSize += gazeData.RightEye.PupilSize;
				pupilSize /= 2.0;
			}
		}
		
		LastPupilSize = (float)pupilSize;
		LastFixated = gazeData.IsFixated;
		
		//Debug.Log(String.Format("{0} - {1}", x, y));
	}
	
	#endregion
}
