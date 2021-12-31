using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DigitDetector : MonoBehaviour
{
    public Survey survey;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other) {
        var key = other.GetComponentInChildren<TextMeshPro>();
        if (key != null) {
            var feedback = other.gameObject.GetComponent<DigitInput>();

            if (feedback && feedback.keyCanBeHit) {
                feedback.keyHit = true;
                survey.SendDigit(key.text);
            }
        }
    }
}
