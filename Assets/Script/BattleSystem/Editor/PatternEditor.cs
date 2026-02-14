#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using BattleSystem;
using Effects;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Pattern))]
public class PatternEditor : Editor
{
  
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawPatternGrid();
        GUILayout.Space(10);
        DrawEffectsList();

        serializedObject.ApplyModifiedProperties();
    }

    // =========================
    // 3x3 GRID
    // =========================
    private void DrawPatternGrid()
    {
        var cellsProp = serializedObject.FindProperty("cells");

        GUILayout.Label("Pattern (3x3)", EditorStyles.boldLabel);

        for (int y = 0; y < 3; y++)
        {
            GUILayout.BeginHorizontal();
            for (int x = 0; x < 3; x++)
            {
                int index = y * 3 + x;
                var cellProp = cellsProp.GetArrayElementAtIndex(index);

                var colorEnum = (CubeColor)cellProp.enumValueIndex;
                GUI.backgroundColor = colorEnum.ToUnityColor();

                if (GUILayout.Button("", GUILayout.Width(40), GUILayout.Height(40)))
                {
                    cellProp.enumValueIndex =
                        (cellProp.enumValueIndex + 1) % cellProp.enumNames.Length;
                }
            }
            GUILayout.EndHorizontal();
        }

        GUI.backgroundColor = Color.black;
    }

    // =========================
    // EFFECTS
    // =========================
    private void DrawEffectsList()
    {
        var effectsProp = serializedObject.FindProperty("effects");

        GUILayout.Label("Effects", EditorStyles.boldLabel);

        for (int i = 0; i < effectsProp.arraySize; i++)
        {
            var element = effectsProp.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginVertical("box");

            string typeName = GetNiceTypeName(element);
            EditorGUILayout.LabelField(typeName, EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(element, true);

            if (GUILayout.Button("Remove"))
            {
                effectsProp.DeleteArrayElementAtIndex(i);
                break;
            }

            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("Add Effect"))
        {
            ShowAddEffectMenu(effectsProp);
        }
    }

    private void ShowAddEffectMenu(SerializedProperty effectsProp)
    {
        var menu = new GenericMenu();

        foreach (var type in GetAllEffectTypes())
        {
            menu.AddItem(
                new GUIContent(type.Name),
                false,
                () =>
                {
                    int index = effectsProp.arraySize;
                    effectsProp.InsertArrayElementAtIndex(index);

                    var element = effectsProp.GetArrayElementAtIndex(index);
                    element.managedReferenceValue = Activator.CreateInstance(type);

                    serializedObject.ApplyModifiedProperties();
                });
        }

        menu.ShowAsContext();
    }

    private static IEnumerable<Type> GetAllEffectTypes()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t =>
                t.IsSubclassOf(typeof(Effect)) &&
                !t.IsAbstract);
    }

    private static string GetNiceTypeName(SerializedProperty prop)
    {
        if (string.IsNullOrEmpty(prop.managedReferenceFullTypename))
            return "None";

        var parts = prop.managedReferenceFullTypename.Split(' ');
        return parts.Length > 1
            ? parts[1].Split('.').Last()
            : prop.managedReferenceFullTypename;
    }
}
#endif