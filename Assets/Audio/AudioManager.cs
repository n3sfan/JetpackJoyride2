using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // tao bien luu tru audioSource

    public AudioSource musicAudioSource;
    public AudioSource vfxAudioSource;

    // tao bien luu tru audio Clip

    public AudioClip arcFactoryMusicClip;
    public AudioClip arcOceanMusicClip;

    void Start()
    {
        musicAudioSource.clip = arcFactoryMusicClip;
        musicAudioSource.Play();
    }
}
