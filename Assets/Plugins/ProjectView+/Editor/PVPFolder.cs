using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[Serializable]
public class PVPFolder : ISelectable, IComparable<PVPFolder>
{
    public FolderSerializationInfo SerializationInfo;

    #region SerializedFields

    [NonSerialized]
    private List<PVPFolder> _childFolders;

    [NonSerialized]
    private PVPFolder _parentFolder;

    [SerializeField]
    private string _folderPath;

    [SerializeField]
    private string _folderName;

    [SerializeField]
    private GUIContent _folderContent;

    [SerializeField]
    private GUIContent _foldoutContent;

    [SerializeField]
    private static Texture2D _folderIcon;

    [SerializeField]
    private static Texture2D _foldoutIcon;

    [SerializeField]
    private int _depth;

    [SerializeField]
    private bool _childFoldersAreTabs;

    [SerializeField]
    private string[] _filters;

    #region ISelectable

    [SerializeField]
    private UnityEngine.Object selectableUnityObject;

    [SerializeField]
    private Rect selectionRect;

    [SerializeField]
    private bool isVisible;

    private bool isSelected;

    [SerializeField]
    private int selectableIndex;

    [SerializeField]
    private bool isFile;

    #endregion ISelectable

    #endregion SerializedFields

    #region Private Fields

    private bool _fold = false;
    private List<PVPFile> _childFiles;
    private PVPFile[,] _groupedFiles;
    private bool showRenameTextField;

    [NonSerialized]
    private List<PVPFolder> childFoldersToRemove = new List<PVPFolder>();

    #endregion Private Fields

    #region Properties

    public bool ChildFoldersAreTabs { get => _childFoldersAreTabs; private set => _childFoldersAreTabs = value; }
    public List<PVPFolder> ChildFolders { get => _childFolders; set => _childFolders = value; }

    public PVPFolder ParentFolder
    { get { return _parentFolder; } set { _parentFolder = value; } }

    private List<PVPFolder> FoldersToRemove { get => childFoldersToRemove; set => childFoldersToRemove = value; }

    public List<PVPFile> ChildFiles { get => _childFiles; set => _childFiles = value; }
    public string FolderPath { get => _folderPath; set => _folderPath = value; }

    public int Depth { get => _depth; set => _depth = value; }

    public UnityEngine.Object SelectableUnityObject { get => selectableUnityObject; set => selectableUnityObject = value; }
    public Rect SelectionRect { get => selectionRect; set => selectionRect = value; }
    public bool IsVisible { get => isVisible; set => isVisible = value; }
    public bool IsSelected { get => isSelected; set => isSelected = value; }
    public int SelectableIndex { get => selectableIndex; set => selectableIndex = value; }
    public bool RepaintFlag { get => PVPWindow.RepaintFlag; set => PVPWindow.RepaintFlag = value; }

    public bool IsFile { get => isFile; private set => isFile = value; }

    public string Path { get => FolderPath; }

    public PVPFolder SelectableContextFolder { get => this; }

    #endregion Properties

    public PVPFolder(string folderPath, PVPFolder parentFolder, int depth)
    {
        #region Serialization

        PVPSelection.allSelectables.Add(this);
        SelectableIndex = PVPSelection.allSelectables.Count - 1;
        PVPWindow.PVPData.allFolders.Add(this);
        SerializationInfo.folderIndex = PVPWindow.PVPData.allFolders.Count - 1;

        if (parentFolder == null)
        {
            PVPWindow.PVPData.RootFolder = this;
        }
        else
        {
            ParentFolder = parentFolder;
            SerializationInfo.parentFolderIndex = parentFolder.SerializationInfo.folderIndex;

            if (parentFolder.SerializationInfo.childFolderIndeces == null)
                parentFolder.SerializationInfo.childFolderIndeces = new List<int>();

            parentFolder.SerializationInfo.childFolderIndeces.Add(SerializationInfo.folderIndex);
        }

        #endregion Serialization

        _folderPath = folderPath;
        _depth = depth;

        IsFile = false;

        _childFiles = FindChildFiles();
        ChildFolders = FindChildFolders();

        //Create the object by giving its path. Then get the assetpreview.
        SelectableUnityObject = AssetDatabase.LoadAssetAtPath(this._folderPath, typeof(UnityEngine.Object));
        _folderIcon = PVPWindow.PVPData.PVPSettings.FolderIcon;
        _foldoutIcon = PVPWindow.PVPData.PVPSettings.FoldoutIcon;

        //Assets/New Folder-> folderName:New Folder
        string[] splitPath = this._folderPath.Split('\\');
        _folderName = splitPath[splitPath.Length - 1];

        _folderContent = new GUIContent(_folderName, _folderIcon, folderPath);
        _foldoutContent = new GUIContent(_foldoutIcon);
    }

