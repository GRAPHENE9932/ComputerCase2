using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageBoxManager : MonoBehaviour
{
    /// <summary>
    /// Game object of message box.
    /// </summary>
    public GameObject messageBox;
    /// <summary>
    /// Starts message.
    /// </summary>
    /// <param name="text">Text of message.</param>
    /// <param name="length">Time length of message.</param>
    public void StartMessage(string text, int length)
    {
        StopAllCoroutines();
        StartCoroutine(MainAnim(length, text));
    }
    /// <summary>
    /// Message box animation coroutine.
    /// </summary>
    /// <param name="length">Time length of message.</param>
    /// <param name="text">Text of message.</param>
    private IEnumerator MainAnim(int length, string text)
    {
        //Enable message box.
        messageBox.SetActive(true);
        //Get image of message box.
        Image image = messageBox.GetComponent<Image>();
        //Get text of message box.
        Text textC = messageBox.GetComponentInChildren<Text>();

        //Set start time.
        float time = image.color.a / 4F;
        //Set text.
        textC.text = text;

        while (time < .25F)
        {
            time += Time.deltaTime;
            //Set alpha.
            image.color = new Color(0F, 0F, 0F, time * 4F);
            textC.color = new Color(1F, 1F, 1F, time * 4F);
            yield return null;
        }
        //Wait for inputed length.
        yield return new WaitForSeconds(length);
        time = 0F;
        while (time < 1F)
        {
            time += Time.deltaTime;
            image.color = new Color(0F, 0F, 0F, 1F - time);
            textC.color = new Color(1F, 1F, 1F, 1F - time);
            yield return null;
        }
        //Disable message box.
        messageBox.SetActive(false);
    }
}
