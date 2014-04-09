using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
	
	public int EdgeThreshold = 15,
				CameraMoveSpeed = 30,
				MinimumY = 10,
				MaximumY = 30;
	public float CameraScrollMultiplier = 2f,
				CameraRotateMultiplier = 3f;

	[Range(5, 50)]
	public float NonPassibleBorderWidth = 25f;
	
	
	private int screenWidth, screenHeight;
	private Vector3 homePosition = Vector3.zero;
	private GameObject playerObject;
	private Terrain terrain;
	private Vector3 mapMinBounds = Vector3.zero, 
					mapMaxBounds = Vector3.zero;

	
	private GameController _gameController;

	// Use this for initialization
	void Start () {
		screenWidth = Screen.width;
		screenHeight = Screen.height;
		playerObject = this.transform.parent.gameObject;
		homePosition = playerObject.transform.position;
		_gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
		terrain = GameObject.FindGameObjectWithTag("Terrain").GetComponent<Terrain>();
		
		Vector3 terrainSize = terrain.terrainData.size;
		mapMinBounds = new Vector3(terrain.transform.position.x, 0f, terrain.transform.position.z);
		mapMaxBounds += mapMinBounds + new Vector3(terrainSize.x, 0f, terrainSize.z);
		
		mapMinBounds.x += NonPassibleBorderWidth;
		mapMinBounds.z += NonPassibleBorderWidth;
		mapMaxBounds.x -= NonPassibleBorderWidth;
		mapMaxBounds.z -= NonPassibleBorderWidth;
	}
	
	// Update is called once per frame
	void Update () {
		if (_gameController.CurrentGameState == GameController.GameState.PLAY) {
			if (Input.GetKeyUp(KeyCode.Home)) {
				playerObject.transform.position = homePosition;			
				return;
			}
			
			Vector3 mousePos = Input.mousePosition;
			float deltaTime = Time.deltaTime;
			Vector3 cameraVector = Vector3.zero;
			
			if (Input.GetMouseButton(1)) {
				float rotateVelocity = Input.GetAxis("Mouse X") * CameraMoveSpeed * CameraRotateMultiplier * deltaTime;
				playerObject.transform.Rotate(0f, rotateVelocity, 0f, Space.World);
				//Vector3 centerPoint = this.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(screenWidth/2f, 1f, screenHeight/2f));
				//playerObject.transform.RotateAround(centerPoint, Vector3.up, rotateVelocity/2f);
			}
			
			Vector3 edgeMove = edgeCameraMove(mousePos, deltaTime);
			if (edgeMove != Vector3.zero) {
				cameraVector += edgeMove;
			}
			
			Vector3 scrollMove = scrollCameraMove(deltaTime);
			if (scrollMove != Vector3.zero) {
				playerObject.transform.Translate(scrollMove, Space.World);
			}
			
			cameraVector += new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
			
			playerObject.transform.Translate(cameraVector, Space.Self);
			
			limitCameraMovementToTerrain();
		}
	}
	
	private void limitCameraMovementToTerrain() {
		Vector3 playerPos = playerObject.transform.position;
		if (playerPos.x > mapMaxBounds.x) {
			playerObject.transform.position = new Vector3(mapMaxBounds.x, playerPos.y, playerPos.z);
		}
		else if (playerPos.x < mapMinBounds.x) {
			playerObject.transform.position = new Vector3(mapMinBounds.x, playerPos.y, playerPos.z);
		}
		if (playerPos.z > mapMaxBounds.z) {
			playerObject.transform.position = new Vector3(playerPos.x, playerPos.y, mapMaxBounds.z);
		}
		else if (playerPos.z < mapMinBounds.z) {
			playerObject.transform.position = new Vector3(playerPos.x, playerPos.y, mapMinBounds.z);
		}		
		if (this.transform.position.y > MaximumY) {
			this.transform.position = new Vector3(this.transform.position.x, MaximumY, this.transform.position.z);	
		}
		else if (this.transform.position.y < MinimumY) {
			this.transform.position = new Vector3(this.transform.position.x, MinimumY, this.transform.position.z);
		}
	}
	
	private Vector3 scrollCameraMove(float deltaTime) {
		Vector3 cameraVelocity = Vector3.zero;
		
		if (Input.GetAxis("Mouse ScrollWheel") < 0 || Input.GetKey(KeyCode.PageUp)) { // back - zoom out
			if (this.transform.position.y < MaximumY) {
				cameraVelocity = -this.transform.forward * CameraScrollMultiplier * CameraMoveSpeed * deltaTime;
			}
		}
		else if (Input.GetAxis("Mouse ScrollWheel") > 0 || Input.GetKey(KeyCode.PageDown)) { // forward - zoom in
			if (this.transform.position.y > MinimumY) {
				cameraVelocity = this.transform.forward * CameraScrollMultiplier * CameraMoveSpeed * deltaTime;	 
			}
		}		
		
		return cameraVelocity;
	}
	
	private Vector3 edgeCameraMove(Vector3 mousePos, float deltaTime) {
		Vector3 cameraVelocity = Vector3.zero;

		if (mousePos.x > screenWidth - EdgeThreshold) {	
			cameraVelocity = Vector3.right * CameraMoveSpeed * deltaTime;	
		}
		else if (mousePos.x < EdgeThreshold) {
			cameraVelocity = -Vector3.right * CameraMoveSpeed * deltaTime;	
		}
		
		if (mousePos.y > screenHeight - EdgeThreshold) {
			cameraVelocity = Vector3.forward * CameraMoveSpeed * deltaTime;
		}
		else if (mousePos.y < EdgeThreshold) {
			cameraVelocity = -Vector3.forward * CameraMoveSpeed * deltaTime;
		}
		
		return cameraVelocity;		
	}
}
