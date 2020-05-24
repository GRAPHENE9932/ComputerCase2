using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Chipsets supported overclock: X370, B350, X300, A300 (AMD) and ... (Intel)
public enum Chipset
{
    Z390, Z370, Z270, Z170, X570, X99, X470, H110, H270, B150, B250, B360, B365, H370, _990X
}

[CreateAssetMenu(menuName = "Components/Motherboard", fileName = "Motherboard")]
public class Motherboard : PCComponent
{
    public string socket;
    public byte RAMType, RAMCount;
    public byte[] busVersions, busMultipliers;
    public bool SLI, crossfire;
    public Chipset chipset;

    public override string Properties
    {
        get
        {
            string result = null;

            result += $"{fullName};\n";
            result += $"Socket: {socket};\n";

            if (RAMType == 1)
            {
                result += "RAM type: DDR;\n";
            }
            else
            {
                result += $"RAM type: DDR{RAMType};\n";
            }

            result += $"RAM count: {RAMCount};\n";

            result += "GPU interfaces: \n";
            string interfaces = null;
            int countOfType = 1;
            for (int i = busVersions.Length - 2; i >= -1; i--)
            {
                if (i >= 0 && busVersions[i] == busVersions[i + 1] && busMultipliers[i] == busMultipliers[i + 1])
                {
                    countOfType++;
                }
                else
                {
                    interfaces = countOfType + $"{countOfType}x PCIe {busVersions[i + 1]}.0 x{busMultipliers[i + 1]}" + interfaces;
                    countOfType = 1;
                }
            }
            result += interfaces;

            result += "SLI: " + (SLI ? "yes;\n" : "no;\n");
            result += "Crossfire: " + (crossfire ? "yes;\n" : "no;\n");
            result += $"Chipset: {chipset.ToString().RemoveChar('_')};\n";
            result += $"Price: {price}$.";

            return result;
        }
    }

}
