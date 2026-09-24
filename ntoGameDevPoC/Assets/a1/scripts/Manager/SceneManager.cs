using UnityEngine;
using UnityEngine.UI;              // Обязательно для работы с Toggle
using UnityEngine.SceneManagement; // Для работы со сценами

public class SceneController : MonoBehaviour
{
    [Header("Ваши Toggle-переключатели сложности")]
    public Toggle easyToggle;
    public Toggle normalToggle;
    public Toggle hardToggle;

    private void Start()
    {
        // При открытии меню выставляем Toggle, который был выбран в прошлый раз
        int savedDifficulty = PlayerPrefs.GetInt("SelectedDifficulty", 1); // 1 = Normal по умолчанию

        if (savedDifficulty == 0 && easyToggle != null) easyToggle.isOn = true;
        else if (savedDifficulty == 1 && normalToggle != null) normalToggle.isOn = true;
        else if (savedDifficulty == 2 && hardToggle != null) hardToggle.isOn = true;
    }

    // Метод для загрузки сцены по её названию (привязан к вашей кнопке "Играть")
    public void LoadSceneByName(string sceneName)
    {
        SaveDifficultyFromToggles();

        Time.timeScale = 1.0f; // Сбрасываем паузу на случай, если вышли из паузы
        SceneManager.LoadScene(sceneName);
    }

    // Альтернативный метод: загрузка сцены по её индексу
    public void LoadSceneByIndex(int sceneIndex)
    {
        SaveDifficultyFromToggles();

        Time.timeScale = 1.0f;
        SceneManager.LoadScene(sceneIndex);
    }

    // Проверяем, какой Toggle включен, и сохраняем в память игры
    private void SaveDifficultyFromToggles()
    {
        int difficultyIndex = 1; // 0 = Easy, 1 = Normal, 2 = Hard

        if (easyToggle != null && easyToggle.isOn)
        {
            difficultyIndex = 0;
        }
        else if (normalToggle != null && normalToggle.isOn)
        {
            difficultyIndex = 1;
        }
        else if (hardToggle != null && hardToggle.isOn)
        {
            difficultyIndex = 2;
        }

        PlayerPrefs.SetInt("SelectedDifficulty", difficultyIndex);
        PlayerPrefs.Save(); // Принудительно сохраняем на диск

        Debug.Log($"<color=green>[МЕНЮ]</color> Сохранена сложность: <b>{(difficultyIndex == 0 ? "EASY" : difficultyIndex == 1 ? "NORMAL" : "HARD")}</b>");
    }
}