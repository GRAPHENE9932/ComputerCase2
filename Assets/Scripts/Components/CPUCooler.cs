using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Components/CPU cooler", fileName = "CPU cooler")]
[Serializable]
public class CPUCooler : ScriptableObject
{
    public ushort power;
    public uint price;
    public Sprite image;
    public byte level;

    public string Name
    {
        get
        {
            return LangManager.GetString($"cpu_cooler_{level}");
        }
    }
}
