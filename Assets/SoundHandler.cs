using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundHandler : MonoBehaviour
{
    public AudioClip keySound;
    private AudioSource clickSource;
    // Start is called before the first frame update
    void Start()
    {
        clickSource = gameObject.AddComponent<AudioSource>();   
    }

    public void PlayClick() {
        clickSource.PlayOneShot(keySound);
    }
}
