using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GraphicsSettings : MonoBehaviour
{
    [Header("Элементы UI")]
    [Tooltip("Выпадающий список разрешений (TextMeshPro Dropdown)")]
    public TMP_Dropdown resolutionDropdown;

    [Tooltip("Галочка полноэкранного режима")]
    public Toggle fullscreenToggle;

    [Tooltip("Опционально: выпадающий список качества графики (Низкое/Среднее/Ультра)")]
    public TMP_Dropdown qualityDropdown;

    private List<Resolution> uniqueResolutions = new List<Resolution>();

    private void Awake()
    {
        // Применяем сохраненные настройки сразу при загрузке
        LoadSavedSettings();
    }

    private void Start()
    {
        InitResolutionsDropdown();
        InitQualityDropdown();
        InitFullscreenToggle();
    }

    // --- 1. РАЗРЕШЕНИЕ ЭКРАНА ---
    private void InitResolutionsDropdown()
    {
        if (resolutionDropdown == null) return;

        resolutionDropdown.ClearOptions();
        uniqueResolutions.Clear();

        List<string> options = new List<string>();
        int currentResIndex = 0;

        Resolution[] allResolutions = Screen.resolutions;

        // Фильтруем список, чтобы не было дублей с разной частотой кадров (60Hz, 144Hz и т.д.)
        for (int i = 0; i < allResolutions.Length; i++)
        {
            Resolution res = allResolutions[i];

            bool alreadyExists = uniqueResolutions.Exists(r => r.width == res.width && r.height == res.height);

            if (!alreadyExists)
            {
                uniqueResolutions.Add(res);
                options.Add($"{res.width} x {res.height}");

                // Проверяем, совпадает ли с текущим разрешением экрана
                if (res.width == Screen.currentResolution.width && res.height == Screen.currentResolution.height)
                {
                    currentResIndex = uniqueResolutions.Count - 1;
                }
            }
        }

        resolutionDropdown.AddOptions(options);

        // Если в памяти уже есть сохраненный выбор — берем его
        int savedIndex = PlayerPrefs.GetInt("SelectedResolutionIndex", currentResIndex);
        if (savedIndex >= uniqueResolutions.Count) savedIndex = currentResIndex;

        resolutionDropdown.value = savedIndex;
        resolutionDropdown.RefreshShownValue();

        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    public void SetResolution(int resolutionIndex)
    {
        if (resolutionIndex < 0 || resolutionIndex >= uniqueResolutions.Count) return;

        Resolution selected = uniqueResolutions[resolutionIndex];
        bool isFullscreen = Screen.fullScreen;

        Screen.SetResolution(selected.width, selected.height, isFullscreen);

        PlayerPrefs.SetInt("SelectedResolutionIndex", resolutionIndex);
        PlayerPrefs.SetInt("ResWidth", selected.width);
        PlayerPrefs.SetInt("ResHeight", selected.height);
        PlayerPrefs.Save();

        Debug.Log($"<color=cyan>[ГРАФИКА]</color> Установлено разрешение: <b>{selected.width} x {selected.height}</b>");
    }

    // --- 2. ПОЛНЫЙ ЭКРАН ---
    private void InitFullscreenToggle()
    {
        if (fullscreenToggle == null) return;

        bool isFullscreen = PlayerPrefs.GetInt("IsFullscreen", Screen.fullScreen ? 1 : 0) == 1;

        fullscreenToggle.isOn = isFullscreen;
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;

        PlayerPrefs.SetInt("IsFullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log($"<color=cyan>[ГРАФИКА]</color> Полный экран: <b>{isFullscreen}</b>");
    }

    // --- 3. ПРЕСЕТЫ КАЧЕСТВА (Low / Medium / High / Ultra) ---
    private void InitQualityDropdown()
    {
        if (qualityDropdown == null) return;

        qualityDropdown.ClearOptions();

        List<string> options = new List<string>(QualitySettings.names);
        qualityDropdown.AddOptions(options);

        int savedQuality = PlayerPrefs.GetInt("SelectedQualityLevel", QualitySettings.GetQualityLevel());
        qualityDropdown.value = savedQuality;
        qualityDropdown.RefreshShownValue();

        qualityDropdown.onValueChanged.AddListener(SetQualityLevel);
    }

    public void SetQualityLevel(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex, true);

        PlayerPrefs.SetInt("SelectedQualityLevel", qualityIndex);
        PlayerPrefs.Save();

        Debug.Log($"<color=cyan>[ГРАФИКА]</color> Пресет качества: <b>{QualitySettings.names[qualityIndex]}</b>");
    }

    // Применение настроек при старте игры
    private void LoadSavedSettings()
    {
        if (PlayerPrefs.HasKey("ResWidth") && PlayerPrefs.HasKey("ResHeight"))
        {
            int width = PlayerPrefs.GetInt("ResWidth");
            int height = PlayerPrefs.GetInt("ResHeight");
            bool isFull = PlayerPrefs.GetInt("IsFullscreen", 1) == 1;

            Screen.SetResolution(width, height, isFull);
        }

        if (PlayerPrefs.HasKey("SelectedQualityLevel"))
        {
            QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("SelectedQualityLevel"), true);
        }
    }
}