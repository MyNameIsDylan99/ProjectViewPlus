using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UnityFile
{
    private static GUISkin _buttonSkin;
    private string path;
    private string extension;
    private string fileName;
    Object fileObject;

    private Texture2D fileIcon;
    private GUIContent fileContent;
    private static GUIStyle _buttonStyle;

    public UnityFile(string path)
    {
        this.path = path;
        extension = FindExtension(path);
        fileName = FindFileName(path);



        fileObject = AssetDatabase.LoadAssetAtPath(this.path, typeof(Object));
        fileIcon = AssetPreview.GetMiniThumbnail(fileObject);

        fileContent = new GUIContent(fileName, fileIcon, path);
        if (_buttonSkin = null)
        {
           string[] buttonSkinPath = AssetDatabase.FindAssets("ButtonSkin t:GUISkin");
            _buttonSkin = AssetDatabase.LoadAssetAtPath<GUISkin>(buttonSkinPath[0]);
            
        }
        if(_buttonStyle == null)
        {
            _buttonStyle = new GUIStyle();
        }

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

        //GUILayout.BeginHorizontal(EditorStyles.helpBox,GUILayout.Width(300));
        GUI.skin.button.alignment = TextAnchor.MiddleLeft;
        if(Selection.activeObject == fileObject)
        {
            GUI.color = Color.blue;
        }
        if (GUILayout.Button(fileContent, GUILayout.Height(50)))
        {
            Selection.activeObject = fileObject;
        }
        //GUILayout.Label(fileContent,GUILayout.Width(256),GUILayout.Height(64));
        //GUILayout.EndHorizontal();
    }
    public string GetPath()
    {
        return path;

    }
    public string GetExtension()
    {
        return extension;

    }


}
