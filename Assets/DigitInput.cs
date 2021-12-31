using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DigitInput : MonoBehaviour
{
    public bool keyHit = false;
    public bool keyCanBeHit = false;

    private float originalY;

    private const float depressTime = 0.25f;
    private const float buttonRepeatTime = 0.25f;

    private SoundHandler soundHandler;

    private float timer;
    private const float depressDepth = 0.01f;
    // Start is called before the first frame update
    void Start()
    {
        originalY = transform.position.y;
        timer = 0f;
        soundHandler = GameObject.FindGameObjectWithTag("SoundHandler").GetComponent<SoundHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > 0) {
            timer -= Time.deltaTime;
        }
        else if (keyHit && timer <= 0) {
            soundHandler.PlayClick();
            keyCanBeHit = false;
            keyHit = false;
            transform.Translate(Vector3.down * depressDepth);
            timer = depressTime;
        }
        else if (transform.position.y < originalY && timer <= 0) {
            transform.Translate(Vector3.up * depressDepth);
            timer = buttonRepeatTime;
        }
        else {
            keyCanBeHit = true;
        }
    }
}
