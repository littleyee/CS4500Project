// Edited and adapted by Ben 

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections;


public class TTCReactor : MonoBehaviour {
    // Object to instantiate with location to instantiate from
    public CollideObject prefab;
    public CollideObject robot;
    public CollideObject soccerball;
    public CollideObject jet;
    public CollideObject car;
    public CollideObject bus;
    public CollideObject dino;
    public Text uiText;

    public PrimaryButtonWatcher watcher;
    private string myFilePath;

    private CollideObject currentObject = null;

    private int trial = 0;
    private int totalTrials = 60;

    private List<int> int_list = new List<int>(60) { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5 };

    // used for timing ttc
    private System.Diagnostics.Stopwatch stopwatch;


    // Start is called before the first frame update
    void Start() {
        watcher.primaryButtonPress.AddListener(onPrimaryButtonEvent);
        Debug.Log("Button Status: Up");

        stopwatch = new System.Diagnostics.Stopwatch();

        //set file path
        myFilePath = Path.Combine(Application.persistentDataPath, string.Format("ttc-data_{0}.csv", System.DateTime.Now.ToString("MM-dd-yy_h-mm-tt")));
        if (File.Exists(myFilePath)) {
            try {
                File.Delete(myFilePath);
            }
            catch (System.Exception e) {
                Debug.LogError("File does not exist. Error: " + e.Message);
            }
        }
        WriteToFile("Trial,Condition,Speed,Distance,Estimated_TTC,Actual_TTC\n");

    }

    // Update is called once per frame
    void Update() {

    }

    public void onPrimaryButtonEvent(bool pressed) {
        // on button down while there are still trials remaining
        if (pressed) {
            Debug.Log("Button Status: Down");
            
            // only record data if the current object exists
            if (currentObject != null) {
                // stop stopwatch
                stopwatch.Stop();

                /*
                Plugged into PC with Link:
                    filePath = C:\Users\local_admin\AppData\LocalLow\DefaultCompany\{yourProjectName}
                Running APK on Oculus:
                    filePath = This PC\Quest 2\Internal shared storage\Android\data\com.DefaultCompany.VRTEST\files
                */
                float speed = currentObject.GetSpeed();
                float dist = currentObject.GetStartDistance();
                WriteToFile(string.Format("{0},{1},{2},{3},{4},{5}\n", trial, currentObject.conditionName,
                    speed, dist, dist/speed, stopwatch.Elapsed.TotalSeconds));

                // destroy current object
                Destroy(currentObject.gameObject);
            }

            if (trial < totalTrials) {
                trial += 1;
                uiText.text = string.Format("Trial {0}/{1}", trial, totalTrials);

                // choose a distance, then select a speed that will allow it to be viewable for (total) 3.5-6 seconds
                float newDist = Random.Range(30f, 150f);
                float newSpeed = Random.Range(newDist / 3.5f, newDist / 6f);
                if (int_list.Count > 0) {
                    int randomNumber = Random.Range(0, int_list.Count - 1);
                    int myInt = int_list[randomNumber];
                    int_list.RemoveAt(randomNumber);

                    switch (myInt) {
                        case 0:
                            currentObject = Instantiate(robot, new Vector3(0, 0, newDist), Quaternion.identity);
                            break;
                        case 1:
                            currentObject = Instantiate(soccerball, new Vector3(0, .5f, newDist), Quaternion.identity);
                            break;
                        case 2:
                            currentObject = Instantiate(car, new Vector3(0, 0, newDist), Quaternion.identity);
                            break;
                        case 3:
                            currentObject = Instantiate(jet, new Vector3(0, .75f, newDist), Quaternion.identity);
                            break;
                        case 4:
                            currentObject = Instantiate(bus, new Vector3(0, 0, newDist), Quaternion.identity);
                            break;
                        default:
                            currentObject = Instantiate(dino, new Vector3(0, 0, newDist), Quaternion.identity);
                            break;
                    }
                    currentObject.SetSpeed(newSpeed);
                    currentObject.SetStartDistance(newDist);
                }
                // reset stopwatch
                stopwatch.Reset();
                stopwatch.Start();
            }
            else {
                uiText.text = "That's all of the trials. Thank you for participating!";
                currentObject = null;
            }
        }

        //on button up
        if (!pressed) {
            Debug.Log("Button Status: Up");
        }

    }

    public void WriteToFile(string message) {
        try {
            StreamWriter fileWriter = new StreamWriter(myFilePath, true);
            fileWriter.Write(message);
            fileWriter.Close();
        }
        catch (System.Exception e) {
            Debug.LogError("Cannot write in the file. Error: " + e.Message);
        }

    }

}
