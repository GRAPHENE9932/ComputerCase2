using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Components/CPU", fileName = "CPU")]
public class CPU : PCComponent
{
    public int frequency, cores, TDP, power;
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
}
