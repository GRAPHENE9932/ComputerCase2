using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LangLabel : MonoBehaviour
{
    public static List<LangLabel> labels = new List<LangLabel>();

    public string key;

    private void Awake()
    {
        labels.Add(this);
    }

    private void Start()
    {
        UpdateLabel();
    }

    public void UpdateLabel()
    {
        try
        {
            GetComponent<Text>().text = LangManager.GetString(key);
        }
        catch (KeyNotFoundException)
        {
            Debug.LogError($"Cannot find \"{key}\"");
        }
    }
}
