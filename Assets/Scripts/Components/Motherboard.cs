using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Chipset
{
    Z390, Z370, Z270, Z170, X570, X99
}

[CreateAssetMenu(menuName = "Components/Motherboard", fileName = "Motherboard")]
public class Motherboard : PCComponent
{
    public string socket;
    public int RAMType, RAMCount;
    public GPUInterface[] GPUInterfaces;
    public bool SLI, crossfire;
    public Chipset chipset;
}
