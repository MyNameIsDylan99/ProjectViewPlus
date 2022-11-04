using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

public class ScriptableObjectEditorWindow : EditorWindow
{
    public List<Type> types;
    public List<CharacterSO> allCharacterSOs;
    public List<SerializedObject> serializedObjects = new List<SerializedObject>();

    [MenuItem("Tools/ScriptableObject Editor")]
    static void OpenScriptableObjectEditor()
    {
        var window = GetWindow<ScriptableObjectEditorWindow>();
        window.types = GetListOfTypesThatInheritFromType<ScriptableObject>();
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Find all ScriptableObjects"))
        {
            allCharacterSOs = FindAssetsByType<CharacterSO>();

            foreach (var scriptableObject in allCharacterSOs)
            {
                SerializedObject serializedObject = new SerializedObject(scriptableObject);
                serializedObjects.Add(serializedObject);
            }
        }

        EditorGUILayout.LabelField("All ScriptableObject Types");
        EditorGUILayout.Space(5);
        if (types == null || types.Count<0)
            return;
        foreach (var type in types)
        {
            EditorGUILayout.LabelField(type.Name);
        }

        if (serializedObjects != null && serializedObjects.Count > 0)
        {
            foreach (var serializedObject in serializedObjects)
            {
                foreach (var item in typeof(CharacterSO).GetMembers())
                {
                    var serializedProperty = serializedObject.FindProperty(item.Name);
                    if (serializedProperty != null)
                        EditorGUILayout.PropertyField(serializedProperty);
                }
            }
        }

    }

    public static List<T> FindAssetsByType<T>() where T : UnityEngine.Object
    {
        List<T> assets = new List<T>();
        string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));
        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null)
            {
                assets.Add(asset);
            }
        }
        return assets;
    }

    public static List<Type> GetListOfTypesThatInheritFromType<T>() where T : class
    {
        List<Type> objects = new List<Type>();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Debug.Log(assembly.FullName);
            foreach (Type type in assembly.GetTypes() 
            .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
            {
                objects.Add(type);
            }
        }

        return objects;
    }
}

