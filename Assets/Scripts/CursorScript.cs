using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KlimSoft;

public class CursorScript : MonoBehaviour
{
    /// <summary>
    /// The component on cursor.
    /// </summary>
    [HideInInspector]
    public PCComponent currentComponent;

    public AudioSource mainSource;
    public AudioClip[] scrollClips;

    public Toggle vibrationToggle;

    /// <summary>
    /// New component collided.
    /// </summary>
    /// <param name="collision">Collided collider.</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        StatisticsScript.itemsScrolled++;
        currentComponent = collision.gameObject.GetComponent<Cell>().component;
        //Play random sound.
        mainSource.PlayOneShot(scrollClips[Random.Range(0, scrollClips.Length)]);
        //Vibrate.
        if (vibrationToggle.toggled)
            AndroidFeatures.Vibrate(50);
    }
}
