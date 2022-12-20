using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ProjectViewPlus
{
    /// <summary>
    /// This class draws the PVPWindow. It calls visualize folder on the root folder which causes all files and folders to be displayed.
    /// </summary>
    public class PVPWindow : EditorWindow
    {
        #region Private Fields

        private string[] _categoryNames;
        private Vector2 _scrollPosition;
        private int _selectedCategoryIndex;

        #endregion Private Fields

        #region Properties

        public static string CurrentDirectory { get; private set; }
        public static int IconSize { get; set; }
        public static Rect Position { get; private set; }
        public static PVPDataSO PVPData { get; private set; }
        public static PVPSettingsSO PVPSettings { get; private set; }
        public static bool RepaintFlag { get; set; }

        public static List<LayoutEventAction> LayoutEventActions { get; set; } = new List<LayoutEventAction>();

        #endregion Properties

        private void OnDisable()
        {
            UnsubscribeToEvents();
            PVPData.RemoveNullReferencesAsync();
        }

        private void OnEnable()
        {
            SubscribeToEvents();

            //Get references to PVPData and PVPSettings instance
            CurrentDirectory = PVPPathUtility.GetDirectoryOfScriptableObject(this);
            PVPData = AssetDatabase.LoadAssetAtPath<PVPDataSO>(CurrentDirectory + "/PVPData.asset");
            PVPSettings = AssetDatabase.LoadAssetAtPath<PVPSettingsSO>(CurrentDirectory + "/PVPSettings.asset");

            if (PVPSettings == null)
                CreateNewPVPSettingsInstance();

            if (PVPData == null)
            {
                CreateNewPVPDataInstance();
                SynchronizeData();
            }

            CheckIconSize();
            PVPData.OnBeforeDeserialize();
        }

        private void OnGUI()
        {
            //Changing gui layout elements between the layout event and the repaint event(like drag and dropping files and folders) causes errors. This is a fix for this:

            if (Event.current.type == EventType.Layout && LayoutEventActions.Count > 0)
            {
                foreach (var layoutEventAction in LayoutEventActions)
                {
                    layoutEventAction.Execute();
                }
                LayoutEventActions.Clear();
            }

            Position = position;

            PVPSelection.CheckDeleteKey();

            if (GUI.Button(new Rect(Position.width - 200, 35, 100, 30), "Synchronize"))
            {
                SynchronizeData();
            }

            GUI.skin = PVPSettings.GUISkin;

            if (GUI.Button(new Rect(Position.width - 75, 25, 50, 50), new GUIContent(PVPSettings.SettingsIcon)))
            {
                PVPSettingsWindow.OpenWindow();
            }

            //Start visualizing folders and files

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            PVPData.RootFolder.VisualizeFolder();

            GUILayout.EndScrollView();

            EditorUtility.SetDirty(PVPData);

            //Repaint flag to allow for a more responsive ui
            if (RepaintFlag)
            {
                Repaint();
                RepaintFlag = false;
            }
        }

        [MenuItem("Tools/ProjectView+")]
        private static void OpenOverviewPlusWindow()
        {
            var window = GetWindow<PVPWindow>();
            window.titleContent = new GUIContent("ProjectView+");
            window.minSize = new Vector2(500, 500);
            window.autoRepaintOnSceneChange = false;
        }

        private void CheckIconSize()
        {
            switch (PVPSettings.IconSize)
            {
                case PVPSettingsSO.IconSizes.Small:
                    IconSize = PVPSettings.SmallSize;
                    break;

                case PVPSettingsSO.IconSizes.Normal:
                    IconSize = PVPSettings.NormalSize;
                    break;

                case PVPSettingsSO.IconSizes.Large:
                    IconSize = PVPSettings.LargeSize;
                    break;
            }
        }

        private void CreateNewPVPDataInstance()
        {
            PVPData = CreateInstance<PVPDataSO>();
            AssetDatabase.CreateAsset(PVPData, CurrentDirectory + '\\' + "PVPData.asset");
        }

        private void CreateNewPVPSettingsInstance()
        {
            PVPSettings = CreateInstance<PVPSettingsSO>();
            PVPSettings.SmallSize = 25;
            PVPSettings.NormalSize = 30;
            PVPSettings.LargeSize = 50;
            PVPSettings.FilesPerRow = 1;
            PVPSettings.FolderIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(CurrentDirectory + "/Sprites/Folder.png");
            PVPSettings.FoldoutIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(CurrentDirectory + "/Sprites/FoldoutArrow.png");
            PVPSettings.NormalBackground = AssetDatabase.LoadAssetAtPath<Texture2D>(CurrentDirectory + "/Sprites/CompleteTransparent.png");
            PVPSettings.SelectedBackground = AssetDatabase.LoadAssetAtPath<Texture2D>(CurrentDirectory + "/Sprites/SelectedBackground.png");
            PVPSettings.SettingsIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(CurrentDirectory + "/Sprites/gear.png");
            PVPSettings.GUISkin = AssetDatabase.LoadAssetAtPath<GUISkin>(CurrentDirectory + "/GUISkins/PVPSkin.guiskin");
            AssetDatabase.CreateAsset(PVPSettings, CurrentDirectory + '\\' + "PVPSettings.asset");
        }

        /// <summary>
        /// Synchronizes the files and folders of the ProjectView+ window with all files that exist in the project.
        /// </summary>
        private void SynchronizeData()
        {
            Undo.RecordObject(PVPData, "projectViewPlusDataChanged");
            PVPData.RootFolder = null;
            PVPData.allFolders = new List<PVPFolder>();
            PVPData.allFiles = new List<PVPFile>();
            PVPSelection.allSelectables = new List<ISelectable>();
            PVPData.RootFolder = new PVPFolder("Assets", null, 0); //
        }

        /// <summary>
        /// Whenever assemblys get reloaded make sure the files and folders don't lose their unserialized references
        /// </summary>
        private void OnAfterAssemblyReload()
        {
            PVPData.OnBeforeDeserialize();
        }

        private void SubscribeToEvents()
        {
            PVPEvents.RepaintWindowEvent += Repaint;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
        }

        private void UnsubscribeToEvents()
        {
            PVPEvents.RepaintWindowEvent -= Repaint;
            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
        }

        #region For the future

        private void DrawTabs()
        {
            _selectedCategoryIndex = GUILayout.Toolbar(_selectedCategoryIndex, _categoryNames);
        }

        #region Filters

        /// <summary>
        /// Finds all assets of the specified type T in the assets folder of the project.
        /// </summary>
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

        /// <summary>
        /// Returns a list of types that inherit from the specified type T.
        /// </summary>
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

        #endregion Filters

        #endregion For the future
    }
}