using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GameController : NetworkBehaviour {

	public static GameController instance;
	public PlayerComponent player1;
	public PlayerComponent player2;
	public GameObject planet;
	public GameObject gameOverText;
	public bool gameOver;
	public GameObject gameOverPanel;

	// Use this for initialization
	void Start () {
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy (gameObject);
		}
		gameOver = false;
		gameOverText.GetComponent<Text> ().text = "";
		gameOverPanel.SetActive(false);
		planet.transform.position = Random.insideUnitCircle * 50;	
	}

	public static GameController getInstance () {
		return instance;
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
		planet.transform.position = Random.insideUnitCircle * 50;

	}
    
    [ClientRpc]
    public void RpcGameOver(NetworkInstanceId nid)
	{
		if(this.isLocalPlayer && this.netId==nid){
			//winning player calls this with their net id thing
//			player1.score += 1;
			gameOverText.GetComponent<Text> ().text = "You won :)";
		}else{
			gameOverText.GetComponent<Text> ().text = "loser :>D";
		}
		gameOver = true;
		gameOverPanel.SetActive(true);

	}

    public int RegisterPlayer(PlayerComponent player)
    {
        if (player1 == null)
        {
            player1 = player;
            return 1;
        } else if (player2 == null)
        {
            player2 = player;
            return 2;
        } else
        {
            throw new UnityException("too many players");
        }
    }

    public int GetLayer(int playerNumber)
    {
        return playerNumber + 7;
    }


}
