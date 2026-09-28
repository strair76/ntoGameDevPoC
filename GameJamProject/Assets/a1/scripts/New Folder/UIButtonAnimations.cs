using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIButtonAnimator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Настройки сжатия (Scale)")]
    public float pressedScale = 0.9f;
    public float hoverScale = 1.05f;
    public float animationSpeed = 15f;

    [Header("Настройки для Toggle")]
    public float toggleOnScale = 1.08f;
    public bool animateColor = true;
    public Graphic targetGraphic;
    public Color normalColor = Color.white;
    public Color toggleActiveColor = new Color(0.2f, 1f, 0.6f);

    private Vector3 originalScale;
    private Toggle toggle;
    private Selectable selectable; // Button или Toggle
    private bool isPointerDown = false;
    private bool isPointerInside = false;

    private Coroutine scaleCoroutine;
    private Coroutine colorCoroutine;

    private void Awake()
    {
        originalScale = transform.localScale;
        toggle = GetComponent<Toggle>();
        selectable = GetComponent<Selectable>();

        if (targetGraphic == null)
            targetGraphic = GetComponent<Graphic>();

        if (targetGraphic != null && !animateColor)
            normalColor = targetGraphic.color;
    }

    private void Start()
    {
        if (toggle != null)
        {
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
            ApplyToggleVisualInstant(toggle.isOn);
        }
    }

    private void OnEnable()
    {
        isPointerDown = false;
        isPointerInside = false;

        if (toggle != null)
            ApplyToggleVisualInstant(toggle.isOn);
        else
            transform.localScale = originalScale;
    }

    // Проверка: можно ли сейчас нажимать на кнопку
    private bool IsInteractable()
    {
        return selectable == null || selectable.interactable;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsInteractable()) return; // Если заблокирована — игнорируем нажатие!
        isPointerDown = true;
        StopAndStartScale(GetTargetScale());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!IsInteractable()) return;
        isPointerDown = false;
        StopAndStartScale(GetTargetScale());
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsInteractable()) return;
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

    private void OnToggleValueChanged(bool isOn)
    {
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

    private Vector3 GetTargetScale()
    {
        if (!IsInteractable()) return originalScale;

        if (isPointerDown) return originalScale * pressedScale;
        if (toggle != null && toggle.isOn) return originalScale * toggleOnScale;
        if (isPointerInside) return originalScale * hoverScale;

        return originalScale;
    }

    private void StopAndStartScale(Vector3 target, bool punch = false)
    {
        if (!gameObject.activeInHierarchy) return;

        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(AnimateScaleRoutine(target, punch));
    }

    private IEnumerator AnimateScaleRoutine(Vector3 target, bool punch)
    {
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

        while (Vector3.Distance(transform.localScale, target) > 0.002f)
        {
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