using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class PVPSelection
{
    public static List<ISelectable> allSelectables = new List<ISelectable>();
    public static List<ISelectable> SelectedElements = new List<ISelectable>();


    public static void SelectSingleElement(ISelectable element)
    {
        UnselectAllSelectedElements();


        SelectedElements = new List<ISelectable> { element };
        Selection.activeObject = element.SelectableUnityObject;
        element.IsSelected = true;
        SetGUISkinToSelected();
        PVPEvents.InvokeRepaintWindowEvent();
    }

    public static void AddElement(ISelectable element)
    {

        SelectedElements.Add(element);

        element.IsSelected = true;
        element.RepaintFlag = true;

        List<Object> selectedObjects = Selection.objects.ToList<Object>();

        selectedObjects.Add(element.SelectableUnityObject);

        Selection.objects = selectedObjects.ToArray();
    }

    private static void RemoveElement(ISelectable element)
    {
        SelectedElements.Remove(element);
        element.IsSelected = false;
        element.RepaintFlag = true;

        List<Object> selectedObjects = Selection.objects.ToList<Object>();

        selectedObjects.Remove(element.SelectableUnityObject);

        Selection.objects = selectedObjects.ToArray();
    }

    public static void ShiftSelect(ISelectable element)
    {
        int clickedIndex = element.SelectableIndex;
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

    public static void ControlSelect(ISelectable element)
    {
        if (SelectedElements.Contains(element))
        {
            RemoveElement(element);
        }
        else
        {
            AddElement(element);
        }
    }

    public static void UnselectAllSelectedElements()
    {
        foreach (var selectedElement in SelectedElements)
        {
            selectedElement.IsSelected = false;
            selectedElement.RepaintFlag = true;
        }

        SelectedElements.Clear();
    }

    public static bool CheckForShiftSelectInput(ISelectable element)
    {
        var evt = Event.current;
        return evt.type == EventType.MouseDown && evt.shift && element.SelectionRect.Contains(evt.mousePosition);
    }

    public static bool CheckForQtrlSelectInput(ISelectable element)
    {
        var evt = Event.current;
        return evt.type == EventType.MouseDown && evt.control && element.SelectionRect.Contains(evt.mousePosition);
    }

    public static bool CheckForSingleSelectionInput(ISelectable element)
    {
        var evt = Event.current;
        return evt.type == EventType.MouseDown && !evt.shift && !evt.control && element.SelectionRect.Contains(evt.mousePosition) && !SelectedElements.Contains(element) || SelectedElements.Contains(element) && evt.type == EventType.MouseUp && element.SelectionRect.Contains(evt.mousePosition) && !evt.shift && !evt.control;

    }

    public static bool CheckForOpenAssetInput(ISelectable element)
    {
        var evt = Event.current;
        return evt.type == EventType.MouseDown && evt.clickCount >= 2 && element.SelectionRect.Contains(evt.mousePosition);
    }

    public static bool IsElementSelected(ISelectable element)
    {
        return SelectedElements.Contains(element);
    }

    public static void SetGUISkinToSelected()
    {
        GUI.skin.box.normal.background = PVPWindow.PVPData.PVPSettings.SelectedBackground;
        GUI.skin.box.hover.background = PVPWindow.PVPData.PVPSettings.SelectedBackground;
    }

    public static void SetGUISkinToNormal()
    {
        GUI.skin.box.normal.background = PVPWindow.PVPData.PVPSettings.NormalBackground;
        GUI.skin.box.hover.background = PVPWindow.PVPData.PVPSettings.NormalBackground;
    }

    public static void MoveSelectable(ISelectable selectable)
    {
        allSelectables.Move<ISelectable>(selectable, selectable.SelectableIndex);

        for (int i = 0; i < allSelectables.Count - 1; i++)
        {
            allSelectables[i].SelectableIndex = i;
        }
    }

    public static void MoveSelectables(ISelectable[] selectables)
    {
        foreach (var selectable in selectables)
        {
            allSelectables.Move<ISelectable>(selectable, selectable.SelectableIndex);
        }

        for (int i = 0; i < allSelectables.Count - 1; i++)
        {
            allSelectables[i].SelectableIndex = i;
        }
    }

}
