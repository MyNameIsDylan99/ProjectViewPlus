using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class PVPFile : ISelectable, IComparable<PVPFile>
{
    public FileSerializationInfo FileSerializationInfo;

    #region SerializedFields

    [SerializeField]
    private string path;

    [SerializeField]
    private string extension;

    [SerializeField]
    private string fileName;

    [NonSerialized]
    private PVPFolder parentFolder;

    [SerializeField]
    private Texture2D fileIcon;

    [SerializeField]
    private GUIContent fileContent;

    #region ISelectable

    [SerializeField]
    private UnityEngine.Object selectableObject;

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

    private bool showRenameTextField;

    #endregion Private Fields

    #region Properties

    public PVPFolder ParentFolder { get => parentFolder; set => parentFolder = value; }
    public UnityEngine.Object SelectableUnityObject { get => selectableObject; set => selectableObject = value; }
    public Rect SelectionRect { get => selectionRect; set => selectionRect = value; }
    public bool IsVisible { get => isVisible; set => isVisible = value; }
    public bool IsSelected { get => isSelected; set => isSelected = value; }
    public int SelectableIndex { get => selectableIndex; set => selectableIndex = value; }
    public bool RepaintFlag { get => PVPWindow.RepaintFlag; set => PVPWindow.RepaintFlag = value; }

    public bool IsFile { get => isFile; private set => isFile = value; }

    public string Path { get => path; }

    public PVPFolder SelectableContextFolder { get => ParentFolder; }

    #endregion Properties

    public PVPFile(string path, PVPFolder parentFolder)
    {
        PVPSelection.allSelectables.Add(this);
        SelectableIndex = PVPSelection.allSelectables.Count - 1;
        PVPWindow.PVPData.allFiles.Add(this);
        FileSerializationInfo.fileIndex = PVPWindow.PVPData.allFiles.Count - 1;

        FileSerializationInfo.parentFolderIndex = parentFolder.SerializationInfo.folderIndex;

        if (parentFolder.SerializationInfo.childFileIndeces == null)
            parentFolder.SerializationInfo.childFileIndeces = new List<int>();
        parentFolder.SerializationInfo.childFileIndeces.Add(FileSerializationInfo.fileIndex);

        this.path = path;
        extension = FindExtension(path);
        fileName = FindFileName(path);
        IsFile = true;

        this.parentFolder = parentFolder;

        SelectableUnityObject = AssetDatabase.LoadAssetAtPath(this.path, typeof(UnityEngine.Object));
        fileIcon = AssetPreview.GetMiniThumbnail(SelectableUnityObject);

        fileContent = new GUIContent(fileName, fileIcon, path);
    }

    #region Methods

    #region Getter Methods

    public string GetPath()
    {
        return path;
    }

    public string GetExtension()
    {
        return extension;
    }

    public string GetName()
    {
        return fileName + extension;
    }

    public bool IsChildOfRootFolder()
    {
        return parentFolder.IsRootFolder();
    }

    #endregion Getter Methods

    #region Visualization

    public void VisualizeFile(int depth)
    {
        SelectionRect = GUILayoutUtility.GetRect(0, PVPWindow.IconSize);
        Rect fileLabel = new Rect(SelectionRect.position.x + depth * 25, SelectionRect.position.y, 300, PVPWindow.IconSize);

        IsVisible = true;

        if (CheckForRenamingInput(SelectionRect))
        {
            showRenameTextField = true;
        }

        if (PVPSelection.CheckForSingleSelectionInput(this))
        {
            PVPSelection.SelectSingleElement(this);
        }

        if (PVPSelection.CheckForOpenAssetInput(this))
        {
            AssetDatabase.OpenAsset(SelectableUnityObject);
        }

        if (PVPSelection.CheckForShiftSelectInput(this))
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
                AssetDatabase.RenameAsset(GetPath(), fileName);
                path = AssetDatabase.GetAssetPath(SelectableUnityObject);
                fileContent.tooltip = path;
            }

            showRenameTextField = false;
        }

        GUI.Box(SelectionRect, "");
        if (showRenameTextField)
        {
            fileName = GUI.TextField(fileLabel, fileName);
            fileContent.text = fileName;
        }
        else
        {
            GUI.Label(fileLabel, fileContent);
        }

        PVPSelection.SetGUISkinToNormal();

        PVPContextMenu.DisplayContextMenu(this);
        CheckForDragAndDrop(SelectionRect);
    }

    #endregion Visualization

    #region Utility

    private string FindExtension(string path)
    {
        string[] splitPath = path.Split('.');
        string ext = splitPath[splitPath.Length - 1];
        return ext;
    }

    private string FindFileName(string path)
    {
        string[] splitPath = path.Split('\\');
        string fullName = splitPath[splitPath.Length - 1];
        string splitExt = fullName.Split('.')[0];

        return splitExt;
    }

    private bool CheckForRenamingInput(Rect selectionRect)
    {
        var evt = Event.current;
        return isSelected && evt.keyCode == KeyCode.F2 && evt.type == EventType.KeyDown || isSelected && evt.type == EventType.MouseDown && evt.button == 0 && selectionRect.Contains(evt.mousePosition) && PVPSelection.SelectedElements.Count <= 1;
    }

    public void Delete()
    {
        parentFolder.ChildFiles.Remove(this); //Remove this file from old parent folder
        parentFolder.SerializationInfo.childFileIndeces.Remove(FileSerializationInfo.fileIndex);
        PVPWindow.PVPData.allFiles.Remove(this);
        PVPSelection.allSelectables.Remove(this);

        AssetDatabase.DeleteAsset(path);
    }

    #endregion Utility

    #region Drag and Drop

    public void Move(PVPFolder targetFolder)
    {
        parentFolder.ChildFiles.Remove(this); //Remove this file from old parent folder
        parentFolder.SerializationInfo.childFileIndeces.Remove(FileSerializationInfo.fileIndex);

        var newPath = targetFolder.FolderPath + "\\" + fileName + "." + extension;

        AssetDatabase.MoveAsset(path, newPath);

        this.path = newPath;

        this.parentFolder = targetFolder;
        FileSerializationInfo.parentFolderIndex = parentFolder.SerializationInfo.folderIndex;

        targetFolder.AddChildFile(this);
    }

    private void CheckForDragAndDrop(Rect dragArea)
    {
        var evt = Event.current;

        if (evt.type == EventType.MouseDrag && dragArea.Contains(evt.mousePosition))
        {
            PVPDragAndDrop.StartDrag();
        }
    }

    public int CompareTo(PVPFile other)
    {
        return fileName.CompareTo(other.GetName());
    }

    #endregion Drag and Drop

    #endregion Methods
}