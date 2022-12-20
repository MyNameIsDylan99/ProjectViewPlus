using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectViewPlus
{
    /// <summary>
    /// A struct that holds indeces for the file itself and the parentFolder.
    /// </summary>
    /// <remarks>
    /// This struct is needed because folders reference themselves which would cause recursive serialization.
    /// </remarks>
    [Serializable]
    public struct FileSerializationInfo
    {
        public int fileIndex;
        public int parentFolderIndex;
    }

    /// <summary>
    /// A struct that holds indeces for the folder itself, the parentFolder, childFolder and childFiles for a specific folder.
    /// </summary>
    /// <remarks>
    /// This struct is needed because folders reference themselves which would cause recursive serialization.
    /// </remarks>
    [Serializable]
    public struct FolderSerializationInfo
    {
        public List<int> childFileIndeces;
        public List<int> childFolderIndeces;
        public int folderIndex;
        public int parentFolderIndex;
    }

    /// <summary>
    /// Stores all files and folders in a list and has extra functionality to counteract recursive serialization.
    /// </summary>
    public class PVPDataSO : ScriptableObject
    {
        public List<PVPFile> allFiles = new List<PVPFile>();
        public List<PVPFolder> allFolders = new List<PVPFolder>();
        public PVPFolder RootFolder;

        /// <summary>
        ///
        /// </summary>
        public void OnBeforeDeserialize()
        {
            PVPSelection.allSelectables = new List<ISelectable>();

            //Add files to the non-serialized allSelectables list and a reference to the files parent folder.

            foreach (var file in allFiles)
            {
                file.ParentFolder = allFolders[file.FileSerializationInfo.parentFolderIndex];
                PVPSelection.allSelectables.Add(file);
                file.IsSelected = false;
            }

            //Add child folders to folder

            foreach (var folder in allFolders)
            {
                PVPSelection.allSelectables.Add(folder);
                folder.IsSelected = false;
                var childFolders = new List<PVPFolder>();

                if (folder.SerializationInfo.childFolderIndeces != null)
                {
                    foreach (var childFolderIndex in folder.SerializationInfo.childFolderIndeces)
                    {
                        childFolders.Add(allFolders[childFolderIndex]);
                    }
                }

                folder.ChildFolders = childFolders;

                //Add child files to folder

                var childFiles = new List<PVPFile>();
                if (folder.SerializationInfo.childFileIndeces != null)
                {
                    foreach (var childFileIndex in folder.SerializationInfo.childFileIndeces)
                    {
                        childFiles.Add(allFiles[childFileIndex]);
                    }
                }

                folder.ChildFiles = childFiles;

                //Add parent folder reference to the folder and check if it is root folder.
                if (!folder.IsRootFolder())
                {
                    folder.ParentFolder = allFolders[folder.SerializationInfo.parentFolderIndex];
                }
                else
                {
                    RootFolder = folder;
                }

                folder.ParentFolder = allFolders[folder.SerializationInfo.parentFolderIndex];
            }

            //Sort selectables list and give it to PVPSelection

            var sortedSelectables = new List<ISelectable>();
            for (int i = 0; i < PVPSelection.allSelectables.Count; i++)
            {
                ISelectable selectable = PVPSelection.allSelectables.Where<ISelectable>(x => x.SelectableIndex == i).FirstOrDefault();
                sortedSelectables.Add(selectable);
            }
            PVPSelection.allSelectables = sortedSelectables;
        }

        /// <summary>
        /// Removes file from allFiles list and updates all indeces that have been shifted due to the removal.
        /// </summary>
        public void RemoveFile(PVPFile file)
        {
            allFiles.Remove(file);
            for (int i = 0; i < allFiles.Count; i++)
            {
                allFiles[i].ParentFolder.SerializationInfo.childFileIndeces.Remove(allFiles[i].FileSerializationInfo.fileIndex);
                allFiles[i].FileSerializationInfo.fileIndex = i;
                allFiles[i].ParentFolder.SerializationInfo.childFileIndeces.Add(i);
            }
        }

        /// <summary>
        /// Removes folder from allFolders list and updates all indeces that have been shifted due to the removal.
        /// </summary>
        public void RemoveFolder(PVPFolder folder)
        {
            allFolders.Remove(folder);
            for (int i = 0; i < allFolders.Count; i++)
            {
                if (allFolders[i].SerializationInfo.childFolderIndeces != null && allFolders[i].SerializationInfo.childFolderIndeces.Contains(folder.SerializationInfo.folderIndex))
                {
                    allFolders[i].ParentFolder.SerializationInfo.childFolderIndeces.Remove(allFolders[i].SerializationInfo.folderIndex);
                    allFolders[i].SerializationInfo.folderIndex = i;
                    allFolders[i].ParentFolder.SerializationInfo.childFolderIndeces.Add(i);
                }
                foreach (var childFolder in allFolders[i].ChildFolders)
                {
                    childFolder.SerializationInfo.parentFolderIndex = i;
                }
            }
        }
    }
}