using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Components/GPU", fileName = "GPU")]
public class GPU : PCComponent
{
    public int memory, TDP, power;
    public string GPUInterface;
}
