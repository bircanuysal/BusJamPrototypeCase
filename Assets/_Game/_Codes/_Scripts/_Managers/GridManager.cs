using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager
{
    private GameObject gridCellPrefab;
    private GameObject decisionWallPrefab;
    private GameObject busWaitingGridsPrefab;
    private Transform gridParent;
    private Dictionary<Vector2Int, GameObject> GridObjects;

    public GridManager(GameObject gridCellPrefab, GameObject decisionWallPrefab, GameObject busWaitingGridsPrefab, Transform gridParent)
    {
        this.gridCellPrefab = gridCellPrefab;
        this.gridParent = gridParent;
        this.decisionWallPrefab = decisionWallPrefab;
        this.busWaitingGridsPrefab = busWaitingGridsPrefab;
        GridObjects = new Dictionary<Vector2Int, GameObject>();
    }

    public void GenerateGrid(int gridWidth, int gridHeight, LevelData levelData)
    {
        DestroyChildren(gridParent);
        GridObjects.Clear();

        float gridScaleFactor = gridCellPrefab.transform.localScale.x;
        float cellOffset = gridScaleFactor + 0.1f; // Gridler arası boşluk

        List<GameObject> lastRowGrids = new List<GameObject>();

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Vector2Int cellPosition = new Vector2Int(x, y);
                Vector3 worldPosition = new Vector3(x * cellOffset, 0, y * cellOffset);

                GameObject gridObj = Object.Instantiate(gridCellPrefab, worldPosition, Quaternion.identity, gridParent);
                gridObj.name = $"GridCell_{x}_{y}";
                gridObj.transform.localScale = new Vector3(gridScaleFactor, 0.1f, gridScaleFactor);
                Grid gridComponent = gridObj.GetComponent<Grid>();
                GridCell foundCell = levelData?.gridData.FirstOrDefault(cell => cell.position == cellPosition);
                Colors cellColor = (foundCell != null) ? foundCell.color : Colors.White;
                bool isBlocked = (foundCell != null);
                gridComponent.GridCell = new GridCell
                {
                    position = cellPosition,
                    color = cellColor,
                    isBlocked = isBlocked
                };

                GridObjects[cellPosition] = gridObj;

                if (y == gridHeight - 1)
                {
                    lastRowGrids.Add(gridObj);
                }
            }
        }

        SpawnEnvironmentProps(lastRowGrids, gridScaleFactor);

        Debug.Log($"Grid oluşturuldu: {gridWidth}x{gridHeight} ve ortadaki gridin 3 birim önüne Cube spawnlandı.");
    }

    public void SpawnEnvironmentProps(List<GameObject> lastRowGrids, float gridScaleFactor)
    {
        if (lastRowGrids.Count == 0) return;

        float forwardFactor = 2f;
        int middleIndex = lastRowGrids.Count / 2;
        GameObject middleGrid = lastRowGrids[middleIndex];

        float positionOffsetX = (lastRowGrids.Count % 2 == 0) ? -gridScaleFactor / 2 : 0;
        Vector3 basePosition = middleGrid.transform.position + new Vector3(positionOffsetX, 0, 0);

        GameManager gameManager = GameManager.Instance;
        GameObject decisionWall = Object.Instantiate(decisionWallPrefab, basePosition + Vector3.forward * forwardFactor, Quaternion.identity, gridParent);
        Object.Instantiate(busWaitingGridsPrefab, basePosition + Vector3.forward * (forwardFactor * 2), Quaternion.identity, gridParent);
        gameManager.SetDecisionWall(decisionWall.transform);
        gameManager.SetBusStationPos(basePosition + Vector3.forward * (forwardFactor * 4));
    }


    public Dictionary<Vector2Int, GameObject> GetGridObjects()
    {
        return GridObjects;
    }

    private void DestroyChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Object.DestroyImmediate(child.gameObject);
        }
    }
}
