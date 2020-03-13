using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonAnimation : MonoBehaviour
{
    public Image img;
    bool down;
    /// <summary>
    ///     При натисненні на кнопку.
    /// </summary>
    public void OnDown()
    {
        down = true;
        Debug.Log("Down");
    }
    /// <summary>
    ///     При відпусканні кнопки.
    /// </summary>
    public void OnUp()
    {
        down = false;
        Debug.Log("Up");
    }
    /// <summary>
    ///     Кожного кадру.
    /// </summary>
    private void Update()
    {
        if (down)
        {
            if (img.fillAmount < 1)
                img.fillAmount += Time.deltaTime * 10;
        }
        else
        {
            if (img.fillAmount > 0)
                img.fillAmount -= Time.deltaTime * 10;
        }
    }
}
