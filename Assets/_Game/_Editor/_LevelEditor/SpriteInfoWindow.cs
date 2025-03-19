using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class SpriteInfoWindow : EditorWindow
{
    private static Vector2Int selectedCell;
    private static Dictionary<Colors, int> totalColors = new Dictionary<Colors, int>(); // 🎯 HATA DÜZELTİLDİ
    private static Dictionary<Vector2Int, Dictionary<Colors, int>> spriteMissingColors = new Dictionary<Vector2Int, Dictionary<Colors, int>>();
    private static Dictionary<Vector2Int, List<Colors>> spriteColorQueues = new Dictionary<Vector2Int, List<Colors>>();
    private Colors? selectedColor = null;

    public static void ShowWindow(Vector2Int cell, Dictionary<Colors, int> _totalColors, Dictionary<Colors, int> placedColors)
    {
        SpriteInfoWindow window = GetWindow<SpriteInfoWindow>("Sprite Info");
        window.minSize = new Vector2(300, 300);
        selectedCell = cell;

        totalColors = new Dictionary<Colors, int>(_totalColors);

        if (!spriteMissingColors.ContainsKey(cell))
        {
            spriteMissingColors[cell] = new Dictionary<Colors, int>();
            spriteColorQueues[cell] = new List<Colors>();

            foreach (var color in totalColors.Keys)
            {
                int needed = totalColors[color] - (placedColors.ContainsKey(color) ? placedColors[color] : 0);
                if (needed > 0)
                {
                    spriteMissingColors[cell][color] = needed;
                }
            }
        }
    }

    private void OnEnable()
    {
        GridEditorWindow.OnGridChanged += UpdateMissingColors;
    }

    private void OnDisable()
    {
        GridEditorWindow.OnGridChanged -= UpdateMissingColors;
    }

    private void UpdateMissingColors()
    {
        if (!spriteMissingColors.ContainsKey(selectedCell)) return;

        Dictionary<Colors, int> placedColors = GridEditorWindow.GetCurrentPaintedCounts();
        spriteMissingColors[selectedCell].Clear();

        foreach (var color in totalColors.Keys)
        {
            int needed = totalColors[color] - (placedColors.ContainsKey(color) ? placedColors[color] : 0);
            if (needed > 0)
            {
                spriteMissingColors[selectedCell][color] = needed;
            }
        }

        Repaint();
    }

    private void OnGUI()
    {
        GUILayout.Label($"Sprite Info at {selectedCell}", EditorStyles.boldLabel);
        GUILayout.Space(10);

        GUILayout.Label("Missing Colors (Not Placed Yet):", EditorStyles.boldLabel);

        if (!spriteMissingColors.ContainsKey(selectedCell)) return;

        foreach (var entry in spriteMissingColors[selectedCell])
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{entry.Key}: {entry.Value} needed", EditorStyles.label);
            if (GUILayout.Button("Select", GUILayout.Width(80)))
            {
                selectedColor = entry.Key;
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(10);

        GUILayout.Label("Selected Color Queue:", EditorStyles.boldLabel);

        if (!spriteColorQueues.ContainsKey(selectedCell)) return;

        for (int i = 0; i < spriteColorQueues[selectedCell].Count; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{i + 1}. {spriteColorQueues[selectedCell][i]}", EditorStyles.label);

            if (i > 0 && GUILayout.Button("▲", GUILayout.Width(25)))
            {
                SwapQueueElements(i, i - 1);
            }

            if (i < spriteColorQueues[selectedCell].Count - 1 && GUILayout.Button("▼", GUILayout.Width(25)))
            {
                SwapQueueElements(i, i + 1);
            }

            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                Colors removedColor = spriteColorQueues[selectedCell][i];
                spriteColorQueues[selectedCell].RemoveAt(i);

                GridEditorWindow.UpdatePaintedCount(removedColor, -1);
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.Space(10);

        if (selectedColor != null)
        {
            if (GUILayout.Button("Add to Queue"))
            {
                spriteColorQueues[selectedCell].Add(selectedColor.Value);

                GridEditorWindow.UpdatePaintedCount(selectedColor.Value, +1);

                selectedColor = null;
            }
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Close", GUILayout.Height(30)))
        {
            this.Close();
        }
    }

    private void SwapQueueElements(int indexA, int indexB)
    {
        Colors temp = spriteColorQueues[selectedCell][indexA];
        spriteColorQueues[selectedCell][indexA] = spriteColorQueues[selectedCell][indexB];
        spriteColorQueues[selectedCell][indexB] = temp;
    }
}
