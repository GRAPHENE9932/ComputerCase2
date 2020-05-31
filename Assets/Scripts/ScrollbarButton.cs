using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollbarButton : MonoBehaviour
{
    public Scrollbar scrollbar;
    /// <summary>
    /// It is top button (true) or bottom (false)?
    /// </summary>
    public bool goUp;

    public void Clicked()
    {
        //Increase or decrease scrollbar value.
        if (goUp)
            scrollbar.value += 1F / (scrollbar.numberOfSteps + 1);
        else
            scrollbar.value -= 1F / (scrollbar.numberOfSteps + 1);

        //Value borders.
        if (scrollbar.value > 1)
            scrollbar.value = 1;
        if (scrollbar.value < 0)
            scrollbar.value = 0;
    }
}
