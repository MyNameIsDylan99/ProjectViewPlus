using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PVPContextMenu
{
    private static ISelectable contextElement;
    private static GenericMenu menu;

    private static GenericMenu contextMenu
    {
        get
        {
            if (menu == null)
            {
                menu = new GenericMenu();
                menu.AddItem(new GUIContent("Create/Script"), false, CreateScript);
                menu.AddItem(new GUIContent("Create/Material"), false, CreateMaterial);
                menu.AddItem(new GUIContent("Create/Prefab"), false, CreatePrefab);
                menu.AddItem(new GUIContent("Create/Animation"), false, CreateAnimation);
                menu.AddItem(new GUIContent("Create/ScriptableObject"), false, CreateScriptableObject);
                menu.AddItem(new GUIContent("Create/Scene"), false, CreateScene);
                menu.AddItem(new GUIContent("Create/GUISkin"), false, CreateGUISkin);
            }

            return menu;
        }
        set { menu = value; }
    }
    public static void DisplayContextMenu(ISelectable contextElemnt)
    {
        var current = Event.current;

        if (current.type == EventType.ContextClick && contextElemnt.SelectionRect.Contains(current.mousePosition))
        {
            Debug.Log("Display context menu");
            contextElement = contextElemnt;
            contextMenu.ShowAsContext();
            current.Use();
        }
    }

    private static void CreateAnimation()
    {
        throw new NotImplementedException();
    }

    private static void CreateAnimationClip()
    {
        // Create a new animation clip asset
        string defaultName = "NewAnimationClip.anim";
        string directory = contextElement.SelectableContextFolder.FolderPath;
        string path = directory + '\\' + defaultName;

        // Check if the animation clip name already exists in the project
        int animationClipCount = 0;
        string animationClipName = "NewAnimationClip";
        while (AssetDatabase.LoadAssetAtPath(path, typeof(AnimationClip)) != null)
        {
            animationClipCount++;
            animationClipName = "NewAnimationClip" + (animationClipCount > 0 ? animationClipCount.ToString() : "");
            path = directory + '\\' + animationClipName + ".anim";
        }

        // Create the new animation clip asset
        AnimationClip animationClip = new AnimationClip();
        animationClip.name = animationClipName;
        AssetDatabase.CreateAsset(animationClip, path);
        AssetDatabase.SaveAssets();

        // Import the new animation clip asset
        AssetDatabase.ImportAsset(path);

        var newAnimationClip = new PVPFile(path, contextElement.SelectableContextFolder);
        contextElement.SelectableContextFolder.AddChildFile(newAnimationClip);
    }

    private static void CreateAudioClip()
    {
        // Create a new audio clip asset
        string defaultName = "NewAudioClip.wav";
        string directory = contextElement.SelectableContextFolder.FolderPath;
        string path = directory + '\\' + defaultName;

        // Check if the audio clip name already exists in the project
        int audioClipCount = 0;
        string audioClipName = "NewAudioClip";
        while (AssetDatabase.LoadAssetAtPath(path, typeof(AudioClip)) != null)
        {
            audioClipCount++;
            audioClipName = "NewAudioClip" + (audioClipCount > 0 ? audioClipCount.ToString() : "");
            path = directory + '\\' + audioClipName + ".wav";
        }

        // Create the new audio clip asset
        AudioClip audioClip = AudioClip.Create(audioClipName, 1, 1, 44100, false);
        AssetDatabase.CreateAsset(audioClip, path);
        AssetDatabase.SaveAssets();

        // Import the new audio clip asset
        AssetDatabase.ImportAsset(path);

        var newAudioClip = new PVPFile(path, contextElement.SelectableContextFolder);
        contextElement.SelectableContextFolder.AddChildFile(newAudioClip);
    }
    private static void CreateGUISkin()
    {
        // Create a new GUI Skin asset
        string defaultName = "NewGUISkin.guiskin";
        string directory = contextElement.SelectableContextFolder.FolderPath;
        string path = directory + '\\' + defaultName;

        // Check if the GUI Skin name already exists in the project
        int guiSkinCount = 0;
        string guiSkinName = "NewGUISkin";
        while (AssetDatabase.LoadAssetAtPath(path, typeof(GUISkin)) != null)
        {
            guiSkinCount++;
            guiSkinName = "NewGUISkin" + (guiSkinCount > 0 ? guiSkinCount.ToString() : "");
            path = directory + '\\' + guiSkinName + ".guiskin";
        }

        // Create the new GUI Skin asset
        GUISkin guiSkin = new GUISkin();
        guiSkin.name = guiSkinName;
        AssetDatabase.CreateAsset(guiSkin, path);
        AssetDatabase.SaveAssets();

        // Import the new GUI Skin asset
        AssetDatabase.ImportAsset(path);

        var newGUISkin = new PVPFile(path, contextElement.SelectableContextFolder);
        contextElement.SelectableContextFolder.AddChildFile(newGUISkin);
    }

    private static void CreateMaterial()
    {
        // Create a new material asset
        string defaultName = "NewMaterial.mat";
        string directory = contextElement.SelectableContextFolder.FolderPath;
        string path = directory + '\\' + defaultName;

        // Check if the material name already exists in the project
        int materialCount = 0;
        string materialName = "NewMaterial";
        while (AssetDatabase.LoadAssetAtPath(path, typeof(Material)) != null)
        {
            materialCount++;
            materialName = "NewMaterial" + (materialCount > 0 ? materialCount.ToString() : "");
            path = directory + '\\' + materialName + ".mat";
        }

        // Create the new material asset
        Material material = new Material(Shader.Find("Standard"));
        AssetDatabase.CreateAsset(material, path);
        AssetDatabase.SaveAssets();

        // Import the new material asset
        AssetDatabase.ImportAsset(path);

        var newMaterial = new PVPFile(path, contextElement.SelectableContextFolder);
        contextElement.SelectableContextFolder.AddChildFile(newMaterial);
    }

    private static void CreatePhysicsMaterial()
    {
        // Create a new physics material asset
        string defaultName = "NewPhysicsMaterial.physicMaterial";
        string directory = contextElement.SelectableContextFolder.FolderPath;
        string path = directory + '\\' + defaultName;

        // Check if the physics material name already exists in the project
        int physicsMaterialCount = 0;
        string physicsMaterialName = "NewPhysicsMaterial";
        while (AssetDatabase.LoadAssetAtPath(path, typeof(PhysicMaterial)) != null)
        {
            physicsMaterialCount++;
            physicsMaterialName = "NewPhysicsMaterial" + (physicsMaterialCount > 0 ? physicsMaterialCount.ToString() : "");
            path = directory + '\\' + physicsMaterialName + ".physicMaterial";
        }

        // Create the new physics material asset

        PhysicMaterial physicsMaterial = new PhysicMaterial();
        physicsMaterial.name = physicsMaterialName;
        AssetDatabase.CreateAsset(physicsMaterial, path);
        AssetDatabase.SaveAssets();

        // Import the new physics material asset
        AssetDatabase.ImportAsset(path);

        var newPhysicsMaterial = new PVPFile(path, contextElement.SelectableContextFolder);
        contextElement.SelectableContextFolder.AddChildFile(newPhysicsMaterial);
    }

    private static void CreatePhysicsMaterial2D()
    {
        // Create a new physics material 2D asset
        string defaultName = "NewPhysicsMaterial2D.physicMaterial2D";
        string directory = contextElement.SelectableContextFolder.FolderPath;
        string path = directory + '\\' + defaultName;

        // Check if the physics material 2D name already exists in the project
        int physicsMaterial2DCount = 0;
        string physicsMaterial2DName = "NewPhysicsMaterial2D";
        while (AssetDatabase.LoadAssetAtPath(path, typeof(PhysicsMaterial2D)) != null)
        {
            physicsMaterial2DCount++;
            physicsMaterial2DName = "NewPhysicsMaterial2D" + (physicsMaterial2DCount > 0 ? physicsMaterial2DCount.ToString() : "");
            path = directory + '\\' + physicsMaterial2DName + ".physicMaterial2D";
        }

        // Create the new physics material 2D asset
        PhysicsMaterial2D physicsMaterial2D = new PhysicsMaterial2D();
        physicsMaterial2D.name = physicsMaterial2DName;
        AssetDatabase.CreateAsset(physicsMaterial2D, path);
        AssetDatabase.SaveAssets();

        // Import the new physics material 2D asset
        AssetDatabase.ImportAsset(path);

        var newPhysicsMaterial2D = new PVPFile(path, contextElement.SelectableContextFolder);
        contextElement.SelectableContextFolder.AddChildFile(newPhysicsMaterial2D);
    }

    private static void CreatePrefab()
    {
        // Create a new Prefab asset
        string defaultName = "NewPrefab.prefab";
        string directory = contextElement.SelectableContextFolder.FolderPath;
        string path = directory + '\\' + defaultName;

        // Check if the Prefab name already exists in the project
        int prefabCount = 0;
        string prefabName = "NewPrefab";
        while (AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) != null)
        {
            prefabCount++;
            prefabName = "NewPrefab" + (prefabCount > 0 ? prefabCount.ToString() : "");
            path = directory + '\\' + prefabName + ".prefab";
        }

        // Create the new Prefab asset
        GameObject prefab = new GameObject();
        prefab.name = prefabName;
        PrefabUtility.SaveAsPrefabAsset(prefab, path);
        GameObject.DestroyImmediate(prefab);

        // Import the new Prefab asset
        AssetDatabase.ImportAsset(path);

        var newPrefab = new PVPFile(path, contextElement.SelectableContextFolder);
        contextElement.SelectableContextFolder.AddChildFile(newPrefab);
    }

    private static void CreateScene()
    {
        // Create a new scene
        string sceneName = "NewScene";
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);

        // Save the scene to the project
        string defaultName = "NewScene.unity";
        string directory = contextElement.SelectableContextFolder.FolderPath;
        string path = directory + '\\' + defaultName;

        // Check if the scene name already exists in the project
        int sceneCount = 0;
        while (AssetDatabase.LoadAssetAtPath(path, typeof(SceneAsset)) != null)
        {
            sceneCount++;
            sceneName = "NewScene" + (sceneCount > 0 ? sceneCount.ToString() : "");
            path = directory + '\\' + sceneName + ".unity";
        }

        // Save the new scene to the project.
        EditorSceneManager.SaveScene(newScene, path);

        var newSceneAsset = new PVPFile(path, contextElement.SelectableContextFolder);
        contextElement.SelectableContextFolder.AddChildFile(newSceneAsset);
    }

    private static void CreateScript()
    {
        // Check if the class name already exists in the global namespace
        int classCount = 0;
        string className = "NewScript";
        while (Type.GetType(className) != null)
        {
            classCount++;
            className = "NewScript" + classCount;
        }

        // Create a new text file with the code for a MonoBehaviour script
        string defaultName = className + ".cs";
        string directory = contextElement.SelectableContextFolder.FolderPath;
        string path = directory + '\\' + defaultName;

        // Write the code for the new script to the text file
        File.WriteAllText(path, $@"using UnityEngine;

public class {className} : MonoBehaviour
{{
    // Your code here...
}}");

        // Import the text file as a script asset
        AssetDatabase.ImportAsset(path);

        var newFile = new PVPFile(path, contextElement.SelectableContextFolder);
        contextElement.SelectableContextFolder.AddChildFile(newFile);
    }

    private static void CreateScriptableObject()
    {
        throw new NotImplementedException();
    }

    private static void CreateShader()
    {
        // Create a new shader asset
        string defaultName = "NewShader.shader";
        string directory = contextElement.SelectableContextFolder.FolderPath;
        string path = directory + '\\' + defaultName;

        // Check if the shader name already exists in the project
        int shaderCount = 0;
        string shaderName = "NewShader";
        while (AssetDatabase.LoadAssetAtPath(path, typeof(Shader)) != null)
        {
            shaderCount++;
            shaderName = "NewShader" + (shaderCount > 0 ? shaderCount.ToString() : "");
            path = directory + '\\' + shaderName + ".shader";
        }

        // Create the new shader asset
        Shader shader = Shader.Find("Standard");
        shader.name = shaderName;
        AssetDatabase.CreateAsset(shader, path);
        AssetDatabase.SaveAssets();

        // Import the new shader asset
        AssetDatabase.ImportAsset(path);

        var newShader = new PVPFile(path, contextElement.SelectableContextFolder);
        contextElement.SelectableContextFolder.AddChildFile(newShader);
    }
}