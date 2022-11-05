using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NSC", menuName = "Overview+/NPC", order = 10)]
public class NPC_SO : ScriptableObject
{
    public Sprite Sprite;
    public Vector3 Spawnpunkt;
    public string[] Dialoge;
    public MissionSO[] Missions;
}
