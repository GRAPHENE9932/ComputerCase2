using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitScript : MonoBehaviour
{
    public Animation windowAnim;
    /// <summary>
    ///     Вихід.
    /// </summary>
    public void Exit_Clicked()
    {
        windowAnim.gameObject.SetActive(true);
        windowAnim.Play("Open quit window");
    }

    public void Yes() => Application.Quit();

    public void No() => StartCoroutine(NoCorutine());

    private IEnumerator NoCorutine()
    {
        windowAnim.Play("Close quit window");
        yield return new WaitForSeconds(windowAnim.GetClip("Close quit window").length);
        windowAnim.gameObject.SetActive(false);
    }
}
