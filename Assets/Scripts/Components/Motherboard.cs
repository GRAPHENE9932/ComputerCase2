using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Chipset
{
    Z390, Z370, Z270, Z170, X570, X99, X470, H110, H270, B150, B250, B360, B365, H370, _990X
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
