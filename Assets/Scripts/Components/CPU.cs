using System;
using System.Reflection;
using UnityEngine;

[CreateAssetMenu(menuName = "Components/CPU", fileName = "CPU")]
[Serializable]
public class CPU : PCComponent
{
    public enum Architecture
    {
        K8, Zen, ZenPlus, Zen2
    }

    [Space]
    public int frequency;
    public int cores;
    public int TDP;
    public byte IPC;
    public Architecture architecture;
    public string socket;
    [Space]
    public byte[] additionalRAMTypes;
    public byte maxRAMChannels, RAMType;
    [Space]
    public bool unlocked;
    public bool _64bit;
    public bool integratedGraphics;

    public override string FullProperties
    {
        get
        {
            string result = null;

            result += fullName + ";\n";
            result += $"{LangManager.GetString("socket:")} {socket};\n";
            result += $"{LangManager.GetString("frequency:")} {frequency} MHz;\n";

            if (cores % 10 == 1 && cores % 100 != 11)
            {
                //Ядро
                result += cores + $" {LangManager.GetString("core_0")};\n";
            }
            else if ((cores % 10 >= 2 && cores % 10 <= 4) && (cores % 100 - cores % 10) / 10 != 1)
            {
                //Ядра
                result += cores + $" {LangManager.GetString("core_1")};\n";
            }
            else
            {
                //Ядер
                result += cores + $" {LangManager.GetString("core_2")};\n";
            }

            result += $"TDP: {TDP} W;\n";
            result += $"{LangManager.GetString("unlocked:")} {(LangManager.GetString(unlocked ? "yes" : "no"))};\n";
            result += $"{LangManager.GetString("instruction_set:")} {(_64bit ? "64 bit" : "32 bit")};\n";
            result += $"{LangManager.GetString("integrated_graphics:")} {(LangManager.GetString(integratedGraphics ? "yes" : "no"))};\n";
            result += $"{LangManager.GetString("price:")} {price}$.";

            return result;
        }
    }

    public override string ShortProperties
    {
        get
        {
            string result = null;

            result += fullName + ";\n";
            result += $"{LangManager.GetString("socket:")} {socket};\n";
            result += $"{LangManager.GetString("frequency:")} {frequency} MHz;\n";

            if (cores % 10 == 1 && cores % 100 != 11)
            {
                //Ядро
                result += cores + $" {LangManager.GetString("core_0")};\n";
            }
            else if ((cores % 10 >= 2 && cores % 10 <= 4) && (cores % 100 - cores % 10) / 10 != 1)
            {
                //Ядра
                result += cores + $" {LangManager.GetString("core_1")};\n";
            }
            else
            {
                //Ядер
                result += cores + $" {LangManager.GetString("core_2")};\n";
            }

            result += $"TDP: {TDP} W;\n";
            result += $"{LangManager.GetString("price:")} {price}$.";

            return result;
        }
    }
}
