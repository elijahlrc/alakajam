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


    public float TIME_TO_CAPTURE = 30f;

    private bool p1Capturing = false;
    private bool p2Capturing = false;

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

        gameOverLossPanel = Instantiate(gameOverLossPanel, Vector3.zero, Quaternion.identity);
        gameOverWinPanel = Instantiate(gameOverWinPanel, Vector3.zero, Quaternion.identity);

        gameOverLossPanel.SetActive(false);
        gameOverWinPanel.SetActive(false);

        if (GetComponent<NetworkIdentity>().isServer)
        {
			planet = Instantiate(planet, Vector3.zero, Quaternion.identity);
            planet.transform.position = Random.insideUnitCircle * 2;

            NetworkServer.Spawn(planet);
        }
    }

    public static GameController getInstance () {
		return instance;
	}
	
	// Update is called once per frame
	void Update () {
        if (GetComponent<NetworkIdentity>().isServer) {
            if (gameOver && Input.GetMouseButtonDown(0))
            {
                restartGame();
            } else if (player1 != null && player2 != null)
            {
                if (PlayerOnPlanet(player1))
                {
                    if (p1Capturing)
                    {
                        player1.captureTime += Time.deltaTime;
                        CheckCaptureProgress(player1);
                    }
                    p1Capturing = true;
                } else
                {
                    p1Capturing = false;
                }

                if (PlayerOnPlanet(player2))
                {
                    if (p2Capturing)
                    {
                        player2.captureTime += Time.deltaTime;
                        CheckCaptureProgress(player2);
                    }
                    p2Capturing = true;
                } else
                {
                    p2Capturing = false;
                }
            }
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
        player1.captureTime = 0f;
        player2.captureTime = 0f;
        RpcRestartGame(planet.transform.position);

    }

    [ClientRpc]
    //Restarts game on client, sets players visible and game over screen invisible.
    public void RpcRestartGame(Vector2 planetloc) {
        player1.SetAlive(true);
        player2.SetAlive(true);
        gameOverLossPanel.SetActive(false);
        gameOverWinPanel.SetActive(false);
        if(player1.isLocalPlayer)
        {
            player1.captureRing.transform.position = planetloc;
        }
        if (player2.isLocalPlayer)
        {
            player2.captureRing.transform.position = planetloc;
        }
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

    private bool PlayerOnPlanet(PlayerComponent player)
    {
        Vector2 playerPos = player.transform.position;
        Vector2 planetPos = planet.transform.position;
        float dist = Vector2.Distance(playerPos, planetPos);
        return dist < planet.GetComponent<CircleCollider2D>().radius * planet.transform.lossyScale.x;
    }

    private void CheckCaptureProgress(PlayerComponent player)
    {
        if (player.captureTime >= TIME_TO_CAPTURE)
        {
            if (player == player1)
            {
                RpcGameOver(player2.GetComponent<NetworkIdentity>().netId);
            } else
            {
                RpcGameOver(player1.GetComponent<NetworkIdentity>().netId);
            }
        }

    }

}
