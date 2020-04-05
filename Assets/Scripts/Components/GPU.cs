using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GPUInterface
{
    PCIe_3_0_x16
}
[CreateAssetMenu(menuName = "Components/GPU", fileName = "GPU")]
public class GPU : PCComponent
{
    public int memory, TDP, power;
    public GPUInterface motherboardInterface;
}
