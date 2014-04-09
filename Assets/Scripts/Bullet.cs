using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {
	
	public Vector3 Target = Vector3.zero;
	
	public GameObject Owner = null;
	
	public float BulletSpeed = 5f;
	protected float maxForce = 10f;
		
//	private float initialDistance = 0f;
	
	private GameController _gameController;
	
	void Start() {
		this.transform.parent = Owner.transform;
		
		this.transform.localPosition = new Vector3(0f, 1.1f, 0f);
//		initialDistance = Vector3.Distance(this.transform.position, Target);
		
		_gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
	}
	
	void FixedUpdate() {
		if (_gameController.CurrentGameState == GameController.GameState.PLAY) {
			this.transform.LookAt(Target);
			
			float currentDistance = Vector3.Distance(this.transform.position, Target);
			Vector3 direction = this.transform.forward * BulletSpeed * Time.fixedDeltaTime * currentDistance;
			Vector3 velocity = Vector3.ClampMagnitude(direction, maxForce);
			this.transform.Translate(velocity, Space.World);
		}
	}
	
	void LateUpdate() {
		float currentDistance = Vector3.Distance(this.transform.position, Target);
		
		if (currentDistance < 1f) {
			Destroy(this.gameObject);
		}
	}
	
}
