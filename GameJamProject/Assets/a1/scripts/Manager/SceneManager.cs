using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

    [Header("Тогглы сложности")]
    public Toggle easyToggle;
    public Toggle normalToggle;
    public Toggle hardToggle;

    [Header("Тогглы локаций")]
    public Toggle location1Toggle;
    public Toggle location2Toggle;
    public Toggle location3Toggle;

    [Header("UI Тексты в Главном Меню")]
    [Tooltip("Текстовый объект (TextMeshPro) для отображения монет")]
    public TextMeshProUGUI totalCoinsText;

    [Tooltip("Текстовый объект (TextMeshPro) для отображения рекорда дистанции")]
    public TextMeshProUGUI bestDistanceText;

    [Header("Кастомизация текста (Настройте под себя)")]
    [TextArea(2, 4)]
    [Tooltip("Метка {coins} автоматически заменится на число монет")]
    public string coinsTemplate = "Баланс: <b>{coins}</b>";

    [TextArea(2, 4)]
    [Tooltip("Метка {record} или {distance} автоматически заменится на метры рекорда")]
    public string recordTemplate = "Рекорд: <b>{record} м</b>";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 1. Восстанавливаем сохраненную сложность
        int savedDiff = PlayerPrefs.GetInt("SelectedDifficulty", 1);
        if (savedDiff == 0 && easyToggle != null) easyToggle.isOn = true;
        else if (savedDiff == 1 && normalToggle != null) normalToggle.isOn = true;
        else if (savedDiff == 2 && hardToggle != null) hardToggle.isOn = true;

        // 2. Восстанавливаем сохраненную локацию
        int savedLoc = PlayerPrefs.GetInt("SelectedLocation", 0);
        if (savedLoc == 0 && location1Toggle != null) location1Toggle.isOn = true;
        else if (savedLoc == 1 && location2Toggle != null) location2Toggle.isOn = true;
        else if (savedLoc == 2 && location3Toggle != null) location3Toggle.isOn = true;

        // 3. Выводим текст с вашими настройками
        UpdateRecordsUI();
    }

    // Публичный метод: обновляет баланс и рекорд по вашим шаблонам
    public void UpdateRecordsUI()
    {
        int totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        int bestDistance = Mathf.FloorToInt(PlayerPrefs.GetFloat("BestDistance", 0f));

        // Подставляем монеты в ваш шаблон
        if (totalCoinsText != null)
        {
            string formattedCoins = coinsTemplate.Replace("{coins}", totalCoins.ToString());
            totalCoinsText.text = formattedCoins;
        }

        // Подставляем рекорд в ваш шаблон (поддерживает и {record}, и {distance})
        if (bestDistanceText != null)
        {
            string formattedRecord = recordTemplate
                .Replace("{record}", bestDistance.ToString())
                .Replace("{distance}", bestDistance.ToString());

            bestDistanceText.text = formattedRecord;
        }
    }

    public void LoadSceneByName(string sceneName)
    {
        SaveSettings();
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(sceneName);
    }

    public void LoadSceneByIndex(int sceneIndex)
    {
        SaveSettings();
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(sceneIndex);
    }

    private void SaveSettings()
    {
        int diffIndex = 1;
        if (easyToggle != null && easyToggle.isOn) diffIndex = 0;
        else if (normalToggle != null && normalToggle.isOn) diffIndex = 1;
        else if (hardToggle != null && hardToggle.isOn) diffIndex = 2;
        PlayerPrefs.SetInt("SelectedDifficulty", diffIndex);

        int locIndex = 0;
        if (location1Toggle != null && location1Toggle.isOn) locIndex = 0;
        else if (location2Toggle != null && location2Toggle.isOn) locIndex = 1;
        else if (location3Toggle != null && location3Toggle.isOn) locIndex = 2;
        PlayerPrefs.SetInt("SelectedLocation", locIndex);

        PlayerPrefs.Save();
    }
}