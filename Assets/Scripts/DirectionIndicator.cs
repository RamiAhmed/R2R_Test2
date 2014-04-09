using UnityEngine;
using System.Collections;

public class DirectionIndicator : MonoBehaviour {

    public float floatSpeed = 2f;
    public float amplitude = 2f;
    //public float rotationSpeed = 5f;
	
	private GameController _gameController;
	private MeshRenderer[] renderers;
	private float startY = 0f;
	
	// Use this for initialization
	void Start () {
		_gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
		startY = this.transform.position.y;
        renderers = this.gameObject.GetComponentsInChildren<MeshRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		if (_gameController.CurrentGameState == GameController.GameState.PLAY && _gameController.CurrentPlayState == GameController.PlayState.BUILD) {
	        float yPos = startY + amplitude * Mathf.Sin(floatSpeed * Time.time);                        
	        this.transform.position = new Vector3(this.transform.position.x, yPos, this.transform.position.z);                        
	        //this.transform.Rotate(this.transform.up, rotationSpeed);                        
			ToggleRenderers(true);
		}
		else {
			ToggleRenderers(false);	
		}
	}
	
    private void ToggleRenderers(bool enable) {
        foreach (MeshRenderer r in renderers)
        {
            if (r.enabled != enable) {
                r.enabled = enable;
			}
        }
    }        

}
