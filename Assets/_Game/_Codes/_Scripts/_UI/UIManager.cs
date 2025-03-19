using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : LocalSingleton<UIManager>
{

    public GameObject levelStarterUI;
    public GameObject levelFailedUI;
    public GameObject levelCompletedUI;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI time;

    public Button startButton;
    public Button retryButton;
    public Button nextLevelButton;

    private float remainingTime;
    private bool isCountingDown = false;


    private void Start()
    {
        startButton.onClick.AddListener(GameManager.Instance.StartLevel);
        retryButton.onClick.AddListener(GameManager.Instance.RetryLevel);
        nextLevelButton.onClick.AddListener(GameManager.Instance.NextLevel);
        remainingTime = GameManager.Instance.GetLevelTime();
    }
    private void LateUpdate()
    {
        if (isCountingDown)
        {
            remainingTime -= Time.deltaTime;
            if (remainingTime <= 0)
            {
                remainingTime = 0;
                isCountingDown = false;
                GameManager.Instance.SetGameMode(GameMode.Failed);
            }
            UpdateTimeText();
        }
    }
    private void OnEnable()
    {
        EventManager.Ui.ShowLevelStarterUI += ShowLevelStarterUI;
        EventManager.Ui.UpdateLevelText += UpdateLevelText;
        EventManager.Ui.LevelFailed += LevelFailed;
        EventManager.Ui.LevelCompleted += LevelCompleted;
        EventManager.Ui.HideAllUI += HideAllUI;
    }

    private void OnDisable()
    {
        EventManager.Ui.ShowLevelStarterUI -= ShowLevelStarterUI;
        EventManager.Ui.UpdateLevelText -= UpdateLevelText;
        EventManager.Ui.LevelFailed -= LevelFailed;
        EventManager.Ui.LevelCompleted -= LevelCompleted;
        EventManager.Ui.HideAllUI -= HideAllUI;
    }

    private void ShowLevelStarterUI()
    {
        levelStarterUI.SetActive(true);
        levelFailedUI.SetActive(false);
        levelCompletedUI.SetActive(false);
    }

    private void LevelFailed()
    {
        levelFailedUI.SetActive(true);
    }

    private void LevelCompleted()
    {
        levelCompletedUI.SetActive(true);
    }

    private void UpdateLevelText(int level)
    {
        levelText.text = "LEVEL " + level;
    }

    private void UpdateTimeText()
    {
        time.text = $"{Mathf.FloorToInt(remainingTime / 60):00}:{Mathf.FloorToInt(remainingTime % 60):00}";

    }
    private void HideAllUI()
    {
        levelStarterUI.SetActive(false);
        levelFailedUI.SetActive(false);
        levelCompletedUI.SetActive(false);
        StartCountRemaining();
    }
    public void StartCountRemaining()
    {
        isCountingDown = true;
    }
}

public static class SceneLoader
{
    public static void LoadScene(string sceneName)
    {
        if (SceneExists(sceneName))
        {
            DOTween.KillAll();
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"Sahne '{sceneName}' bulunamadı! Lütfen Level Editor'den yeni bir level dizayn ediniz.");
        }
    }

    private static bool SceneExists(string sceneName)
    {
        int sceneIndex = SceneUtility.GetBuildIndexByScenePath(sceneName);
        return sceneIndex != -1;
    }
}