using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class KeyRandomizer : MonoBehaviour
{
    public PlayerController playerController;

    [Header("Постоянный HUD (текущие клавиши)")]
    public TextMeshProUGUI keyDisplayTMP;

    [Header("Плавное предупреждение (Canvas Group)")]
    [Tooltip("Объект WarningBanner с компонентом Canvas Group")]
    public CanvasGroup warningCanvasGroup;

    [Tooltip("Текст внутри WarningBanner")]
    public TextMeshProUGUI warningTMP;

    [Tooltip("За сколько секунд до смены начать плавное появление")]
    public float warningDuration = 3.0f;

    [Tooltip("Скорость плавного появления и затухания (чем больше, тем быстрее)")]
    public float fadeSpeed = 3.5f;

    [Header("Текущие клавиши")]
    public Key currentLeftKey = Key.A;
    public Key currentRightKey = Key.D;

    [Header("Пулы для случайной генерации")]
    public Key[] leftSidePool = new Key[]
    {
        Key.LeftCtrl, Key.LeftShift, Key.LeftAlt, Key.Tab, Key.CapsLock,
        Key.F1, Key.F2, Key.F3, Key.F4,
        Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5,
        Key.Q, Key.W, Key.E, Key.R, Key.T, Key.A, Key.S, Key.D, Key.F, Key.G, Key.Z, Key.X, Key.C, Key.V
    };

    public Key[] rightSidePool = new Key[]
    {
        Key.Enter, Key.Backspace, Key.RightShift, Key.RightCtrl, Key.RightAlt,
        Key.F9, Key.F10, Key.F11, Key.F12,
        Key.Digit7, Key.Digit8, Key.Digit9, Key.Digit0,
        Key.Y, Key.U, Key.I, Key.O, Key.P, Key.H, Key.J, Key.K, Key.L, Key.N, Key.M
    };

    private float timer;
    private float currentInterval = 15f;
    private DifficultyManager.DifficultyData activeData;
    private float postSwitchDisplayTimer = 0f; // Таймер показа сообщения "Клавиши сменены"

    private void Start()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        if (DifficultyManager.Instance != null)
        {
            activeData = DifficultyManager.Instance.GetActiveSettings();
            currentInterval = activeData.keyChangeInterval;
        }

        currentLeftKey = Key.A;
        currentRightKey = Key.D;

        if (playerController != null)
        {
            playerController.SetKeys(currentLeftKey, currentRightKey);
        }

        timer = currentInterval;

        // На старте плашка полностью прозрачна
        if (warningCanvasGroup != null)
        {
            warningCanvasGroup.alpha = 0f;
        }

        UpdateHUD();
    }

    private void Update()
    {
        if (WorldManager.Instance != null && !WorldManager.Instance.isWorldActive) return;
        if (Time.timeScale == 0) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            ChooseNextKeyCombination();
            timer = currentInterval;
            postSwitchDisplayTimer = 1.0f; // Держим плашку 1 секунду после смены
        }

        // Обновляем состояние интерфейса и плавную анимацию прозрачности
        UpdateWarningAnimation();
        UpdateHUD();
    }

    // Плавная анимация появления и исчезновения
    private void UpdateWarningAnimation()
    {
        if (warningCanvasGroup == null) return;

        float targetAlpha = 0f;

        // 1. ФАЗА ПРЕДУПРЕЖДЕНИЯ (за 3 секунды до смены)
        if (timer <= warningDuration)
        {
            targetAlpha = 1f; // Плавно проявляем

            if (warningTMP != null)
            {
                int secondsLeft = Mathf.CeilToInt(timer);
                warningTMP.text = $"<color=#FF3333>⚠️ ВНИМАНИЕ!</color>\nСмена клавиш через: <b>{secondsLeft}</b>...";
            }
        }
        // 2. ФАЗА СРАЗУ ПОСЛЕ СМЕНЫ (показываем новые клавиши)
        else if (postSwitchDisplayTimer > 0f)
        {
            postSwitchDisplayTimer -= Time.deltaTime;
            targetAlpha = 1f;

            if (warningTMP != null)
            {
                string leftName = FormatKeyName(currentLeftKey);
                string rightName = FormatKeyName(currentRightKey);
                warningTMP.text = $"<color=#00FF88>КЛАВИШИ СМЕНЕНЫ!</color>\n[<b>{leftName}</b>] | [<b>{rightName}</b>]";
            }
        }
        // 3. ФАЗА СПОКОЙНОЙ ЕЗДЫ (плавно растворяем)
        else
        {
            targetAlpha = 0f;
        }

        // Плавная интерполяция прозрачности к целевому значению
        warningCanvasGroup.alpha = Mathf.MoveTowards(warningCanvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
    }

    public void ChooseNextKeyCombination()
    {
        if (activeData == null && DifficultyManager.Instance != null)
            activeData = DifficultyManager.Instance.GetActiveSettings();

        if (activeData == null) return;

        int totalWeight = 0;
        for (int i = 0; i < activeData.presetCombinations.Count; i++)
            totalWeight += activeData.presetCombinations[i].weight;

        if (activeData.allowRandomPoolKeys && activeData.randomPoolWeight > 0)
            totalWeight += activeData.randomPoolWeight;

        if (totalWeight <= 0) return;

        int randomRoll = Random.Range(0, totalWeight);
        int accumulatedWeight = 0;

        for (int i = 0; i < activeData.presetCombinations.Count; i++)
        {
            accumulatedWeight += activeData.presetCombinations[i].weight;
            if (randomRoll < accumulatedWeight)
            {
                ApplyKeys(activeData.presetCombinations[i].leftKey, activeData.presetCombinations[i].rightKey);
                return;
            }
        }

        Key lKey = leftSidePool[Random.Range(0, leftSidePool.Length)];
        Key rKey = rightSidePool[Random.Range(0, rightSidePool.Length)];
        ApplyKeys(lKey, rKey);
    }

    private void ApplyKeys(Key left, Key right)
    {
        currentLeftKey = left;
        currentRightKey = right;

        if (playerController != null)
            playerController.SetKeys(currentLeftKey, currentRightKey);
    }

    private void UpdateHUD()
    {
        if (keyDisplayTMP != null)
        {
            string leftFormatted = FormatKeyName(currentLeftKey);
            string rightFormatted = FormatKeyName(currentRightKey);

            keyDisplayTMP.text = $"Влево: [<b><color=#00FFCC>{leftFormatted}</color></b>] | Вправо: [<b><color=#FFCC00>{rightFormatted}</color></b>]";
        }
    }

    private string FormatKeyName(Key key)
    {
        switch (key)
        {
            case Key.LeftCtrl: return "L-CTRL";
            case Key.RightCtrl: return "R-CTRL";
            case Key.LeftShift: return "L-SHIFT";
            case Key.RightShift: return "R-SHIFT";
            case Key.LeftAlt: return "L-ALT";
            case Key.RightAlt: return "R-ALT";
            case Key.CapsLock: return "CAPS";
            case Key.Backspace: return "BACKSPACE";
            case Key.Enter: return "ENTER";
            case Key.Tab: return "TAB";
            case Key.Digit1: return "1";
            case Key.Digit2: return "2";
            case Key.Digit3: return "3";
            case Key.Digit4: return "4";
            case Key.Digit5: return "5";
            case Key.Digit6: return "6";
            case Key.Digit7: return "7";
            case Key.Digit8: return "8";
            case Key.Digit9: return "9";
            case Key.Digit0: return "0";
            default: return key.ToString().ToUpper();
        }
    }
}