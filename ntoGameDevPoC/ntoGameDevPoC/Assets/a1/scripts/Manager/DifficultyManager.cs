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
        public float startRoadSpeed = 18f;
        public float maxRoadSpeed = 40f;
        public float speedIncreasePerSec = 0.2f;

        [Header("Встречные машины")]
        public float startCarExtraSpeed = 8f;
        public float maxCarExtraSpeed = 22f;
        public float carSpeedIncreasePerSec = 0.2f;

        [Header("Окно реакции на спавн")]
        public float startSafeGap = 1.8f;
        public float minSafeGap = 1.2f;

        [Header("Смена клавиш")]
        public float keyChangeInterval = 15.0f;
        public bool allowRandomPoolKeys = true;
        [Range(0, 100)] public int randomPoolWeight = 15;

        public List<KeyCombination> presetCombinations = new List<KeyCombination>();
    }

    [Header("Текущая сложность")]
    public DifficultyLevel currentDifficulty = DifficultyLevel.Normal;

    [Header("Пресеты сложности")]
    public DifficultyData easyPreset = new DifficultyData();
    public DifficultyData normalPreset = new DifficultyData();
    public DifficultyData hardPreset = new DifficultyData();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 1. ЗАГРУЖАЕМ ВСЕГДА (без всяких галочек и условий)
        if (PlayerPrefs.HasKey("SelectedDifficulty"))
        {
            int savedIndex = PlayerPrefs.GetInt("SelectedDifficulty");
            currentDifficulty = (DifficultyLevel)savedIndex;
        }

        // 2. Гарантированно выставляем контрастные значения для проверки
        SetupPresets();

        // 3. Выводим в консоль подтверждение, что сложность ПРИМЕНЕНА
        DifficultyData active = GetActiveSettings();
        Debug.Log($"<color=yellow>[ИГРА]</color> Применена сложность: <b>{currentDifficulty}</b> | Скорость дороги: <b>{active.startRoadSpeed}</b> | Смена клавиш: <b>{active.keyChangeInterval}с</b>");
    }

    private void SetupPresets()
    {
        // ЛЕГКИЙ
        easyPreset.startRoadSpeed = 16f;
        easyPreset.maxRoadSpeed = 35f;
        easyPreset.speedIncreasePerSec = 0.15f;
        easyPreset.startCarExtraSpeed = 6f;
        easyPreset.maxCarExtraSpeed = 15f;
        easyPreset.carSpeedIncreasePerSec = 0.1f;
        easyPreset.startSafeGap = 1.9f;
        easyPreset.minSafeGap = 1.3f;
        easyPreset.keyChangeInterval = 25.0f; // Каждые 25 секунд
        easyPreset.allowRandomPoolKeys = false;

        if (easyPreset.presetCombinations.Count == 0)
        {
            easyPreset.presetCombinations.Add(new KeyCombination("База A/D", Key.A, Key.D, 50));
            easyPreset.presetCombinations.Add(new KeyCombination("W / S", Key.W, Key.S, 25));
        }

        // СРЕДНИЙ
        normalPreset.startRoadSpeed = 24f;
        normalPreset.maxRoadSpeed = 52f;
        normalPreset.speedIncreasePerSec = 0.3f;
        normalPreset.startCarExtraSpeed = 12f;
        normalPreset.maxCarExtraSpeed = 25f;
        normalPreset.carSpeedIncreasePerSec = 0.25f;
        normalPreset.startSafeGap = 1.5f;
        normalPreset.minSafeGap = 0.85f;
        normalPreset.keyChangeInterval = 15.0f; // Каждые 15 секунд
        normalPreset.allowRandomPoolKeys = true;
        normalPreset.randomPoolWeight = 20;

        if (normalPreset.presetCombinations.Count == 0)
        {
            normalPreset.presetCombinations.Add(new KeyCombination("База A/D", Key.A, Key.D, 40));
            normalPreset.presetCombinations.Add(new KeyCombination("L-Shift / Enter", Key.LeftShift, Key.Enter, 15));
        }

        // ХАРДКОР (ОЧЕНЬ БЫСТРО)
        hardPreset.startRoadSpeed = 38f; // Машина полетит сразу на бешеной скорости
        hardPreset.maxRoadSpeed = 80f;
        hardPreset.speedIncreasePerSec = 0.6f;
        hardPreset.startCarExtraSpeed = 20f;
        hardPreset.maxCarExtraSpeed = 40f;
        hardPreset.carSpeedIncreasePerSec = 0.45f;
        hardPreset.startSafeGap = 1.0f;
        hardPreset.minSafeGap = 0.55f;
        hardPreset.keyChangeInterval = 7.0f; // Смена каждые 7 секунд!
        hardPreset.allowRandomPoolKeys = true;
        hardPreset.randomPoolWeight = 40;

        if (hardPreset.presetCombinations.Count == 0)
        {
            hardPreset.presetCombinations.Add(new KeyCombination("A / D", Key.A, Key.D, 10));
            hardPreset.presetCombinations.Add(new KeyCombination("F1 / F11", Key.F1, Key.F11, 25));
            hardPreset.presetCombinations.Add(new KeyCombination("L-Ctrl / R-Alt", Key.LeftCtrl, Key.RightAlt, 25));
        }
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