using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Ваши Toggle-переключатели сложности")]
    public Toggle easyToggle;
    public Toggle normalToggle;
    public Toggle hardToggle;

    [Header("Кнопка старта игры")]
    public Button playButton;

    [Header("Название игровой сцены")]
    [Tooltip("Точное имя вашей сцены с заездом на машине")]
    public string gameSceneName = "GameScene";

    private void Start()
    {
        // 1. Привязываем нажатие на кнопку "Играть"
        if (playButton != null)
        {
            playButton.onClick.AddListener(StartGame);
        }

        // 2. Восстанавливаем ранее сохраненный Toggle (по умолчанию Normal)
        int savedDifficulty = PlayerPrefs.GetInt("SelectedDifficulty", 1); // 0 = Easy, 1 = Normal, 2 = Hard

        if (savedDifficulty == 0 && easyToggle != null) easyToggle.isOn = true;
        else if (savedDifficulty == 1 && normalToggle != null) normalToggle.isOn = true;
        else if (savedDifficulty == 2 && hardToggle != null) hardToggle.isOn = true;
    }

    public void StartGame()
    {
        // Определяем выбранную сложность по активному Toggle
        int chosenDifficulty = 1; // По умолчанию средняя (Normal)

        if (easyToggle != null && easyToggle.isOn)
        {
            chosenDifficulty = 0; // Easy
        }
        else if (normalToggle != null && normalToggle.isOn)
        {
            chosenDifficulty = 1; // Normal
        }
        else if (hardToggle != null && hardToggle.isOn)
        {
            chosenDifficulty = 2; // Hard
        }

        // Сохраняем сложность в память
        PlayerPrefs.SetInt("SelectedDifficulty", chosenDifficulty);
        PlayerPrefs.Save();

        // Загружаем сцену с игрой
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }
}