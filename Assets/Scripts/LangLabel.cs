using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LangLabel : MonoBehaviour
{
    public static List<LangLabel> labels = new List<LangLabel>();

    public string key;

    public bool alreadyUpdated;

    public LangLabel()
    {
        labels.Add(this);
    }

    public void UpdateLabel()
    {
        try
        {
            GetComponent<Text>().text = LangManager.GetString(key);
            alreadyUpdated = true;
        }
        catch (KeyNotFoundException)
        {
            Debug.LogError($"Cannot find \"{key}\"");
        }
    }
}
