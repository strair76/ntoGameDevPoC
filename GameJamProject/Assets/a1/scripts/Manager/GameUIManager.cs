using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    [Header("Панель меню")]
    [Tooltip("Главная панель меню (фон/окно, в котором лежат кнопки)")]
    public GameObject menuPanel;

    [Header("Заголовки (Создайте свои объекты в UI)")]
    [Tooltip("Ваш созданный текст для паузы (включится при нажатии Esc)")]
    public GameObject pauseTitleObject;

    [Tooltip("Ваш созданный текст для смерти (включится при аварии)")]
    public GameObject deathTitleObject;

    [Header("Кнопки")]
    public Button resumeButton;   // Кнопка "Продолжить" (скрывается при смерти)
    public Button restartButton;  // Кнопка "Заново"
    public Button menuButton;     // Кнопка "В главное меню"
    public Button quitButton;     // Кнопка "Выход из игры"

    [Header("Настройки сцен")]
    [Tooltip("Точное имя вашей сцены с главным меню")]
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
        // Скрываем меню и тексты на старте
        if (menuPanel != null) menuPanel.SetActive(false);
        if (pauseTitleObject != null) pauseTitleObject.SetActive(false);
        if (deathTitleObject != null) deathTitleObject.SetActive(false);

        // Привязываем клики к кнопкам
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
        if (menuButton != null) menuButton.onClick.AddListener(GoToMainMenu);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        // Открытие / закрытие паузы на Esc
        if (kb.escapeKey.wasPressedThisFrame && !isGameOver)
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    // --- ПАУЗА (ESC) ---
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        // Включаем ВАШ текст паузы и выключаем текст смерти
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

    // --- СМЕРТЬ ИГРОКА ---
    public void TriggerGameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f;

        // Включаем ВАШ текст смерти и выключаем текст паузы
        if (pauseTitleObject != null) pauseTitleObject.SetActive(false);
        if (deathTitleObject != null) deathTitleObject.SetActive(true);

        if (resumeButton != null) resumeButton.gameObject.SetActive(false); // Нельзя продолжить мертвым
        if (menuPanel != null) menuPanel.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}