using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropScript : MonoBehaviour
{
    /// <summary>
    /// Case scroller script.
    /// </summary>
    public CaseScroller caseScroller;
    /// <summary>
    /// Navigation script.
    /// </summary>
    public NavigationScript navigation;
    /// <summary>
    /// Inventory script.
    /// </summary>
    public Inventory inventory;
    /// <summary>
    /// Money system script.
    /// </summary>
    public MoneySystem money;

    public Button sellButton, equipButton;
    public ButtonAnimation sellAnim, equipAnim;

    /// <summary>
    /// This function invoked at drop of component in case scroller script.
    /// </summary>
    public void OnDrop()
    {
        //Enable sell and equip button.
        sellButton.interactable = true;
        equipButton.interactable = true;
        sellAnim.disabled = false;
        equipAnim.disabled = false;
    }
    /// <summary>
    /// "One more" button clicked.
    /// </summary>
    public void OneMore()
    {
        if (caseScroller.EnoughtMoney)
            //Play animation of close if enought money.
            caseScroller.dropAnim.Play("CloseDroppedComponent");
        //Start case.
        caseScroller.StartCase();
    }
    /// <summary>
    /// "Back" button clicked.
    /// </summary>
    public void Back()
    {
        //Navigate to cases menu.
        navigation.StartCoroutine(navigation.SwitchMenu(MenuState.CasesMenu));
    }
    /// <summary>
    /// "Sell" button clicked.
    /// </summary>
    public void Sell()
    {
        //Add money.
        money.Money += inventory.components[inventory.components.Count - 1].price / 20;
        //Disable sell and equip button.
        sellButton.interactable = false;
        equipButton.interactable = false;
        sellAnim.disabled = true;
        equipAnim.disabled = true;
        //Remove dropped component.
        inventory.components.Remove(caseScroller.currentComponent);
    }
}
