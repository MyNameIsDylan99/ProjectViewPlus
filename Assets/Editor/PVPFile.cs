using System;
using UnityEditor;
using UnityEngine;

[Serializable]
public class PVPFile
{
    private static GUISkin _buttonSkin;
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
    private  PVPDataSO pvpData;
    private bool isBeingDraged = false;
    public PVPFolder ParentFolder { get => parentFolder; set => parentFolder = value; }
    public FileSerializationInfo FileSerializationInfo;

    public PVPFile(string path, PVPFolder parentFolder)
    {
        if (pvpData == null)
        {
            pvpData = AssetDatabase.LoadAssetAtPath<PVPDataSO>("Assets/Editor/PVPData.asset");
        }
        pvpData.allFiles.Add(this);
        FileSerializationInfo.fileIndex = pvpData.allFiles.Count - 1;
        FileSerializationInfo.parentFolderIndex = parentFolder.SerializationInfo.folderIndex;
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
        if (Selection.activeObject == fileObject)
        {

        }
        Rect fileRect = GUILayoutUtility.GetRect(10, 25);
        GUI.Box(fileRect,fileContent);
        var evt = Event.current;
        if (evt.type == EventType.MouseUp && fileRect.Contains(evt.mousePosition))
        {
            Selection.activeObject = fileObject;
        }
        CheckForDragAndDrop(fileRect);
    }

    private void CheckForDragAndDrop(Rect dragArea)
    {
        var evt = Event.current;

        if (evt.type == EventType.MouseDrag && dragArea.Contains(evt.mousePosition))
        {
            DragAndDrop.StartDrag($"Drag file {fileName}");
            DragAndDrop.SetGenericData("File", fileObject);
            Debug.Log("Start Drag on file" + fileName);
            DragAndDrop.visualMode = DragAndDropVisualMode.Move;
            isBeingDraged = true;
        }
        else if(evt.type == EventType.DragPerform && isBeingDraged)
        {
            if (parentFolder == null)
            {
                parentFolder = pvpData.allFolders[FileSerializationInfo.parentFolderIndex]; //TODO: Find the root cause of this.
            }

            if (parentFolder == null)
            {
                Debug.Log("Parent folder null");
            }
            else
            {
                Debug.Log("Parent folder not null");
                parentFolder.ChildFiles.Remove(this);
            }

            
        }
    }
    //
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
