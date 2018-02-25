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

    public int MaxRockets = 4;
    public float TimeToReload = 1.5f;
    private int currentRockets;
    private float lastReload;

    public float radarPingCooldown = 5;
    private float lastRadarPingTime;

    private Animator MyAnim;

    bool WasThrusting;
    [SyncVar(hook = "OnPlayerNumberSet")]
    int playerNumber;

    public GameObject captureRingPrefab;
    public GameObject captureRing;
    [SyncVar(hook = "OnCaptureProgress")]
    public float captureTime = 0f;

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

        MyAnim = GetComponent<Animator>();

        if (isLocalPlayer)
        {
            captureRing = Instantiate(captureRingPrefab, GetGameController().planet.transform.position, GetGameController().planet.transform.rotation);
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
            Vector3 MousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (NowThrusting) {
                Vector2 goalPosInWorldSpace = MousePos;
            CmdAccelerateInDirection(true, goalPosInWorldSpace);
                WasThrusting = true;
            } else {
                if (WasThrusting) {
                    CmdAccelerateInDirection(false, Vector2.zero);
                }
                WasThrusting = false;
            }

            bool firePressed = Input.GetKeyDown("mouse 0");
            if (firePressed && currentRockets > 0)
            {
                currentRockets  -= 1;
                Vector2 goalPosInWorldSpace = MousePos;
                CmdDropMissile(goalPosInWorldSpace);
            }
            if (currentRockets < MaxRockets)
            {
                if (Time.time - lastReload > TimeToReload)
                {
                    lastReload = Time.time;
                    currentRockets += 1;
                }
            }

            if (lastRadarPingTime < Time.time - radarPingCooldown) {
                foreach(RadarDetectible Obj in RadarDetectible.DetectableObjects){
                    Obj.PingMe(this.transform.position);
                }
                lastRadarPingTime = Time.time;
            }

            //animator
            Vector2 FacingDirection = MousePos - transform.position;
            MyAnim.Play("ShipRotator", 0, (12f / 24f) - (Mathf.Atan2(FacingDirection.y, FacingDirection.x) / (2 * Mathf.PI)));
        }

        if (MyNetworkID.isServer) {
        }

    }

    public override void PingMe(Vector2 PingCenter){
        //maybe some kind of "is visible" check?
        if (!isLocalPlayer && currentAcc != Vector2.zero) {
            GameObject RadarSignature = Instantiate(radarSignaturePFX, transform.position, transform.rotation);
            RadarSignature.GetComponent<RadarPingDelay>().delay = ((Vector2)transform.position - PingCenter).magnitude / 3f;
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
            GetComponent<CircleCollider2D>().enabled = alive;
            GetComponent<SpriteRenderer>().enabled = alive;
        }
    }

    private GameController GetGameController()
    {
        return GameController.getInstance();
    }

    void OnCaptureProgress(float captureTime)
    {
        if (isLocalPlayer)
        {
            float frac = Mathf.Min(captureTime / GetGameController().TIME_TO_CAPTURE, 1f);
            print("Capture progress " + frac);
            Color oldColor = captureRing.GetComponent<SpriteRenderer>().color;
            Color newColor = new Color(oldColor.r, oldColor.g, oldColor.b, frac);
            captureRing.GetComponent<SpriteRenderer>().color = newColor;
        }
    }
}


