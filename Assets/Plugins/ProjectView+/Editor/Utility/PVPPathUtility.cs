using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectViewPlus
{
    /// <summary>
    /// Static class with utility function for finding paths.
    /// </summary>
    public static class PVPPathUtility
    {
        /// <summary>
        /// Returns the directory the monoscript asset is in
        /// </summary>
        public static string GetDirectoryOfMonoScript(MonoScript monoScript)
        {
            var path = AssetDatabase.GetAssetPath(monoScript);
            return GetDirectory(path);
        }

        /// <summary>
        /// Returns the directory the monobehaviour asset is in
        /// </summary>
        public static string GetDirectoryOfMonobehaviour(MonoBehaviour monoBehaviour)
        {
            var thisMS = MonoScript.FromMonoBehaviour(monoBehaviour);
            var path = AssetDatabase.GetAssetPath(thisMS);
            return GetDirectory(path);
        }

        /// <summary>
        /// Returns the directory the scriptableobject asset is in
        /// </summary>
        public static string GetDirectoryOfScriptableObject(ScriptableObject scriptableObject)
        {
            var thisMS = MonoScript.FromScriptableObject(scriptableObject);
            var path = AssetDatabase.GetAssetPath(thisMS);
            return GetDirectory(path);
        }

        /// <summary>
        /// Returns the directory of the path
        /// </summary>
        public static string GetDirectory(string path)
        {
            return Path.GetDirectoryName(path);
        }
    }
}