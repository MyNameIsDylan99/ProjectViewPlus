using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


public class PVPWindow : EditorWindow
{
    #region Private Fields
    private string[] _categoryNames;
    private int _selectedCategoryIndex;
    private List<List<PVPFile>> _categoryFiles = new List<List<PVPFile>>();
    private Vector2 _scrollPosition;
    #endregion

    #region Properties
    public static PVPDataSO PVPData { get; private set; }
    public static string CurrentPath { get; private set; }
    public static int IconSize { get; private set; }
    public static Rect Position { get; private set; }

    public static bool RepaintFlag {get; set;}


    #endregion

    [MenuItem("Tools/ProjectView+")]
    static void OpenOverviewPlusWindow()
    {
        var window = GetWindow<PVPWindow>();
        window.titleContent = new GUIContent("ProjectView+");
    }

    private void OnEnable()
    {
        SubscribeToEvents();
        CurrentPath = PVPPathUtility.GetPathOfScriptableObject(this);
        PVPData = AssetDatabase.LoadAssetAtPath<PVPDataSO>(CurrentPath + "/PVPData.asset");

        if (PVPData == null)
        {
            CreateNewPVPDataInstance();
            FetchData();
        }

        CheckIconSize();
        PVPData.OnBeforeDeserialize();
        
    }

    private void OnAfterAssemblyReload()
    {
        PVPData.OnBeforeDeserialize();
    }

    private void OnGUI()
    {
        Position = position;
        GUI.skin = PVPData.PVPSettings.GUISkin;
        
        if (GUILayout.Button("Fetch data"))
        {
            FetchData();
        }
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

        try
        {
            PVPData.RootFolder.VisualizeFolder();
        }
        catch (Exception)
        {

        }
        
        GUILayout.EndScrollView();
        EditorUtility.SetDirty(PVPData);

        if (RepaintFlag)
        {
            Repaint();
            RepaintFlag = false;
        }
    }

    private void CheckIconSize()
    {

        switch (PVPData.PVPSettings.IconSize)
        {
            case PVPSettings.IconSizes.Small:
                IconSize = PVPData.PVPSettings.SmallSize;
                break;
            case PVPSettings.IconSizes.Normal:
                IconSize = PVPData.PVPSettings.NormalSize;
                break;
            case PVPSettings.IconSizes.Large:
                IconSize = PVPData.PVPSettings.LargeSize;
                break;
        }
    }

    private void SubscribeToEvents()
    {
        PVPEvents.RepaintWindowEvent += Repaint;
        AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
    }
    
    private void CreateNewPVPDataInstance()
    {
        PVPData = PVPDataSO.CreateInstance<PVPDataSO>();
        AssetDatabase.CreateAsset(PVPData, CurrentPath + "/PVPData.asset");
        PVPData.PVPSettings.FolderIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(CurrentPath + "/Sprites/Folder.png");
        PVPData.PVPSettings.FoldoutIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(CurrentPath + "/Sprites/FoldoutArrow.png");
        PVPData.PVPSettings.NormalBackground = AssetDatabase.LoadAssetAtPath<Texture2D>(CurrentPath + "/Sprites/CompleteTransparent.png");
        PVPData.PVPSettings.SelectedBackground = AssetDatabase.LoadAssetAtPath<Texture2D>(CurrentPath + "/Sprites/SelectedBackground.png");
        PVPData.PVPSettings.GUISkin = AssetDatabase.LoadAssetAtPath<GUISkin>(CurrentPath + "/GUISkins/PVPSkin.guiskin");
    }

    private void FetchData()
    {
        Undo.RecordObject(PVPData, "projectViewPlusDataChanged");
        PVPData.RootFolder = null;
        PVPData.allFolders = new List<PVPFolder>();
        PVPData.allFiles = new List<PVPFile>();
        PVPSelection.allSelectables = new List<ISelectable>();
        PVPData.RootFolder = new PVPFolder("Assets", null, 0); //
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

