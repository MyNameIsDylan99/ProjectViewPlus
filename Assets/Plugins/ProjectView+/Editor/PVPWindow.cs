using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


public class PVPWindow : EditorWindow
{
    private PVPDataSO pvpData;

    private string[] _categoryNames;
    private int _selectedCategoryIndex;
    private List<List<PVPFile>> _categoryFiles = new List<List<PVPFile>>();
    private Vector2 _scrollPosition;
    public static string CurrentPath;
    public static int IconSize;

    [MenuItem("Tools/ProjectView+")]
    static void OpenOverviewPlusWindow()
    {
        var window = GetWindow<PVPWindow>();
        window.titleContent = new GUIContent("ProjectView+");
    }

    private void OnEnable()
    {
        SubscribeToEvents();
        CurrentPath = PVPRelativePathUtility.GetPathOfScriptableObject(this);
        pvpData = AssetDatabase.LoadAssetAtPath<PVPDataSO>(CurrentPath + "/PVPData.asset");

        if (pvpData == null)
        {
            pvpData = PVPDataSO.CreateInstance<PVPDataSO>();
            AssetDatabase.CreateAsset(pvpData, CurrentPath + "/PVPData.asset");
            pvpData.PVPSettings.FolderIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(CurrentPath + "/Sprites/Folder.png");
            pvpData.PVPSettings.FoldoutIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(CurrentPath + "/Sprites/FoldoutArrow.png");
            pvpData.PVPSettings.NormalBackground = AssetDatabase.LoadAssetAtPath<Texture2D>(CurrentPath + "/Sprites/CompleteTransparent.png");
            pvpData.PVPSettings.SelectedBackground = AssetDatabase.LoadAssetAtPath<Texture2D>(CurrentPath + "/Sprites/SelectedBackground.png");
            pvpData.PVPSettings.GUISkin = AssetDatabase.LoadAssetAtPath<GUISkin>(CurrentPath + "/GUISkins/PVPSkin.guiskin");
        }

        Undo.RecordObject(pvpData, "projectViewPlusDataChanged");

        if (pvpData.RootFolder == null)
        {
            pvpData.RootFolder = new PVPFolder("Assets", null, 0);
        }
        else
        {
            pvpData.OnBeforeDeserialize();
        }

        switch (pvpData.PVPSettings.IconSize)
        {
            case PVPSettings.IconSizes.Small:
                IconSize = pvpData.PVPSettings.SmallSize;
                break;
            case PVPSettings.IconSizes.Normal:
                IconSize = pvpData.PVPSettings.NormalSize;
                break;
            case PVPSettings.IconSizes.Large:
                IconSize = pvpData.PVPSettings.LargeSize;
                break;
        }

        AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
    }

    private void OnAfterAssemblyReload()
    {
        pvpData.OnBeforeDeserialize();
    }

    private void OnGUI()
    {

        GUI.skin = pvpData.PVPSettings.GUISkin;

        if (GUILayout.Button("Fetch data"))
        {
            FetchData();
        }
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
        pvpData.RootFolder.VisualizeFolder();
        DisplayPopupMenu();
        GUILayout.EndScrollView();

            
    }

    private void SubscribeToEvents()
    {
        PVPEvents.RepaintWindowEvent += Repaint;
    }
    

    private void FetchData()
    {
        Undo.RecordObject(pvpData, "projectViewPlusDataChanged");
        pvpData.RootFolder = null;
        pvpData.allFolders = new List<PVPFolder>();
        pvpData.allFiles = new List<PVPFile>();
        pvpData.RootFolder = new PVPFolder("Assets", null, 0);
    }

    public void DropAreaGUI()
    {

        Event evt = Event.current;
        Rect drop_area = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(drop_area, "Add Trigger");

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!drop_area.Contains(evt.mousePosition))
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (UnityEngine.Object dragged_object in DragAndDrop.objectReferences)
                    {
                        // Do On Drag Stuff here
                    }
                }
                break;
        }
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

    //private void FindCategorySOs()
    //{
    //    List<CategorySO> categories = new List<CategorySO>();
    //    var guids = AssetDatabase.FindAssets("t:CategorySO");

    //    if (guids != null)
    //    {
    //        foreach (var guid in guids)
    //        {
    //            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
    //            categories.Add(AssetDatabase.LoadAssetAtPath<CategorySO>(assetPath));
    //        }
    //    }

    //    _categories = categories;
    //}


    /// <returns>Returns true if any files have been added</returns>
    //private void GetObjectsForAllCategories()
    //{
    //    if (_categories == null || _categories.Count <= 0)
    //    {
    //        Debug.Log("No categories");
    //        _filesLoaded = false;
    //        return;
    //    }

    //    for (int i = 0; i < _categories.Count; i++)
    //    {
    //        _categoryFiles.Add(GetObjectsForCategory(_categories[i]));
    //    }
    //    _filesLoaded = true;
    //}



    //private void CategoriesToStringArray()
    //{
    //    string[] array = new string[_categories.Count];

    //    for (int i = 0; i < _categories.Count; i++)
    //    {
    //        array[i] = _categories[i].Name;
    //    }

    //    _categoryNames = array;
    //}



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

