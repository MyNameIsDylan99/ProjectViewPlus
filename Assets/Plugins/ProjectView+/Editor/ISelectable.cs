using System;
using UnityEditor;
using UnityEngine;

public interface ISelectable
{

    public UnityEngine.Object SelectableObject {get; set; }

public ISelectable ParentSelectable { get;}

    public Rect SelectionRect { get; set; }

    public bool IsVisible { get; set; }

    public bool IsSelected { get; set; }

    public int SelectableIndex { get; set; }

    public bool RepaintFlag { get; set; }

    public void Move(PVPFolder targetFolder);
}
