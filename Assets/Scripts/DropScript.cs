using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropScript : MonoBehaviour
{
    public CaseScroller caseScroller;
    public NavigationScript navigation;
    public Inventory inventory;
    public MoneySystem money;

    public Button sellButton, equipButton;
    public ButtonAnimation sellAnim, equipAnim;

    public void OnDrop()
    {
        sellButton.interactable = true;
        equipButton.interactable = true;
        sellAnim.disabled = false;
        equipAnim.disabled = false;
    }
    public void OneMore()
    {
        caseScroller.dropAnim.Play("CloseDroppedComponent");
        caseScroller.StartCase();
    }
    public void Back()
    {
        navigation.StartCoroutine(navigation.SwitchMenu(MenuState.CasesMenu));
    }
    public void Sell()
    {
        money.Money += inventory.components[inventory.components.Count - 1].price / 20;
        sellButton.interactable = false;
        equipButton.interactable = false;
        sellAnim.disabled = true;
        equipAnim.disabled = true;
    }
}
