using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIButtonAnimator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Настройки сжатия (Scale)")]
    [Tooltip("Масштаб при нажатии (0.9 = сжатие на 10%)")]
    public float pressedScale = 0.9f;

    [Tooltip("Масштаб при наведении мыши (для ПК)")]
    public float hoverScale = 1.05f;

    [Tooltip("Скорость перехода анимации")]
    public float animationSpeed = 15f;

    [Header("Настройки для Toggle (Переключателей)")]
    [Tooltip("Масштаб кнопки, когда Toggle ВКЛЮЧЕН (выбран)")]
    public float toggleOnScale = 1.08f;

    [Tooltip("Включить плавную смену цвета при выборе?")]
    public bool animateColor = true;

    [Tooltip("Картинка, цвет которой будет меняться (если не указана, возьмет Image с этого объекта)")]
    public Graphic targetGraphic;

    public Color normalColor = Color.white;
    public Color toggleActiveColor = new Color(0.2f, 1f, 0.6f); // Сочный неоново-зеленый

    private Vector3 originalScale;
    private Toggle toggle;
    private bool isPointerDown = false;
    private bool isPointerInside = false;

    private Coroutine scaleCoroutine;
    private Coroutine colorCoroutine;

    private void Awake()
    {
        originalScale = transform.localScale;
        toggle = GetComponent<Toggle>();

        if (targetGraphic == null)
            targetGraphic = GetComponent<Graphic>();

        if (targetGraphic != null && !animateColor)
            normalColor = targetGraphic.color;
    }

    private void Start()
    {
        // Если на объекте есть Toggle — подписываемся на смену состояния
        if (toggle != null)
        {
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
            ApplyToggleVisualInstant(toggle.isOn);
        }
    }

    private void OnEnable()
    {
        // Сброс при повторном открытии меню
        isPointerDown = false;
        isPointerInside = false;

        if (toggle != null)
            ApplyToggleVisualInstant(toggle.isOn);
        else
            transform.localScale = originalScale;
    }

    // --- ОБРАБОТКА НАЖАТИЙ МЫШИ / ПАЛЬЦА ---

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
        StopAndStartScale(GetTargetScale());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;
        StopAndStartScale(GetTargetScale());
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerInside = true;
        if (!isPointerDown)
            StopAndStartScale(GetTargetScale());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerInside = false;
        isPointerDown = false;
        StopAndStartScale(GetTargetScale());
    }

    // --- ЛОГИКА ДЛЯ TOGGLE ---

    private void OnToggleValueChanged(bool isOn)
    {
        // Небольшой сочный "хлопок" (Punch) при переключении
        StopAndStartScale(GetTargetScale(), punch: true);

        if (animateColor && targetGraphic != null)
        {
            Color targetColor = isOn ? toggleActiveColor : normalColor;
            if (colorCoroutine != null) StopCoroutine(colorCoroutine);
            colorCoroutine = StartCoroutine(AnimateColorRoutine(targetColor));
        }
    }

    private void ApplyToggleVisualInstant(bool isOn)
    {
        transform.localScale = isOn ? (originalScale * toggleOnScale) : originalScale;

        if (animateColor && targetGraphic != null)
        {
            targetGraphic.color = isOn ? toggleActiveColor : normalColor;
        }
    }

    // Определение целевого размера кнопки прямо сейчас
    private Vector3 GetTargetScale()
    {
        // 1. Палец зажат -> кнопка сжата
        if (isPointerDown)
        {
            return originalScale * pressedScale;
        }

        // 2. Это Toggle и он ВКЛЮЧЕН -> кнопка чуть увеличена
        if (toggle != null && toggle.isOn)
        {
            return originalScale * toggleOnScale;
        }

        // 3. Мышь наведена -> легкое увеличение
        if (isPointerInside)
        {
            return originalScale * hoverScale;
        }

        // 4. Покой -> стандартный размер
        return originalScale;
    }

    // --- ПЛАВНЫЕ КОРОУТИНЫ (РАБОТАЮТ НА ПАУЗЕ) ---

    private void StopAndStartScale(Vector3 target, bool punch = false)
    {
        if (!gameObject.activeInHierarchy) return;

        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        scaleCoroutine = StartCoroutine(AnimateScaleRoutine(target, punch));
    }

    private IEnumerator AnimateScaleRoutine(Vector3 target, bool punch)
    {
        // Если это переключение Toggle — сначала делаем короткий упругий отскок
        if (punch)
        {
            Vector3 punchScale = target * 1.12f;
            float punchTime = 0f;

            while (punchTime < 1f)
            {
                punchTime += Time.unscaledDeltaTime * (animationSpeed * 1.5f);
                transform.localScale = Vector3.Lerp(transform.localScale, punchScale, punchTime);
                yield return null;
            }
        }

        // Плавная доводка до итогового размера
        while (Vector3.Distance(transform.localScale, target) > 0.002f)
        {
            // Time.unscaledDeltaTime позволяет анимации работать даже при Time.timeScale = 0 (на паузе)
            transform.localScale = Vector3.Lerp(transform.localScale, target, Time.unscaledDeltaTime * animationSpeed);
            yield return null;
        }

        transform.localScale = target;
    }

    private IEnumerator AnimateColorRoutine(Color targetColor)
    {
        while (targetGraphic != null && Vector4.Distance(targetGraphic.color, targetColor) > 0.01f)
        {
            targetGraphic.color = Color.Lerp(targetGraphic.color, targetColor, Time.unscaledDeltaTime * animationSpeed);
            yield return null;
        }

        if (targetGraphic != null)
            targetGraphic.color = targetColor;
    }
}