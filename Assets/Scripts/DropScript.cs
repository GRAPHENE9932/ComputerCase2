using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropScript : MonoBehaviour
{
    public CaseScroller caseScroller;
    public NavigationScript navigation;
    public void OneMore()
    {
        caseScroller.dropAnim.Play("CloseDroppedComponent");
        caseScroller.StartCase();
    }
    public void Back()
    {
        navigation.StartCoroutine(navigation.SwitchMenu(MenuState.CasesMenu));
    }
}
