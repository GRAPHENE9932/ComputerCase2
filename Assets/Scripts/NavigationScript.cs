using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public enum MenuState
{
    CasesMenu, Inventory, Computer, ShopMenu, Minigames, Statistics, Settings,
    CasesMenu2, Case, Monitor, ComponentsShop, Exchange,CasesStatistics, CrashMinigame,
    EnterCode
}

public class NavigationScript : MonoBehaviour
{
    /// <summary>
    ///     Menu sections array.
    /// </summary>
    public GameObject[] menus;
    /// <summary>
    ///     Main background image for navigation menu, it has mask.
    /// </summary>
    public RectTransform mainMenuTransform;
    /// <summary>
    ///     Veriable which means is opened menu now.
    /// </summary>
    private static bool menuOpened = false;
    /// <summary>
    ///     Variable which means is playing animation of opening or closing now.
    /// </summary>
    private static bool menuAnimating;
    /// <summary>
    ///     Variable which means is playing animation of canvas changing now.
    /// </summary>
    private static bool switchAnimating;
    /// <summary>
    ///     Menu section, which showed now.
    /// </summary>
    [HideInInspector]
    public MenuState currentState;
    /// <summary>
    ///     Menu section changing event.
    /// </summary>
    public UnityEvent onToggle;
    /// <summary>
    ///     Object of drop in case page.
    /// </summary>
    public GameObject dropObj;
    /// <summary>
    ///     While blocked cannot switch page.
    /// </summary>
    public static bool blocked;

    public AudioClip audioClip;
    public SoundManager soundMgr;
    /// <summary>
    ///     Exceptions, on which sound will not be played.
    /// </summary>
    public MenuState[] soundExceptions;

    public delegate void MenuSwitched(byte idx);
    public static event MenuSwitched onMenuSwitched;

    /// <summary>
    ///     Event of clicking on menu buttton. Starts animation coroutine
    ///     if animation of opening or closing not playing now.
    /// </summary>
    public void MenuButtonClicked()
    {
        if (!menuAnimating)
            StartCoroutine(MenuChangeState());
    }
    /// <summary>
    ///     Open menu if it closed or close menu if it opened.
    /// </summary>
    private IEnumerator MenuChangeState()
    {
        //Event invoking.
        onToggle.Invoke();
        menuAnimating = true;
        //Changing menu state.
        menuOpened = !menuOpened;
        //Path initialization.
        float fill = 1;
        //Menu image width.
        float width;
        //While path < 0...
        while (fill > 0)
        {
            //0.5 s
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
            //Invoke event
            onMenuSwitched.Invoke((byte)state);
            //Make both canvases uninteractable.
            menus[(int)currentState].GetComponent<CanvasGroup>().interactable = false;
            menus[(int)state].GetComponent<CanvasGroup>().interactable = false;
            //Play sound if it is not in exceptions.
            if (!IsInExceptions(state, currentState))
                soundMgr.PlaySound(audioClip);
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
            //Remove imprecision.
            newGroup.alpha = 0F;
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
            //Remove imprecision.
            newGroup.alpha = 1F;
            //Фінальна заміна старого відділу на новий
            currentState = state;
            //Анімація закінчилась.
            switchAnimating = false;
            //Make both canvases interactable.
            menus[(int)currentState].GetComponent<CanvasGroup>().interactable = true;
            menus[(int)state].GetComponent<CanvasGroup>().interactable = true;
        }
    }

    private bool IsInExceptions(MenuState s1, MenuState s2)
    {
        for (int i = 0; i < soundExceptions.Length / 2; i++)
        {
            if (soundExceptions[i * 2] == s1 && soundExceptions[i * 2 + 1] == s2 ||
                soundExceptions[i * 2] == s2 && soundExceptions[i * 2 + 1] == s1)
                return true;
        }
        return false;
    }
}
