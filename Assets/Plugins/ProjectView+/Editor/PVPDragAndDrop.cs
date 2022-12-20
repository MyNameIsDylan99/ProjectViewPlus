using System.Collections.Generic;
using UnityEditor;

namespace ProjectViewPlus
{
    /// <summary>
    /// Helper class that interacts with Unitys DragAndDrop class.
    /// </summary>
    public static class PVPDragAndDrop
    {
        /// <summary>
        /// Start the drag and drop operation
        /// </summary>
        public static void StartDrag()
        {
            DragAndDrop.StartDrag("PVPDragAndDrop");
            var objReferences = new List<UnityEngine.Object>();
            foreach (var selectable in PVPSelection.SelectedElements)
            {
                objReferences.Add(selectable.SelectableUnityObject);
            }
            DragAndDrop.objectReferences = objReferences.ToArray();
            DragAndDrop.visualMode = DragAndDropVisualMode.Move;
        }

        /// <summary>
        /// Accepts the drag and drop operation and moves all selected files and folders to the folder they were dragged to
        /// </summary>
        public static void AcceptDrag(PVPFolder targetFolder)
        {
            DragAndDrop.AcceptDrag();

            foreach (var selectable in PVPSelection.SelectedElements)
            {
                if (selectable == targetFolder)
                    return;

                //Changing gui layout elements between the layout event and the repaint event(like drag and dropping files and folders) causes errors. This is a fix for this:
                PVPWindow.LayoutEventActions.Add(new LayoutEventAction(selectable, targetFolder));
            }
        }
    }

    public class LayoutEventAction
    {
        private ISelectable element;
        private PVPFolder targetFolder;

        public LayoutEventAction(ISelectable element, PVPFolder targetFolder)
        {
            this.element = element;
            this.targetFolder = targetFolder;
        }

        public void Execute()
        {
            element.Move(targetFolder);
        }
    }
}