using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorScript : MonoBehaviour
{
    /// <summary>
    /// The component on cursor.
    /// </summary>
    public PCComponent currentComponent;
    /// <summary>
    /// New component collided.
    /// </summary>
    /// <param name="collision">Collided collider.</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        StatisticsScript.itemsScrolled++;
        currentComponent = collision.gameObject.GetComponent<Cell>().component;
    }
}
