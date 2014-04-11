using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MouseTracker : MonoBehaviour {

	public float MouseTrackFrequencyPerSecond = 2f;
	private float lastMouse = 0f;

	private GameController _gameControllerRef = null;
	private Camera playerCamRef = null;

	private EyeTribeClient eyeClient = null;


	// Use this for initialization
	void Start () {
		_gameControllerRef = this.GetComponent<GameController>();
		eyeClient = GameObject.FindGameObjectWithTag("EyeTribeHandler").GetComponent<EyeTribeClient>();
	}
	
	// Update is called once per frame
	void Update () {
		if (playerCamRef == null) 
			playerCamRef = _gameControllerRef.players[0].PlayerCam;

		if (eyeClient == null) 
			eyeClient = GameObject.FindGameObjectWithTag("EyeTribeHandler").GetComponent<EyeTribeClient>();

		if (Time.time - lastMouse > 1f/MouseTrackFrequencyPerSecond) {
			lastMouse = Time.time;

			trackMouse();
			trackEyes();
		}
	}

	private void trackEyes() {
		if (eyeClient == null)
			return;

		Vector3 currentGaze = eyeClient.gazePosInvertY;
		if (currentGaze.z > 0) {
			Vector2 eyesPos = new Vector2(currentGaze.x, currentGaze.y);
			Vector3 eyesPos2D = new Vector3 (eyesPos.x, eyesPos.y, 0f);
			GA.API.Design.NewEvent("EyesPos2D", eyesPos2D);

			if (playerCamRef != null) {
				Ray eyesRay = playerCamRef.ScreenPointToRay (eyesPos2D);
				RaycastHit[] hits = Physics.RaycastAll (eyesRay);
				Vector3 eyesPos3D = Vector3.zero;
				foreach (RaycastHit hit in hits) {
					if (hit.collider.GetType () == typeof(TerrainCollider)) {
							eyesPos3D = new Vector3 (hit.point.x, hit.point.y, hit.point.z);
							break;
					}
				}

				GA.API.Design.NewEvent("EyesPos3D", eyesPos3D);
			}
		}
	}

	private void trackMouse() {
		Vector2 mousePos = Input.mousePosition;
		Vector3 mousePos2D = new Vector3(mousePos.x, Screen.height - mousePos.y, 0f);
		GA.API.Design.NewEvent("MousePos2D", mousePos2D);

		if (playerCamRef != null) {
			Ray mouseRay = playerCamRef.ScreenPointToRay(mousePos2D);
			RaycastHit[] hits = Physics.RaycastAll(mouseRay);
			Vector3 mousePos3D = Vector3.zero;
			foreach (RaycastHit hit in hits) {
				if (hit.collider.GetType() == typeof(TerrainCollider)) {
					mousePos3D = new Vector3(hit.point.x, hit.point.y, hit.point.z);
					break;
				}
			}

			GA.API.Design.NewEvent("MousePos3D", mousePos3D);
		}
	}

}
