using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;

public class LangManager : MonoBehaviour
{
    public static Dictionary<string, string> currentDictionary = new Dictionary<string, string>();
    public static string lang;
    public EnumerationSetting langSetting;

    public static string GetString(string key)
    {
        if (!currentDictionary.ContainsKey(key))
            Debug.LogError($"Cannot find key \"{key}\"");
        return currentDictionary[key];
    }

    private void Awake()
    {
        langSetting.Index = GameSaver.Saves.lang;
        Apply();
    }

    public void Apply()
    {
        switch (langSetting.Index)
        {
            case 0:
                lang = "ENG";
                break;
            case 1:
                lang = "RUS";
                break;
            case 2:
                lang = "UKR";
                break;
        }
        string str = Resources.Load<TextAsset>("Languages/" + lang).text;
        string[] valPairs = str.Split(new string[] { "\n<pair>\n", "\r<pair>\r", "\r\n<pair>\r\n" }, System.StringSplitOptions.None);
        currentDictionary.Clear();
        for (int i = 0; i < valPairs.Length; i++)
        {
            string[] pair = Regex.Split(valPairs[i], "<val>");
            currentDictionary.Add(pair[0], pair[1]);
        }
        UpdateLabels();
    }

    private void UpdateLabels()
    {
        for (int i = 0; i < LangLabel.labels.Count; i++)
            LangLabel.labels[i].UpdateLabel();
    }
}
