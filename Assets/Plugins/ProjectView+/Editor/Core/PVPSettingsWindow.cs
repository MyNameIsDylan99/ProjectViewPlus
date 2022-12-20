using UnityEditor;
using UnityEngine;

namespace ProjectViewPlus
{
    public class PVPSettingsWindow : EditorWindow
    {
        private static SerializedObject pvpSettings;

        private SerializedProperty filesPerRowProp;
        private SerializedProperty folderIconProp;
        private SerializedProperty foldoutIconProp;
        private SerializedProperty guiSkinProp;
        private SerializedProperty iconSizeProp;
        private SerializedProperty largeSizeProp;
        private SerializedProperty normalBackgroundProp;
        private SerializedProperty normalSizeProp;
        private SerializedProperty selectedBackgroundProp;
        private SerializedProperty settingsIconProp;
        private SerializedProperty smallSizeProp;

        /// <summary>
        /// Opens the PVP settings window.
        /// </summary>
        public static void OpenWindow()
        {
            var window = GetWindow<PVPSettingsWindow>();
            window.titleContent = new GUIContent("PVP Settings");
        }

        private void OnDisable()
        {
            AssemblyReloadEvents.afterAssemblyReload -= InitializePVPSettingsSO;
        }

        private void OnEnable()
        {
            AssemblyReloadEvents.afterAssemblyReload += InitializePVPSettingsSO;

            InitializePVPSettingsSO();
        }

        private void OnGUI()
        {
            pvpSettings.Update();

            EditorGUILayout.PropertyField(smallSizeProp, new GUIContent("Small Size"));
            EditorGUILayout.PropertyField(normalSizeProp, new GUIContent("Normal Size"));
            EditorGUILayout.PropertyField(largeSizeProp, new GUIContent("Large Size"));
            EditorGUILayout.PropertyField(filesPerRowProp, new GUIContent("Files Per Row"));
            EditorGUILayout.PropertyField(folderIconProp, new GUIContent("Folder Icon"));
            EditorGUILayout.PropertyField(foldoutIconProp, new GUIContent("Foldout Icon"));
            EditorGUILayout.PropertyField(settingsIconProp, new GUIContent("Settings Icon"));
            EditorGUILayout.PropertyField(normalBackgroundProp, new GUIContent("Normal Background"));
            EditorGUILayout.PropertyField(selectedBackgroundProp, new GUIContent("Selected Background"));
            EditorGUILayout.PropertyField(guiSkinProp, new GUIContent("GUI Skin"));
            EditorGUILayout.PropertyField(iconSizeProp, new GUIContent("Icon Size"));

            if (pvpSettings.hasModifiedProperties)
            {
                PVPEvents.InvokeRepaintWindowEvent();
                pvpSettings.ApplyModifiedProperties();
                CheckIconSize();
            }
        }

        private void CheckIconSize()
        {
            switch (PVPWindow.PVPSettings.IconSize)
            {
                case PVPSettingsSO.IconSizes.Small:
                    PVPWindow.IconSize = PVPWindow.PVPSettings.SmallSize;
                    break;

                case PVPSettingsSO.IconSizes.Normal:
                    PVPWindow.IconSize = PVPWindow.PVPSettings.NormalSize;
                    break;

                case PVPSettingsSO.IconSizes.Large:
                    PVPWindow.IconSize = PVPWindow.PVPSettings.LargeSize;
                    break;
            }
        }

        private void InitializePVPSettingsSO()
        {
            pvpSettings = new SerializedObject(PVPWindow.PVPSettings);
            smallSizeProp = pvpSettings.FindProperty("SmallSize");
            normalSizeProp = pvpSettings.FindProperty("NormalSize");
            largeSizeProp = pvpSettings.FindProperty("LargeSize");
            filesPerRowProp = pvpSettings.FindProperty("FilesPerRow");
            folderIconProp = pvpSettings.FindProperty("FolderIcon");
            foldoutIconProp = pvpSettings.FindProperty("FoldoutIcon");
            normalBackgroundProp = pvpSettings.FindProperty("NormalBackground");
            selectedBackgroundProp = pvpSettings.FindProperty("SelectedBackground");
            settingsIconProp = pvpSettings.FindProperty("SettingsIcon");
            guiSkinProp = pvpSettings.FindProperty("GUISkin");
            iconSizeProp = pvpSettings.FindProperty("IconSize");
        }
    }
}