using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientSounds : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip windSounds;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        windSounds = (AudioClip)Resources.Load("Sounds/wind");
        audioSource.loop = true;
        audioSource.clip = windSounds;
        audioSource.Play();
        audioSource.volume = .5f;
    }

}
