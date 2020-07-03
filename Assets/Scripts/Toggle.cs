using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Toggle : MonoBehaviour
{
    public Animation anim;
    public UnityEvent toggledEvent;

    public RectTransform point;
    public float xPosON, xPosOFF;
    public Color colorON, colorOFF;

    public bool toggled;

    private void Awake()
    {
        FastToggle();
    }

    public void OnToggle()
    {
        if (!anim.isPlaying)
        {
            toggled = !toggled;
            anim.Play(toggled ? "ToggleON" : "ToggleOFF");
            toggledEvent.Invoke();
        }
    }

    private void FastToggle()
    {
        if (toggled)
        {
            point.GetComponent<Image>().color = colorON;
            point.anchoredPosition = new Vector2(xPosON, point.anchoredPosition.y);
        }
        else
        {
            point.GetComponent<Image>().color = colorOFF;
            point.anchoredPosition = new Vector2(xPosOFF, point.anchoredPosition.y);
        }
    }
}
