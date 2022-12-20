using System;

namespace ProjectViewPlus
{
    /// <summary>
    /// Container for all project view plus specific events
    /// </summary>
    public static class PVPEvents
    {
        public static event Action RepaintWindowEvent;

        public static void InvokeRepaintWindowEvent()
        {
            if (RepaintWindowEvent != null)
                RepaintWindowEvent();
        }
    }
}