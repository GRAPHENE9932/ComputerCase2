using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonColorToggler : MonoBehaviour
{
    private bool toggled;
    public Color powerOnColor, powerOffColor;
    public float delay;
    [Space]
    public Image img;
    public Button button;

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

        StartCoroutine(Delay());
    }

    private IEnumerator Delay()
    {
        button.interactable = false;
        yield return new WaitForSeconds(delay);
        button.interactable = true;
    }
}
