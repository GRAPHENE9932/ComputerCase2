using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toggle : MonoBehaviour
{
    public Animation anim;

    public bool toggled;

    public void OnToggle()
    {
        if (!anim.isPlaying)
        {
            toggled = !toggled;
            anim.Play(toggled ? "ToggleON" : "ToggleOFF");
        }
    }
}
