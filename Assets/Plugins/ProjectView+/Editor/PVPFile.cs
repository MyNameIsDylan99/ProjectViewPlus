using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class PVPFile
{
    private static GUISkin _buttonSkin;
    private bool _selected;
    [SerializeField]
    private string path;
    [SerializeField]
    private string extension;
    [SerializeField]
    private string fileName;
    [SerializeField]
    private UnityEngine.Object fileObject;
    [NonSerialized]
    private PVPFolder parentFolder;
    [SerializeField]
    private Texture2D fileIcon;
    [SerializeField]
    private GUIContent fileContent;
    [SerializeField]
    private PVPDataSO pvpData;

    public PVPFolder ParentFolder { get => parentFolder; set => parentFolder = value; }
    public FileSerializationInfo FileSerializationInfo;

    public PVPFile(string path, PVPFolder parentFolder, int? fileIndex = null)
    {
        if (pvpData == null)
        {
            pvpData = AssetDatabase.LoadAssetAtPath<PVPDataSO>(PVPWindow.CurrentPath + "/PVPData.asset");
        }
        if (fileIndex == null)
        {
            pvpData.allFiles.Add(this);
            FileSerializationInfo.fileIndex = pvpData.allFiles.Count - 1;
        }
        else
        {
            pvpData.allFiles[(int)fileIndex] = this;
            FileSerializationInfo.fileIndex = (int)fileIndex;
        }
        FileSerializationInfo.parentFolderIndex = parentFolder.SerializationInfo.folderIndex;

        if (parentFolder.SerializationInfo.childFileIndeces == null)
            parentFolder.SerializationInfo.childFileIndeces = new List<int>();
        parentFolder.SerializationInfo.childFileIndeces.Add(FileSerializationInfo.fileIndex);


        this.path = path;
        extension = FindExtension(path);
        fileName = FindFileName(path);

        this.parentFolder = parentFolder;

        fileObject = AssetDatabase.LoadAssetAtPath(this.path, typeof(UnityEngine.Object));
        fileIcon = AssetPreview.GetMiniThumbnail(fileObject);

        fileContent = new GUIContent(fileName, fileIcon, path);

    }

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
    public void VisualizeFile()
    {


        Rect fileRect = GUILayoutUtility.GetRect(0, PVPWindow.IconSize);
        
        var evt = Event.current;
        if (evt.type == EventType.MouseUp && fileRect.Contains(evt.mousePosition))
        {
            Selection.activeObject = fileObject;
            GUI.skin.box.normal.background = pvpData.PVPSettings.SelectedBackground;
            GUI.skin.box.hover.background = pvpData.PVPSettings.SelectedBackground;
            PVPEvents.InvokeRepaintWindowEvent();
        }
        else if (evt.type == EventType.MouseDown && evt.clickCount >= 2 && fileRect.Contains(evt.mousePosition))
        {
            AssetDatabase.OpenAsset(fileObject);
        }
        CheckForDragAndDrop(fileRect);
        _selected = Selection.activeObject == fileObject;

        if (_selected)
        {
            GUI.skin.box.normal.background = pvpData.PVPSettings.SelectedBackground;
            GUI.skin.box.hover.background = pvpData.PVPSettings.SelectedBackground;
        }
        GUI.Box(fileRect, fileContent);
        GUI.skin.box.normal.background = pvpData.PVPSettings.NormalBackground;
        GUI.skin.box.hover.background = pvpData.PVPSettings.NormalBackground;
    }

    private void CheckForDragAndDrop(Rect dragArea)
    {
        var evt = Event.current;

        if (evt.type == EventType.MouseDrag && dragArea.Contains(evt.mousePosition))
        {
            PVPDragAndDrop.StartDrag(this);
        }
    }
    public int RemoveAllFileReferences()
    {
        var fileIndex = FileSerializationInfo.fileIndex;
        pvpData.allFiles[fileIndex] = null;
        parentFolder.ChildFiles.Remove(this);
        parentFolder.SerializationInfo.childFileIndeces.Remove(FileSerializationInfo.fileIndex);
        return fileIndex;
    }
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
}
