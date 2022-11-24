using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[Serializable]
public class PVPFolder
{
    private string folderPath;
    private string folderName;

    private static PVPDataSO pvpData;

    public FolderSerializationInfo SerializationInfo;

    [NonSerialized]
    private List<PVPFolder> childFolders = new List<PVPFolder>();
    [NonSerialized]
    private PVPFolder parentFolder;
    private List<PVPFile> child_files;
    private PVPFile[,] groupedFiles;


    private GUIContent folderContent;

    private Texture2D folderIcon;

    UnityEngine.Object folderobj;

    private bool fold = false;
    private int depth;

    private bool childFoldersAreTabs;
    public bool ChildFoldersAreTabs { get => childFoldersAreTabs; private set => childFoldersAreTabs =value; }
    public List<PVPFolder> ChildFolders { get => childFolders; set => childFolders = value; }

    public PVPFolder ParentFolder { get { return parentFolder; } set { parentFolder = value; } }

    string[] filters;

    private Rect position;

    public PVPFolder(string folderPath, PVPFolder parentFolder,int depth, Rect position)
    {
        #region Serialization
        if (pvpData == null) { 
        pvpData = AssetDatabase.LoadAssetAtPath<PVPDataSO>("Assets/Editor/PVPData.asset");
        }

        pvpData.allFolders.Add(this);
        SerializationInfo.folderIndex = pvpData.allFolders.Count - 1;

        if (parentFolder == null) {
            pvpData.RootFolder = this;
        }
        else
        {
            SerializationInfo.parentFolderIndex = parentFolder.SerializationInfo.folderIndex;
            if (parentFolder.SerializationInfo.childFolderIndeces == null)
            {
                parentFolder.SerializationInfo.childFolderIndeces = new List<int>();
            }
            parentFolder.SerializationInfo.childFolderIndeces.Add(SerializationInfo.folderIndex);
        }

        #endregion

        this.folderPath = folderPath;
        this.depth = depth;
        this.position = position;//Position is a variable of the EditorWindow.


        ChildFolders = FindChildFolders();
        child_files = FindChildFiles();
       
        //Create the object by giving its path. Then get the assetpreview.
       folderobj = AssetDatabase.LoadAssetAtPath(this.folderPath,typeof(UnityEngine.Object));
        folderIcon = AssetPreview.GetMiniThumbnail(folderobj);


        //Assets/New Folder-> folderName:New Folder
        string[] splitPath = this.folderPath.Split('\\');
        folderName = splitPath[splitPath.Length - 1];

        folderContent = new GUIContent(folderName,folderIcon,folderPath);
    }

    public void VisualizeFolder()
    {
        GUILayout.BeginVertical();

        //Do this to give horizontal space
        GUILayout.BeginHorizontal();
        GUILayout.Space(15*depth);
        fold = EditorGUILayout.Foldout(fold,folderContent,true);
        GUILayout.EndHorizontal();


        if (fold)
        {
            VisualizeChildFiles();
            foreach (var VARIABLE in ChildFolders)
                VARIABLE.VisualizeFolder();

        }
        
        GUILayout.EndVertical();
    }

    private List<PVPFolder> FindChildFolders()
    {
        //GetDirectories will return all the subfolders in the given path.
        string[] dirs = Directory.GetDirectories(folderPath);
        List<PVPFolder> folders = new List<PVPFolder>();
        foreach (var directory in dirs)
        {
            //Turn all directories into our 'UnityFolder' Object.
            PVPFolder newfolder = new PVPFolder(directory,this,depth+1,position);
            folders.Add(newfolder);
        }
        return folders;
    }

    private List<PVPFile> FindChildFiles()
    {
        //GetFiles is similar but returns all the files under the path(obviously)
        string[] fileNames = Directory.GetFiles(folderPath);
        List<PVPFile> files = new List<PVPFile>();
        foreach (var file in fileNames)
        {
            PVPFile newfile = new PVPFile(file,this);
            //Pass meta files.
            if (newfile.GetExtension().Equals("meta"))
                continue;
            files.Add(newfile);
        }

        return files;

    }

    private PVPFile[,] GroupChildFiles(List<PVPFile> files)
    {
        //This method groups files by rows of 3. You can edit this
        //to change visuals.
        int size = files.Count;
        int rows = (size / 3)+1;
        PVPFile[,] groupedFiles = new PVPFile[rows,3];
        int index = 0;
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < 3; j++)
                if(i*3+j<=size-1)
                    groupedFiles[i, j] = files[index++];
       
        return groupedFiles;
    }

    private void VisualizeChildFiles()
    {
        int size = child_files.Count;
        int rows = (size / 3)+1;
        groupedFiles = GroupChildFiles(child_files);
        int i = 0, j = 0;
        for (i = 0; i < rows; i++)
        {
            
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();

            for (j = 0; j < 3; j++)
            {
                if (i * 3 + j <= size - 1)
                    groupedFiles[i, j].VisualizeFile();
                
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();


        }

    }

    private void AddFilesFromFilter()
    {
        List<PVPFile> files = new List<PVPFile>();
        foreach (var filter in filters)
        {
            string[] guids = AssetDatabase.FindAssets(filter, new string[] { "Assets" });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                PVPFile file = new PVPFile(path,this);

                bool userWantsToMoveFile = false;
                if (!file.IsChildOfRootFolder())
                {
                    userWantsToMoveFile = EditorUtility.DisplayDialog("File conflict", $"File {file.GetName()} is currently in this path: {file.GetPath()}. Do you want to move the file to {path}?", "Yes", "No");
                }

                if (!child_files.Contains(file)&&userWantsToMoveFile)
                {
                    child_files.Add(file);
                }
                Debug.Log("New file added : " + file.GetPath());
            }
        }
        
    }

    public string GetName()
    {
        return folderPath;
    }

    public int GetDepth()
    {
        return depth;
    }

    public bool IsRootFolder()
    {
        if(parentFolder == null)
        {
            return true;
        }
        return false;
    }
}
