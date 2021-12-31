using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollideObject : MonoBehaviour
{
    // keeps track of time visible
    private float timer; 
    private bool isVisible = true;

    private float speed = 0;
    private float startDistance = 0;

    // stores proper height to display object
    public float yPosition;
    // how many seconds to show the object before hiding it
    public float timeUntilHide = 3;
    public string conditionName;

    // Start is called before the first frame update
    void Start()
    {
        timer = timeUntilHide;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(-Vector3.forward * speed * Time.deltaTime);

        // decrease time left shown
        if (isVisible) {
            timer -= Time.deltaTime;
            // if out of time, hide the object
            if (timer <= 0) {
                gameObject.SetActive(false);
                isVisible = false;
            }
        }
    }

    public void SetSpeed(float s) {
        this.speed = s;
    }

    public float GetSpeed() {
        return this.speed;
    }

    public void SetStartDistance(float sd) {
        this.startDistance = sd;
    }

    public float GetStartDistance() {
        return this.startDistance;
    }
}
