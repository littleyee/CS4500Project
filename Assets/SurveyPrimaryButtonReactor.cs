using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class SurveyPrimaryButtonReactor : MonoBehaviour {

    public PrimaryButtonWatcher watcher;
    public Survey survey;

    // Start is called before the first frame update
    void Start() {
        watcher.primaryButtonPress.AddListener(onPrimaryButtonEvent);

    }

    // Update is called once per frame
    void Update() {

    }

    public void onPrimaryButtonEvent(bool pressed) {
        //on button down
        if (pressed) {
            survey.NextPage();
        }
    }
}
