using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Components/RAM", fileName = "RAM")]
public class RAM : PCComponent
{
    public int memory, frequency, type;

    public override string Properties
    {
        get
        {
            string result = null;

            result += fullName + ";\n";
            result += memory + " MB;\n";
            result += frequency + " MHz;\n";

            if (type == 1)
            {
                result += "Type: DDR;\n";
            }
            else
            {
                result += "Type: DDR" + type + ";\n";
            }

            result += price + "$.";

            return result;
        }
    }
}
