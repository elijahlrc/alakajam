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
	public GameObject gameOverLossPanel;
    public GameObject gameOverWinPanel;


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


        planet.transform.position = Random.insideUnitCircle * 2;

        gameOverLossPanel = Instantiate(gameOverLossPanel, Vector3.zero, Quaternion.identity);
        gameOverWinPanel = Instantiate(gameOverWinPanel, Vector3.zero, Quaternion.identity);

        gameOverLossPanel.SetActive(false);
        gameOverWinPanel.SetActive(false);

        if (GetComponent<NetworkIdentity>().isServer)
        {
			planet = Instantiate(planet, Vector3.zero, Quaternion.identity);

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
        if (spawnMinDist > spawnZoneSize*2)
        {
            spawnZoneSize = spawnMinDist;
        }
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
        gameOverLossPanel.SetActive(false);
        gameOverWinPanel.SetActive(false);

    }

    [ClientRpc]
    public void RpcGameOver(NetworkInstanceId nid)
	{
        NetworkIdentity player1NetId = player1.GetComponent<NetworkIdentity>();
        NetworkIdentity player2NetId = player2.GetComponent<NetworkIdentity>();

        if (player1NetId.isLocalPlayer && player1NetId.netId == nid ||
            player2NetId.isLocalPlayer && player2NetId.netId == nid)
        {
            gameOverLossPanel.SetActive(true);
        }
        else {
            gameOverWinPanel.SetActive(true);
        }
        gameOver = true;
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
