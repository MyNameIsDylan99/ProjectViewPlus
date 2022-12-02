using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Events;

public class PVPDataSO : ScriptableObject
{
    public List<PVPFolder> allFolders = new List<PVPFolder>();
    public List<PVPFile> allFiles = new List<PVPFile>();
    public PVPFolder RootFolder;
    public PVPSettings PVPSettings;

    public void OnBeforeDeserialize()
    {
        foreach (var file in allFiles)
        {
            file.ParentFolder = allFolders[file.FileSerializationInfo.parentFolderIndex];
        }

        foreach (var folder in allFolders)
        {
            var childFolders = new List<PVPFolder>();

            if (folder.SerializationInfo.childFolderIndeces != null)
            {

                foreach (var childFolderIndex in folder.SerializationInfo.childFolderIndeces)
                {
                    childFolders.Add(allFolders[childFolderIndex]);
                }
            }


            folder.ChildFolders = childFolders;

            var childFiles = new List<PVPFile>();
            if (folder.SerializationInfo.childFileIndeces != null)
            {
                foreach (var childFileIndex in folder.SerializationInfo.childFileIndeces)
                {
                    childFiles.Add(allFiles[childFileIndex]);
                }
            }
            folder.ChildFiles = childFiles;

            if (!folder.IsRootFolder())
            {
                folder.ParentFolder = allFolders[folder.SerializationInfo.parentFolderIndex];
            }
            else
            {
                RootFolder = folder;
            }
            
        }
    }

}

[Serializable]
public struct FolderSerializationInfo
{
    public int parentFolderIndex;
    public int folderIndex;
    public List<int> childFolderIndeces;
    public List<int> childFileIndeces;

}

[Serializable]
public struct FileSerializationInfo
{
    public int fileIndex;
    public int parentFolderIndex;
}

[Serializable]
public struct PVPSettings
{
    public enum IconSizes
    {
        Small,
        Normal,
        Large
    }

    [Range(0,1000)]
    public int SmallSize;
    [Range(0, 1000)]
    public int NormalSize;
    [Range(0, 1000)]
    public int LargeSize;
    [Range(1,5)]
    public int FilesPerRow;
    public Texture2D FolderIcon;
    public Texture2D FoldoutIcon;
    public Texture2D NormalBackground;
    public Texture2D SelectedBackground;
    public GUISkin GUISkin;
    public IconSizes IconSize;



}

