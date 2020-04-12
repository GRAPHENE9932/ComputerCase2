using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorScript : MonoBehaviour
{
    //The component on cursor.
    public PCComponent currentComponent;
    //New component collided.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        currentComponent = collision.gameObject.GetComponent<Cell>().component;
    }
}
