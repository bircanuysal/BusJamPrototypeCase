using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    private List<Colors> assignedColors;

    public void Initialize(List<Colors> colors)
    {
        assignedColors = colors;
        Debug.Log($"Spawner initialized with user-selected colors: {string.Join(", ", assignedColors)}");
    }
}
