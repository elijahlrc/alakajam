using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class PlayerComponent : RadarDetectible
{

    NetworkIdentity MyNetworkID;
    Rigidbody2D Rb;
    public GameObject missilePrefab;
    public GameObject explosionEffect;
    [SyncVar]
    Vector2 currentAcc;
    public GameObject radarSignaturePFX;

    public float radarPingCooldown = 5;
    private float lastRadarPingTime;
    
    bool WasThrusting;
    [SyncVar(hook = "OnPlayerNumberSet")]
    int playerNumber;
    // Use this for initialization
    void Start () {
        //base.Start();
        MyNetworkID = GetComponent<NetworkIdentity>();
        Rb = GetComponent<Rigidbody2D>();
        if (!isLocalPlayer) {
            GetComponent<SpriteRenderer>().enabled = false;
        }

        if (MyNetworkID.isServer)
        {
            playerNumber = GetGameController().RegisterPlayer(this);
            //gameObject.layer = gameController.GetLayer(playerNumber);
        }
    }

    void OnPlayerNumberSet(int playerNumber)
    {
        gameObject.layer = GetGameController().GetLayer(playerNumber);
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

            bool shouldDropMissile = Input.GetKeyDown("mouse 0");
            if (shouldDropMissile)
            {
                Vector2 goalPosInWorldSpace = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                CmdDropMissile(goalPosInWorldSpace);
            }

            if (lastRadarPingTime < Time.time - radarPingCooldown) {
                foreach(RadarDetectible Obj in RadarDetectible.DetectableObjects){
                    Obj.PingMe(this.transform.position);
                }
                lastRadarPingTime = Time.time;
            }
        }

        if (MyNetworkID.isServer) {
        }

    }

    public override void PingMe(Vector2 PingCenter){
        //maybe some kind of "is visible" check?
        if (!isLocalPlayer) { //&& currentAcc != Vector2.zero) {
            Instantiate(radarSignaturePFX, transform.position, transform.rotation);
        }
    }


    [Command]
    public void CmdAccelerateInDirection(bool thrusting,  Vector2 goalLoc){
        if (thrusting) {
            currentAcc = (goalLoc - Rb.position).normalized;
        } else {
            currentAcc = Vector2.zero;
        }
    }

    [Command]
    public void CmdDropMissile(Vector2 goalLoc)
    {
        Vector2 direction = goalLoc - Rb.position;
        direction.Normalize();
        GameObject missile = Instantiate(missilePrefab, transform.position, Quaternion.LookRotation(Vector3.forward, direction));
        missile.GetComponent<DelayMissile>().layer = gameObject.layer;
        NetworkServer.Spawn(missile);
    }

    private void FixedUpdate() {
        Rb.velocity += currentAcc * Time.fixedDeltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (MyNetworkID.isServer) {
            if (collision.gameObject.GetComponent<DelayMissile>())
            {
                RpcDie();
                GetGameController().RpcGameOver(MyNetworkID.netId);
                GetGameController().gameOver = true;
            }
        }
    }

    [ClientRpc]
    private void RpcDie() {
        Instantiate(explosionEffect, transform.position, transform.rotation);
        SetAlive(false);
    }

    public void SetAlive(bool alive) {
        GetComponent<PlayerComponent>().enabled = alive;
        if (isLocalPlayer)
        {
            GetComponent<PolygonCollider2D>().enabled = alive;
            GetComponent<SpriteRenderer>().enabled = alive;
        }
    }

    private GameController GetGameController()
    {
        return GameController.getInstance();
    }
}


