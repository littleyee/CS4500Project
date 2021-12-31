// Written by Ben Stone, 2021

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectTimedDestroy : MonoBehaviour {
    public float destroyTimeSeconds;

    // Start is called before the first frame update
    void Start() {
        Destroy(gameObject, destroyTimeSeconds);
    }

    // Update is called once per frame
    void Update() {

    }
}
