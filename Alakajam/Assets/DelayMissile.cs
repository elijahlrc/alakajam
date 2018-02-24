using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DelayMissile : MonoBehaviour {

    public static float LAUNCH_DELAY = 1f;
    public static float ACCEL_DURATION = 1f;
    public static float ACCELERATION = 1f;

    private float timeTillLaunch;
    private float accelTimeLeft;

    private Rigidbody2D rb;

	// Use this for initialization
	void Start () {
        timeTillLaunch = LAUNCH_DELAY;
        accelTimeLeft = ACCEL_DURATION;
        shouldExist = true;
        rb = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if (timeTillLaunch > 0)
        {
            timeTillLaunch -= Time.fixedDeltaTime;
        } else if (accelTimeLeft > 0)
        {
            accelTimeLeft -= Time.fixedDeltaTime;
            rb.velocity += ((Vector2) transform.up) * (ACCELERATION * Time.fixedDeltaTime);
        }

        if (OutOfBounds())
        {
            SelfDestruct();
        }
	}

    private bool OutOfBounds()
    {
        return false;
    }

    private void SelfDestruct()
    {
        Destroy(this.gameObject);
    }
}
