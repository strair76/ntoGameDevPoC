using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance { get; private set; }

    [System.Serializable]
    public class ObstacleConfig
    {
        public string label = "Препятствие";
        public GameObject prefab;
        public float spawnY = 0f;
        public Vector3 rotationOffset = Vector3.zero;
    }

    [Header("Текущее состояние мира")]
    public float roadSpeed;
    public float activeCarExtraSpeed; // Динамически растущая скорость встречных машин
    public float currentSafeTimeGap;
    public float elapsedTime = 0f;
    public bool isWorldActive = true;

    [Header("Движение дороги")]
    public Transform[] roadSegments;
    public float roadSegmentLength = 20.0f;

    [Header("Препятствия")]
    public ObstacleConfig[] staticObstacles;
    public ObstacleConfig[] carObstacles;
    [Range(0f, 1f)] public float carSpawnChance = 0.4f;
    public bool rotateCarsToFacePlayer = true;

    [Header("Геометрия спавна")]
    public float laneOffset = 2.5f;
    public float spawnZ = 80.0f;
    public float destroyZ = -15.0f;

    private Dictionary<GameObject, Queue<ObstacleMover>> poolDictionary = new Dictionary<GameObject, Queue<ObstacleMover>>();
    private Transform poolHolder;

    private float activeMaxRoadSpeed;
    private float activeRoadSpeedIncrease;
    private float activeMaxCarSpeed;
    private float activeCarSpeedIncrease;
    private float activeMinSafeGap;
    private float gapShrinkRate = 0.012f;
    private float minSameLaneGap = 0.8f;

    private int lastLaneIndex = 1;
    private float lastArrivalTime = 0f;
    private Coroutine spawnCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        poolHolder = new GameObject("[POOL_CONTAINER]").transform;
    }

    private void Start()
    {
        DifficultyManager.DifficultyData settings = (DifficultyManager.Instance != null) 
            ? DifficultyManager.Instance.GetActiveSettings() 
            : new DifficultyManager.DifficultyData();

        roadSpeed = settings.startRoadSpeed;
        activeMaxRoadSpeed = settings.maxRoadSpeed;
        activeRoadSpeedIncrease = settings.speedIncreasePerSec;

        activeCarExtraSpeed = settings.startCarExtraSpeed;
        activeMaxCarSpeed = settings.maxCarExtraSpeed;
        activeCarSpeedIncrease = settings.carSpeedIncreasePerSec;

        currentSafeTimeGap = settings.startSafeGap;
        activeMinSafeGap = settings.minSafeGap;

        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    private void Update()
    {
        if (!isWorldActive) return;

        MoveRoad();

        if (Time.timeScale > 0)
        {
            elapsedTime += Time.deltaTime;

            // 1. Ускоряем дорогу
            roadSpeed = Mathf.MoveTowards(roadSpeed, activeMaxRoadSpeed, activeRoadSpeedIncrease * Time.deltaTime);

            // 2. Ускоряем встречные машины со временем
            activeCarExtraSpeed = Mathf.MoveTowards(activeCarExtraSpeed, activeMaxCarSpeed, activeCarSpeedIncrease * Time.deltaTime);

            // 3. Уменьшаем зазор на реакцию
            currentSafeTimeGap = Mathf.MoveTowards(currentSafeTimeGap, activeMinSafeGap, gapShrinkRate * Time.deltaTime);
        }
    }

    public void StopWorld()
    {
        isWorldActive = false;
        roadSpeed = 0f;
        activeCarExtraSpeed = 0f;
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
    }

    private void MoveRoad()
    {
        if (roadSegments == null || roadSegments.Length == 0) return;

        for (int i = 0; i < roadSegments.Length; i++)
        {
            if (roadSegments[i] == null) continue;

            roadSegments[i].position += Vector3.back * roadSpeed * Time.deltaTime;

            if (roadSegments[i].position.z <= -roadSegmentLength)
            {
                roadSegments[i].position += new Vector3(0, 0, roadSegmentLength * roadSegments.Length);
            }
        }
    }

    private ObstacleMover GetObstacleFromPool(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(prefab)) poolDictionary[prefab] = new Queue<ObstacleMover>();

        ObstacleMover mover;
        if (poolDictionary[prefab].Count > 0)
        {
            mover = poolDictionary[prefab].Dequeue();
            mover.transform.position = position;
            mover.transform.rotation = rotation;
            mover.gameObject.SetActive(true);
        }
        else
        {
            GameObject newObj = Instantiate(prefab, position, rotation);
            mover = newObj.GetComponent<ObstacleMover>();
            if (mover == null) mover = newObj.AddComponent<ObstacleMover>();
        }

        return mover;
    }

    public void ReturnObstacleToPool(ObstacleMover mover)
    {
        mover.gameObject.SetActive(false);
        mover.transform.SetParent(poolHolder);

        if (mover.SourcePrefab != null)
        {
            if (!poolDictionary.ContainsKey(mover.SourcePrefab)) poolDictionary[mover.SourcePrefab] = new Queue<ObstacleMover>();
            poolDictionary[mover.SourcePrefab].Enqueue(mover);
        }
        else
        {
            Destroy(mover.gameObject);
        }
    }

    private List<ObstacleConfig> GetValidConfigs(ObstacleConfig[] array)
    {
        List<ObstacleConfig> list = new List<ObstacleConfig>();
        if (array == null) return list;
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] != null && array[i].prefab != null) list.Add(array[i]);
        }
        return list;
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(1.2f);

        while (isWorldActive)
        {
            List<ObstacleConfig> validStatics = GetValidConfigs(staticObstacles);
            List<ObstacleConfig> validCars = GetValidConfigs(carObstacles);

            if (validStatics.Count == 0 && validCars.Count == 0)
            {
                yield return new WaitForSeconds(1.0f);
                continue;
            }

            bool spawnCar = (validCars.Count > 0 && validStatics.Count > 0) 
                ? (Random.value < carSpawnChance) 
                : (validCars.Count > 0);

            List<ObstacleConfig> pool = spawnCar ? validCars : validStatics;
            ObstacleConfig chosen = pool[Random.Range(0, pool.Count)];

            int targetLane = Random.Range(0, 2);

            float totalSpeed = roadSpeed + (spawnCar ? activeCarExtraSpeed : 0f);
            float travelTimeToPlayer = spawnZ / Mathf.Max(totalSpeed, 1f);

            float requiredArrivalTime = (targetLane != lastLaneIndex)
                ? lastArrivalTime + currentSafeTimeGap
                : lastArrivalTime + minSameLaneGap;

            float scheduledSpawnTime = requiredArrivalTime - travelTimeToPlayer;
            float waitTime = (scheduledSpawnTime - Time.time) + Random.Range(0.05f, 0.25f);

            if (waitTime > 0f) yield return new WaitForSeconds(waitTime);
            if (!isWorldActive) break;

            float spawnX = (targetLane == 0) ? -laneOffset : laneOffset;
            Vector3 spawnPosition = new Vector3(spawnX, chosen.spawnY, spawnZ);

            Quaternion baseRotation = (spawnCar && rotateCarsToFacePlayer)
                ? Quaternion.Euler(0, 180f, 0)
                : Quaternion.identity;

            Quaternion finalRotation = baseRotation * Quaternion.Euler(chosen.rotationOffset);

            ObstacleMover mover = GetObstacleFromPool(chosen.prefab, spawnPosition, finalRotation);
            // Передаем флаг isCar: если true, машина будет постоянно считывать актуальный activeCarExtraSpeed
            mover.Init(spawnCar, destroyZ, chosen.prefab);

            lastLaneIndex = targetLane;
            lastArrivalTime = Time.time + travelTimeToPlayer;
        }
    }
}