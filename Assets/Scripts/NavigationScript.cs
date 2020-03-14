using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NavigationScript : MonoBehaviour
{
    /// <summary>
    ///     Основне заднє зобракження меню навігації, на ній маска.
    /// </summary>
    public RectTransform mainMenuTransform;
    /// <summary>
    ///     Змінна, що показує, чи відкрите зараз меню.
    /// </summary>
    private bool menuOpened = true;
    /// <summary>
    ///     Змінна, що показує, чи програється зараз анімація закриття або відкриття меню.
    /// </summary>
    private bool menuAnimating;
    /// <summary>
    ///     Подія натиснення на кнопку меню. Запускає корутину анімації, якщо зараз не програється анімація відкриття або закриття.
    /// </summary>
    public void MenuButtonClicked()
    {
        if (!menuAnimating)
            StartCoroutine(MenuChangeState());
    }
    /// <summary>
    ///     Відкрити меню, якщо воно закрите, або закрити меню, якщо воно відкрите.
    /// </summary>
    private IEnumerator MenuChangeState()
    {
        menuAnimating = true;
        //Змінення стану відкритості меню навігації.
        menuOpened = !menuOpened;
        //Ініціалізація шляху.
        float fill = 1;
        //Ширина зображення меню
        float width;
        //Поки шлях більше нуля...
        while (fill > 0)
        {
            //Зменшення шляху так, щоб від почав дорівнювати нуль на 0.5-ій секунді.
            fill -= Time.deltaTime * 2;
            //Якщо йде відкриття меню...
            if (menuOpened)
            {
                //Перетворення перед зміненням заповнення.
                //Оскільки fill підноситься до куба, анімація йде з прискоренням.
                //Заповнення має коливатись від 0.1(6) до 1. Ось звідки взялось 0.8(3) і 0.1(6).
                //600 - це ширина у відкритому режимі.
                width = ((1 - fill * fill * fill) * .8333333F + .1666666F) * 600F;
            }
            //Якщо йде закриття меню...
            else
            {
                //Перетворення перед зміненням заповнення.
                //Оскільки fill підноситься до куба, анімація йде з прискоренням.
                //Заповнення має коливатись від 0.1(6) до 1. Ось звідки взялось 0.8(3) і 0.1(6).
                //600 - це ширина у відкритому режимі.
                width = (fill * fill * fill * .8333333F + .1666666F) * 600F;
            }
            //Встановлюється ширина, а висота не змінюється.
            mainMenuTransform.sizeDelta = new Vector2(width, mainMenuTransform.sizeDelta.y);
            yield return null;
        }
        //Ліквідація похибки.
        if (menuOpened)
            width = 600F;
        else
            width = 100F;
        mainMenuTransform.sizeDelta = new Vector2(width, mainMenuTransform.sizeDelta.y);

        menuAnimating = false;
    }
}
