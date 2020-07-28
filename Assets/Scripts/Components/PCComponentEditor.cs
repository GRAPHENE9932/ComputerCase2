using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(PCComponent), true)]
public class PCComponentEditor : Editor
{
    private static ushort maxId = ushort.MinValue;
    private static List<SerializedProperty> ids = new List<SerializedProperty>();

    private void OnEnable()
    {
        ids.Clear();
        ids.Add(serializedObject.FindProperty("id"));
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        foreach (SerializedProperty id in ids)
            if (id.intValue > maxId)
                maxId = (ushort)id.intValue;

        EditorGUILayout.HelpBox($"Max id: {maxId}", MessageType.Info);
    }
}
