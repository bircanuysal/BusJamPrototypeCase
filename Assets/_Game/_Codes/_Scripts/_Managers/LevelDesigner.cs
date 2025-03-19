using System.Collections.Generic;
using UnityEngine;

public class LevelDesigner : LocalSingleton<LevelDesigner>
{

    [Header("Dependencies")]
    public LevelData levelData;
    public LevelEditorDatas levelEditorDatas;

    [Header("Parents")]
    public Transform gridParent;
    public Transform busParent;
    public Transform charactersParent;

    [Header("Prefabs")]
    public GameObject gridCellPrefab;
    public GameObject busPrefab;
    public GameObject stickmanPrefab;
    public GameObject decisionWallPrefab;
    public GameObject busWaitingGridsPrefab;

    private GridManager gridManager;
    private BusManager busManager;
    private StickmanManager stickmanManager;

    public int gridWidth = 5;
    public int gridHeight = 5;
    public float levelTime = 5;

    protected override void Awake()
    {
        base.Awake();
        InıtGrids();
        InıtStickmans();
        InıtBuses();
        GameManager.Instance.SetLevelTime(levelTime);
    }
    private void InıtGrids()
    {
        gridManager = new GridManager(gridCellPrefab, decisionWallPrefab, busWaitingGridsPrefab, gridParent);
        DestroyAllChildren(gridParent);
        gridManager.GenerateGrid(levelData.gridWidth, levelData.gridHeight,levelData);
    }
    private void InıtStickmans()
    {
        stickmanManager = new StickmanManager(stickmanPrefab, charactersParent);
        DestroyAllChildren(charactersParent);
        stickmanManager.SpawnStickmen(levelData.gridData, gridManager.GetGridObjects());
    }
    private void InıtBuses()
    {
        busManager = new BusManager(busPrefab, busParent);
        DestroyAllChildren(busParent);
        busManager.SpawnBuses(levelData.busQueue);
    }
    public void GenerateLevel()
    {
        if (levelData == null)
        {
            Debug.LogError("LevelData atanmadı!");
            return;
        }
        InıtGrids();
        InıtStickmans();
        InıtBuses();
        GameManager.Instance.SetLevelTime(levelTime);

        //stickmanManager.SpawnStickmen(levelData.gridData, gridManager.GetGridObjects());
        //busManager.SpawnBuses(levelData.busQueue);

        Debug.Log("Level sahneye başarıyla aktarıldı!");
    }

    private void DestroyAllChildren(Transform parent)
    {
        if (parent == null)
        {
            Debug.LogWarning("Parent nesne atanmadı, işlem iptal edildi.");
            return;
        }

        int childCount = parent.childCount;

        for (int i = childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            Object.DestroyImmediate(parent.GetChild(i).gameObject); // 🎯 Editor modunda çalışırken
#else
        Destroy(parent.GetChild(i).gameObject); // 🎯 Oyun çalışırken
#endif
        }

        Debug.Log($"{parent.name} altındaki {childCount} nesne yok edildi.");
    }

    public GridManager GetGridManager()
    {
        if (gridManager == null)
        {
            Debug.LogError("yok üretiliyor");
            gridManager = new GridManager(gridCellPrefab, decisionWallPrefab, busWaitingGridsPrefab, gridParent);
        }
        return gridManager;
    }

}
