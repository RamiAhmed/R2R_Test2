using UnityEngine;
using System.Collections;

public class MouseTracker : MonoBehaviour {

	public float MouseTrackFrequencyPerSecond = 2f;
	private float lastMouse = 0f;

	private GameController _gameControllerRef = null;
	private Camera playerCamRef = null;

	// Use this for initialization
	void Start () {
		_gameControllerRef = this.GetComponent<GameController>();

	}
	
	// Update is called once per frame
	void Update () {
		if (playerCamRef == null) 
			playerCamRef = _gameControllerRef.players[0].PlayerCam;

		if (Time.time - lastMouse > 1f/MouseTrackFrequencyPerSecond) {
			lastMouse = Time.time;

			string timeNow = _gameControllerRef.GameTime.ToString("F2");
			trackMouse(timeNow);
			trackEyes(timeNow);
		}
	}

	private void trackEyes(string timeNow) {
		/*
		Vector2 eyesPos = Vector2.zero;
		Vector3 eyesPos2D = new Vector3(eyesPos.x, eyesPos.y, 0f);
		GA.API.Design.NewEvent("EyesPosition2D:" + timeNow, eyesPos2D);

		if (playerCamRef != null) {
			Ray eyesRay = playerCamRef.ScreenPointToRay(eyesPos2D);
			RaycastHit[] hits = Physics.RaycastAll(eyesRay);
			Vector3 eyesPos3D = Vector3.zero;
			foreach (RaycastHit hit in hits) {
				if (hit.collider.GetType() == typeof(TerrainCollider)) {
					eyesPos3D = new Vector3(hit.point.x, hit.point.y, hit.point.z);
					break;
				}
			}

			GA.API.Design.NewEvent("EyesPosition3D:" + timeNow, eyesPos3D);
		}
		*/
	}

	private void trackMouse(string timeNow) {
		Vector2 mousePos = Input.mousePosition;
		Vector3 mousePos2D = new Vector3(mousePos.x, mousePos.y, 0f);
		GA.API.Design.NewEvent("MousePosition2D:" + timeNow, mousePos2D);

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

			GA.API.Design.NewEvent("MousePosition3D:" + timeNow, mousePos3D);
		}
	}
}
