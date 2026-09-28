using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    private bool isCar;
    private float destroyZ;
    private bool isReady = false;
    private bool isStopped = false;

    public GameObject SourcePrefab { get; private set; }

    public void Init(bool isCar, float despawnZ, GameObject sourcePrefab)
    {
        this.isCar = isCar;
        this.destroyZ = despawnZ;
        this.SourcePrefab = sourcePrefab;
        this.isStopped = false;
        this.isReady = true;
    }

    public void StopMovement()
    {
        isStopped = true;
    }

    private void Update()
    {
        if (!isReady || isStopped) return;

        float extra = 0f;
        float baseRoadSpeed = 0f;

        if (WorldManager.Instance != null && WorldManager.Instance.isWorldActive)
        {
            baseRoadSpeed = WorldManager.Instance.roadSpeed;
            extra = isCar ? WorldManager.Instance.activeCarExtraSpeed : 0f;
        }

        float currentSpeed = baseRoadSpeed + extra;

        // Запоминаем позицию Z ДО движения
        float prevZ = transform.position.z;

        // Перемещаем объект
        transform.position += Vector3.back * currentSpeed * Time.deltaTime;

        // Позиция Z ПОСЛЕ движения
        float currZ = transform.position.z;

        // ==============================================================
        //  МАТЕМАТИЧЕСКИЙ ДЕТЕКТОР СТОЛКНОВЕНИЯ (НЕ ЗАВИСИТ ОТ КОЛЛАЙДЕРОВ)
        // ==============================================================
        if (PlayerController.Instance != null && !PlayerController.Instance.IsDead)
        {
            Vector3 playerPos = PlayerController.Instance.transform.position;

            // 1. Проверяем полосу (на одной ли полосе машина и куб по оси X)
            bool sameLane = Mathf.Abs(transform.position.x - playerPos.x) < 1.6f;

            // 2. Проверяем высоту (с запасом по Y)
            bool sameHeight = Mathf.Abs(transform.position.y - playerPos.y) < 3.0f;

            // 3. Проверяем глубину: куб либо рядом с машиной, ЛИБО перелетел через неё за этот кадр
            bool touchedZ = Mathf.Abs(currZ - playerPos.z) < 1.4f;
            bool crossedOverZ = (prevZ >= playerPos.z && currZ <= playerPos.z);

            // ЕСЛИ ВСЕ УСЛОВИЯ СОВПАЛИ — ЭТО АВАРИЯ!
            if (sameLane && sameHeight && (touchedZ || crossedOverZ))
            {
                // Прижимаем встык ровно перед капотом
                Vector3 snapPos = transform.position;
                snapPos.z = playerPos.z + 1.6f;
                transform.position = snapPos;

                // Запускаем аварию
                PlayerController.Instance.TriggerCrash(this);
                return;
            }
        }

        // Если улетел за камеру — возвращаем в пул
        if (transform.position.z <= destroyZ)
        {
            Despawn();
        }
    }

    public void Despawn()
    {
        isReady = false;

        if (WorldManager.Instance != null)
        {
            WorldManager.Instance.ReturnObstacleToPool(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}