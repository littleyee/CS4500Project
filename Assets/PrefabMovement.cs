// Written by Ben Stone, Fall 2021

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabMovement : MonoBehaviour {
    // Each prefab gets a random speed for movement and rotation 
    // to keep things interesting
    public float movementSpeed;
    public float rotationSpeed;

    // used to randomize rotation of prefab
    private Vector3 axis;

    // Start is called before the first frame update
    void Awake() {
        movementSpeed = Random.Range(1f, 5f);
        rotationSpeed = Random.Range(50f, 150f);

        // set up rotation axis randomly
        Vector3[] axes = { Vector3.up, Vector3.down, Vector3.forward, Vector3.back, Vector3.right, Vector3.left };
        axis = axes[Random.Range(0, axes.Length)];
}

    // Update is called once per frame
    void Update() {
        // translate up
        transform.Translate(Vector3.up * movementSpeed * Time.deltaTime);
        // rotate randomly each frame
        transform.Rotate(axis, rotationSpeed * Time.deltaTime);
    }

    public float GetSpeed() {
        return movementSpeed;
    }

    public float GetRotationSpeed() {
        return rotationSpeed;
    }
}
