using UnityEngine;

namespace ProjectViewPlus
{
    /// <summary>
    /// An interface for all selectable elements in project view plus. (Files and Folders)
    /// </summary>
    public interface ISelectable
    {
        /// <summary>
        /// The unity object the selectable is representing
        /// </summary>
        public UnityEngine.Object SelectableUnityObject { get; set; }

        /// <summary>
        /// Path of the selectable element
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Parent folder of the selectable element
        /// </summary>
        public PVPFolder ParentFolder { get; }

        /// <summary>
        /// The rect that is used for checking if a selectable got clicked by the mouse in the gui
        /// </summary>
        public Rect SelectionRect { get; set; }

        /// <summary>
        /// Is the element currently visible in the gui
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Is element selected
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Is it a file or a folder
        /// </summary>
        public bool IsFile { get; }

        /// <summary>
        /// Index of the element in the allSelectables list
        /// </summary>
        public int SelectableIndex { get; set; }

        /// <summary>
        /// The window will get repainted at the end of ongui if set to true
        /// </summary>
        public bool RepaintFlag { get; set; }

        /// <summary>
        /// The folder that the context menu uses as a reference point. For files it's the parent folder. For folders it's themselves.
        /// </summary>
        public PVPFolder SelectableContextFolder { get; }

        /// <summary>
        /// Move the element to a new folder
        /// </summary>
        public void Move(PVPFolder targetFolder);

        /// <summary>
        /// Delete the element.
        /// </summary>
        public void Delete();
    }
}