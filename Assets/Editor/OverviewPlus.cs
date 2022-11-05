using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

public class OverviewPlus : EditorWindow
{
    //public List<Type> types;
    //public List<CharacterSO> allCharacterSOs;
    //public List<SerializedObject> serializedObjects = new List<SerializedObject>();


    private List<CategorySO> _categories = new List<CategorySO>();
    private string[] _categoryNames;
    private CategoryTypes _selectedCategory;

    enum CategoryTypes
    {
        Animations,
        Images,
        Materials,
        Prefabs,
        ScriptableObjects,
        Scripts,
        SoundFiles
    }

    

    [MenuItem("Tools/Overview+")]
    static void OpenOverviewPlusWindow()
    {
        var window = GetWindow<OverviewPlus>();
        window.titleContent = new GUIContent("Overview+");
        window._categories = FindCategorySOs();
        window._categoryNames = window.CategoriesToStringArray();
        //window.types = GetListOfTypesThatInheritFromType<ScriptableObject>();
    }

    private void OnEnable()
    {
        
    }

    private void OnGUI()
    {
        DrawTabs();
        //if (GUILayout.Button("Find all ScriptableObjects"))
        //{
        //    allCharacterSOs = FindAssetsByType<CharacterSO>();

        //    foreach (var scriptableObject in allCharacterSOs)
        //    {
        //        SerializedObject serializedObject = new SerializedObject(scriptableObject);
        //        serializedObjects.Add(serializedObject);
        //    }
        //}

        //EditorGUILayout.LabelField("All ScriptableObject Types");
        //EditorGUILayout.Space(5);
        //if (types == null || types.Count<0)
        //    return;
        //foreach (var type in types)
        //{
        //    EditorGUILayout.LabelField(type.Name);
        //}

        //if (serializedObjects != null && serializedObjects.Count > 0)
        //{
        //    foreach (var serializedObject in serializedObjects)
        //    {
        //        foreach (var item in typeof(CharacterSO).GetMembers())
        //        {
        //            var serializedProperty = serializedObject.FindProperty(item.Name);
        //            if (serializedProperty != null)
        //                EditorGUILayout.PropertyField(serializedProperty);
        //        }
        //    }
        //}

    }


    // AssetDatabase.FindAssets supports the following syntax:
    // 't:type' syntax (e.g 't:Texture2D' will show Texture2D objects)
    // 'l:assetlabel' syntax (e.g 'l:architecture' will show assets with AssetLabel 'architecture')
    // 'ref[:id]:path' syntax (e.g 'ref:1234' will show objects that references the object with instanceID 1234)
    // 'v:versionState' syntax (e.g 'v:modified' will show objects that are modified locally)
    // 's:softLockState' syntax (e.g 's:inprogress' will show objects that are modified by anyone (except you))
    // 'a:area' syntax (e.g 'a:all' will s search in all assets, 'a:assets' will s search in assets folder only and 'a:packages' will s search in packages folder only)
    // 'glob:path' syntax (e.g 'glob:Assets/**/*.{png|PNG}' will show objects in any subfolder with name ending by .png or .PNG)

    private void DrawTabs()
    {
        var index = (int)_selectedCategory;
        index = GUILayout.Toolbar(index, _categoryNames);
        _selectedCategory = (CategoryTypes)index;
    }

    private static List<CategorySO> FindCategorySOs()
    {
        List<CategorySO> categories = new List<CategorySO>();
        var guids = AssetDatabase.FindAssets("t:CategorySO");

        if (guids != null)
        {
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                categories.Add(AssetDatabase.LoadAssetAtPath<CategorySO>(assetPath));
            }
        }

        return categories;
    }

    private string[] CategoriesToStringArray()
    {
        string[] array = new string[_categories.Count];

        for (int i = 0; i < _categories.Count; i++)
        {
            array[i] = _categories[i].Name;
        }

        return array;
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

