// Edited and adapted by Ben 

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class ButtonReactor : MonoBehaviour
{
    // Object to instantiate with location to instantiate from
    public GameObject prefab;
    public Transform instanceSource;
    // Distance forward to instantiate
    public float instanceOffsetForward;

    public PrimaryButtonWatcher watcher;
    private string myFilePath;

    // Start is called before the first frame update
    void Start()
    {
        watcher.primaryButtonPress.AddListener(onPrimaryButtonEvent);
        Debug.Log("Button Status: Up");

        //set file path
        myFilePath = Path.Combine(Application.persistentDataPath, "project3LogBenStone.txt");
        if (File.Exists(myFilePath))
        {
            try
            {
                File.Delete(myFilePath);
            }
            catch (System.Exception e)
            {
                Debug.LogError("File does not exist. Error: " + e.Message);
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onPrimaryButtonEvent(bool pressed)
    {

        //on button down
        if (pressed)
        {
            Debug.Log("Button Status: Down");

            // instantiate prefab with forward offset
            PrefabMovement created = Instantiate(prefab, instanceSource.position + (transform.forward * instanceOffsetForward), 
                                        instanceSource.rotation).GetComponent(typeof(PrefabMovement)) as PrefabMovement;

            /*
            Plugged into PC with Link:
                filePath = C:\Users\local_admin\AppData\LocalLow\DefaultCompany\{yourProjectName}
            Running APK on Oculus:
                filePath = This PC\Quest 2\Internal shared storage\Android\data\com.DefaultCompany.VRTEST\files
            */
            WriteToFile(string.Format("Button pressed at {0}, creating object with movement speed {1:0.0} and rotation speed {2:0.0}\n",
                            System.DateTime.Now, created.GetSpeed(), created.GetRotationSpeed()));
        }

        //on button up
        if (!pressed)
        {
            Debug.Log("Button Status: Up");
        }
        
    }

    public void WriteToFile(string message)
    {
        try
        {
            StreamWriter fileWriter = new StreamWriter(myFilePath, true);
            fileWriter.Write(message);
            fileWriter.Close();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Cannot write in the file. Error: " + e.Message);
        }

    }
}
