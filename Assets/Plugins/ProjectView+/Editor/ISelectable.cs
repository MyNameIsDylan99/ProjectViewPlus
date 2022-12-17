using UnityEngine;

public interface ISelectable
{
    public UnityEngine.Object SelectableUnityObject { get; set; }

    public string Path { get; }

    public PVPFolder ParentFolder { get; }

    public Rect SelectionRect { get; set; }

    public bool IsVisible { get; set; }

    public bool IsSelected { get; set; }

    public bool IsFile { get; }

    public int SelectableIndex { get; set; }

    public bool RepaintFlag { get; set; }

    public PVPFolder SelectableContextFolder { get; }

    public void Move(PVPFolder targetFolder);

    public void Delete();
}