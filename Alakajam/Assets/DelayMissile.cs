﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(ParticleSystem))]
public class DelayMissile : MonoBehaviour {

    public static float LAUNCH_DELAY = 1f;
    public static float ACCEL_DURATION = 1f;
    public static float ACCELERATION = 1f;

    private float timeTillLaunch;
    private float accelTimeLeft;

    private Rigidbody2D rb;
    public GameObject explosionEffect;
    private ParticleSystem thrusterEffect;

	// Use this for initialization
	void Start () {
        timeTillLaunch = LAUNCH_DELAY;
        accelTimeLeft = ACCEL_DURATION;
        rb = GetComponent<Rigidbody2D>();
        thrusterEffect = GetComponent<ParticleSystem>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if (timeTillLaunch > 0)
        {
            timeTillLaunch -= Time.fixedDeltaTime;
        } else if (accelTimeLeft > 0)
        {
            if (!thrusterEffect.isEmitting)
            {
                thrusterEffect.Play();
            }
            accelTimeLeft -= Time.fixedDeltaTime;
            rb.velocity += ((Vector2) transform.up) * (ACCELERATION * Time.fixedDeltaTime);
        } else if (thrusterEffect.isEmitting)
        {
            thrusterEffect.Stop();
        }

        if (OutOfBounds())
        {
            Destroy(this.gameObject);
        }
	}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        thrusterEffect.Stop();
        Instantiate(explosionEffect, transform.position, transform.rotation);
        Destroy(this.gameObject);

    }

    private bool OutOfBounds()
    {
        return false;
    }
}
