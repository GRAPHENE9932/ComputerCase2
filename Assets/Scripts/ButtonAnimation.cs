using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonAnimation : MonoBehaviour
{
    /// <summary>
    ///     Зображення кнопки, яке буде зазнавати анімації.
    /// </summary>
    public Image img;
    /// <summary>
    ///     Змінна, яка показує, чи утримується палець на кнопці.
    /// </summary>
    bool down;
    /// <summary>
    ///     При натисненні на кнопку.
    /// </summary>
    public void OnDown()
    {
        down = true;
    }
    /// <summary>
    ///     При відпусканні кнопки.
    /// </summary>
    public void OnUp()
    {
        down = false;
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
