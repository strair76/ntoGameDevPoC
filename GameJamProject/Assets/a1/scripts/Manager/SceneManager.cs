using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneController : MonoBehaviour
{
    [Header("Тогглы сложности")]
    public Toggle easyToggle;
    public Toggle normalToggle;
    public Toggle hardToggle;

    [Header("Тогглы локаций")]
    public Toggle location1Toggle; // Например: Город
    public Toggle location2Toggle; // Например: Пустыня
    public Toggle location3Toggle; // Например: Ночная трасса

    [Header("Рекорды в Главном Меню")]
    public TextMeshProUGUI totalCoinsText;
    public TextMeshProUGUI bestDistanceText;

    private void Start()
    {
        // 1. Восстанавливаем сохраненную сложность
        int savedDiff = PlayerPrefs.GetInt("SelectedDifficulty", 1);
        if (savedDiff == 0 && easyToggle != null) easyToggle.isOn = true;
        else if (savedDiff == 1 && normalToggle != null) normalToggle.isOn = true;
        else if (savedDiff == 2 && hardToggle != null) hardToggle.isOn = true;

        // 2. Восстанавливаем сохраненную локацию (0 по умолчанию)
        int savedLoc = PlayerPrefs.GetInt("SelectedLocation", 0);
        if (savedLoc == 0 && location1Toggle != null) location1Toggle.isOn = true;
        else if (savedLoc == 1 && location2Toggle != null) location2Toggle.isOn = true;
        else if (savedLoc == 2 && location3Toggle != null) location3Toggle.isOn = true;

        UpdateRecordsUI();
    }

    private void UpdateRecordsUI()
    {
        int totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        int bestDistance = Mathf.FloorToInt(PlayerPrefs.GetFloat("BestDistance", 0f));

        if (totalCoinsText != null) totalCoinsText.text = $"Баланс: <b>{totalCoins}</b>";
        if (bestDistanceText != null) bestDistanceText.text = $"Рекорд: <b>{bestDistance} м</b>";
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
        // Сохраняем сложность
        int diffIndex = 1;
        if (easyToggle != null && easyToggle.isOn) diffIndex = 0;
        else if (normalToggle != null && normalToggle.isOn) diffIndex = 1;
        else if (hardToggle != null && hardToggle.isOn) diffIndex = 2;

        PlayerPrefs.SetInt("SelectedDifficulty", diffIndex);

        // Сохраняем локацию
        int locIndex = 0;
        if (location1Toggle != null && location1Toggle.isOn) locIndex = 0;
        else if (location2Toggle != null && location2Toggle.isOn) locIndex = 1;
        else if (location3Toggle != null && location3Toggle.isOn) locIndex = 2;

        PlayerPrefs.SetInt("SelectedLocation", locIndex);
        PlayerPrefs.Save();

        Debug.Log($"<color=green>[МЕНЮ]</color> Сохранена сложность: {diffIndex} | Локация: {locIndex}");
    }
}