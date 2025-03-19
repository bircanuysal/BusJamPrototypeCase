using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public enum GameMode
{
    Play,Failed,Success
}

[ExecuteInEditMode]
public class GameManager : LocalSingleton<GameManager>
{
    [SerializeField] private LevelEditorDatas levelEditorDatas;
    [SerializeField] private BusQueueManager busQueueManager;
    [SerializeField] private BusWaitingGridManager busWaitingGridManager;
    private List<IUnit> units = new List<IUnit>();
    private int nextUnitID = 0;
    [SerializeField]private Transform BusStation;
    private float levelTime;
    private Transform DecisionWall;
    private bool CanClickOnStickman = true;
    private GameMode gameMode;

    protected override void Awake()
    {
        base.Awake();
    }
    private void Start()
    {
        LoadLevelData();
        EventManager.Ui.UpdateLevelText(currentLevel);

        string currentScene = SceneManager.GetActiveScene().name;
        string expectedLevelName = "Level" + currentLevel;

        if (currentScene != expectedLevelName)
        {
            EventManager.Ui.ShowLevelStarterUI();
        }
        else
        {
            EventManager.Ui.HideAllUI();
        }
    }

    private void OnValidate()
    {
        if (levelEditorDatas == null)
        {
            Debug.LogError("LevelEditorDatas atanmamýþ! Lütfen GameManager içinden atayýn.");
        }
    }
    void OnApplicationQuit()
    {
        Debug.Log("Oyun kapatýldý!");
    }
    public LevelEditorDatas GetLevelEditorDatas()
    {
        return levelEditorDatas;
    }

    public void RegisterUnit(IUnit unit)
    {
        if (unit == null) return;
        unit.SetUnitID(nextUnitID++);
        units.Add(unit);
    }

    public void UnregisterUnit(IUnit unit)
    {
        if (unit == null) return;
        units.Remove(unit);
    }

    public void SetBusStationPos(Vector3 newPos)
    {
        BusStation.position = newPos;
    }
    public Transform GetBusStation()
    {
        return BusStation;
    }
    public void SetDecisionWall(Transform gameObject)
    {
        DecisionWall = gameObject;
    }
    public Transform GetDecisionWall()
    {
        return DecisionWall;
    }
    public BusQueueManager GetBusQueueManager()
    {
        return busQueueManager;
    }
    public BusWaitingGridManager GetBusWaitingGridManager()
    {
        return busWaitingGridManager;
    }
    private int currentLevel;
    public float GetLevelTime()
    {
        return levelTime;
    }
    public void SetLevelTime(float time)
    {
        levelTime = time;
    }
    private void LoadLevelData()
    {
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
    }

    public void SetGameMode(GameMode gameMode)
    {
        this.gameMode = gameMode;

        if (gameMode == GameMode.Failed)
        {
            EventManager.Ui.LevelFailed();
        }
        else if (gameMode == GameMode.Success)
        {
            EventManager.Ui.LevelCompleted();
        }
    }

    public void StartLevel()
    {
        EventManager.Ui.HideAllUI();       
        SceneLoader.LoadScene("Level" + currentLevel);
    }

    public void RetryLevel()
    {
        SceneLoader.LoadScene("Level" + currentLevel);
    }

    public void NextLevel()
    {
        currentLevel++;
        PlayerPrefs.SetInt("CurrentLevel", currentLevel);
        PlayerPrefs.Save();

        SceneLoader.LoadScene("Level" + currentLevel);
    }
    public void SetCanClickOnStickman(bool value)
    {
        CanClickOnStickman = value;
    }
    public bool GetCanClickOnStickman()
    {
        return CanClickOnStickman;
    }
}

