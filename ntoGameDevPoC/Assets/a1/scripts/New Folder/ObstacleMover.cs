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

        transform.position += Vector3.back * currentSpeed * Time.deltaTime;

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