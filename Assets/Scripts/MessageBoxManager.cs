using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageBoxManager : MonoBehaviour
{
    public GameObject messageBox;
    public Transform parent;
    public void StartMessage(string text, int length)
    {
        StopAllCoroutines();
        StartCoroutine(MainAnim(length, text));
    }

    private IEnumerator MainAnim(int length, string text)
    {
        messageBox.SetActive(true);
        Image image = messageBox.GetComponent<Image>();
        Text textC = messageBox.GetComponentInChildren<Text>();

        float time = image.color.a / 4F;

        textC.text = text;

        while (time < .25F)
        {
            time += Time.deltaTime;
            image.color = new Color(0F, 0F, 0F, time * 4F);
            textC.color = new Color(1F, 1F, 1F, time * 4F);
            yield return null;
        }
        yield return new WaitForSeconds(length);
        time = 0F;
        while (time < 1F)
        {
            time += Time.deltaTime;
            image.color = new Color(0F, 0F, 0F, 1F - time);
            textC.color = new Color(1F, 1F, 1F, 1F - time);
            yield return null;
        }
        messageBox.SetActive(false);
    }
}
