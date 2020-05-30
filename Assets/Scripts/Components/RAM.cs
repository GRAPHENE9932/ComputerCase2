using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Components/RAM", fileName = "RAM")]
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

    public override string ShortProperties => FullProperties;

    /// <summary>
    /// Clones this object.
    /// </summary>
    /// <returns>
    /// Clonned object.
    /// </returns>
    public override PCComponent Clone()
    {
        PCComponent component = ScriptableObject.CreateInstance<RAM>();
        component.fullName = this.fullName;
        component.shortName = this.shortName;
        component.price = this.price;
        component.time = this.time;
        component.rarity = this.rarity;
        component.image = this.image;

        ((RAM)component).memory = this.memory;
        ((RAM)component).frequency = this.frequency;
        ((RAM)component).type = this.type;

        return component;
    }
}