    public void VisualizeFolder()
    {
        IsVisible = true;

        GUILayout.BeginVertical();

        FoldoutWithFolder();

        if (_fold)
        {
            VisualizeChildFiles();
            foreach (var VARIABLE in ChildFolders)
                VARIABLE.VisualizeFolder();
        }
        else
        {
            SetChildrenNotVisible();
        }

        GUILayout.EndVertical();

        if (childFoldersToRemove != null && childFoldersToRemove.Count > 0)
        {
            foreach (var folder in childFoldersToRemove)
            {
                ChildFolders.Remove(folder);
            }
            childFoldersToRemove.Clear();
        }
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
                    PVPDragAndDrop.AcceptDrag(this);
                }
                break;
        }
    }

    private void CheckForDragAndDrop(Rect dragArea)
    {
        var evt = Event.current;

        if (evt.type == EventType.MouseDrag && dragArea.Contains(evt.mousePosition))
        {
            PVPDragAndDrop.StartDrag();
        }
    }

    private void SetChildrenNotVisible()
    {
        foreach (var childFile in ChildFiles)
        {
            childFile.IsVisible = false;
        }

        foreach (var childFolder in ChildFolders)
        {
            childFolder.IsVisible = false;
        }
    }

    private void FoldoutWithFolder()
    {
        GUILayout.BeginHorizontal();

        SelectionRect = GUILayoutUtility.GetRect(PVPWindow.Position.width, PVPWindow.IconSize);
        Rect buttonRect = new Rect(SelectionRect.x + _depth * 15, SelectionRect.y, PVPWindow.IconSize, PVPWindow.IconSize);
        Rect labelRect = new Rect(buttonRect.position.x + PVPWindow.IconSize, buttonRect.y, 300, PVPWindow.IconSize);

        var evt = Event.current;

        if (CheckForRenamingInput(SelectionRect))
        {
            showRenameTextField = true;
        }

        if (PVPSelection.CheckForSingleSelectionInput(this) && !buttonRect.Contains(evt.mousePosition))
        {
            PVPSelection.SelectSingleElement(this);
        }
        if (PVPSelection.CheckForShiftSelectInput(this) && !buttonRect.Contains(evt.mousePosition))
        {
            PVPSelection.ShiftSelect(this);
        }

        if (PVPSelection.CheckForQtrlSelectInput(this))
        {
            PVPSelection.ControlSelect(this);
        }

        if (IsSelected)
        {
            PVPSelection.SetGUISkinToSelected();
        }
        else
        {
            if (showRenameTextField)
            {
                AssetDatabase.RenameAsset(FolderPath, _folderName);
                FolderPath = AssetDatabase.GetAssetPath(SelectableUnityObject);
                _folderContent.tooltip = FolderPath;
            }

            showRenameTextField = false;
        }

        GUI.Box(SelectionRect, "");

        if (showRenameTextField)
        {
            _folderName = GUI.TextField(labelRect, _folderName);
            _folderContent.text = _folderName;
        }
        else
        {
            GUI.Label(labelRect, _folderContent);
        }

        var matrix = GUI.matrix;
        if (_fold)
        {
            EditorGUIUtility.RotateAroundPivot(90, buttonRect.center);
        }

        if (GUI.Button(buttonRect, _foldoutContent))
        {
            _fold = !_fold;
            if (!_fold)
            {
                List<ISelectable> selectables = new List<ISelectable>();
                foreach (var selectable in ChildFolders)
                {
                    selectables.Add(selectable);
                }
                foreach (var selectable in ChildFiles)
                {
                    selectables.Add(selectable);
                }
            }
        }
        GUI.matrix = matrix;
        CheckForDragAndDrop(labelRect);
        PVPContextMenu.DisplayContextMenu(this);
        DropAreaGUI(labelRect);
        PVPSelection.SetGUISkinToNormal();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private bool CheckForRenamingInput(Rect selectionRect)
    {
        var evt = Event.current;
        return isSelected && evt.keyCode == KeyCode.F2 && evt.type == EventType.KeyDown || isSelected && evt.type == EventType.MouseDown && evt.button == 0 && selectionRect.Contains(evt.mousePosition) && PVPSelection.SelectedElements.Count <= 1;
    }

    private List<PVPFolder> FindChildFolders()
    {
        //GetDirectories will return all the subfolders in the given path.
        string[] dirs = Directory.GetDirectories(_folderPath);
        List<PVPFolder> folders = new List<PVPFolder>();
        foreach (var directory in dirs)
        {
            //Turn all directories into our 'UnityFolder' Object.
            PVPFolder newfolder = new PVPFolder(directory, this, _depth + 1);
            folders.Add(newfolder);
        }
        return folders;
    }

    private List<PVPFile> FindChildFiles()
    {
        //GetFiles is similar but returns all the files under the path(obviously)
        string[] fileNames = Directory.GetFiles(_folderPath);
        List<PVPFile> files = new List<PVPFile>();
        foreach (var file in fileNames)
        {
            PVPFile newfile = new PVPFile(file, this);
            //Pass meta files..
            if (newfile.GetExtension().Equals("meta"))
            {
                SerializationInfo.childFileIndeces.Remove(newfile.FileSerializationInfo.fileIndex);
                PVPWindow.PVPData.allFiles.Remove(newfile);
                PVPSelection.allSelectables.Remove(newfile);
                continue;
            }

            files.Add(newfile);
        }

        return files;
    }

    private PVPFile[,] GroupChildFiles(List<PVPFile> files)
    {
        //This method groups files by rows of how many files the user wants to display per row.
        int size = files.Count;
        int rows = (size / PVPWindow.PVPData.PVPSettings.FilesPerRow) + 1;
        _groupedFiles = new PVPFile[rows, PVPWindow.PVPData.PVPSettings.FilesPerRow];
        int index = 0;
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < PVPWindow.PVPData.PVPSettings.FilesPerRow; j++)
                if (i * PVPWindow.PVPData.PVPSettings.FilesPerRow + j <= size - 1)
                    _groupedFiles[i, j] = files[index++];
        return _groupedFiles;
    }

    private void VisualizeChildFiles()
    {
        int size = _childFiles.Count;
        int rows = (size / PVPWindow.PVPData.PVPSettings.FilesPerRow) + 1;
        _groupedFiles = GroupChildFiles(_childFiles);
        int i = 0, j = 0;
        for (i = 0; i < rows; i++)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            for (j = 0; j < PVPWindow.PVPData.PVPSettings.FilesPerRow; j++)
            {
                if (i * PVPWindow.PVPData.PVPSettings.FilesPerRow + j <= size - 1)
                    _groupedFiles[i, j].VisualizeFile(_depth);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
    }

    private void AddFilesFromFilter()
    {
        List<PVPFile> files = new List<PVPFile>();
        foreach (var filter in _filters)
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

                if (!_childFiles.Contains(file) && userWantsToMoveFile)
                {
                    _childFiles.Add(file);
                }
                Debug.Log("New file added : " + file.GetPath());
            }
        }
    }

    public void Delete()
    {
        if(ChildFiles != null && ChildFiles.Count > 0)
        {
            foreach (var file in ChildFiles)
            {
                file.Delete();
            }
        }
        if (ChildFolders != null && ChildFolders.Count > 0)
        {
            foreach (var folder in ChildFolders)
            {
                folder.Delete();
            }
        }
        ParentFolder.childFoldersToRemove.Add(this);
        ParentFolder.SerializationInfo.childFolderIndeces.Remove(SerializationInfo.folderIndex);
        PVPWindow.PVPData.allFolders.Remove(this);
        AssetDatabase.DeleteAsset(FolderPath);

    }

    public string GetName()
    {
        return _folderPath;
    }

    public int GetDepth()
    {
        return _depth;
    }

    public bool IsRootFolder()
    {
        if (_folderPath == "Assets")
        {
            return true;
        }
        return false;
    }

    private int GetSortedIndex(List<string> list, string value)
    {
        // Add the value to the list if it is not already there
        if (!list.Contains(value))
        {
            list.Add(value);
        }

        // Sort the list alphabetically
        list.Sort();

        // Find the index of the value in the sorted list
        return list.IndexOf(value);
    }

    public void AddChildFolder(PVPFolder folder)
    {
        Undo.RecordObject(PVPWindow.PVPData, "");
        List<string> folderNameList = new List<string>(ChildFolders.Count);

        for (int i = 0; i < ChildFolders.Count; i++)
        {
            folderNameList.Add(ChildFolders[i].GetName());
        }

        var newFileIndex = GetSortedIndex(folderNameList, folder.GetName());

        if (ChildFolders.Count < 1)
        {
            ChildFolders.Add(folder);
        }
        else
        {
            ChildFolders.Insert(newFileIndex, folder);
        }

        if (SerializationInfo.childFolderIndeces == null)
            SerializationInfo.childFolderIndeces = new List<int>();

        SerializationInfo.childFolderIndeces.Add(folder.SerializationInfo.folderIndex);

        folder.Depth = _depth + 1;
        var folderSelectableIndex = SelectableIndex + newFileIndex + ChildFiles.Count + 1;
        folder.SelectableIndex = folderSelectableIndex;
        PVPSelection.MoveSelectable(folder);
    }

    public void AddChildFile(PVPFile file)
    {
        Undo.RecordObject(PVPWindow.PVPData, "");
        List<string> fileNameList = new List<string>(ChildFiles.Count);

        for (int i = 0; i < ChildFiles.Count; i++)
        {
            fileNameList.Add(ChildFiles[i].GetName());
        }
        ChildFiles.Sort();

        var newFileIndex = GetSortedIndex(fileNameList, file.GetName());

        if (ChildFiles.Count < 1)
        {
            ChildFiles.Add(file);
        }
        else
        {
            ChildFiles.Insert(newFileIndex, file);
        }

        if (SerializationInfo.childFileIndeces == null)
            SerializationInfo.childFileIndeces = new List<int>();

        if (!SerializationInfo.childFileIndeces.Contains(file.FileSerializationInfo.fileIndex))
            SerializationInfo.childFileIndeces.Add(file.FileSerializationInfo.fileIndex);
        else
        {
            SerializationInfo.childFileIndeces.Remove(file.FileSerializationInfo.fileIndex);
            SerializationInfo.childFileIndeces.Insert(newFileIndex, file.FileSerializationInfo.fileIndex);
        }

        var fileSelectableIndex = SelectableIndex + newFileIndex + 1;
        file.SelectableIndex = fileSelectableIndex;
        PVPSelection.MoveSelectable(file);
    }

    public void Move(PVPFolder targetFolder)
    {
        if (ParentFolder.FoldersToRemove == null)
            ParentFolder.FoldersToRemove = new List<PVPFolder>();
        ParentFolder.FoldersToRemove.Add(this); //Remove this folder from old parent folder
        ParentFolder.SerializationInfo.childFolderIndeces.Remove(SerializationInfo.folderIndex);

        var newPath = targetFolder.FolderPath + "\\" + _folderName;

        AssetDatabase.MoveAsset(FolderPath, newPath);

        FolderPath = newPath;

        this.ParentFolder = targetFolder;
        SerializationInfo.parentFolderIndex = ParentFolder.SerializationInfo.folderIndex;

        targetFolder.AddChildFolder(this);
        AdjustChildrenSelectableIndex();
    }

    public void AdjustChildrenSelectableIndex(List<ISelectable> selectablesToAdjust = null)
    {
        if (selectablesToAdjust == null)
            selectablesToAdjust = new List<ISelectable>();

        for (int i = 0; i < ChildFiles.Count; i++)
        {
            ChildFiles[i].SelectableIndex = SelectableIndex + i + 1;
            selectablesToAdjust.Add(ChildFiles[i]);
        }

        if (ChildFolders == null || ChildFolders.Count == 0)
        {
            PVPSelection.MoveSelectables(selectablesToAdjust.ToArray());
            return;
        }

        for (int i = 0; i < ChildFolders.Count; i++)
        {
            ChildFolders[i].SelectableIndex = SelectableIndex + ChildFiles.Count + i + 1;
            selectablesToAdjust.Add(ChildFolders[i]);
        }

        foreach (var childFolder in ChildFolders)
        {
            AdjustChildrenSelectableIndex(selectablesToAdjust);
        }
    }

    public int CompareTo(PVPFolder other)
    {
        return other.GetName().CompareTo(_folderName);
    }
}