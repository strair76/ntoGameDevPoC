using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Настройки экономики")]
    [Tooltip("Сколько метров нужно проехать для получения 1 монеты")]
    public float metersPerCoin = 100f;

    [Header("3 отдельных текста статистики на экране")]
    [Tooltip("1. Текстовый объект для отображения дистанции")]
    public TextMeshProUGUI distanceText;

    [Tooltip("2. Текстовый объект для отображения монет")]
    public TextMeshProUGUI coinsText;

    [Tooltip("3. Текстовый объект для отображения рекорда")]
    public TextMeshProUGUI recordText;

    [Header("Кастомизация шаблонов в Инспекторе")]
    [TextArea(2, 3)]
    [Tooltip("Метка {distance} заменится на метры текущего заезда")]
    public string distanceTemplate = "Дистанция:\n<b>{distance} м</b>";

    [TextArea(2, 3)]
    [Tooltip("Метка {coins} — общий банк монет. Метка {runCoins} — монеты только за этот заезд")]
    public string coinsTemplate = "Монеты:\n<b>{coins}</b>";

    [TextArea(2, 3)]
    [Tooltip("Метка {record} заменится на лучший рекорд дистанции")]
    public string recordTemplate = "Рекорд:\n<b>{record} м</b>";

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

        // Считываем сохраненный банк монет и рекорд из памяти
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
        // Пока машина едет — начисляем метры и считаем монеты
        if (WorldManager.Instance != null && WorldManager.Instance.isWorldActive && Time.timeScale > 0f)
        {
            if (PlayerController.Instance == null || !PlayerController.Instance.IsDead)
            {
                currentDistance += WorldManager.Instance.roadSpeed * Time.deltaTime;
                currentCoins = Mathf.FloorToInt(currentDistance / metersPerCoin);
            }
        }

        // Обновляем все 3 текста на экране в реальном времени
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        int distInt = Mathf.FloorToInt(currentDistance);

        // Общие монеты = банк из памяти + то, что заработано прямо сейчас в заезде
        int totalCoins = savedBankCoins + currentCoins;

        // Рекорд = максимум между старым рекордом и текущей дистанцией
        int bestDist = Mathf.FloorToInt(Mathf.Max(savedBestDistance, currentDistance));

        // 1. Обновляем текст дистанции
        if (distanceText != null)
        {
            distanceText.text = distanceTemplate.Replace("{distance}", distInt.ToString());
        }

        // 2. Обновляем текст монет (поддерживает и общий банк {coins}, и монеты за заезд {runCoins})
        if (coinsText != null)
        {
            coinsText.text = coinsTemplate
                .Replace("{coins}", totalCoins.ToString())
                .Replace("{runCoins}", currentCoins.ToString());
        }

        // 3. Обновляем текст рекорда
        if (recordText != null)
        {
            recordText.text = recordTemplate
                .Replace("{record}", bestDist.ToString())
                .Replace("{distance}", bestDist.ToString());
        }
    }

    public void SaveResults()
    {
        if (isSaved) return;
        isSaved = true;

        // Сохраняем дистанцию этого заезда (для меню)
        PlayerPrefs.SetFloat("LastDistance", currentDistance);

        // Пополняем общий несгораемый банк монет
        int totalCoins = savedBankCoins + currentCoins;
        PlayerPrefs.SetInt("TotalCoins", totalCoins);

        // Обновляем рекорд
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