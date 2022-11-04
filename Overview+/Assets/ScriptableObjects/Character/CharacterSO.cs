using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "ScriptableObjects/Character",order =10)]
public class CharacterSO : ScriptableObject
{
    public Sprite Sprite;
    public string Name;
    public float MaxHealth;
    public float Speed;
    public float Strength;
}
