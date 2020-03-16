using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Components/CPU", fileName = "CPU")]
public class CPU : PCComponent
{
    public int frequency, cores, TDP, power;
    public string socket;
}
