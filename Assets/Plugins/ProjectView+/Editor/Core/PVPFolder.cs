using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectViewPlus
{
    /// <summary>
    /// This class represents a folder in the project view plus window. It stores relevant data of the folder it is representing, like its path, reference to the icon, childFiles and more.
    /// </summary>
    [Serializable]
    public class PVPFolder : ISelectable, IComparable<PVPFolder>
    {
        public FolderSerializationInfo SerializationInfo;

        #region SerializedFields

        [SerializeField]
        private static Texture2D _folderIcon;

        [SerializeField]
        private static Texture2D _foldoutIcon;

        [NonSerialized]
        private List<PVPFolder> _childFolders;

        [SerializeField]
        private bool _childFoldersAreTabs;

        [SerializeField]
        private int _depth;

        [SerializeField]
        private string[] _filters;

        [SerializeField]
        private GUIContent _folderContent;

        [SerializeField]
        private string _folderName;

        [SerializeField]
        private string _folderPath;

        [SerializeField]
        private GUIContent _foldoutContent;

        [NonSerialized]
        private PVPFolder _parentFolder;

        [SerializeField]
        private string placeholderRenamingName;

        #region ISelectable

        [SerializeField]
        private bool isFile;

        private bool isSelected;

        [SerializeField]
        private bool isVisible;

        [SerializeField]
        private int selectableIndex;

        [SerializeField]
        private UnityEngine.Object selectableUnityObject;

        [SerializeField]
        private Rect selectionRect;

        #endregion ISelectable

        #endregion SerializedFields

        #region Private Fields

        private List<PVPFile> _childFiles;
        private bool _fold = false;
        private PVPFile[,] _groupedFiles;

        [NonSerialized]
        private List<PVPFolder> childFoldersToRemove = new List<PVPFolder>();

        private bool showRenameTextField;

        #endregion Private Fields

        #region Properties

        public List<PVPFile> ChildFiles { get => _childFiles; set => _childFiles = value; }
        public List<PVPFolder> ChildFolders { get => _childFolders; set => _childFolders = value; }
        public bool ChildFoldersAreTabs { get => _childFoldersAreTabs; private set => _childFoldersAreTabs = value; }
        public int Depth { get => _depth; set => _depth = value; }

        public List<PVPFile> FilesToRemove { get; set; }

        public string FolderPath { get => _folderPath; set => _folderPath = value; }

        public List<PVPFolder> FoldersToRemove { get => childFoldersToRemove; set => childFoldersToRemove = value; }

        public bool IsFile { get => isFile; private set => isFile = value; }

        public bool IsSelected { get => isSelected; set => isSelected = value; }

        public bool IsVisible { get => isVisible; set => isVisible = value; }

        public bool SortChildFiles { get; set; }

        public bool SortChildFolders { get; set; }

        public bool DeleteFlag { get; set; }
        public PVPFolder ParentFolder
        { get { return _parentFolder; } set { _parentFolder = value; } }

        public string Path { get => FolderPath; }
        public bool RepaintFlag { get => PVPWindow.RepaintFlag; set => PVPWindow.RepaintFlag = value; }
        public PVPFolder SelectableContextFolder { get => this; }
        public int SelectableIndex { get => selectableIndex; set => selectableIndex = value; }
        public UnityEngine.Object SelectableUnityObject { get => selectableUnityObject; set => selectableUnityObject = value; }
        public Rect SelectionRect { get => selectionRect; set => selectionRect = value; }

        #endregion Properties

        public PVPFolder(string folderPath, PVPFolder parentFolder, int depth)
        {
            #region Serialization

            //Add the folder to the allSelectables list und update the selectable index
            PVPSelection.allSelectables.Add(this);
            SelectableIndex = PVPSelection.allSelectables.Count - 1;

            //Add folder to allFolder list and update folder index
            PVPWindow.PVPData.allFolders.Add(this);
            SerializationInfo.folderIndex = PVPWindow.PVPData.allFolders.Count - 1;

            //Check if it's root folder
            if (parentFolder == null)
            {
                PVPWindow.PVPData.RootFolder = this;
            }
            else
            {
                //Set parent folder and parentFolder index
                ParentFolder = parentFolder;
                SerializationInfo.parentFolderIndex = parentFolder.SerializationInfo.folderIndex;

                //Add folder index to child folder indeces list of parent folder
                if (parentFolder.SerializationInfo.childFolderIndeces == null)
                    parentFolder.SerializationInfo.childFolderIndeces = new List<int>();

                parentFolder.SerializationInfo.childFolderIndeces.Add(SerializationInfo.folderIndex);
            }

            #endregion Serialization

            _folderPath = folderPath;
            _depth = depth;

            IsFile = false;

            _childFiles = FindChildFiles();
            ChildFolders = FindChildFolders();

            //Create the object by giving its path. Then get the assetpreview.
            SelectableUnityObject = AssetDatabase.LoadAssetAtPath(_folderPath, typeof(UnityEngine.Object));
            _folderIcon = PVPWindow.PVPSettings.FolderIcon;
            _foldoutIcon = PVPWindow.PVPSettings.FoldoutIcon;

            //Assets/New Folder-> folderName:New Folder
            string[] splitPath = _folderPath.Split('\\');
            _folderName = splitPath[splitPath.Length - 1];
            placeholderRenamingName = _folderName;

            _folderContent = new GUIContent(_folderName, _folderIcon, folderPath);
            _foldoutContent = new GUIContent(_foldoutIcon);
        }

        #region Visualization

        /// <summary>
        /// This method visualizes the folder and calls the VisualizeFile and VisualizeFolder method on it's child files and folders. If this method is called on the root folder it causes all files and folders in the directory to visualize themselves.
        /// </summary>
        public void VisualizeFolder()
        {
            IsVisible = true;

            GUILayout.BeginVertical();

            FoldoutWithFolder();

            if (_fold)
            {
                VisualizeChildFiles();

                foreach (var VARIABLE in ChildFolders)
                    VARIABLE.VisualizeFolder();
            }
            else
            {
                SetChildrenNotVisible();
            }

            GUILayout.EndVertical();

            //Remove the files after the visualization so the collections (childFiles and folders) don't get modified while looping through them

            RemoveChildFilesAndFolders();

            SortChildFilesAndFolders();
        }

        private void SortChildFilesAndFolders()
        {
            if (SortChildFiles)
            {
                ChildFolders.Sort();
                SortChildFiles = false;
            }
            if (SortChildFolders)
            {
                ChildFolders.Sort();
                SortChildFolders = false;
            }
        }

        /// <summary>
        /// Displays all gui elements that make up the folder and checks for different inputs
        /// </summary>
        private void FoldoutWithFolder()
        {
            GUILayout.BeginHorizontal();

            //Get rects for the selection box, foldout icon and folder label.

            SelectionRect = GUILayoutUtility.GetRect(PVPWindow.Position.width, PVPWindow.IconSize);
            Rect foldoutRect = new Rect(SelectionRect.x + _depth * 15, SelectionRect.y, PVPWindow.IconSize, PVPWindow.IconSize);
            Rect labelRect = new Rect(foldoutRect.position.x + PVPWindow.IconSize, foldoutRect.y, 300, PVPWindow.IconSize);

            //Check for all possible inputs.

            var evt = Event.current;

            if (CheckForRenamingInput(SelectionRect, foldoutRect))
            {
                showRenameTextField = true;
            }

            if (PVPSelection.CheckForSingleSelectionInput(this) && !foldoutRect.Contains(evt.mousePosition))
            {
                PVPSelection.SelectSingleElement(this);
            }
            if (PVPSelection.CheckForShiftSelectionInput(this) && !foldoutRect.Contains(evt.mousePosition))
            {
                PVPSelection.ShiftSelection(this);
            }

            if (PVPSelection.CheckForQtrlSelectInput(this))
            {
                PVPSelection.ControlSelect(this);
            }

            if (IsSelected)
            {
                PVPSelection.SetGUISkinToSelected();

                if (showRenameTextField)
                {
                    if (evt.keyCode == KeyCode.Return)
                    {
                        RenameFolderAndStopRenaming();
                    }
                    else if (evt.keyCode == KeyCode.Escape)
                    {
                        placeholderRenamingName = _folderName;
                        showRenameTextField = false;
                        PVPWindow.RepaintFlag = true;
                    }
                }
            }
            //If user was renaming the folder and then clicked on a different file / folder.
            else if (showRenameTextField)
            {
                RenameFolderAndStopRenaming();
            }

            GUI.Box(SelectionRect, "");

            if (showRenameTextField)
            {
                placeholderRenamingName = GUI.TextField(labelRect, placeholderRenamingName);
            }
            else
            {
                GUI.Label(labelRect, _folderContent);
            }

            //Visualize and rotate foldout icon

            var matrix = GUI.matrix;
            if (_fold)
            {
                GUIUtility.RotateAroundPivot(90, foldoutRect.center);
            }

            if (GUI.Button(foldoutRect, _foldoutContent))
            {
                _fold = !_fold;
            }
            GUI.matrix = matrix; //Return rotation back to normal

            CheckForDragAndDrop(labelRect);
            PVPContextMenu.DisplayContextMenu(this);
            DropAreaGUI(labelRect);
            PVPSelection.SetGUISkinToNormal();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void RemoveChildFilesAndFolders()
        {
            if (childFoldersToRemove != null && childFoldersToRemove.Count > 0)
            {
                foreach (var folder in childFoldersToRemove)
                {
                    ChildFolders.Remove(folder);
                }
                childFoldersToRemove.Clear();
            }

            if (FilesToRemove != null && FilesToRemove.Count > 0)
            {
                foreach (var file in FilesToRemove)
                {
                    ChildFiles.Remove(file);
                }
                FilesToRemove.Clear();
            }
        }

        #endregion Visualization

        #region Drag and Drop

        private void CheckForDragAndDrop(Rect dragArea)
        {
            var evt = Event.current;

            if (evt.type == EventType.MouseDrag && dragArea.Contains(evt.mousePosition))
            {
                PVPDragAndDrop.StartDrag();
            }
        }

        /// <summary>
        /// Check if a file/folder was dragged on to the folder
        /// </summary>
        private void DropAreaGUI(Rect dropArea)
        {
            Event evt = Event.current;

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(evt.mousePosition))
                        return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;

                    //Check if dragged element is a parent folder. If yes cancel the drag and drop operation

                    bool containsChildFolder = false;

                    foreach (var selectedElement in PVPSelection.SelectedElements)
                    {
                        containsChildFolder = IsParentFolder(selectedElement.Path);
                        if (containsChildFolder || ContainsSelectableWithSameName(selectedElement))
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                            return;
                        }
                    }

                    if (evt.type == EventType.DragPerform)
                    {
                        PVPDragAndDrop.AcceptDrag(this);
                    }
                    break;
            }
        }

        #endregion Drag and Drop

        #region Renaming

        private bool CheckForRenamingInput(Rect selectionRect, Rect foldoutRect)
        {
            var evt = Event.current;
            return isSelected && evt.keyCode == KeyCode.F2 && evt.type == EventType.KeyDown || isSelected && evt.type == EventType.MouseDown && evt.button == 0 && selectionRect.Contains(evt.mousePosition) && PVPSelection.SelectedElements.Count <= 1 && !foldoutRect.Contains(evt.mousePosition) && evt.clickCount == 1;
        }

        private void RenameFolderAndStopRenaming()
        {
            _folderName = placeholderRenamingName;
            _folderContent.text = _folderName;
            AssetDatabase.RenameAsset(FolderPath, _folderName);
            FolderPath = AssetDatabase.GetAssetPath(SelectableUnityObject);
            _folderContent.tooltip = FolderPath;

            PVPWindow.RepaintFlag = true;
            showRenameTextField = false;
            ParentFolder.SortChildFolders = true;
        }

        #endregion Renaming

        #region Child files and folders

        public bool ContainsSelectableWithSameName(ISelectable element)
        {
            var selectableName = element.SelectableUnityObject.name;

            foreach (var folder in ChildFolders)
            {
                if (folder.SelectableUnityObject.name == selectableName)
                {
                    return true;
                }
            }

            foreach (var file in ChildFiles)
            {
                if (file.SelectableUnityObject.name == selectableName)
                {
                    return true;
                }
            }
            return false;
        }

        public void AddChildFile(PVPFile file)
        {
            ChildFiles.Add(file);
            ChildFiles.Sort();

            SerializationInfo.childFileIndeces = new List<int>();


            for (int i = 0; i < ChildFiles.Count; i++)
            {
                SerializationInfo.childFileIndeces.Add(ChildFiles[i].FileSerializationInfo.fileIndex);
                ChildFiles[i].SelectableIndex = i + 1 + SelectableIndex;//
            }

            AdjustChildrenSelectableIndex();
        }

        public void AddChildFolder(PVPFolder folder)
        {
            folder.Depth = _depth + 1;

            ChildFolders.Add(folder);
            ChildFolders.Sort();

            SerializationInfo.childFolderIndeces = new List<int>();

            for (int i = 0; i < ChildFolders.Count; i++)
            {
                SerializationInfo.childFolderIndeces.Add(ChildFolders[i].SerializationInfo.folderIndex);
                ChildFolders[i].SelectableIndex = i + 1 + SelectableIndex + ChildFiles.Count;
            }

            AdjustChildrenSelectableIndex();
        }

        /// <summary>
        /// This method get's called after a file or folder was dragged and dropped. It adjusts the selectable indeces of its child files and folders and calls the AdjustChildrenSelectableIndex function on it's childFolders until there are none left.
        /// </summary>
        /// <param name="selectablesToAdjust">The root folder that calls this function leaves it to null. This list will be filled up with all children of the folder and passed down until there are no child folders left.</param>
        public void AdjustChildrenSelectableIndex(List<ISelectable> selectablesToAdjust = null)
        {
            if (selectablesToAdjust == null)
                selectablesToAdjust = new List<ISelectable>();

            for (int i = 0; i < ChildFiles.Count; i++)
            {
                ChildFiles[i].SelectableIndex = SelectableIndex + i + 1;
                selectablesToAdjust.Add(ChildFiles[i]);
            }

            if (ChildFolders == null || ChildFolders.Count == 0)
            {
                PVPSelection.MoveSelectables(selectablesToAdjust.ToArray());
                return;
            }

            for (int i = 0; i < ChildFolders.Count; i++)
            {
                ChildFolders[i].SelectableIndex = SelectableIndex + ChildFiles.Count + i + 1;
                selectablesToAdjust.Add(ChildFolders[i]);
            }

            foreach (var childFolder in ChildFolders)
            {
                childFolder.AdjustChildrenSelectableIndex(selectablesToAdjust);
            }
        }

        private List<PVPFile> FindChildFiles()
        {
            //GetFiles is similar but returns all the files under the path
            string[] fileNames = Directory.GetFiles(_folderPath);
            List<PVPFile> files = new List<PVPFile>();
            foreach (var file in fileNames)
            {
                PVPFile newfile = new PVPFile(file, this);
                //Pass meta files..
                if (newfile.Extension.Equals("meta"))
                {
                    SerializationInfo.childFileIndeces.Remove(newfile.FileSerializationInfo.fileIndex);
                    PVPWindow.PVPData.allFiles.Remove(newfile);
                    PVPSelection.allSelectables.Remove(newfile);
                    continue;
                }

                files.Add(newfile);
            }

            return files;
        }

        private List<PVPFolder> FindChildFolders()
        {
            //GetDirectories will return all the subfolders in the given path.
            string[] dirs = Directory.GetDirectories(_folderPath);
            List<PVPFolder> folders = new List<PVPFolder>();
            foreach (var directory in dirs)
            {
                //Turn all directories into our 'UnityFolder' Object.
                PVPFolder newfolder = new PVPFolder(directory, this, _depth + 1);
                folders.Add(newfolder);
            }
            return folders;
        }

        /// <summary>
        /// This method groups files by rows of how many files the user wants to display per row.
        /// </summary>
        private PVPFile[,] GroupChildFiles(List<PVPFile> files)
        {
            int size = files.Count;
            int rows = size / PVPWindow.PVPSettings.FilesPerRow + 1;
            _groupedFiles = new PVPFile[rows, PVPWindow.PVPSettings.FilesPerRow];
            int index = 0;
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < PVPWindow.PVPSettings.FilesPerRow; j++)
                    if (i * PVPWindow.PVPSettings.FilesPerRow + j <= size - 1)
                        _groupedFiles[i, j] = files[index++];
            return _groupedFiles;
        }

        private void SetChildrenNotVisible()
        {
            foreach (var childFile in ChildFiles)
            {
                childFile.IsVisible = false;
            }

            foreach (var childFolder in ChildFolders)
            {
                childFolder.IsVisible = false;
            }
        }

        private void VisualizeChildFiles()
        {
            int size = _childFiles.Count;
            int rows = size / PVPWindow.PVPSettings.FilesPerRow + 1;
            _groupedFiles = GroupChildFiles(_childFiles);
            int i = 0, j = 0;
            for (i = 0; i < rows; i++)
            {
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                for (j = 0; j < PVPWindow.PVPSettings.FilesPerRow; j++)
                {
                    if (i * PVPWindow.PVPSettings.FilesPerRow + j <= size - 1)
                        _groupedFiles[i, j].VisualizeFile(_depth);
                }

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
        }

        #endregion Child files and folders

        #region Utility

        /// <summary>
        /// Updates standard compare to function so that the folder name is used as a comparison basis
        /// </summary>
        public int CompareTo(PVPFolder other)
        {
            var value = _folderName.CompareTo(other.GetName());
            return value;
        }

        /// <summary>
        /// Deletes the folder with all it's child files and folders.
        /// </summary>
        public void Delete()
        {
            //Delete all child files
            if (ChildFiles != null && ChildFiles.Count > 0)
            {
                foreach (var file in ChildFiles)
                {
                    file.Delete();
                }
            }
            //Delete all child folders
            if (ChildFolders != null && ChildFolders.Count > 0)
            {
                foreach (var folder in ChildFolders)
                {
                    folder.Delete();
                }
            }
            //Remove folder from parent folder
            if (ParentFolder.FoldersToRemove == null)
                ParentFolder.FoldersToRemove = new List<PVPFolder>();

            ParentFolder.FoldersToRemove.Add(this);
            ParentFolder.SerializationInfo.childFolderIndeces.Remove(SerializationInfo.folderIndex);

            //Remove folder from allSelectables list
            PVPSelection.RemoveElement(this);

            //Remove folder from allFolders list
            PVPWindow.PVPData.RemoveFolder(this);

            //Delete folder asset
            AssetDatabase.DeleteAsset(FolderPath);

            AdjustChildrenSelectableIndex();
        }

        public int GetDepth()
        {
            return _depth;
        }

        public string GetName()
        {
            return _folderName;
        }

        public bool IsRootFolder()
        {
            if (_folderPath == "Assets")
            {
                return true;
            }
            return false;
        }

        public void Move(PVPFolder targetFolder)
        {
            if (targetFolder == ParentFolder)
                return;

            if (ParentFolder.FoldersToRemove == null)
                ParentFolder.FoldersToRemove = new List<PVPFolder>();
            ParentFolder.FoldersToRemove.Add(this); //Remove this folder from old parent folder
            ParentFolder.SerializationInfo.childFolderIndeces.Remove(SerializationInfo.folderIndex);

            var newPath = targetFolder.FolderPath + "\\" + _folderName;

            AssetDatabase.MoveAsset(FolderPath, newPath);

            FolderPath = newPath;

            ParentFolder = targetFolder;
            SerializationInfo.parentFolderIndex = ParentFolder.SerializationInfo.folderIndex;

            targetFolder.AddChildFolder(this);
            AdjustChildrenSelectableIndex();
        }

        private int GetSortedIndex(List<string> list, string value)
        {
            // Add the value to the list if it is not already there
            if (!list.Contains(value))
            {
                list.Add(value);
            }

            // Sort the list alphabetically
            list.Sort();

            // Find the index of the value in the sorted list
            return list.IndexOf(value);
        }

        private bool IsParentFolder(string parentFolder)
        {
            string parentFolderPath = System.IO.Path.GetFullPath(parentFolder);
            string childFolderPath = System.IO.Path.GetFullPath(FolderPath);
            // Check if the child folder is a subfolder of the parent folder
            // by checking if the child folder path starts with the parent folder path
            return childFolderPath.StartsWith(parentFolderPath, StringComparison.OrdinalIgnoreCase);
        }

        #endregion Utility

        #region For the future

        /// <summary>
        /// Not implemented yet
        /// </summary>
        private void AddFilesFromFilter()
        {
            List<PVPFile> files = new List<PVPFile>();
            foreach (var filter in _filters)
            {
                string[] guids = AssetDatabase.FindAssets(filter, new string[] { "Assets" });
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    PVPFile file = new PVPFile(path, this);

                    bool userWantsToMoveFile = false;
                    if (!file.IsChildOfRootFolder())
                    {
                        userWantsToMoveFile = EditorUtility.DisplayDialog("File conflict", $"File {file.GetNameWithExtension()} is currently in this path: {file.Path}. Do you want to move the file to {path}?", "Yes", "No");
                    }

                    if (!_childFiles.Contains(file) && userWantsToMoveFile)
                    {
                        _childFiles.Add(file);
                    }
                    Debug.Log("New file added : " + file.Path);
                }
            }
        }

        #endregion For the future
    }
}