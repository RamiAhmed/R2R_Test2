using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MouseTracker : MonoBehaviour {

	public Texture2D EyesHeatmapImage = null;
	public Texture2D HeatmapImage = null;
	public bool bForceRenderHeatmap = false;

	public float MouseTrackFrequencyPerSecond = 4f;
	private float lastMouse = 0f;

	private GameController _gameControllerRef = null;
	private Camera playerCamRef = null;

	private EyeTribeClient eyeClient = null;

	public List<Vector2> MousePoints2D = new List<Vector2>();
	public List<Vector2> EyesPoints2D = new List<Vector2>();
	public List<Vector2> LeftClickPoints2D = new List<Vector2>();
	public List<Vector2> RightClickPoints2D = new List<Vector2>();

	public List<Vector3> MousePoints3D = new List<Vector3>();
	public List<Vector3> EyesPoints3D = new List<Vector3>();
	public List<Vector3> LeftClickPoints3D = new List<Vector3>();
	public List<Vector3> RightClickPoints3D = new List<Vector3>();

	public List<Vector2> TAISMousePoints2D = new List<Vector2>();
	public List<Vector2> TAISEyesPoints2D = new List<Vector2>();
	public List<Vector2> TAISLeftClickPoints2D = new List<Vector2>();
	public List<Vector2> TAISRightClickPoints2D = new List<Vector2>();
	
	public List<Vector3> TAISMousePoints3D = new List<Vector3>();
	public List<Vector3> TAISEyesPoints3D = new List<Vector3>();
	public List<Vector3> TAISLeftClickPoints3D = new List<Vector3>();
	public List<Vector3> TAISRightClickPoints3D = new List<Vector3>();

	public List<bool> FixationsList = new List<bool>();
	public List<float> PupilSizeList = new List<float>();

	private PlayerController playerRef = null;

	// Use this for initialization
	void Start () {
		_gameControllerRef = this.GetComponent<GameController>();
		eyeClient = GameObject.FindGameObjectWithTag("EyeTribeHandler").GetComponent<EyeTribeClient>();

		playerRef = _gameControllerRef.players[0].GetComponent<PlayerController>();
	}
	
	// Update is called once per frame
	void Update () {
		if (playerCamRef == null) 
			playerCamRef = _gameControllerRef.players[0].PlayerCam;

		if (eyeClient == null) 
			eyeClient = GameObject.FindGameObjectWithTag("EyeTribeHandler").GetComponent<EyeTribeClient>();

		if (playerCamRef != null && eyeClient != null &&
		    _gameControllerRef.CurrentGameState == GameController.GameState.PLAY) {

			if (Time.time - lastMouse > 1f/MouseTrackFrequencyPerSecond) {
				lastMouse = Time.time;

				trackMouse();
				trackEyes();

				FixationsList.Add(eyeClient.LastFixated);
				PupilSizeList.Add(eyeClient.LastPupilSize);
			}

			if (Input.GetMouseButtonDown(0)) {
				Vector2 clickPos = Input.mousePosition;
				clickPos.y = Screen.height - clickPos.y;

				if (playerRef.bSelectingTactics)
					TAISLeftClickPoints2D.Add(clickPos);

				LeftClickPoints2D.Add(clickPos);

				Ray clickRay = playerCamRef.ScreenPointToRay(clickPos);
				RaycastHit[] hits = Physics.RaycastAll(clickRay);
				Vector3 clickPos3D = Vector3.zero;
				foreach (RaycastHit hit in hits) {
					if (hit.collider.GetType() == typeof(TerrainCollider)) {
						clickPos3D = new Vector3(hit.point.x, hit.point.y, hit.point.z);
						break;
					}
				}
				if (playerRef.bSelectingTactics)
					TAISLeftClickPoints3D.Add(clickPos3D);

				LeftClickPoints3D.Add(clickPos3D);
			}

			if (Input.GetMouseButtonDown(1)) {
				Vector2 clickPos = Input.mousePosition;
				clickPos.y = Screen.height - clickPos.y;

				if (playerRef.bSelectingTactics)
					TAISRightClickPoints2D.Add(clickPos);

				RightClickPoints2D.Add(clickPos);

				Ray clickRay = playerCamRef.ScreenPointToRay(clickPos);
				RaycastHit[] hits = Physics.RaycastAll(clickRay);
				Vector3 clickPos3D = Vector3.zero;
				foreach (RaycastHit hit in hits) {
					if (hit.collider.GetType() == typeof(TerrainCollider)) {
						clickPos3D = new Vector3(hit.point.x, hit.point.y, hit.point.z);
						break;
					}
				}

				if (playerRef.bSelectingTactics)
					TAISRightClickPoints3D.Add(clickPos3D);

				RightClickPoints3D.Add(clickPos3D);
			}
		}
	}

	void OnGUI() {
		if (Input.GetKey(KeyCode.F4) || bForceRenderHeatmap) {
			render2DHeatmap();
		}
	}

	private void trackEyes() {
		if (eyeClient == null)
			return;

		Vector3 currentGaze = eyeClient.gazePosInvertY;
		if (currentGaze.z > 0) {
			Vector2 eyesPos = new Vector2(currentGaze.x, currentGaze.y);
			Vector3 eyesPos2D = new Vector3(eyesPos.x, eyesPos.y, 0f);
			//GA.API.Design.NewEvent("EyesPos2D", eyesPos2D);

			if (playerRef.bSelectingTactics)
				TAISEyesPoints2D.Add(new Vector2(eyesPos2D.x, Screen.height - eyesPos2D.y));

			EyesPoints2D.Add(new Vector2(eyesPos2D.x, Screen.height - eyesPos2D.y));

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

				if (playerRef.bSelectingTactics)
					TAISEyesPoints3D.Add(eyesPos3D);

				EyesPoints3D.Add(eyesPos3D);
				//GA.API.Design.NewEvent("EyesPos3D", eyesPos3D);
			}
		}
	}

	private void trackMouse() {
		Vector2 mousePos = Input.mousePosition;
		Vector3 mousePos2D = new Vector3(mousePos.x, mousePos.y, 0f);
		//GA.API.Design.NewEvent("MousePos2D", mousePos2D);

		if (playerRef.bSelectingTactics)
			TAISMousePoints2D.Add(new Vector2(mousePos2D.x, mousePos2D.y));

		MousePoints2D.Add(new Vector2(mousePos2D.x, mousePos2D.y));

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

			if (playerRef.bSelectingTactics)
				TAISMousePoints3D.Add(mousePos3D);

			MousePoints3D.Add(mousePos3D);
			//GA.API.Design.NewEvent("MousePos3D", mousePos3D);
		}
	}

	private void render2DHeatmap() {
		if (HeatmapImage != null) {
			float pointWidth = 20f,
				  pointHeight = 20f;
			foreach (Vector2 point in MousePoints2D) {
				GUI.DrawTexture(new Rect(point.x - (pointWidth/2f), point.y - (pointHeight/2f), pointWidth, pointHeight), HeatmapImage);
			}

			if (EyesHeatmapImage != null && EyesPoints2D.Count > 0) {
				foreach (Vector2 point in EyesPoints2D) {
					GUI.DrawTexture(new Rect(point.x - (pointWidth/2f), point.y - (pointHeight/2f), pointWidth, pointHeight), EyesHeatmapImage);
				}
			}

			if (!GUI.skin.box.wordWrap) 
				GUI.skin.box.wordWrap = true;

			GUI.Box(new Rect(10f, Screen.height/3f - 25f, 100f, 50f), "Rendering Heatmap");
		}
		else {
			Debug.LogWarning("Heatmap Image is null");
		}
	}

}
