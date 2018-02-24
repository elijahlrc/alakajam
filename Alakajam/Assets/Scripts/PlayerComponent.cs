using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class PlayerComponent : NetworkBehaviour {

    NetworkIdentity MyNetworkID;
    Rigidbody2D Rb;
    GameController gameController;
    Vector2 currentAcc;
    public GameObject radarSignaturePFX;
    bool WasThrusting;
    // Use this for initialization
    void Start () {
        MyNetworkID = GetComponent<NetworkIdentity>();
        Rb = GetComponent<Rigidbody2D>();
	gameController = GameController.getInstance();
        if (!isLocalPlayer) {
            GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    // Update is called once per frame
    void Update () {
        if(isLocalPlayer) {
            bool NowThrusting = Input.GetKey("mouse 1");
            if (NowThrusting) {
                Vector2 goalPosInWorldSpace = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CmdAccelerateInDirection(true, goalPosInWorldSpace);
                WasThrusting = true;
            } else {
                if (WasThrusting) {
                    CmdAccelerateInDirection(false, Vector2.zero);
                }
                WasThrusting = false;
            }
            if (Input.GetKeyDown("space")) {
                PlayerComponent P1 = GameController.getInstance().player1;
                if (P1 != this)
                {
                    Instantiate(radarSignaturePFX, P1.transform);
                } else {
                    PlayerComponent P2 = GameController.getInstance().player2;
                    Instantiate(radarSignaturePFX, P2.transform);
                }
            }
        }

        if (MyNetworkID.isServer) {
            print("IsServer true");
        }

    }

    [Command]
    public void CmdAccelerateInDirection(bool thrusting,  Vector2 goalLoc){
        if (thrusting) {
            currentAcc = (goalLoc - Rb.position);
        } else {
            currentAcc = Vector2.zero;
        }
    }

    private void FixedUpdate() {
        Rb.velocity += currentAcc * Time.fixedDeltaTime;
    }
}

