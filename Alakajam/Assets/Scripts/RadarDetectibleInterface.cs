using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class RadarDetectible : NetworkBehaviour{
    public static List<RadarDetectible> DetectableObjects = new List<RadarDetectible>();
    public abstract void PingMe(Vector2 PingCenter);
    public void Awake() {
        DetectableObjects.Add(this);
    }
    public void OnDestroy(){
        DetectableObjects.Remove(this);
    }
}