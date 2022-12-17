using System.IO;
using UnityEditor;
using UnityEngine;

public static class PVPPathUtility
{
    public static string GetPathOfMonoScript(MonoScript monoScript)
    {
        var currentPathWithName = AssetDatabase.GetAssetPath(monoScript);
        var splitCurrentPath = currentPathWithName.Split('/');
        string currentPath = "";
        for (int i = 0; i < splitCurrentPath.Length - 1; i++)
        {
            if (i > 0)
                currentPath += "/";

            currentPath += splitCurrentPath[i];
        }

        return currentPath;
    }

    public static string GetPathOfMonobehaviour(MonoBehaviour monoBehaviour)
    {
        var thisMS = MonoScript.FromMonoBehaviour(monoBehaviour);
        var currentPathWithName = AssetDatabase.GetAssetPath(thisMS);
        var splitCurrentPath = currentPathWithName.Split('/');
        string currentPath = "";
        for (int i = 0; i < splitCurrentPath.Length - 1; i++)
        {
            if (i > 0)
                currentPath += "/";

            currentPath += splitCurrentPath[i];
        }

        return currentPath;
    }

    public static string GetPathOfScriptableObject(ScriptableObject scriptableObject)
    {
        var thisMS = MonoScript.FromScriptableObject(scriptableObject);
        var currentPathWithName = AssetDatabase.GetAssetPath(thisMS);
        var splitCurrentPath = currentPathWithName.Split('/');
        string currentPath = "";
        for (int i = 0; i < splitCurrentPath.Length - 1; i++)
        {
            if (i > 0)
                currentPath += "/";

            currentPath += splitCurrentPath[i];
        }

        return currentPath;
    }

    public static string RemoveFileNameFromPath(string path)
    {
        return Path.GetDirectoryName(path);
    }
}