using System;

public static class PVPEvents
{
    public static event Action RepaintWindowEvent;

    public static void InvokeRepaintWindowEvent()
    {
        if (RepaintWindowEvent != null)
            RepaintWindowEvent();
    }
}