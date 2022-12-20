using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectViewPlus
{
    /// <summary>
    /// This class represents a file in the project view plus window. It stores relevant data of the file it is representing, like its path, reference to the icon and more.
    /// </summary>
    [Serializable]
    public class PVPFile : ISelectable, IComparable<PVPFile>
    {
        public FileSerializationInfo FileSerializationInfo;

        #region SerializedFields

        [SerializeField]
        private string extension;

        [SerializeField]
        private GUIContent fileContent;

        [SerializeField]
        private Texture2D fileIcon;

        [SerializeField]
        private string fileName;

        [NonSerialized]
        private PVPFolder parentFolder;

        [SerializeField]
        private string path;

        #region ISelectable

        [SerializeField]
        private bool isFile;

        private bool isSelected;

        [SerializeField]
        private bool isVisible;

        [SerializeField]
        private string placeholderRenamingName;

        [SerializeField]
        private int selectableIndex;

        [SerializeField]
        private UnityEngine.Object selectableObject;

        [SerializeField]
        private Rect selectionRect;

        #endregion ISelectable

        #endregion SerializedFields

        #region Private Fields

        private bool showRenameTextField;

        #endregion Private Fields

        #region Properties

        public string Extension { get => extension; }
        public bool IsFile { get => isFile; private set => isFile = value; }
        public bool IsSelected { get => isSelected; set => isSelected = value; }
        public bool IsVisible { get => isVisible; set => isVisible = value; }
        public PVPFolder ParentFolder { get => parentFolder; set => parentFolder = value; }
        public string Path { get => path; }
        public bool RepaintFlag { get => PVPWindow.RepaintFlag; set => PVPWindow.RepaintFlag = value; }
        public PVPFolder SelectableContextFolder { get => ParentFolder; }
        public int SelectableIndex { get => selectableIndex; set => selectableIndex = value; }
        public UnityEngine.Object SelectableUnityObject { get => selectableObject; set => selectableObject = value; }
        public Rect SelectionRect { get => selectionRect; set => selectionRect = value; }

        #endregion Properties

        public PVPFile(string path, PVPFolder parentFolder)
        {
            //Add file to allSelectables list and update selectable index
            PVPSelection.allSelectables.Add(this);
            SelectableIndex = PVPSelection.allSelectables.Count - 1;

            //Add file to allFiles list and update file index
            PVPWindow.PVPData.allFiles.Add(this);
            FileSerializationInfo.fileIndex = PVPWindow.PVPData.allFiles.Count - 1;

            //Set the parent folder index
            FileSerializationInfo.parentFolderIndex = parentFolder.SerializationInfo.folderIndex;

            //Add file index to parentFolders childFileIndeces list
            if (parentFolder.SerializationInfo.childFileIndeces == null)
                parentFolder.SerializationInfo.childFileIndeces = new List<int>();

            parentFolder.SerializationInfo.childFileIndeces.Add(FileSerializationInfo.fileIndex);

            //Initialize all fields
            this.path = path;
            extension = FindExtension(path);
            fileName = FindFileName(path);
            placeholderRenamingName = fileName;
            IsFile = true;

            this.parentFolder = parentFolder;

            SelectableUnityObject = AssetDatabase.LoadAssetAtPath(this.path, typeof(UnityEngine.Object));
            fileIcon = AssetPreview.GetMiniThumbnail(SelectableUnityObject);

            fileContent = new GUIContent(fileName, fileIcon, path);
        }

        #region Methods

        #region Getter Methods

        public string GetNameWithExtension()
        {
            return fileName + extension;
        }

        public bool IsChildOfRootFolder()
        {
            return parentFolder.IsRootFolder();
        }

        #endregion Getter Methods

        #region Visualization

        /// <summary>
        /// This methods visualizes the file. Since this method get's called in the ongui event of the PVPWindow all gui event based checks like check if file gets selected happen here.
        /// </summary>
        public void VisualizeFile(int depth)
        {
            //Get rects
            SelectionRect = GUILayoutUtility.GetRect(0, PVPWindow.IconSize);
            Rect fileLabel = new Rect(SelectionRect.position.x + depth * 25, SelectionRect.position.y, 300, PVPWindow.IconSize);

            IsVisible = true;

            //Check for different inputs and execute logic accordingly
            if (CheckForRenamingInput(SelectionRect))
            {
                showRenameTextField = true;
            }

            if (PVPSelection.CheckForSingleSelectionInput(this))
            {
                PVPSelection.SelectSingleElement(this);
            }

            if (PVPSelection.CheckForOpenAssetInput(this))
            {
                AssetDatabase.OpenAsset(SelectableUnityObject);
            }

            if (PVPSelection.CheckForShiftSelectionInput(this))
            {
                PVPSelection.ShiftSelection(this);
            }

            if (PVPSelection.CheckForQtrlSelectInput(this))
            {
                PVPSelection.ControlSelect(this);
            }

            //If selected display selected guiskin.
            if (IsSelected)
            {
                PVPSelection.SetGUISkinToSelected();

                if (showRenameTextField)
                {
                    var evt = Event.current;
                    if (evt.keyCode == KeyCode.Return)
                    {
                        RenameFileAndStopRenaming();
                    }
                    else if (evt.keyCode == KeyCode.Escape)
                    {
                        placeholderRenamingName = fileName;
                        showRenameTextField = false;
                        PVPWindow.RepaintFlag = true;
                    }
                }
            }
            //If user was renaming and clicked away
            else if (showRenameTextField)
            {
                RenameFileAndStopRenaming();
            }
            GUI.Box(SelectionRect, "");
            if (showRenameTextField)
            {
                placeholderRenamingName = GUI.TextField(fileLabel, placeholderRenamingName);
            }
            else
            {
                GUI.Label(fileLabel, fileContent);
            }

            PVPSelection.SetGUISkinToNormal();

            //Check for ContextMenu input and display if needed
            PVPContextMenu.DisplayContextMenu(this);

            //Check for drag and drop input and execute if needed
            CheckForDragAndDrop(SelectionRect);
        }

        #endregion Visualization

        #region Utility

        /// <summary>
        /// Deletes the file and removes all references that other scripts had to it.
        /// </summary>
        public void Delete()
        {
            if (parentFolder.FilesToRemove == null)
                parentFolder.FilesToRemove = new List<PVPFile>();

            parentFolder.FilesToRemove.Add(this); //Remove this file from old parent folder
            parentFolder.SerializationInfo.childFileIndeces.Remove(FileSerializationInfo.fileIndex);
            PVPWindow.PVPData.RemoveFile(this);
            PVPSelection.RemoveElement(this);

            AssetDatabase.DeleteAsset(path);
        }

        private bool CheckForRenamingInput(Rect selectionRect)
        {
            var evt = Event.current;
            return isSelected && evt.keyCode == KeyCode.F2 && evt.type == EventType.KeyDown || isSelected && evt.type == EventType.MouseDown && evt.button == 0 && selectionRect.Contains(evt.mousePosition) && PVPSelection.SelectedElements.Count <= 1 && evt.clickCount == 1;
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

        private void RenameFileAndStopRenaming()
        {
            fileName = placeholderRenamingName;
            fileContent.text = fileName;
            AssetDatabase.RenameAsset(Path, fileName);
            path = AssetDatabase.GetAssetPath(SelectableUnityObject);
            fileContent.tooltip = path;

            PVPWindow.RepaintFlag = true;
            showRenameTextField = false;
            parentFolder.SortChildFiles = true;
        }

        #endregion Utility

        #region Drag and Drop

        /// <summary>
        /// Updates standard compare to function so that the fileName is used as a comparison basis
        /// </summary>
        public int CompareTo(PVPFile other)
        {
            return fileName.CompareTo(other.GetNameWithExtension());
        }

        /// <summary>
        /// Move the file to a new folder. Makes sure all references to the file are updated accordingly
        /// </summary>
        public void Move(PVPFolder targetFolder)
        {
            if (targetFolder == parentFolder)
                return;

            if (parentFolder.FilesToRemove == null)
                parentFolder.FilesToRemove = new List<PVPFile>();

            parentFolder.FilesToRemove.Add(this); //Remove this file from old parent folder
            parentFolder.SerializationInfo.childFileIndeces.Remove(FileSerializationInfo.fileIndex);

            //Set new path
            var newPath = targetFolder.FolderPath + "\\" + fileName + "." + extension;

            //Actually move the file
            AssetDatabase.MoveAsset(path, newPath);

            //Update fields
            path = newPath;

            parentFolder = targetFolder;
            FileSerializationInfo.parentFolderIndex = parentFolder.SerializationInfo.folderIndex;

            //Add file to new parent folder
            targetFolder.AddChildFile(this);
        }

        private void CheckForDragAndDrop(Rect dragArea)
        {
            var evt = Event.current;

            if (evt.type == EventType.MouseDrag && dragArea.Contains(evt.mousePosition))
            {
                PVPDragAndDrop.StartDrag();
            }
        }

        #endregion Drag and Drop

        #endregion Methods
    }
}