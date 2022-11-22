using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Category", menuName = "Overview+/Category")]
public class CategorySO : ScriptableObject
{
    public string Name;
    public string[] Filters;

}

