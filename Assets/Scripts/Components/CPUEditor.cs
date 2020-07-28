using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CPU))]
public class CPUEditor : Editor
{
    private SerializedProperty architecture, IPC;
    private void OnEnable()
    {
        architecture = serializedObject.FindProperty("architecture");
        IPC = serializedObject.FindProperty("IPC");
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CPU.Architecture architecture = (CPU.Architecture)this.architecture.intValue;

        switch (architecture)
        {
            case CPU.Architecture.K8:
                IPC.intValue = 3;
                break;
            case CPU.Architecture.Zen:
                IPC.intValue = 16;
                break;
            case CPU.Architecture.ZenPlus:
                IPC.intValue = 16;
                break;
            case CPU.Architecture.Zen2:
                IPC.intValue = 32;
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
