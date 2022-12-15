using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PVPEvents
{
    public static event Action RepaintWindowEvent;

    public static void InvokeRepaintWindowEvent()
    {
        if (RepaintWindowEvent != null)
            RepaintWindowEvent();
    }

}
