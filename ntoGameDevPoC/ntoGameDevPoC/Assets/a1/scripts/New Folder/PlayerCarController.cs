using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Полосы")]
    public bool startOnRightLane = true;
    public float laneChangeSpeed = 16.0f;

    [Header("Текущие клавиши управления")]
    public Key leftKey = Key.A;
    public Key rightKey = Key.D;

    private float targetX;
    private float laneOffset = 2.5f;
    private bool isDead = false;
    private Collider playerCollider;

    private void Awake()
    {
        playerCollider = GetComponent<Collider>();
    }

    private void Start()
    {
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
        if (kb == null) return;

        if (kb[leftKey].wasPressedThisFrame)
        {
            targetX = -laneOffset;
        }
        else if (kb[rightKey].wasPressedThisFrame)
        {
            targetX = laneOffset;
        }

        Vector3 currentPos = transform.position;
        float newX = Mathf.MoveTowards(currentPos.x, targetX, laneChangeSpeed * Time.deltaTime);
        transform.position = new Vector3(newX, currentPos.y, currentPos.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        ObstacleMover mover = other.GetComponentInParent<ObstacleMover>();
        if (mover != null)
        {
            if (playerCollider != null)
            {
                float carFrontZ = playerCollider.bounds.max.z;
                float obstacleHalfZ = other.bounds.extents.z;

                Vector3 fixedPos = mover.transform.position;
                fixedPos.z = carFrontZ + obstacleHalfZ;
                mover.transform.position = fixedPos;
            }

            mover.StopMovement();

            if (WorldManager.Instance != null)
            {
                WorldManager.Instance.StopWorld();
            }

            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        // Открываем меню смерти
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