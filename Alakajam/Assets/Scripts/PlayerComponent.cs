using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class PlayerComponent : NetworkBehaviour {

    NetworkIdentity MyNetworkID;
    Rigidbody2D Rb;
	GameController gameController;

    // Use this for initialization
    void Start () {
        MyNetworkID = GetComponent<NetworkIdentity>();
        Rb = GetComponent<Rigidbody2D>();
		gameController = GameController.getInstance();
    }

    // Update is called once per frame
    void Update () {
        if(isLocalPlayer) {
            bool thrusting = Input.GetKey("mouse 0");
            if (thrusting) {
                Vector2 goalPosInWorldSpace = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                CmdAccelerateInDirection(goalPosInWorldSpace);


            }
        }

        if (MyNetworkID.isServer) {
            print("IsServer true");
        }

    }

    [Command]
    public void CmdAccelerateInDirection(Vector2 GoalLoc){
        Rb.velocity += (GoalLoc - Rb.position);
    }

	public void Die() {
		gameController.GameOver (MyNetworkID.netId);
	}
}

