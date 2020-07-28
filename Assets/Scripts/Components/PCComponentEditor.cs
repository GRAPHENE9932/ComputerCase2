using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(PCComponent), true)]
[CanEditMultipleObjects]
public class PCComponentEditor : Editor
{
    private static ushort maxId = ushort.MinValue;
    private static readonly List<SerializedProperty> ids = new List<SerializedProperty>();

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

        //Help box about max id.
        EditorGUILayout.HelpBox($"Max id: {maxId}", MessageType.Info);

        //Help box about price.
        if (serializedObject.FindProperty("price").intValue == 0)
            EditorGUILayout.HelpBox("Price is undefined.", MessageType.Warning);

        //Help box about CPU architecture.
        if (serializedObject.targetObject.GetType() == typeof(CPU))
            if (serializedObject.FindProperty("architecture").intValue == 0)
                EditorGUILayout.HelpBox("Architecture undefined", MessageType.Warning);
    }
}
