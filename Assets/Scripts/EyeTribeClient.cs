using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

public class EyeTribeClient : MonoBehaviour {

	public Vector3 gazePosNormalY = new Vector3(Screen.width/2, Screen.height/2, 0);
	public Vector3 gazePosInvertY = new Vector3(Screen.width/2, Screen.height/2, 0);

	public float LastPupilSize = 0f;
	public bool LastFixated = false;
	
	private ETListener listener;
	
	// Use this for initialization
	void Start () {
		try
		{
			listener = new ETListener();			
		}
		catch( SocketException e )
		{
			print(e);
		}
	}
	
	public void startCalibration() {
	
		Debug.Log("Sending Start calibration");
		
		try 
		{
			listener.startCalibration();
		}
		catch 
		{
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
