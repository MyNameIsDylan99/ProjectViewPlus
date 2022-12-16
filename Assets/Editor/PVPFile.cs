﻿using System;
using System.Collections;
using System.Collections.Generic;
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
    private static PVPDataSO pvpData;
    private bool showRenameTextField;

    public PVPFolder ParentFolder { get => parentFolder; set => parentFolder = value; }
    public FileSerializationInfo FileSerializationInfo;


    //Normal constructor
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


        
        fileObject = AssetDatabase.LoadAssetAtPath(this.path, typeof(UnityEngine.Object));
        fileIcon = AssetPreview.GetMiniThumbnail(fileObject);

        fileContent = new GUIContent(fileName, fileIcon, path);

        if (_buttonSkin = null)
        {
           string[] buttonSkinPath = AssetDatabase.FindAssets("ButtonSkin t:GUISkin");
            _buttonSkin = AssetDatabase.LoadAssetAtPath<GUISkin>(buttonSkinPath[0]);
        }

    }

    //Preview file when creating in context Menu
    public PVPFile(PVPFolder parentFolder, UnityEngine.Object fileObject) {
        this.parentFolder = parentFolder;
        this.fileObject = fileObject;
        fileIcon = AssetPreview.GetMiniThumbnail(fileObject);
        fileContent = new GUIContent(fileIcon);
        fileName = "New File";
        showRenameTextField = true;
    }

    private string FindExtension(string path)
    {
        string[] splitPath = path.Split('.');
        string ext = splitPath[splitPath.Length - 1];
        return ext;
    }
    private string FindFileName(string path)
    {
        string[] splitPath = path.Split('/');
        string fullName = splitPath[splitPath.Length - 1];
        string splitExt = fullName.Split('.')[0];

        return splitExt;
    }
    public void VisualizeFile()
    {


        GUI.skin.button.alignment = TextAnchor.MiddleLeft;
        if(Selection.activeObject == fileObject)
        {
            
        }
        if (GUILayout.Button(fileContent, GUILayout.Height(50)))
        {
            Selection.activeObject = fileObject;
        }
        if (showRenameTextField)
            fileName = GUILayout.TextField(fileName);

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
        return fileName+extension;
    }

    public bool IsChildOfRootFolder()
    {
        return parentFolder.IsRootFolder();
    }
}