using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Components/CPU", fileName = "CPU")]
public class CPU : PCComponent
{
    public int frequency, cores, TDP, power;
    public byte[] additionalRAMTypes;
    public byte maxRAMChannels, RAMType;
    public bool unlocked, _64bit, integratedGraphics;
    public string socket;

    public override string Properties
    {
        get
        {
            string result = null;

            result += fullName + ";\n";
            result += "Socket " + socket + ";\n";
            result += frequency + "MHz;\n";
           
            if (cores % 10 == 1 && cores % 100 != 11)
            {
                //Ядро
                result += cores + " core;\n";
            }
            else if ((cores % 10 >= 2 && cores % 10 <= 4) && (cores % 100 - cores % 10) / 10 != 1)
            {
                //Ядра
                result += cores + " cores;\n";
            }
            else
            {
                //Ядер
                result += cores + " cores;\n";
            }

            result += "TDP: " + TDP + " W;\n";
            result += "Performance: " + power + " GFlops;\n";
            result += "Price: " + price + "$.";

            return result;
        }
    }
    
    /// <summary>
    /// Clones this object.
    /// </summary>
    /// <returns>
    /// Clonned object.
    /// </returns>
    public override PCComponent Clone()
    {
        PCComponent component = ScriptableObject.CreateInstance<CPU>();
        component.fullName = this.fullName;
        component.shortName = this.shortName;
        component.price = this.price;
        component.time = this.time;
        component.rarity = this.rarity;
        component.image = this.image;

        ((CPU)component).frequency = this.frequency;
        ((CPU)component).cores = this.cores;
        ((CPU)component).TDP = this.TDP;
        ((CPU)component).power = this.power;
        ((CPU)component).additionalRAMTypes = this.additionalRAMTypes;
        ((CPU)component).maxRAMChannels = this.maxRAMChannels;
        ((CPU)component).RAMType = this.RAMType;
        ((CPU)component).unlocked = this.unlocked;
        ((CPU)component)._64bit = this._64bit;
        ((CPU)component).integratedGraphics = this.integratedGraphics;
        ((CPU)component).socket = this.socket;

        return component;
    }
}
