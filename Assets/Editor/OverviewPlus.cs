using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// AssetDatabase.FindAssets supports the following syntax:
// 't:type' syntax (e.g 't:Texture2D' will show Texture2D objects)
// 'l:assetlabel' syntax (e.g 'l:architecture' will show assets with AssetLabel 'architecture')
// 'ref[:id]:path' syntax (e.g 'ref:1234' will show objects that references the object with instanceID 1234)
// 'v:versionState' syntax (e.g 'v:modified' will show objects that are modified locally)
// 's:softLockState' syntax (e.g 's:inprogress' will show objects that are modified by anyone (except you))
// 'a:area' syntax (e.g 'a:all' will s search in all assets, 'a:assets' will s search in assets folder only and 'a:packages' will s search in packages folder only)
// 'glob:path' syntax (e.g 'glob:Assets/**/*.{png|PNG}' will show objects in any subfolder with name ending by .png or .PNG)

public class OverviewPlus : EditorWindow
{

    private List<CategorySO> _categories = new List<CategorySO>();
    private string[] _categoryNames;
    private int _selectedCategoryIndex;
    private List<List<UnityFile>> _categoryFiles = new List<List<UnityFile>>();
    private bool _filesLoaded;
    private Vector2 _scrollPosition;

    [MenuItem("Tools/Overview+")]
    static void OpenOverviewPlusWindow()
    {
        var window = GetWindow<OverviewPlus>();
        window.titleContent = new GUIContent("Overview+");
    }

    private void OnEnable()
    {
        FindCategorySOs();
        CategoriesToStringArray();
        GetObjectsForAllCategories();
    }

    private void OnGUI()
    {
        DrawTabs();
        DisplayPopupMenu();
        _scrollPosition =  GUILayout.BeginScrollView(_scrollPosition);
        if (_filesLoaded) {
            GUILayout.BeginVertical();
            foreach (var file in _categoryFiles[_selectedCategoryIndex])
        {
            file.VisualizeFile();
        }
        GUILayout.EndVertical();
    }
        
        GUILayout.FlexibleSpace();
        GUI.skin.button.alignment = TextAnchor.MiddleCenter;
        GUILayout.BeginHorizontal();
        GUILayout.Space(position.width * 0.5f - 50);
        if (GUILayout.Button("Fetch", GUILayout.Width(100), GUILayout.Height(50)))
        {
            GetObjectsForAllCategories();
        }
        GUILayout.EndHorizontal();

        GUILayout.EndScrollView();
    }

    private void DisplayPopupMenu()
    {
        Event current = Event.current;
        Vector2 mousePos = current.mousePosition;
        if (current.type == EventType.ContextClick)
        {
            EditorUtility.DisplayPopupMenu(new Rect(mousePos.x, mousePos.y, 0, 0), "Assets/", null);
            current.Use();
        }
    }

    private void DrawTabs()
    {
        _selectedCategoryIndex = GUILayout.Toolbar(_selectedCategoryIndex, _categoryNames);

    }

    private void FindCategorySOs()
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

        _categories = categories;
    }


    /// <returns>Returns true if any files have been added</returns>
    private void GetObjectsForAllCategories()
    {
        if (_categories == null || _categories.Count <= 0)
        {
            Debug.Log("No categories");
            _filesLoaded = false;
            return;
        }

        for (int i = 0; i < _categories.Count; i++)
        {
            _categoryFiles.Add(GetObjectsForCategory(_categories[i]));
        }
        _filesLoaded = true;
    }

    private List<UnityFile> GetObjectsForCategory(CategorySO category)
    {
        List<UnityFile> files = new List<UnityFile>();
        foreach (var filter in category.Filters)
        {
            string[] guids = AssetDatabase.FindAssets(filter, new string[] { "Assets" });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                UnityFile file = new UnityFile(path);
                files.Add(file);
                Debug.Log("New file added : " + file.GetPath());
            }
        }
        return files;
    }

    private void CategoriesToStringArray()
    {
        string[] array = new string[_categories.Count];

        for (int i = 0; i < _categories.Count; i++)
        {
            array[i] = _categories[i].Name;
        }

        _categoryNames = array;
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
            foreach (Type type in assembly.GetTypes()
            .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
            {
                objects.Add(type);
                Debug.Log(type.Name);
                if (type.Name == "Script")
                    Debug.LogWarning(type.Name);
            }
        }

        return objects;
    }
}

