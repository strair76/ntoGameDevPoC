using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    public enum DifficultyLevel { Easy, Normal, Hard }

    [System.Serializable]
    public class KeyCombination
    {
        public string label = "Связка";
        public Key leftKey = Key.A;
        public Key rightKey = Key.D;

        [Range(1, 100)]
        public int weight = 10;

        public KeyCombination(string label, Key left, Key right, int weight)
        {
            this.label = label;
            this.leftKey = left;
            this.rightKey = right;
            this.weight = weight;
        }
    }

    [System.Serializable]
    public class DifficultyData
    {
        [Header("Скорость дороги")]
        public float startRoadSpeed = 20f;
        public float maxRoadSpeed = 45f;
        public float speedIncreasePerSec = 0.25f;

        [Header("Встречные машины")]
        public float startCarExtraSpeed = 10f;
        public float maxCarExtraSpeed = 25f;
        public float carSpeedIncreasePerSec = 0.2f;

        [Header("Окно реакции на спавн")]
        public float startSafeGap = 1.6f;
        public float minSafeGap = 1.0f;

        [Header("Смена клавиш")]
        [Tooltip("Интервал смены клавиш в секундах для этой сложности")]
        public float keyChangeInterval = 15.0f;

        public bool allowRandomPoolKeys = true;
        [Range(0, 100)] public int randomPoolWeight = 15;

        public List<KeyCombination> presetCombinations = new List<KeyCombination>();
    }

    [Header("Текущая сложность")]
    public DifficultyLevel currentDifficulty = DifficultyLevel.Normal;

    [Header("Пресеты сложности (НАСТРАИВАЮТСЯ ЗДЕСЬ)")]
    public DifficultyData easyPreset = new DifficultyData 
    { 
        startRoadSpeed = 16f, maxRoadSpeed = 35f, speedIncreasePerSec = 0.15f,
        startCarExtraSpeed = 6f, maxCarExtraSpeed = 15f, carSpeedIncreasePerSec = 0.1f,
        startSafeGap = 1.9f, minSafeGap = 1.3f, keyChangeInterval = 25.0f, allowRandomPoolKeys = false 
    };

    public DifficultyData normalPreset = new DifficultyData 
    { 
        startRoadSpeed = 24f, maxRoadSpeed = 52f, speedIncreasePerSec = 0.3f,
        startCarExtraSpeed = 12f, maxCarExtraSpeed = 25f, carSpeedIncreasePerSec = 0.25f,
        startSafeGap = 1.5f, minSafeGap = 0.85f, keyChangeInterval = 15.0f, allowRandomPoolKeys = true, randomPoolWeight = 20 
    };

    public DifficultyData hardPreset = new DifficultyData 
    { 
        startRoadSpeed = 38f, maxRoadSpeed = 80f, speedIncreasePerSec = 0.55f,
        startCarExtraSpeed = 18f, maxCarExtraSpeed = 35f, carSpeedIncreasePerSec = 0.4f,
        startSafeGap = 1.0f, minSafeGap = 0.55f, keyChangeInterval = 7.0f, allowRandomPoolKeys = true, randomPoolWeight = 40 
    };

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Загружаем сохраненную сложность из Меню
        if (PlayerPrefs.HasKey("SelectedDifficulty"))
        {
            currentDifficulty = (DifficultyLevel)PlayerPrefs.GetInt("SelectedDifficulty");
        }

        // Выводим в консоль ТОЧНЫЕ цифры из вашего Инспектора
        DifficultyData active = GetActiveSettings();
        Debug.Log($"<color=cyan>[ПРЕСЕТ АКТИВИРОВАН]</color> Режим: <b>{currentDifficulty}</b> | " +
                  $"Стартовая скорость дороги: <b>{active.startRoadSpeed}</b> | " +
                  $"Интервал смены клавиш: <b>{active.keyChangeInterval}с</b>");
    }

    public DifficultyData GetActiveSettings()
    {
        switch (currentDifficulty)
        {
            case DifficultyLevel.Easy: return easyPreset;
            case DifficultyLevel.Hard: return hardPreset;
            default: return normalPreset;
        }
    }
}