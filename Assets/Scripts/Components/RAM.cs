using System;
using System.Reflection;
using UnityEngine;

[CreateAssetMenu(menuName = "Components/RAM", fileName = "RAM")]
[Serializable]
public class RAM : PCComponent
{
    public int memory, frequency, type;

    public override string FullProperties
    {
        get
        {
            string result = null;

            result += fullName + ";\n";
            result += memory + " MB;\n";
            result += frequency + " MHz;\n";

            if (type == 1)
            {
                result += $"{LangManager.GetString("gen:")} DDR;\n";
            }
            else
            {
                result += $"{LangManager.GetString("gen:")} DDR{type};\n";
            }

            result += price + "$.";

            return result;
        }
    }

    public override string ShortProperties => FullProperties;
}
