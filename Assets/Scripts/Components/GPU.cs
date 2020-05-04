using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GPUInterface
{
    PCIe_4_0_x16, PCIe_4_0_x8, PCIe_3_0_x16, PCIe_3_0_x8, PCIe_3_0_x4, PCIe_3_0_x1, PCIe_2_0_x1, PCIe_4_0_x4, PCIe_4_0_x1, PCIe_2_0_x4, PCIe_1_0_x16,
    PCIe_1_0_x1, PCIe_3_0_x2, PCIe_2_0_x16, PCIe_2_0_x8
}
[CreateAssetMenu(menuName = "Components/GPU", fileName = "GPU")]
public class GPU : PCComponent
{
    public short memory, TDP, power;
    public byte busVersion, busMultiplier;
    public GPUInterface motherboardInterface;

    public override string Properties
    {
        get
        {
            string result = null;

            result += fullName + ";\n";
            result += "Memory: " + memory + " MB;\n";
            result += "TDP: " + TDP + " W;\n";

            result += "Interface: " + GPUInterfaceToString(motherboardInterface);

            result += "Performance: " + power + " GFlops;\n";
            result += "Price: " + price + "$.";

            return result;
        }
    }

    public static string GPUInterfaceToString(GPUInterface i)
    {
        switch (i)
        {
            case GPUInterface.PCIe_1_0_x1:
                return "PCIe 1.0 x1;\n";
            case GPUInterface.PCIe_1_0_x16:
                return "PCIe 1.0 x16;\n";
            case GPUInterface.PCIe_2_0_x1:
                return "PCIe 2.0 x1;\n";
            case GPUInterface.PCIe_2_0_x16:
                return "PCIe 2.0 x16;\n";
            case GPUInterface.PCIe_2_0_x4:
                return "PCIe 2.0 x4;\n";
            case GPUInterface.PCIe_2_0_x8:
                return "PCIe 2.0 x8;\n";
            case GPUInterface.PCIe_3_0_x1:
                return "PCIe 3.0 x1;\n";
            case GPUInterface.PCIe_3_0_x16:
                return "PCIe 3.0 x16;\n";
            case GPUInterface.PCIe_3_0_x2:
                return "PCIe 3.0 x2;\n";
            case GPUInterface.PCIe_3_0_x4:
                return "PCIe 3.0 x4;\n";
            case GPUInterface.PCIe_3_0_x8:
                return "PCIe 3.0 x8;\n";
            case GPUInterface.PCIe_4_0_x1:
                return "PCIe 4.0 x1;\n";
            case GPUInterface.PCIe_4_0_x16:
                return "PCIe 4.0 x16;\n";
            case GPUInterface.PCIe_4_0_x4:
                return "PCIe 4.0 x4;\n";
            case GPUInterface.PCIe_4_0_x8:
                return "PCIe 4.0 x8;\n";
            default:
                return "Error! Code 1.";
        }
    }
}