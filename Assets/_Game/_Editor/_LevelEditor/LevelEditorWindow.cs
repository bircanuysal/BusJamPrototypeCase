using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;
using System.Xml.Serialization;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class LevelEditorWindow : EditorWindow
{
    private LevelData levelData;
    private SerializedObject serializedLevelData;
    private ReorderableList busList;
    private List<bool> foldoutStates = new List<bool>();
    private int gridWidth = 5;
    private int gridHeight = 5;
    private Dictionary<Vector2Int, Colors> gridColors = new Dictionary<Vector2Int, Colors>();
    private static Dictionary<Vector2Int, Sprite> gridSprites = new Dictionary<Vector2Int, Sprite>();

    private Vector2 scrollPosition;

    [MenuItem("Tools/Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditorWindow>("Level Editor");
    }

    private void OnEnable()
    {
        if (levelData != null)
        {
            InitReorderableList();
            LoadGridData();  // Kaydedilen grid verisini yükle
        }
    }
    private void LoadGridData()
    {
        if (levelData == null) return;

        gridColors.Clear();
        gridSprites.Clear();

        int maxY = gridHeight - 1; // Kaydedilen verileri geri çevirirken kullanacağız

        foreach (var cell in levelData.gridData)
        {
            Vector2Int correctedPosition = new Vector2Int(cell.position.x, maxY - cell.position.y); // ✅ Kaydedilen ters dönüşümü geri al
            gridColors[correctedPosition] = cell.color;
        }

        foreach (var spriteCell in levelData.spriteData)
        {
            Vector2Int correctedPosition = new Vector2Int(spriteCell.position.x, maxY - spriteCell.position.y); // ✅ Aynı işlem sprite'lar için de
            gridSprites[correctedPosition] = spriteCell.sprite;
        }

        Repaint();
    }





    private void InitReorderableList()
    {
        serializedLevelData = new SerializedObject(levelData);
        SerializedProperty busQueueProperty = serializedLevelData.FindProperty("busQueue");

        busList = new ReorderableList(serializedLevelData, busQueueProperty, true, true, true, true);

        busList.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, "Bus Queue (Drag to Reorder)");
        };

        busList.elementHeightCallback = (int index) => {
            if (index >= foldoutStates.Count) foldoutStates.Add(false);
            return foldoutStates[index] ? 90 : 30;
        };

        busList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
            if (index >= foldoutStates.Count)
            {
                foldoutStates.Add(false);
            }

            var element = busQueueProperty.GetArrayElementAtIndex(index);
            SerializedProperty colorProperty = element.FindPropertyRelative("busColor");
            SerializedProperty capacityProperty = element.FindPropertyRelative("capacity");

            rect.y += 2;

            Rect indexRect = new Rect(rect.x, rect.y, 20, 20);
            EditorGUI.LabelField(indexRect, (index + 1).ToString());

            Rect foldoutRect = new Rect(rect.x + 25, rect.y, 20, 20);
            foldoutStates[index] = EditorGUI.Foldout(foldoutRect, foldoutStates[index], "");

            Rect colorRect = new Rect(rect.x + 50, rect.y, 100, 20);
            GUIStyle colorStyle = new GUIStyle(GUI.skin.box);
            colorStyle.normal.background = MakeTexture(100, 20, ColorPalette.Colors[(Colors)colorProperty.enumValueIndex]);
            GUI.Box(colorRect, "", colorStyle);

            if (foldoutStates[index])
            {
                rect.y += 25;
                Rect dropdownRect = new Rect(rect.x + 50, rect.y, 100, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(dropdownRect, colorProperty, GUIContent.none);

                rect.y += 25;
                Rect sliderLabelRect = new Rect(rect.x + 50, rect.y, 60, EditorGUIUtility.singleLineHeight);
                GUI.Label(sliderLabelRect, "Capacity Projenin Ileri kisminda etkinlestir:");

                //Rect sliderRect = new Rect(rect.x + 110, rect.y, rect.width - 160, EditorGUIUtility.singleLineHeight);
                //EditorGUI.IntSlider(sliderRect, capacityProperty, 1, 5, GUIContent.none);
            }
        };

        busList.onAddCallback = (ReorderableList list) =>
        {
            levelData.busQueue.Add(new BusConfig() { busColor = Colors.Red, capacity = 3 });
            foldoutStates.Add(false);
        };

        busList.onRemoveCallback = (ReorderableList list) =>
        {
            levelData.busQueue.RemoveAt(list.index);
            foldoutStates.RemoveAt(list.index);
        };
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        GUILayout.Label("Level Design Editor", EditorStyles.boldLabel);

        levelData = (LevelData)EditorGUILayout.ObjectField("Level Data", levelData, typeof(LevelData), false);

        if (levelData == null)
        {
            EditorGUILayout.EndScrollView();
            return;
        }

        if (busList == null || serializedLevelData.targetObject != levelData)
        {
            InitReorderableList();
        }

        serializedLevelData.Update();
        busList.DoLayoutList();

        GUILayout.Space(10);
        GUILayout.Label("Level Timer (Seconds):", EditorStyles.boldLabel);
        levelData.levelTimer = EditorGUILayout.FloatField("Level Time", levelData.levelTimer);
        serializedLevelData.ApplyModifiedProperties();

        GUILayout.Space(10);
        DrawStickmanInfo();
        DrawGridSettings();

        LoadGridData();
        DrawGridVisualization();

        GUILayout.Space(10);

        if (GUILayout.Button("Sahneye Aktar", GUILayout.Height(40)))
        {
            ApplyToScene();
        }
        GUILayout.Space(10);

        if (GUILayout.Button("Sahneyi Olustur", GUILayout.Height(40)))
        {
            CreateScene();
        }
        EditorGUILayout.EndScrollView();  // Kapatmayı unutma!
    }


    private void ApplyToScene()
    {
        LevelDesigner levelDesigner = GameObject.FindObjectOfType<LevelDesigner>();

        if (levelDesigner == null)
        {
            Debug.LogError("LevelDesigner bulunamadı! Lütfen sahneye bir LevelDesigner ekleyin.");
            return;
        }

        levelDesigner.levelData = levelData;
        levelDesigner.gridWidth = gridWidth;
        levelDesigner.gridHeight = gridHeight;
        levelDesigner.levelTime = levelData.levelTimer;

        levelDesigner.GenerateLevel();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    private void CreateScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();

        if (!activeScene.IsValid())
        {
            Debug.LogError("Geçerli bir sahne bulunamadı!");
            return;
        }

        if (levelData == null)
        {
            Debug.LogError("LevelData bulunamadı!");
            return;
        }

        //if (HasMissingRequiredColors())
        //{
        //    EditorUtility.DisplayDialog(
        //        "Bu seviyede bazı renkler için yeterli sayıda Stickman yerleştirilmemiş. Bu nedenle oyun hiç bitmeyecek!",
        //        "Tamam"
        //    );
        //    return; // Eksik renkler varsa sahne oluşturulmasın
        //}

        string newSceneName = levelData.name;
        string newScenePath = $"Assets/_Game/_Scenes/{newSceneName}.unity";

        if (System.IO.File.Exists(newScenePath))
        {
            Debug.LogWarning($"Sahne zaten var: {newScenePath}");
            return;
        }

        string originalScenePath = activeScene.path;

        if (string.IsNullOrEmpty(originalScenePath))
        {
            Debug.LogError("Sahnenin dosya yolu bulunamadı! Önce sahneyi kaydetmelisiniz.");
            return;
        }

        bool copySuccess = AssetDatabase.CopyAsset(originalScenePath, newScenePath);

        if (copySuccess)
        {
            Debug.Log($"Sahne başarıyla kopyalandı: {newScenePath}");

            Scene newScene = EditorSceneManager.OpenScene(newScenePath, OpenSceneMode.Single);

            if (newScene.IsValid())
            {
                Debug.Log($"Yeni sahne yüklendi: {newScene.name}");

                LevelDesigner levelDesigner = GameObject.FindObjectOfType<LevelDesigner>();
                if (levelDesigner != null)
                {
                    levelDesigner.levelData = levelData;
                    levelDesigner.gridWidth = gridWidth;
                    levelDesigner.gridHeight = gridHeight;
                    levelDesigner.GenerateLevel();

                    EditorSceneManager.MarkSceneDirty(newScene);
                }
                else
                {
                    Debug.LogError("Yeni sahnede LevelDesigner bulunamadı!");
                }
            }
            else
            {
                Debug.LogError("Yeni sahne yüklenirken hata oluştu!");
            }
        }
        else
        {
            Debug.LogError("Sahne kopyalanırken hata oluştu!");
        }

        AssetDatabase.Refresh();
    }
    private bool HasMissingRequiredColors()
    {
        var requiredColors = levelData.busQueue
            .Select(b => b.busColor)
            .Distinct()
            .ToList();

        var placedColors = gridColors.Values.Distinct().ToList();

        var missingColors = requiredColors.Except(placedColors).ToList();

        if (missingColors.Count > 0)
        {
            string missingColorsText = string.Join(", ", missingColors);

            EditorUtility.DisplayDialog(
                "Eksik Stickman Renkleri!",
                $"Bu seviyede bazı renkler için yeterli sayıda Stickman yerleştirilmemiş.\n\nEksik renkler: {missingColorsText}\n\nBu nedenle oyun hiç bitmeyecek!",
                "Tamam"
            );

            return true;
        }

        return false;
    }




    private void DrawStickmanInfo()
    {
        var stickmanByColor = levelData.busQueue.GroupBy(b => b.busColor)
            .ToDictionary(g => g.Key, g => g.Sum(b => b.capacity));

        GUILayout.Label("Stickman Info by Bus Color:", EditorStyles.boldLabel);

        foreach (var entry in stickmanByColor)
        {
            GUILayout.Label($"{entry.Key} Total Stickman: {entry.Value}", EditorStyles.label);
            GUILayout.Label($"{entry.Key} Required Stickman: {entry.Value}\n", EditorStyles.label);
            GUILayout.Space(5);
        }
    }
    private void DrawGridSettings()
    {
        GUILayout.Space(10);
        GUILayout.Label("Grid Settings", EditorStyles.boldLabel);
        gridWidth = EditorGUILayout.IntField("Grid Width:", gridWidth);
        gridHeight = EditorGUILayout.IntField("Grid Height:", gridHeight);
        levelData.gridWidth = gridWidth;
        levelData.gridHeight = gridHeight;
    }
    private void DrawGridVisualization()
    {
        GUILayout.Space(10);
        GUILayout.Label("Grid Preview", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();  // Başlangıç

        EditGridButton();
        DrawGrid();

        GUILayout.EndHorizontal();  // Kapatmayı unutma
    }

    private void EditGridButton()
    {
        if (GUILayout.Button("Edit Grid", GUILayout.Width(80), GUILayout.Height(30)))
        {
            var totalCapacityPerColor = levelData.busQueue
                .GroupBy(b => b.busColor)
                .ToDictionary(g => g.Key, g => g.Sum(b => b.capacity));

            GridEditorWindow.ShowWindow(gridWidth, gridHeight, gridColors, totalCapacityPerColor , levelData);
        }
    }
    private void DrawGrid()
    {
        Event e = Event.current;
        GUILayout.BeginVertical();

        for (int y = 0; y < gridHeight; y++)
        {
            GUILayout.BeginHorizontal();
            for (int x = 0; x < gridWidth; x++)
            {
                DrawGridCell(new Vector2Int(x, y), e);
            }
            GUILayout.EndHorizontal();
        }

        GUI.backgroundColor = Color.white;
        GUILayout.EndVertical();
    }
    private void DrawGridCell(Vector2Int cellPosition, Event e)
    {
        GUI.backgroundColor = Color.white; // Grid beyaz olacak

        if (gridSprites.ContainsKey(cellPosition))
        {
            // Eğer bu hücreye bir sprite yerleştirildiyse, onu çiz
            Rect rect = GUILayoutUtility.GetRect(30, 30, GUILayout.Width(30), GUILayout.Height(30));
            GUI.DrawTexture(rect, gridSprites[cellPosition].texture, ScaleMode.ScaleToFit);
        }
        else
        {
            if (gridColors.ContainsKey(cellPosition))
            {
                GUI.backgroundColor = ColorPalette.Colors[gridColors[cellPosition]];
            }

            if (GUILayout.Button("", GUILayout.Width(30), GUILayout.Height(30)))
            {
                Debug.Log($"Clicked on cell {cellPosition}");
            }
        }
    }

    private Texture2D MakeTexture(int width, int height, Color color)
    {
        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;

        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

}
