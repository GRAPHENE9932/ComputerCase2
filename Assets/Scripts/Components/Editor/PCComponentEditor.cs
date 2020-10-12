using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(PCComponent), true)]
[CanEditMultipleObjects]
public class PCComponentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        //Help box about price.
        if (serializedObject.FindProperty("price").intValue == 0)
            EditorGUILayout.HelpBox("Price is undefined.", MessageType.Warning);

        //Help box about CPU architecture.
        if (serializedObject.targetObject.GetType() == typeof(CPU))
            if (serializedObject.FindProperty("architecture").intValue == 0)
                EditorGUILayout.HelpBox("Architecture undefined", MessageType.Warning);
    }
}
