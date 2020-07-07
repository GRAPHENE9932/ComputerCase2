using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    /// <summary>
    ///     Зображення кнопки, яка буде зазнавати анімації.
    /// </summary>
    public Image img;
    /// <summary>
    ///     Змінна, яка показує, чи утримується палець на кнопці.
    /// </summary>
    bool down;
    public bool disabled;
    /// <summary>
    ///     При натисненні на кнопку.
    /// </summary>
    public void OnPointerDown(PointerEventData data)
    {
        down = true;
    }

    /// <summary>
    ///     При відпусканні кнопки.
    /// </summary>
    public void OnPointerUp(PointerEventData data)
    {
        down = false;
    }
    /// <summary>
    ///     Кожного кадру.
    /// </summary>
    private void Update()
    {
        //If button pushed down and not disabled.
        if (down && !disabled)
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
