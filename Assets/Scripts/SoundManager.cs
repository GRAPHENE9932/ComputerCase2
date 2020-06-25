using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource mainSource;
    public Toggle toggle;

    public AudioClip[] buttonClips;
    public void ToggleChanged()
    {
        mainSource.volume = toggle.toggled ? 1F : 0F;
    }

    public void PlayRandomButton()
    {
        mainSource.PlayOneShot(buttonClips[Random.Range(0, buttonClips.Length)]);
    }
}
