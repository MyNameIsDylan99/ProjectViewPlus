using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[CreateAssetMenu(fileName = "PVPData", menuName = "PVPData")]
public class PVPDataSO : ScriptableObject
{
    public List<PVPFolder> allFolders = new List<PVPFolder>();
    public List<PVPFile> allFiles = new List<PVPFile>();
    public PVPFolder RootFolder;

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
                    Debug.Log($"Added child folder {allFolders[childFolderIndex].GetName()} to {folder.GetName()}");
                    childFolders.Add(allFolders[childFolderIndex]);
                }
            }

            
            folder.ChildFolders = childFolders;
            Debug.Log($"Folder{folder.GetName()} has {folder.ChildFolders.Count} childs");
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

}

[Serializable]
public struct FileSerializationInfo
{
    public int fileIndex;
    public int parentFolderIndex;
}