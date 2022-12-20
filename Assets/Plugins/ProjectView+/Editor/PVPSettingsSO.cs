using UnityEngine;

namespace ProjectViewPlus
{
    /// <summary>
    /// This is the scriptable object blueprint for the settings.
    /// </summary>
    public class PVPSettingsSO : ScriptableObject
    {
        public enum IconSizes
        {
            Small,
            Normal,
            Large
        }

        [Range(0, 1000)]
        public int SmallSize;

        [Range(0, 1000)]
        public int NormalSize;

        [Range(0, 1000)]
        public int LargeSize;

        [Range(1, 5)]
        public int FilesPerRow;

        public Texture2D FolderIcon;
        public Texture2D FoldoutIcon;
        public Texture2D NormalBackground;
        public Texture2D SelectedBackground;
        public Texture2D SettingsIcon;
        public GUISkin GUISkin;
        public IconSizes IconSize;
    }
}