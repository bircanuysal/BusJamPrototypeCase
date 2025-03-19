using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridCell
{
    public Vector2Int position;
    public Colors color;
    public bool isBlocked;
}

[System.Serializable]
public class SpriteCell
{
    public Vector2Int position;
    public Sprite sprite;
    public Colors[] colors;
}

[System.Serializable]
public class LevelData : ScriptableObject
{
    public LevelEditorDatas levelEditorDatas;

    public float levelTimer;
    public List<BusConfig> busQueue = new List<BusConfig>();

    public List<GridCell> gridData = new List<GridCell>();  // Grid renkleri
    public List<SpriteCell> spriteData = new List<SpriteCell>(); // Sprite verileri , spawner icin yetismedi.

    public int gridWidth;
    public int gridHeight;

}

