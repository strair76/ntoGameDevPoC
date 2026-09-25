using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Настройки экономики")]
    [Tooltip("Сколько метров нужно проехать для получения 1 монеты")]
    public float metersPerCoin = 100f;

    [Header("Единый текст статистики (висит всегда)")]
    [Tooltip("Перетащите сюда ваш текстовый объект со скриншота")]
    public TextMeshProUGUI statsDisplayTMP;

    [Header("Кастомизация текста в Инспекторе")]
    [TextArea(5, 8)]
    [Tooltip("Метки для автоподстановки:\n{distance} — текущая дистанция\n{coins} — ОБЩИЙ банк всех монет\n{record} — лучший рекорд")]
    public string statsTemplate = "Дистанция:\n<b>{distance} м</b>\nОбщие монеты: <b>{coins}</b>\nРекорд:\n<b>{record} м</b>";

    [Header("Текущий заезд (Read-Only)")]
    public float currentDistance = 0f;
    public int currentCoins = 0;

    private int savedBankCoins = 0;
    private float savedBestDistance = 0f;
    private bool isSaved = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Считываем сохраненный банк и рекорд из памяти
        savedBankCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        savedBestDistance = PlayerPrefs.GetFloat("BestDistance", 0f);
    }

    private void Start()
    {
        currentDistance = 0f;
        currentCoins = 0;
        isSaved = false;

        UpdateDisplay();
    }

    private void Update()
    {
        // Пока машина едет — начисляем метры и монеты
        if (WorldManager.Instance != null && WorldManager.Instance.isWorldActive && Time.timeScale > 0f)
        {
            if (PlayerController.Instance == null || !PlayerController.Instance.IsDead)
            {
                currentDistance += WorldManager.Instance.roadSpeed * Time.deltaTime;
                currentCoins = Mathf.FloorToInt(currentDistance / metersPerCoin);
            }
        }

        // Текст на экране обновляется ВСЕГДА в реальном времени
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        if (statsDisplayTMP == null) return;

        int distInt = Mathf.FloorToInt(currentDistance);
        
        // Общие монеты = несгораемый банк + то, что заработано прямо сейчас
        int totalCoins = savedBankCoins + currentCoins;

        // Рекорд = максимум между старым рекордом и текущей дистанцией
        int bestDist = Mathf.FloorToInt(Mathf.Max(savedBestDistance, currentDistance));

        // Подставляем цифры в ваш шаблон текста из инспектора
        string formattedText = statsTemplate
            .Replace("{distance}", distInt.ToString())
            .Replace("{coins}", totalCoins.ToString())
            .Replace("{record}", bestDist.ToString());

        statsDisplayTMP.text = formattedText;
    }

    public void SaveResults()
    {
        if (isSaved) return;
        isSaved = true;

        int totalCoins = savedBankCoins + currentCoins;
        PlayerPrefs.SetInt("TotalCoins", totalCoins);

        if (currentDistance > savedBestDistance)
        {
            PlayerPrefs.SetFloat("BestDistance", currentDistance);
        }

        PlayerPrefs.Save();
    }

    private void OnApplicationQuit()
    {
        SaveResults();
    }
}