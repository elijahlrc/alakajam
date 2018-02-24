using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientInput : MonoBehaviour {

	// Use this for initialization
	void Start () {
        
    }

    // Update is called once per frame
    void Update () {
        if(Network.isServer)
        {
            bool thrusting = Input.GetKey("mouse 0");
            if (thrusting)
            {
                Vector2 goalPos = Input.mousePosition;
                print(goalPos);
            }
        }
    }


}
