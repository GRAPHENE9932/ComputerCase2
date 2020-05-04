using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryInfoWindow : MonoBehaviour
{
    /// <summary>
    /// Canvas group on the window.
    /// </summary>
    public CanvasGroup group;
    /// <summary>
    /// Animation of opening window.
    /// </summary>
    public IEnumerator WindowInAnimation()
    {
        //Duration: 0.5 s.
        float time = 0F;
        while (time < 0.25F)
        {
            time += Time.deltaTime;
            group.alpha = time * 4F;
            yield return null;
        }
    }
    /// <summary>
    /// Animation of closing window.
    /// </summary>
    public IEnumerator WindowOutAnimation()
    {
        //Duration: 0.5 s.
        float time = 0.25F;
        while (time > 0F)
        {
            time -= Time.deltaTime;
            group.alpha = time * 4F;
            yield return null;
        }
        gameObject.SetActive(false);
    }
    /// <summary>
    /// Back button event.
    /// </summary>
    public void Back()
    {
        StartCoroutine(WindowOutAnimation());
    }
}
