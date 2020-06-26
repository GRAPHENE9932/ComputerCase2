using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource mainSource;
    public Toggle toggle;

    public AudioClip[] buttonClips, sellClips, clickClips, unclickClips;
    public void ToggleChanged()
    {
        mainSource.volume = toggle.toggled ? 1F : 0F;
    }
    public void PlayRandomButton()
    {
        mainSource.PlayOneShot(buttonClips[Random.Range(0, buttonClips.Length)]);
    }
    public void PlayRandomSell()
    {
        mainSource.PlayOneShot(sellClips[Random.Range(0, sellClips.Length)]);
    }
    public void PlayRandomClick()
    {
        mainSource.PlayOneShot(clickClips[Random.Range(0, clickClips.Length)]);
    }
    public void PlayRandomUnclick()
    {
        mainSource.PlayOneShot(unclickClips[Random.Range(0, unclickClips.Length)]);
    }
    public void PlaySound(AudioClip clip)
    {
        mainSource.PlayOneShot(clip);
    }
}
