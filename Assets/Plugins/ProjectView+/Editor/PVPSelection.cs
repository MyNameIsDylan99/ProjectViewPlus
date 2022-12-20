using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ProjectViewPlus
{
    /// <summary>
    /// Static class that handles all logic related to the selection of files and folders.
    /// </summary>
    public static class PVPSelection
    {
        public static List<ISelectable> allSelectables = new List<ISelectable>();
        public static List<ISelectable> SelectedElements = new List<ISelectable>();

        /// <summary>
        /// Add a new element to the selection.
        /// </summary>
        /// <param name="element"></param>
        public static void AddElement(ISelectable element)
        {
            SelectedElements.Add(element);

            element.IsSelected = true;
            element.RepaintFlag = true;

            List<Object> selectedObjects = Selection.objects.ToList();

            selectedObjects.Add(element.SelectableUnityObject);

            Selection.objects = selectedObjects.ToArray();
        }

        /// <summary>
        /// Check if the delete key was pressed. If yes open file deletion dialogue.
        /// </summary>
        public static void CheckDeleteKey()
        {
            // Check if the delete key was pressed
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Delete)
            {
                // Display a confirmation dialog
                if (EditorUtility.DisplayDialog("Delete selected file(s)?", "Are you sure you want to delete the selected file(s)?", "Delete", "Cancel"))
                {
                    // Delete the selected file
                    DeleteSelectedFiles();
                }
            }
        }

        private static void DeleteSelectedFiles()
        {
            foreach (var element in SelectedElements)
            {
                element.Delete();
            }
        }

        /// <summary>
        /// Check if the element should be opened. Standard is double click on selection rect.
        /// </summary>
        public static bool CheckForOpenAssetInput(ISelectable element)
        {
            var evt = Event.current;
            return evt.type == EventType.MouseDown && evt.clickCount >= 2 && element.SelectionRect.Contains(evt.mousePosition);
        }

        /// <summary>
        /// Returns true if control selection conditions are met.
        /// </summary>
        public static bool CheckForQtrlSelectInput(ISelectable element)
        {
            var evt = Event.current;
            return evt.type == EventType.MouseDown && evt.control && element.SelectionRect.Contains(evt.mousePosition);
        }

        /// <summary>
        /// Returns true if shift selection conditions are met.
        /// </summary>
        public static bool CheckForShiftSelectionInput(ISelectable element)
        {
            var evt = Event.current;
            return evt.type == EventType.MouseDown && evt.shift && element.SelectionRect.Contains(evt.mousePosition);
        }

        /// <summary>
        /// Returns true if single selection conditions are met.
        /// </summary>
        public static bool CheckForSingleSelectionInput(ISelectable element)
        {
            var evt = Event.current;
            return evt.type == EventType.MouseDown && !evt.shift && !evt.control && element.SelectionRect.Contains(evt.mousePosition) && !SelectedElements.Contains(element) || SelectedElements.Contains(element) && evt.type == EventType.MouseUp && element.SelectionRect.Contains(evt.mousePosition) && !evt.shift && !evt.control;
        }

        /// <summary>
        /// If element is already selected. Unselected it. Else select it.
        /// </summary>
        /// <param name="element"></param>
        public static void ControlSelect(ISelectable element)
        {
            if (SelectedElements.Contains(element))
            {
                UnselectElement(element);
            }
            else
            {
                AddElement(element);
            }
        }

        /// <summary>
        /// Moves the selectable to the index defined on "SelectableIndex".
        /// </summary>
        /// <param name="selectable"></param>
        public static void MoveSelectable(ISelectable selectable)
        {
            allSelectables.Move(selectable, selectable.SelectableIndex);

            for (int i = 0; i < allSelectables.Count; i++)
            {
                allSelectables[i].SelectableIndex = i;
            }
        }

        public static void MoveSelectables(ISelectable[] selectables)
        {
            foreach (var selectable in selectables)
            {
                allSelectables.Move(selectable, selectable.SelectableIndex);
            }

            for (int i = 0; i < allSelectables.Count - 1; i++)
            {
                allSelectables[i].SelectableIndex = i;
            }
        }

        /// <summary>
        /// Unselect all previously selected elements and select the element in the parameter.
        /// </summary>
        public static void SelectSingleElement(ISelectable element)
        {
            UnselectAllSelectedElements();

            SelectedElements = new List<ISelectable> { element };
            Selection.activeObject = element.SelectableUnityObject;
            element.IsSelected = true;
        }

        public static void SetGUISkinToNormal()
        {
            GUI.skin.box.normal.background = PVPWindow.PVPSettings.NormalBackground;
            GUI.skin.box.hover.background = PVPWindow.PVPSettings.NormalBackground;
        }

        public static void SetGUISkinToSelected()
        {
            GUI.skin.box.normal.background = PVPWindow.PVPSettings.SelectedBackground;
            GUI.skin.box.hover.background = PVPWindow.PVPSettings.SelectedBackground;
        }

        /// <summary>
        /// Performs shift selection just like you know it from the file explorer or default unity project view.
        /// </summary>
        public static void ShiftSelection(ISelectable element)
        {
            int clickedIndex = element.SelectableIndex;

            //Create a list with all indices of the currently selected elements. (indices correspond to indices in allSelectables list.)

            List<int> selectedIndeces = new List<int>();

            for (int i = 0; i < SelectedElements.Count; i++)
            {
                if (SelectedElements[i].IsVisible)
                    selectedIndeces.Add(SelectedElements[i].SelectableIndex);
            }

            if (selectedIndeces == null || selectedIndeces.Count < 1)
            {
                AddElement(element);
                return;
            }

            //Determine smallest and biggest indices

            int smallestVisualIndex = selectedIndeces.Min();
            int biggestVisualIndex = selectedIndeces.Max();

            // Calculate the distance of the clicked index from the smallest and biggest
            // selected indices
            int smallerIndexDist = clickedIndex - smallestVisualIndex;
            int largerIndexDist = biggestVisualIndex - clickedIndex;

            // Determine which end of the selected range the clicked index is closest to
            int smallerIndex, largerIndex;
            if (smallerIndexDist < largerIndexDist)
            {
                smallerIndex = clickedIndex;
                largerIndex = biggestVisualIndex;
            }
            else
            {
                smallerIndex = smallestVisualIndex;
                largerIndex = clickedIndex;
            }

            //Clear old selectedElements list and add new selected items.

            UnselectAllSelectedElements();

            for (int i = smallerIndex; i <= largerIndex; i++)
            {
                var selectable = allSelectables[i];

                if (selectable.IsVisible)
                {
                    AddElement(selectable);
                }
            }
        }

        /// <summary>
        /// Unselect all currently selected elements.
        /// </summary>
        public static void UnselectAllSelectedElements()
        {
            foreach (var selectedElement in SelectedElements)
            {
                selectedElement.IsSelected = false;
                selectedElement.RepaintFlag = true;
            }

            SelectedElements.Clear();
        }

        private static void UnselectElement(ISelectable element)
        {
            SelectedElements.Remove(element);
            element.IsSelected = false;
            element.RepaintFlag = true;

            List<Object> selectedObjects = Selection.objects.ToList();

            selectedObjects.Remove(element.SelectableUnityObject);

            Selection.objects = selectedObjects.ToArray();
        }

        /// <summary>
        /// Remove element from allSelectables list and update index of the remaining elements to their new index.
        /// </summary>
        /// <param name="element"></param>
        public static void RemoveElement(ISelectable element)
        {
            allSelectables.Remove(element);
            for (int i = 0; i < allSelectables.Count; i++)
            {
                allSelectables[i].SelectableIndex = i;
            }
        }
    }
}