using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEditor;
using UnityEditor.Experimental.TerrainAPI;
using UnityEngine;

[Serializable]
public class PVPFile : ISelectable
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
    private ISelectable parentSelectable;
    [SerializeField]
    private Rect selectionRect;
    [SerializeField]
    private bool isVisible;
    private bool isSelected;
    [SerializeField]
    private int selectableIndex;
    private bool repaintFlag;

    #endregion

    #endregion

    #region Private Fields
    private bool _selected;
    #endregion

    #region Properties
    public PVPFolder ParentFolder { get => parentFolder; set => parentFolder = value; }
    public ISelectable ParentSelectable { get => ParentFolder;}
    public UnityEngine.Object SelectableObject { get => selectableObject; set => selectableObject = value; }
    public Rect SelectionRect { get => selectionRect; set => selectionRect = value; }
    public bool IsVisible { get => isVisible; set => isVisible = value; }
    public bool IsSelected { get => isSelected; set => isSelected = value; }
    public int SelectableIndex { get => selectableIndex; set => selectableIndex = value; }
    public bool RepaintFlag { get => PVPWindow.RepaintFlag; set => PVPWindow.RepaintFlag = value; }
    #endregion

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

    #endregion

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

        this.parentFolder = parentFolder;

        SelectableObject = AssetDatabase.LoadAssetAtPath(this.path, typeof(UnityEngine.Object));
        fileIcon = AssetPreview.GetMiniThumbnail(SelectableObject);

        fileContent = new GUIContent(fileName, fileIcon, path);

    }

    #region Visualization
    public void VisualizeFile(int depth)
    {

        SelectionRect = GUILayoutUtility.GetRect(0, PVPWindow.IconSize);
        Rect fileLabel = new Rect(SelectionRect.position.x + depth * 25, SelectionRect.position.y,300,PVPWindow.IconSize);

        IsVisible = true;

        if (PVPSelection.CheckForSingleSelectionInput(this))
        {
            PVPSelection.SelectSingleElement(this);
        }

        if (PVPSelection.CheckForOpenAssetInput(this))
        {
            AssetDatabase.OpenAsset(SelectableObject);
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

        //if (RepaintFlag && Event.current.type != EventType.Layout)
        //{
        //    PVPEvents.InvokeRepaintWindowEvent();
        //    RepaintFlag = false;
        //}

        GUI.Box(SelectionRect,"");
        GUI.Label(fileLabel, fileContent);

        PVPSelection.SetGUISkinToNormal();


        CheckForDragAndDrop(SelectionRect);
    }

    #endregion

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
    private void CheckForDragAndDrop(Rect dragArea)
    {
        var evt = Event.current;

        if (evt.type == EventType.MouseDrag && dragArea.Contains(evt.mousePosition))
        {
            PVPDragAndDrop.StartDrag();
        }
    }
    public void Move(PVPFolder targetFolder)
    {
        parentFolder.ChildFiles.Remove(this); //Remove this file from old parent folder
        parentFolder.SerializationInfo.childFileIndeces.Remove(FileSerializationInfo.fileIndex);

        var newPath = targetFolder.FolderPath + "\\" + fileName + "."+extension;

        AssetDatabase.MoveAsset(path, newPath);

        this.path = newPath;

        this.parentFolder = targetFolder;
        FileSerializationInfo.parentFolderIndex = parentFolder.SerializationInfo.folderIndex;

        targetFolder.AddChildFile(this);
        
        
    }
    #endregion
}
