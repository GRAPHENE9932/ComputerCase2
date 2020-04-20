using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public enum MenuState
{
    CasesMenu, Inventory, Computer, Shop, Casino, Statistics, Settings, CasesMenu2, Case
}

public class NavigationScript : MonoBehaviour
{
    /// <summary>
    ///     Масив відділів меню.
    /// </summary>
    public GameObject[] menus;
    /// <summary>
    ///     Основне заднє зобракження меню навігації, на ній маска.
    /// </summary>
    public RectTransform mainMenuTransform;
    /// <summary>
    ///     Змінна, що показує, чи відкрите зараз меню.
    /// </summary>
    private bool menuOpened = false;
    /// <summary>
    ///     Змінна, що показує, чи програється зараз анімація закриття або відкриття меню.
    /// </summary>
    private bool menuAnimating;
    /// <summary>
    ///     Змінна, що показує, чи програється зараз анімація зміни відділу меню.
    /// </summary>
    private bool switchAnimating;
    /// <summary>
    ///     Відділ меню, на якому зараз знаходиться гравець.
    /// </summary>
    private MenuState currentState;
    /// <summary>
    /// Подія зміни відділу меню.
    /// </summary>
    public UnityEvent onToggle;
    /// <summary>
    /// Object of drop in case page.
    /// </summary>
    public GameObject dropObj;
    /// <summary>
    /// While blocked cannot switch page.
    /// </summary>
    public bool blocked;
    /// <summary>
    ///     Подія натиснення на кнопку меню. Запускає корутину анімації, якщо зараз не програється анімація відкриття або закриття.
    /// </summary>
    public void MenuButtonClicked()
    {
        //Якщо зараз не йде анімація відкриття/закриття меню.
        if (!menuAnimating)
            StartCoroutine(MenuChangeState());
    }
    /// <summary>
    ///     Відкрити меню, якщо воно закрите, або закрити меню, якщо воно відкрите.
    /// </summary>
    private IEnumerator MenuChangeState()
    {
        //Виклик події.
        onToggle.Invoke();
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
    /// <summary>
    ///     Подія натиснення на елемент меню.
    /// </summary>
    /// <param name="state">
    ///     Відділ меню, на який треба перейти.
    /// </param>
    public void MenuItemClicked(int state)
    {
        //Якщо зараз не йде анімація зміни відділу меню і навігація не заблокована.
        if (!switchAnimating && !blocked)
            StartCoroutine(SwitchMenu((MenuState)state));
    }
    /// <summary>
    ///     Перемкнутися між відділами меню.
    /// </summary>
    /// <param name="state">
    ///     Відділ, на який треба перемкнутися.
    /// </param>
    public IEnumerator SwitchMenu(MenuState state)
    {
        //Якщо відділ, на який треба перейти, не увімкнений зараз...
        if (state != currentState)
        {
            //Зараз програється анімація.
            switchAnimating = true;
            //Шлях анімації виходу старого відділу.
            float fillOut = 0;
            //Ініціалізація компонентів, над якими буде відбуватися анімація.
            RectTransform oldTransform = (RectTransform)menus[(int)currentState].transform;
            CanvasGroup oldGroup = menus[(int)currentState].GetComponent<CanvasGroup>();
            RectTransform newTransform = (RectTransform)menus[(int)state].transform;
            CanvasGroup newGroup = menus[(int)state].GetComponent<CanvasGroup>();
            //Поки fillOut не дійде до 1...
            while (fillOut < 1)
            {
                //Обчислення координати y. Піднесення до куба для плавності.
                float y = fillOut * fillOut * fillOut * 600F;
                //Встановлення координати y.
                oldTransform.anchoredPosition = new Vector2(0, y);
                //Змінення прозорості рівномірно.
                oldGroup.alpha = 1F - fillOut;
                //Збільшення шляху так, щоб через 0.25 сек. він дорівнюівав 1.
                fillOut += Time.deltaTime * 4;
                //Очікування кадру.
                yield return null;
            }
            //Вимкнення старого відділу.
            menus[(int)currentState].SetActive(false);
            //Вимкнення дропу.
            dropObj.SetActive(false);
            //Увімкнення нового.
            menus[(int)state].SetActive(true);
            //Шлях анімації входу нового відділу.
            float fillIn = 1;
            //Поки fillIn не впаде до 0...
            while (fillIn > 0)
            {
                //Обчислення координати y. Піднесення до куба для плавності.
                float y = (1F - fillIn * fillIn * fillIn) * 600F - 600F;
                //Встановлення координати y.
                newTransform.anchoredPosition = new Vector2(0, y);
                //Змінення прозорості рівномірно.
                newGroup.alpha = 1F - fillIn;
                //Зменшення шляху так, щоб через 0.25 сек. він дорівнюівав 0.
                fillIn -= Time.deltaTime * 2;
                //Очікування кадру.
                yield return null;
            }
            //Фінальна заміна старого відділу на новий
            currentState = state;
            //Анімація закінчилась.
            switchAnimating = false;
        }
    }
}
