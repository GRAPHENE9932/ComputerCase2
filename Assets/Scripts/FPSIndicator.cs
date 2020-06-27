using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSIndicator : MonoBehaviour
{
    public Toggle FPSToggle;
    public Text currentText;

    private short count;

    private void Start()
    {
        StartCoroutine(CountCorut());
    }

    private IEnumerator CountCorut()
    {
        float waitTime = 0F;
        while (true)
        {
            count++;
            waitTime += Time.deltaTime;
            if (waitTime >= 1F)
            {
                if (FPSToggle.toggled)
                    currentText.text = count + "FPS";
                else
                    currentText.text = null;
                count = 0;
                waitTime = 0F;
            }
            yield return null;
        }
    }
}
