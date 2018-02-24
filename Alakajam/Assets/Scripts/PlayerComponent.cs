using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class PlayerComponent : NetworkBehaviour {

    NetworkIdentity MyNetworkID;
    Rigidbody2D Rb;
    GameController gameController;
    Vector2 CurrentAcc;
    public GameObject missilePrefab;
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
            }
            else {
                if (WasThrusting){
                    CmdAccelerateInDirection(false, Vector2.zero);
                }
                WasThrusting = false;
            }

            bool shouldDropMissile = Input.GetKeyDown("mouse 0");
            if (shouldDropMissile)
            {
                Vector2 goalPosInWorldSpace = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            }
        }

        if (MyNetworkID.isServer) {
            print("IsServer true");
        }

    }

    [Command]
    public void CmdAccelerateInDirection(bool thrusting,  Vector2 goalLoc){
        if (thrusting) {
            CurrentAcc = (goalLoc - Rb.position);
        } else {
            CurrentAcc = Vector2.zero;
        }
    }

    [Command]
    public void CmdDropMissile(Vector2 goalLoc)
    {
        Vector2 direction = goalLoc - Rb.position;
        direction.Normalize();
        Instantiate(missilePrefab, transform.position, Quaternion.LookRotation(direction));
    }

    private void FixedUpdate() {
        Rb.velocity += CurrentAcc * Time.fixedDeltaTime;
    }

	private void Die() {
		gameController.GameOver (MyNetworkID.netId);
	}
}

