using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    [Header("Панель меню")]
    public GameObject menuPanel;

    [Header("Заголовки")]
    public GameObject pauseTitleObject;
    public GameObject deathTitleObject;

    [Header("Кнопки")]
    public Button resumeButton;
    public Button restartButton;
    public Button menuButton;
    public Button quitButton;

    [Header("Настройки сцен")]
    public string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;
    private bool isGameOver = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        if (pauseTitleObject != null) pauseTitleObject.SetActive(false);
        if (deathTitleObject != null) deathTitleObject.SetActive(false);

        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
        if (menuButton != null) menuButton.onClick.AddListener(GoToMainMenu);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (kb.escapeKey.wasPressedThisFrame && !isGameOver)
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pauseTitleObject != null) pauseTitleObject.SetActive(true);
        if (deathTitleObject != null) deathTitleObject.SetActive(false);
        if (resumeButton != null) resumeButton.gameObject.SetActive(true);
        if (menuPanel != null) menuPanel.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (menuPanel != null) menuPanel.SetActive(false);
    }

    public void TriggerGameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f;

        // Сохраняем монеты и рекорд
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SaveResults();
        }

        if (pauseTitleObject != null) pauseTitleObject.SetActive(false);
        if (deathTitleObject != null) deathTitleObject.SetActive(true);
        if (resumeButton != null) resumeButton.gameObject.SetActive(false);
        if (menuPanel != null) menuPanel.SetActive(true);
    }

    public void RestartGame()
    {
        if (ScoreManager.Instance != null) ScoreManager.Instance.SaveResults();

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        if (ScoreManager.Instance != null) ScoreManager.Instance.SaveResults();

        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        if (ScoreManager.Instance != null) ScoreManager.Instance.SaveResults();

        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}