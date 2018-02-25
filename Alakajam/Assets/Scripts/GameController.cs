using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GameController : NetworkBehaviour {

	public static GameController instance;

    public PlayerComponent player1;
	public PlayerComponent player2;
    public float spawnZoneSize;
    public float spawnMinDist;
    public GameObject planet;
	public bool gameOver;
	public GameObject gameOverPanel;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Use this for initialization
    void Start () {
		gameOver = false;
		gameOverPanel.SetActive(false);
		planet.transform.position = Random.insideUnitCircle * 2;	
        if (GetComponent<NetworkIdentity>().isServer)
        {
			planet = Instantiate(planet, Vector3.zero, Quaternion.identity);
            gameOverPanel = Instantiate(gameOverPanel, Vector3.zero, Quaternion.identity);
        }
	}

	public static GameController getInstance () {
		return instance;
	}
	
	// Update is called once per frame
	void Update () {
		if (gameOver && Input.GetMouseButtonDown(0) && GetComponent<NetworkIdentity>().isServer) 
		{
			restartGame ();
		}
	}

    //does game critical restart logic only on server
    //i.e. place players, then calls RpcRestartGame
	public void restartGame() {
        //reload the current scene
        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        //randomize player and planet positions
        player1.transform.position = Random.insideUnitCircle * spawnZoneSize;
        player2.transform.position = Random.insideUnitCircle * spawnZoneSize;
        float dist = Vector2.Distance(player1.transform.position, player2.transform.position);
        while (dist <= spawnMinDist)
        {
            player2.transform.position = Random.insideUnitCircle * spawnZoneSize;
            dist = Vector2.Distance(player1.transform.position, player2.transform.position);
        }

        planet.transform.position = Random.insideUnitCircle * 2;
        gameOver = false;

        player1.SetAlive(true);
        player2.SetAlive(true);
        RpcRestartGame();

    }

    [ClientRpc]
    //Restarts game on client, sets players visible and game over screen invisible.
    public void RpcRestartGame() {
        player1.SetAlive(true);
        player2.SetAlive(true);
        gameOverPanel.SetActive(false);

    }

    [ClientRpc]
    public void RpcGameOver(NetworkInstanceId nid)
	{
		if(true) {
            //TODO: fix this check and put it back in
            //this.isLocalPlayer && this.netId==nid) {
			//winning player calls this with their net id thing
//			player1.score += 1;
			//gameOverPanel.GetComponentInChildren<Text>().text = "You won :)";
		} else {
            //gameOverPanel.GetComponentInChildren<Text>().text = "loser :>D";
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
            RpcPlayerJoined(player1.GetComponent<NetworkIdentity>().netId, player2.GetComponent<NetworkIdentity>().netId);
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

    [ClientRpc]
    void RpcPlayerJoined(NetworkInstanceId player1NetId, NetworkInstanceId player2NetId)
    {
        GameObject[] Players = GameObject.FindGameObjectsWithTag("GamePlayer");
        foreach(GameObject player in Players)
        {
            if (player.GetComponent<NetworkIdentity>().netId == player1NetId)
            {
                player1 = player.GetComponent<PlayerComponent>();
            } else if (player.GetComponent<NetworkIdentity>().netId == player2NetId)
            {
                player2 = player.GetComponent<PlayerComponent>();
            }
        }
    }
}
