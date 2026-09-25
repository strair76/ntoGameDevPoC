using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class KeyRandomizer : MonoBehaviour
{
    public PlayerController playerController;

    [Header("Постоянный HUD (текущие клавиши в углу)")]
    public TextMeshProUGUI hudKeyDisplayTMP;

    [Header("Ваш собственный объект предупреждения (UI)")]
    [Tooltip("Объект баннера/плашки предупреждения на Canvas")]
    public GameObject warningBannerObject;

    [Tooltip("Компонент CanvasGroup на баннере (для плавного растворения альфы)")]
    public CanvasGroup warningCanvasGroup;

    [Tooltip("Текст внутри вашего баннера предупреждения")]
    public TextMeshProUGUI warningTMP;

    [Header("Настройки времени")]
    [Tooltip("За сколько секунд до смены показывать предупреждение")]
    public float warningDuration = 3.0f;

    [Tooltip("Сколько секунд держать сообщение 'Клавиши сменены' после смены")]
    public float postSwitchDisplayDuration = 1.2f;

    [Tooltip("Скорость плавного появления и растворения")]
    public float fadeSpeed = 3.5f;

    [Header("Кастомизация текста")]
    [TextArea]
    public string warningMessageTemplate = "<color=#FF3333><b>⚠️ ВНИМАНИЕ!</b></color>\nСмена клавиш через: <b>{time}</b>...";

    [TextArea]
    public string switchedMessageTemplate = "<color=#00FF88><b>КЛАВИШИ СМЕНЕНЫ!</b></color>\n[<b>{left}</b>]  |  [<b>{right}</b>]";

    [Header("Текущие активные клавиши")]
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
    private float postSwitchTimer = 0f;

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

        HideWarningImmediate();
        UpdateHUD();
    }

    private void Update()
    {
        // 1. ПРОВЕРКА НА ПАУЗУ ИЛИ СМЕРТЬ:
        bool isGamePausedOrDead = (Time.timeScale == 0f) ||
                                  (PlayerController.Instance != null && PlayerController.Instance.IsDead) ||
                                  (WorldManager.Instance != null && !WorldManager.Instance.isWorldActive);

        // Если игра остановлена (пауза по Esc или авария) — НЕМЕДЛЕННО скрываем баннер
        if (isGamePausedOrDead)
        {
            HideWarningImmediate();
            return;
        }

        // 2. ТАЙМЕРЫ И СМЕНА КЛАВИШ (тикают только когда игра активна)
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            ChooseNextKeyCombination();
            timer = currentInterval;
            postSwitchTimer = postSwitchDisplayDuration;
        }

        UpdateWarningDisplay();
        UpdateHUD();
    }

    // Мгновенное скрытие баннера (вызывается на паузе и при смерти)
    public void HideWarningImmediate()
    {
        if (warningCanvasGroup != null)
        {
            warningCanvasGroup.alpha = 0f;
        }

        if (warningBannerObject != null)
        {
            warningBannerObject.SetActive(false);
        }
    }

    private void UpdateWarningDisplay()
    {
        float targetAlpha = 0f;
        bool shouldBeVisible = false;

        // 1. Фаза предупреждения (обратный отсчет)
        if (timer <= warningDuration)
        {
            shouldBeVisible = true;
            targetAlpha = 1f;

            if (warningTMP != null)
            {
                int secondsLeft = Mathf.CeilToInt(timer);
                warningTMP.text = warningMessageTemplate.Replace("{time}", secondsLeft.ToString());
            }
        }
        // 2. Фаза подтверждения смены
        else if (postSwitchTimer > 0f)
        {
            postSwitchTimer -= Time.deltaTime;
            shouldBeVisible = true;
            targetAlpha = 1f;

            if (warningTMP != null)
            {
                string leftName = FormatKeyName(currentLeftKey);
                string rightName = FormatKeyName(currentRightKey);
                warningTMP.text = switchedMessageTemplate.Replace("{left}", leftName).Replace("{right}", rightName);
            }
        }
        // 3. Фаза покоя
        else
        {
            targetAlpha = 0f;
        }

        // Плавное растворение или обычное включение
        if (warningCanvasGroup != null)
        {
            warningCanvasGroup.alpha = Mathf.MoveTowards(warningCanvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);

            if (warningBannerObject != null)
            {
                warningBannerObject.SetActive(warningCanvasGroup.alpha > 0.01f);
            }
        }
        else if (warningBannerObject != null)
        {
            warningBannerObject.SetActive(shouldBeVisible);
        }
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
        if (hudKeyDisplayTMP != null)
        {
            string leftFormatted = FormatKeyName(currentLeftKey);
            string rightFormatted = FormatKeyName(currentRightKey);
            hudKeyDisplayTMP.text = $"Влево: [<b><color=#00FFCC>{leftFormatted}</color></b>] | Вправо: [<b><color=#FFCC00>{rightFormatted}</color></b>]";
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
            case Key.Backspace: return "BACK";
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