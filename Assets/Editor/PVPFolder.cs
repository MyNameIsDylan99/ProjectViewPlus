using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[Serializable]
public class PVPFolder
{
    [SerializeField]
    private string folderPath;
    [SerializeField]
    private string folderName;
    [SerializeField]
    private PVPDataSO pvpData;

    public FolderSerializationInfo SerializationInfo;

    [NonSerialized]
    private List<PVPFolder> childFolders;
    [NonSerialized]
    private PVPFolder parentFolder;
    [SerializeField]
    private List<PVPFile> childFiles;

    private PVPFile[,] groupedFiles;

    [SerializeField]
    private GUIContent folderContent;
    [SerializeField]
    private GUIContent foldoutContent;
    [SerializeField]
    private static Texture2D folderIcon;
    [SerializeField]
    private static Texture2D foldoutIcon;
    [SerializeField]
    UnityEngine.Object folderobj;
    [SerializeField]
    private bool fold = false;
    [SerializeField]
    private int depth;
    [SerializeField]
    private bool childFoldersAreTabs;
    public bool ChildFoldersAreTabs { get => childFoldersAreTabs; private set => childFoldersAreTabs = value; }
    public List<PVPFolder> ChildFolders { get => childFolders; set => childFolders = value; }

    public PVPFolder ParentFolder { get { return parentFolder; } set { parentFolder = value; } }

    public List<PVPFile> ChildFiles { get => childFiles; set => childFiles = value; }

    [SerializeField]
    string[] filters;
    [SerializeField]
    private Rect position;

    private static GUIStyle foldoutStyle;

    public PVPFolder(string folderPath, PVPFolder parentFolder, int depth, Rect position)
    {
        #region Serialization

        if (pvpData == null)
        {
            pvpData = AssetDatabase.LoadAssetAtPath<PVPDataSO>("Assets/Editor/PVPData.asset");
        }

        pvpData.allFolders.Add(this);
        SerializationInfo.folderIndex = pvpData.allFolders.Count - 1;

        if (parentFolder == null)
        {
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
        childFiles = FindChildFiles();

        //Create the object by giving its path. Then get the assetpreview.
        folderobj = AssetDatabase.LoadAssetAtPath(this.folderPath, typeof(UnityEngine.Object));
        folderIcon = pvpData.FolderIcon;
        foldoutIcon = pvpData.FoldoutIcon;

        //Assets/New Folder-> folderName:New Folder
        string[] splitPath = this.folderPath.Split('\\');
        folderName = splitPath[splitPath.Length - 1];

        folderContent = new GUIContent(folderName, folderIcon, folderPath);
        foldoutContent = new GUIContent(foldoutIcon);



    }

    public void VisualizeFolder()
    {

        GUI.skin = pvpData.GUISkin;
        GUILayout.BeginVertical();

        //Do this to give horizontal space

        FoldoutWithFolder();

        if (fold)
        {
            VisualizeChildFiles();

            foreach (var VARIABLE in ChildFolders)
                VARIABLE.VisualizeFolder();
        }

        GUILayout.EndVertical();
    }

    public void DropAreaGUI(Rect dropArea)
    {

        Event evt = Event.current;

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(evt.mousePosition))
                    return;




                DragAndDrop.visualMode = DragAndDropVisualMode.Move;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    var dragged_object = (UnityEngine.Object)DragAndDrop.GetGenericData("File");

                    Debug.Log($"Performed drag on object {dragged_object.name} to folder {folderName}");

                    var oldPath = AssetDatabase.GetAssetPath(dragged_object);
                    var splitName = oldPath.Split('/');
                    var fileName = splitName[splitName.Length - 1];
                    var newPath = folderPath + "\\" + fileName;
                    AssetDatabase.MoveAsset(oldPath, newPath);
                    childFiles.Add(new PVPFile(newPath, this));

                }
                break;
        }
    }

    private void FoldoutWithFolder()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(15 * depth);
        Rect buttonRect = GUILayoutUtility.GetRect(35, 35);
        var matrix = GUI.matrix;
        if (fold)
        {
            EditorGUIUtility.RotateAroundPivot(90, buttonRect.center);
        }

        if (GUI.Button(buttonRect, foldoutContent))
        {
            fold = !fold;
        }
        GUI.matrix = matrix;
        Rect folderRect = GUILayoutUtility.GetRect(100, 35);
        GUI.Label(folderRect, folderContent);
        DropAreaGUI(folderRect);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private List<PVPFolder> FindChildFolders()
    {
        //GetDirectories will return all the subfolders in the given path.
        string[] dirs = Directory.GetDirectories(folderPath);
        List<PVPFolder> folders = new List<PVPFolder>();
        foreach (var directory in dirs)
        {
            //Turn all directories into our 'UnityFolder' Object.
            PVPFolder newfolder = new PVPFolder(directory, this, depth + 1, position);
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
            PVPFile newfile = new PVPFile(file, this);
            //Pass meta files.
            if (newfile.GetExtension().Equals("meta"))
            {
                pvpData.allFiles.Remove(newfile);
                continue;
            }
                
            files.Add(newfile);
        }

        return files;

    }

    private PVPFile[,] GroupChildFiles( List<PVPFile> files)
    {
        //This method groups files by rows of 3. You can edit this
        //to change visuals.
        int size = files.Count;
        int rows = (size / 3) + 1;
        groupedFiles = new PVPFile[rows, 3];
        int index = 0;
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < 3; j++)
                if (i * 3 + j <= size - 1)
                    groupedFiles[i, j] = files[index++];
        return groupedFiles;
    }

    private void VisualizeChildFiles()
    {
        int size = childFiles.Count;
        int rows = (size / 3) + 1;
       groupedFiles = GroupChildFiles(childFiles);
        int i = 0, j = 0;
        for (i = 0; i < rows; i++)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Space(15 * depth);
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
                PVPFile file = new PVPFile(path, this);

                bool userWantsToMoveFile = false;
                if (!file.IsChildOfRootFolder())
                {
                    userWantsToMoveFile = EditorUtility.DisplayDialog("File conflict", $"File {file.GetName()} is currently in this path: {file.GetPath()}. Do you want to move the file to {path}?", "Yes", "No");
                }

                if (!childFiles.Contains(file) && userWantsToMoveFile)
                {
                    childFiles.Add(file);
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
        if (folderPath == "Assets")
        {
            return true;
        }
        return false;
    }
}
