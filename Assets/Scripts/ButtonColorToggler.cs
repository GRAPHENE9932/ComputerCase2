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
        toggled = !toggled;
        img.color = toggled ? powerOnColor : powerOffColor;

        toggleEvent.Invoke();

        StartCoroutine(Delay());
    }

    private IEnumerator Delay()
    {
        button.interactable = false;
        yield return new WaitForSeconds(delay);
        button.interactable = true;
    }
}
