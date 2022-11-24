using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PVPEvents
{
    public static event Action PVPWindowEnabledEvent;

    public static void InvokePVPWindowEnabledEvent()
    {
        if(PVPWindowEnabledEvent != null)
        {
            PVPWindowEnabledEvent();
        }
    }
}
