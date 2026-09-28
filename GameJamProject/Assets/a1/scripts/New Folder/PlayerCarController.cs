using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Полосы")]
    public bool startOnRightLane = true;
    public float laneChangeSpeed = 16.0f;

    [Header("Текущие клавиши управления")]
    public Key leftKey = Key.A;
    public Key rightKey = Key.D;

    public bool IsDead => isDead;

    private float targetX;
    private float laneOffset = 2.5f;
    private bool isDead = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        isDead = false;
        Time.timeScale = 1.0f;

        if (WorldManager.Instance != null)
        {
            laneOffset = WorldManager.Instance.laneOffset;
        }

        targetX = startOnRightLane ? laneOffset : -laneOffset;

        Vector3 startPos = transform.position;
        transform.position = new Vector3(targetX, startPos.y, startPos.z);
    }

    public void SetKeys(Key newLeft, Key newRight)
    {
        leftKey = newLeft;
        rightKey = newRight;
    }

    private void Update()
    {
        if (isDead) return;

        Keyboard kb = Keyboard.current;
        if (kb != null)
        {
            if (kb[leftKey].wasPressedThisFrame) targetX = -laneOffset;
            else if (kb[rightKey].wasPressedThisFrame) targetX = laneOffset;
        }

        Vector3 currentPos = transform.position;
        float newX = Mathf.MoveTowards(currentPos.x, targetX, laneChangeSpeed * Time.deltaTime);
        transform.position = new Vector3(newX, currentPos.y, currentPos.z);
    }

    // МЕТОД АВАРИИ (вызывается математическим детектором)
    public void TriggerCrash(ObstacleMover mover)
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"<color=red><b>[100% АВАРИЯ!]</b></color> Столкновение с объектом: <b>{mover.gameObject.name}</b>");

        // 1. Останавливаем препятствие
        mover.StopMovement();

        // 2. Останавливаем дорогу и спавн
        if (WorldManager.Instance != null)
        {
            WorldManager.Instance.StopWorld();
        }

        // 3. Открываем меню смерти
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.TriggerGameOver();
        }
        else
        {
            Time.timeScale = 0f;
        }
    }
}