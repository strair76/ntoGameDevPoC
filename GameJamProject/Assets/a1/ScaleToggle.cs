using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ScaleToggle : MonoBehaviour
{
    [Header("Настройки размера")]
    [Tooltip("Размер кнопки, когда она выбрана")]
    public Vector3 selectedScale = new Vector3(1.2f, 1.2f, 1.2f);
    
    [Tooltip("Обычный размер кнопки")]
    public Vector3 normalScale = new Vector3(1f, 1f, 1f);

    [Header("Настройки анимации")]
    [Tooltip("Скорость увеличения/уменьшения (чем выше, тем быстрее)")]
    public float animationSpeed = 10f;

    private Toggle toggle;
    private Vector3 targetScale; // К какому размеру мы стремимся в данный момент

    void Awake()
    {
        toggle = GetComponent<Toggle>();
        
        // Отключаем стандартное изменение цвета
        toggle.transition = Selectable.Transition.None;

        // Подписываемся на изменение состояния (включен/выключен)
        toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    void Start()
    {
        // Задаем начальную цель без анимации, чтобы при старте всё выглядело ровно
        targetScale = toggle.isOn ? selectedScale : normalScale;
        transform.localScale = targetScale;
    }

    void Update()
    {
        // Каждым кадром плавно приближаем текущий размер к целевому
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
    }

    void OnToggleChanged(bool isSelected)
    {
        // При клике просто меняем цель, а Update сделает движение плавным
        targetScale = isSelected ? selectedScale : normalScale;
    }

    void OnDestroy()
    {
        toggle.onValueChanged.RemoveListener(OnToggleChanged);
    }
}
