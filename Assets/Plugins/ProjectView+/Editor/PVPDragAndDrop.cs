using System.Collections.Generic;
using UnityEditor;

/// <summary>
/// Helper class that interacts with Unitys DragAndDrop class.
/// </summary>
public static class PVPDragAndDrop
{
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

    public static void AcceptDrag(PVPFolder targetFolder)
    {
        DragAndDrop.AcceptDrag();
        foreach (var selectable in PVPSelection.SelectedElements)
        {
            if (selectable == targetFolder)
                return;

            selectable.Move(targetFolder);
        }
    }

    //TODO: Finish drag and drop
}