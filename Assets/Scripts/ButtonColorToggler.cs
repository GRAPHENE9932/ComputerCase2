using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ButtonColorToggler : MonoBehaviour
{
    private bool toggled;
    public Color powerOnColor, powerOffColor;
    public float delay;
    [Space]
    public Image img;
    public Button button;

    public UnityEvent toggleEvent;

    private System.DateTime lastClick;

    public bool Toggled
    {
        get { return toggled; }
        set
        {
            toggled = value;
            img.color = img.color = toggled ? powerOnColor : powerOffColor;
        }
    }

    public void Trigger()
    {
        if ((System.DateTime.Now - lastClick).TotalSeconds < delay)
            return;

        toggled = !toggled;
        img.color = toggled ? powerOnColor : powerOffColor;

        toggleEvent.Invoke();

        lastClick = System.DateTime.Now;
    }
}
