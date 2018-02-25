using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class RadarPingDelay : MonoBehaviour {

    public float delay;
    private bool activated = false;
    ParticleSystem PS;
	// Use this for initialization
	void Start () {
        PS = GetComponent<ParticleSystem>();
    }
	
	// Update is called once per frame
	void Update () {
        delay -= Time.deltaTime;
        if (delay <= 0 && activated == false){
            activated = true;
            PS.Play();
        }
        if (activated && !PS.isPlaying)
        {
            Destroy(gameObject);
        }
    }
}
