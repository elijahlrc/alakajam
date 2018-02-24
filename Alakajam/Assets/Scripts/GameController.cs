using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GameController : NetworkBehaviour {

	public static GameController instance;
	public GameObject player1;
	public GameObject player2;
	public Text scoreText;
	public Text restartText;
	public Text gameOverText;
	public bool gameOver;
	public int score;
	public GameObject gameOverPanel;

	// Use this for initialization
	void Start () {
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy (gameObject);
		}
		gameOver = false;
		gameOverText.text = "";
		restartText.text = "";
		scoreText.text = "";
		score = 0;
		gameOverPanel.SetActive(false);
			
	}
	
	// Update is called once per frame
	void Update () {
		if (gameOver && Input.GetMouseButtonDown(0)) 
		{
			restartGame ();
		}
	}

	public void restartGame() {
		player1.transform.position = Random.insideUnitCircle * 50;
		player2.transform.position = Random.insideUnitCircle * 50;
		float dist = Vector2.Distance(player1.transform.position, player2.transform.position);
		while (dist <= 10) {
			player2.transform.position = Random.insideUnitCircle * 50;
			dist = Vector2.Distance(player1.transform.position, player2.transform.position);
		}
		gameOverPanel.SetActive(false);
		player1.SetActive (true);
		player2.SetActive (true);
	}   

	public void GameOver(NetworkInstanceId nid)
	{
		if(this.isLocalPlayer && this.netId==nid){
			//winning player calls this with their net id thing
			gameOverText.text = "You won :)";
		}else{
			gameOverText.text = "loser :>D";
		}
		gameOver = true;
		gameOverPanel.SetActive(true);
		player1.SetActive (false);
		player2.SetActive (false);
	}


}
