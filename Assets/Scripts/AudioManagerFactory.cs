using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManagerFactory : MonoBehaviour
{
    // tao bien luu tru audioSource

    public AudioSource musicAudioSource;
    public AudioSource vfxAudioSource;

    // tao bien luu tru audio Clip

    public AudioClip arcFactoryMusicClip;
    public AudioClip dieSoundClip;

    void Start()
    {
        musicAudioSource.clip = arcFactoryMusicClip;
        musicAudioSource.Play();
    }

    public void PlaySFX(AudioClip sfxClip)
    {
        vfxAudioSource.clip = sfxClip;
        vfxAudioSource.PlayOneShot(sfxClip);
    }
}
