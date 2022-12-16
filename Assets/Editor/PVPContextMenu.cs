using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class PVPContextMenu
{
   private GenericMenu contextMenu;
    public PVPFolder selectedFolder;

    public PVPContextMenu() {
        contextMenu = new GenericMenu();
        contextMenu.AddItem(new GUIContent("Create/Script"), false, CreateScript);
        contextMenu.AddItem(new GUIContent("Create/Material"), false, CreateMaterial);
        contextMenu.AddItem(new GUIContent("Create/Prefab"), false, CreatePrefab);
        contextMenu.AddItem(new GUIContent("Create/Animation"), false, CreateAnimation);
        contextMenu.AddItem(new GUIContent("Create/ScriptableObject"), false, CreateScriptableObject);
        contextMenu.AddItem(new GUIContent("Create/Scene"), false, CreateScene);
        contextMenu.AddItem(new GUIContent("Create/GUISkin"), false, CreateGUISkin);
        
    }

    private void CreateGUISkin() {
        throw new NotImplementedException();
    }

    private void CreateScene() {
        throw new NotImplementedException();
    }

    private void CreateScriptableObject() {
        throw new NotImplementedException();
    }

    private void CreateAnimation() {
        throw new NotImplementedException();
    }

    private void CreatePrefab() {
        throw new NotImplementedException();
    }

    private void CreateMaterial() {
        EditorGUIUtility.IconContent("Material");
    }

    private void CreateScript() {

        // Create a new text file with the code for a MonoBehaviour script
        string path = "Assets/MyScript.cs";
        File.WriteAllText(path, @"using UnityEngine;

public class MyScript : MonoBehaviour
{
    // Your code here...
}");

        // Import the text file as a script asset
        AssetDatabase.ImportAsset(path);
    }

    public void DisplayContextMenu() {
        contextMenu.ShowAsContext();
    }
}
