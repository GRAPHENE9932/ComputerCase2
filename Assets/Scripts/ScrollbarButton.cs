using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollbarButton : MonoBehaviour
{
    public Scrollbar scrollbar;
    public bool goUp;

    public void Clicked()
    {
        if (goUp)
            scrollbar.value += 1F / (scrollbar.numberOfSteps + 1);
        else
            scrollbar.value -= 1F / (scrollbar.numberOfSteps + 1);

        if (scrollbar.value > 1)
            scrollbar.value = 1;
        if (scrollbar.value < 0)
            scrollbar.value = 0;
    }
}
