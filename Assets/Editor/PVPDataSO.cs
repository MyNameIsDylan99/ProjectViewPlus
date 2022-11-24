using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using static PVPDataSO;

[CreateAssetMenu(fileName = "PVPData", menuName = "PVPData")]
public class PVPDataSO : ScriptableObject, ISerializationCallbackReceiver
{
    public List<PVPFolder> allFolders = new List<PVPFolder>();
    public List<PVPFile> allFiles = new List<PVPFile>();
    public PVPFolder RootFolder;

    public void OnBeforeSerialize()
    {

    }

    public void OnAfterDeserialize()
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
            folder.ParentFolder = allFolders[folder.SerializationInfo.parentFolderIndex];
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