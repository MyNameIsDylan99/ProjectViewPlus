using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Helper class that interacts with Unitys DragAndDrop class.
/// </summary>
public static class PVPDragAndDrop
{
    public static PVPFile DraggedFile { get; private set; }

    public static void StartDrag(PVPFile draggedFile)
    {
        DraggedFile = draggedFile;
        DragAndDrop.StartDrag("PVPDragAndDrop");
        DragAndDrop.visualMode = DragAndDropVisualMode.Move;
    }

    public static void AcceptDrag(PVPFolder targetFolder)
    {

        DragAndDrop.AcceptDrag();
        var fileIndex = DraggedFile.RemoveAllFileReferences();

        var oldPath = DraggedFile.GetPath();
        var splitName = oldPath.Split('\\');
        var fileName = splitName[splitName.Length - 1];
        var newPath = targetFolder.FolderPath + "\\" + fileName;
        AssetDatabase.MoveAsset(oldPath, newPath);

        targetFolder.ChildFiles.Add(new PVPFile(newPath, targetFolder, fileIndex));
    }
}
