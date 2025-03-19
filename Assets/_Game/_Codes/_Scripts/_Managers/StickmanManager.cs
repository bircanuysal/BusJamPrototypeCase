using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.Port;

public class StickmanManager
{
    private GameObject stickmanPrefab;
    private Transform charactersParent;

    public StickmanManager(GameObject stickmanPrefab, Transform charactersParent)
    {
        this.stickmanPrefab = stickmanPrefab;
        this.charactersParent = charactersParent;
    }

    public void SpawnStickmen(List<GridCell> gridData, Dictionary<Vector2Int, GameObject> gridObjects)
    {
        DestroyChildren(charactersParent);
        foreach (var cell in gridData)
        {
            if (!gridObjects.ContainsKey(cell.position)) continue;
            if (cell.color == Colors.White || cell.color == Colors.Gray) continue;

            GameObject gridObj = gridObjects[cell.position];
            GameObject stickmanObj = Object.Instantiate(stickmanPrefab, gridObj.transform.position, Quaternion.identity, charactersParent);
            stickmanObj.transform.parent = charactersParent;


            Unit unit = stickmanObj.GetComponent<Unit>();
            if (unit != null)
            {
                unit.Initialize(cell.color);
            }
            else
            {
                Debug.LogError("StickmanDataLoader bileþeni eksik!");
            }
        }

        Debug.Log("Stickman'ler sahneye eklendi!");
    }

    private void DestroyChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Object.DestroyImmediate(child.gameObject);
        }
    }
}
