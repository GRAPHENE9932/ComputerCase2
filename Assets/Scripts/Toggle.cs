using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Toggle : MonoBehaviour
{
    public Animation anim;
    public UnityEvent toggledEvent;

    public bool toggled;

    public void OnToggle()
    {
        if (!anim.isPlaying)
        {
            toggled = !toggled;
            anim.Play(toggled ? "ToggleON" : "ToggleOFF");
            toggledEvent.Invoke();
        }
    }
}
