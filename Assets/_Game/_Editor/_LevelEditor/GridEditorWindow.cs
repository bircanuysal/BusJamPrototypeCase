using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GridEditorWindow : EditorWindow
{
    private static LevelData levelData;
    private static int gridWidth;
    private static int gridHeight;
    private static Dictionary<Vector2Int, Colors> gridColors;
    private static Dictionary<Vector2Int, Sprite> gridSprites;
    public static Dictionary<Vector2Int, Sprite> GridSprites => gridSprites;

    private static Dictionary<Colors, int> requiredStickmen;
    private static Dictionary<Colors, int> currentPaintedCount = new Dictionary<Colors, int>();
    private Vector2 scrollPosition;
    private Colors? selectedColor = null;
    private bool isPainting = false;
    private bool isErasing = false;
    private bool isObstacleMode = false;
    private bool isSpawnerFieldMode = false; // Yeni Mod: Sprite Yerleştirme
    private static Sprite selectedSprite = null; // Kullanıcının seçeceği sprite

    public static void ShowWindow(int width, int height, Dictionary<Vector2Int, Colors> existingGridColors, Dictionary<Colors, int> totalCapacityPerColor, LevelData _levelData)
    {
        GridEditorWindow window = GetWindow<GridEditorWindow>("Grid Editor");
        window.minSize = new Vector2(400, 300);
        levelData = _levelData;
        gridWidth = width;
        gridHeight = height;
        requiredStickmen = new Dictionary<Colors, int>(totalCapacityPerColor);
        gridColors = new Dictionary<Vector2Int, Colors>();
        gridSprites = new Dictionary<Vector2Int, Sprite>();
        selectedSprite = levelData.levelEditorDatas.sticmanSpawnerSprite;
        foreach (var kvp in existingGridColors)
        {
            gridColors[kvp.Key] = kvp.Value;
        }

        foreach (var spriteCell in levelData.spriteData)
        {
            gridSprites[spriteCell.position] = spriteCell.sprite;
        }

        foreach (var color in requiredStickmen.Keys)
        {
            currentPaintedCount[color] = gridColors.Values.Count(c => c == color);
        }
    }


    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        DrawLeftPanel();
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        DrawGrid();
        EditorGUILayout.EndScrollView();
        GUILayout.EndHorizontal();
    }

    private void DrawLeftPanel()
    {
        GUILayout.BeginVertical(GUILayout.Width(150));
        GUILayout.Label("Options", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Save", GUILayout.Height(30)))
        {
            ClearAllMode(); SaveGrid();
        }
        if (GUILayout.Button("Clear Grid", GUILayout.Height(30))) 
        {
            ClearAllMode(); ClearGrid(); 
        }

        if (GUILayout.Button("Select Color", GUILayout.Height(30)))
        {
            ClearAllMode(); ShowColorSelection();
        }

        if (GUILayout.Button("Obstacle", GUILayout.Height(30)))
        {
            ClearAllMode();
            isObstacleMode = !isObstacleMode;
            selectedColor = isObstacleMode ? (Colors?)Colors.Gray : null;
        }

        if (GUILayout.Button("Auto Fill Grid", GUILayout.Height(30))) AutoFillGrid();

        //if (GUILayout.Button("Place Spawner", GUILayout.Height(30)))
        //{
        //    ClearAllMode();
        //    isSpawnerFieldMode = !isSpawnerFieldMode;
        //}

        if (GUILayout.Button("Close", GUILayout.Height(30))) Close();

        GUILayout.Label($"Selected Mode: {(isObstacleMode ? "Obstacle Mode" : isSpawnerFieldMode ? "Sprite Mode" : selectedColor?.ToString() ?? "None")}");
        GUILayout.EndVertical();
    }

    private void ClearAllMode()
    {
        isObstacleMode = false;
        isSpawnerFieldMode = false;
        selectedColor = null;
    }
    private void DrawGrid()
    {
        Event e = Event.current;

        for (int y = 0; y < gridHeight; y++) // ✅ Y ekseni düzeltildi (0'dan yukarı gidiyor)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(5);

            for (int x = 0; x < gridWidth; x++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                Colors currentColor = gridColors.ContainsKey(cell) ? gridColors[cell] : Colors.White;
                Sprite currentSprite = gridSprites.ContainsKey(cell) ? gridSprites[cell] : null;

                GUIStyle cellStyle = new GUIStyle(GUI.skin.button);
                cellStyle.normal.background = MakeTexture(30, 30, ColorPalette.Colors[currentColor]);

                Rect buttonRect = GUILayoutUtility.GetRect(30, 30, GUILayout.Width(30), GUILayout.Height(30));
                GUILayout.Space(5);

                if (e.type == EventType.MouseDown && buttonRect.Contains(e.mousePosition))
                {
                    if (e.button == 0) // Sol tıklama ile işlem yap
                    {
                        if (isSpawnerFieldMode)
                        {
                            PlaceSprite(cell);
                        }
                        else
                        {
                            isPainting = true;
                            PaintGrid(cell);
                        }
                    }
                    else if (e.button == 1) // Sağ tıklama ile sil
                    {
                        isErasing = true;
                        EraseGrid(cell);
                    }
                    e.Use();
                }

                if (isPainting && e.type == EventType.MouseDrag && buttonRect.Contains(e.mousePosition))
                {
                    PaintGrid(cell);
                    e.Use();
                }

                if (isErasing && e.type == EventType.MouseDrag && buttonRect.Contains(e.mousePosition))
                {
                    EraseGrid(cell);
                    e.Use();
                }

                // Eğer hücrede bir sprite varsa, onu çiz
                if (currentSprite != null)
                {
                    GUI.DrawTexture(buttonRect, currentSprite.texture, ScaleMode.ScaleToFit);
                }
                else
                {
                    GUI.Button(buttonRect, "", cellStyle);
                }
            }

            GUILayout.Space(5);
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
        }

        if (e.type == EventType.MouseUp)
        {
            isPainting = false;
            isErasing = false;
        }
    }



    private void OpenSpriteInfoWindow(Vector2Int cell)
    {
        if (!gridSprites.ContainsKey(cell)) return;

        Dictionary<Colors, int> totalColorCounts = levelData.busQueue
            .GroupBy(b => b.busColor)
            .ToDictionary(g => g.Key, g => g.Sum(b => b.capacity));

        Dictionary<Colors, int> placedColorCounts = new Dictionary<Colors, int>();
        foreach (var color in gridColors.Values)
        {
            if (!placedColorCounts.ContainsKey(color))
                placedColorCounts[color] = 0;
            placedColorCounts[color]++;
        }

        // 🎯 `levelData` parametresini de ekleyelim!
        SpriteInfoWindow.ShowWindow(cell, totalColorCounts, placedColorCounts);
    }





    private void PlaceSprite(Vector2Int cell)
    {
        if (selectedSprite == null)
        {
            Debug.LogWarning("No sprite selected! Please select a sprite first.");
            return;
        }

        // Eğer hücre beyaz değilse sprite basılmasını engelle
        if (gridColors.ContainsKey(cell) && gridColors[cell] != Colors.White)
        {
            Debug.LogWarning("Cannot place sprite on a non-white cell!");
            return;
        }

        gridSprites[cell] = selectedSprite;
        Debug.Log($"Sprite placed at {cell}");
        Repaint();
    }




    private void AutoFillGrid()
    {
        List<Vector2Int> emptyCells = new List<Vector2Int>();
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                if (!gridColors.ContainsKey(cell))
                {
                    emptyCells.Add(cell);
                }
            }
        }

        System.Random random = new System.Random();
        emptyCells = emptyCells.OrderBy(c => random.Next()).ToList();

        foreach (var color in requiredStickmen.Keys)
        {
            int remaining = requiredStickmen[color] - currentPaintedCount[color];
            for (int i = 0; i < remaining && emptyCells.Count > 0; i++)
            {
                Vector2Int randomCell = emptyCells[0];
                emptyCells.RemoveAt(0);
                gridColors[randomCell] = color;
                currentPaintedCount[color]++;
            }
        }

        Repaint();
    }
    private void PaintGrid(Vector2Int cell)
    {
        if (isObstacleMode)
        {
            if (gridColors.ContainsKey(cell) && gridColors[cell] != Colors.White) return;
            gridColors[cell] = Colors.Gray;
        }
        else
        {
            if (selectedColor == null || gridColors.ContainsKey(cell)) return;

            Colors color = selectedColor.Value;
            if (!requiredStickmen.ContainsKey(color)) return;

            if (currentPaintedCount[color] >= requiredStickmen[color])
            {
                Debug.Log($"Max limit reached for {color} ({currentPaintedCount[color]}/{requiredStickmen[color]})");
                return;
            }

            gridColors[cell] = color;
            currentPaintedCount[color]++;

            NotifyGridChanged();
        }

        Repaint();
    }


    private void EraseGrid(Vector2Int cell)
    {
        if (gridSprites.ContainsKey(cell))
        {
            gridSprites.Remove(cell); // Sprite'ı kaldır
        }

        if (gridColors.ContainsKey(cell))
        {
            Colors color = gridColors[cell];
            gridColors.Remove(cell);

            if (!isObstacleMode && requiredStickmen.ContainsKey(color))
            {
                currentPaintedCount[color] = Mathf.Max(0, currentPaintedCount[color] - 1);
            }

            NotifyGridChanged();
        }

        Debug.Log($"Cell {cell} cleared");
        Repaint();
    }



    private void ShowColorSelection()
    {
        GenericMenu menu = new GenericMenu();

        foreach (var entry in requiredStickmen)
        {
            Colors color = entry.Key;
            int maxCount = entry.Value;
            int currentCount = currentPaintedCount.ContainsKey(color) ? currentPaintedCount[color] : 0;
            bool isDisabled = currentCount >= maxCount;

            if (!isDisabled)
            {
                menu.AddItem(new GUIContent($"{color} ({currentCount}/{maxCount})"), false, () => { selectedColor = color; isObstacleMode = false; });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent($"{color} (Limit Reached)"));
            }
        }

        menu.ShowAsContext();
    }
    private void SaveGrid()
    {
        if (levelData == null) return;

        levelData.gridData.Clear();
        int maxY = gridHeight - 1; // Y ekseni ters çevirmeye yardımcı olacak

        foreach (var cell in gridColors)
        {
            Vector2Int correctedPosition = new Vector2Int(cell.Key.x, maxY - cell.Key.y); // ✅ Y eksenini çevirerek kaydet
            levelData.gridData.Add(new GridCell { position = correctedPosition, color = cell.Value });
        }

        levelData.spriteData.Clear();
        foreach (var spriteEntry in gridSprites)
        {
            Vector2Int correctedPosition = new Vector2Int(spriteEntry.Key.x, maxY - spriteEntry.Key.y); // ✅ Aynı dönüşüm sprite'lar için de uygulanmalı
            levelData.spriteData.Add(new SpriteCell { position = correctedPosition, sprite = spriteEntry.Value });
        }

        EditorUtility.SetDirty(levelData);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Grid ve Sprite'lar doğru eksende kaydedildi!");
    }

    private void ClearGrid()
    {
        gridColors.Clear(); // Tüm grid renklerini temizle
        gridSprites.Clear(); // Tüm sprite'ları temizle

        foreach (var color in requiredStickmen.Keys)
        {
            currentPaintedCount[color] = 0;
        }

        Debug.Log("Grid & Sprites Cleared!");
        Repaint();
    }

    public static void UpdatePaintedCount(Colors color, int change)
    {
        if (currentPaintedCount.ContainsKey(color))
        {
            currentPaintedCount[color] = Mathf.Max(0, currentPaintedCount[color] + change);
        }
        else
        {
            currentPaintedCount[color] = Mathf.Max(0, change);
        }

        Debug.Log($"Updated {color} count: {currentPaintedCount[color]}/{requiredStickmen[color]}");
    }

    public static event System.Action OnGridChanged;
    public static void NotifyGridChanged()
    {
        OnGridChanged?.Invoke();
    }

    public static Dictionary<Colors, int> GetCurrentPaintedCounts()
    {
        return new Dictionary<Colors, int>(currentPaintedCount);
    }

    private Texture2D MakeTexture(int width, int height, Color color)
    {
        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];

        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;

        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
}
