using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GPUInterface
{
    PCIe_4_0_x16, PCIe_4_0_x8, PCIe_3_0_x16, PCIe_3_0_x8, PCIe_3_0_x4, PCIe_3_0_x1, PCIe_2_0_x1, PCIe_4_0_x4, PCIe_4_0_x1, PCIe_2_0_x4, PCIe_1_0_x16, PCIe_1_0_x1, PCIe_3_0_x2, 
    PCIe_2_0_x16, PCIe_2_0_x8
}
[CreateAssetMenu(menuName = "Components/GPU", fileName = "GPU")]
public class GPU : PCComponent
{
    public int memory, TDP, power;
    public GPUInterface motherboardInterface;
}
