using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Components/Motherboard", fileName = "Motherboard")]
public class Motherboard : PCComponent
{
    public string socket;
    public int RAMType, RAMCount;
    public string[] GPUInterfaces;
}
