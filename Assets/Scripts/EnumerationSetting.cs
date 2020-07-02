using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class EnumerationSetting : MonoBehaviour
{
    public string[] strings;
    public Sprite[] images;

    public SVGImage image;
    public Text text;

    public int index;

    private void Start()
    {
        UpdateUI();
    }

    public int Index
    {
        get
        {
            return index;
        }
        set
        {
            index = value;
            UpdateUI();
        }
    }

    public void Next()
    {
        if (index < strings.Length - 1)
            index++;
        UpdateUI();
    }
    public void Previous()
    {
        if (index > 0)
            index--;
        UpdateUI();
    }
    private void UpdateUI()
    {
        if (image != null)
            image.sprite = images[index];
        text.text = strings[index];
    }
}
