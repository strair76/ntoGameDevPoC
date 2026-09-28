using UnityEngine;

public class LocationManager : MonoBehaviour
{
    public static LocationManager Instance { get; private set; }

    [System.Serializable]
    public class LocationData
    {
        public string locationName = "Локация";

        [Header("1. Визуал и окружение")]
        [Tooltip("Префаб локации из папки Project (или готовый объект со сцены)")]
        public GameObject environmentPrefabOrObject;

        [Tooltip("Материал дороги для этой темы")]
        public Material roadMaterial;

        [Tooltip("Скайбокс (небо) для этой темы")]
        public Material skyboxMaterial;

        [Tooltip("Цвет тумана для этой темы")]
        public Color fogColor = Color.gray; // <-- С маленькой буквы!

        [Header("2. Препятствия под эту локацию")]
        public WorldManager.ObstacleConfig[] staticObstacles;
        public WorldManager.ObstacleConfig[] carObstacles;

        [Header("3. Машина игрока под тему (Опционально)")]
        public GameObject playerCarModel;
    }

    [Header("Список ваших локаций")]
    public LocationData[] locations;

    [Header("Ссылки на куски дороги на сцене")]
    public Renderer[] roadRenderers;

    private GameObject currentSpawnedEnvironment;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        int selectedIndex = PlayerPrefs.GetInt("SelectedLocation", 0);
        ApplyLocation(selectedIndex);
    }

    public void ApplyLocation(int index)
    {
        if (locations == null || locations.Length == 0) return;

        if (index < 0 || index >= locations.Length) index = 0;

        LocationData activeLoc = locations[index];

        // 1. Окружение (удаляем старый префаб, спавним новый)
        if (currentSpawnedEnvironment != null)
        {
            Destroy(currentSpawnedEnvironment);
        }

        for (int i = 0; i < locations.Length; i++)
        {
            bool isCurrent = (i == index);
            GameObject env = locations[i].environmentPrefabOrObject;

            if (env != null)
            {
                if (env.scene.rootCount != 0)
                {
                    env.SetActive(isCurrent);
                }
                else if (isCurrent)
                {
                    currentSpawnedEnvironment = Instantiate(env, Vector3.zero, Quaternion.identity);
                }
            }

            if (locations[i].playerCarModel != null)
            {
                locations[i].playerCarModel.SetActive(isCurrent);
            }
        }

        // 2. Материал дороги
        if (activeLoc.roadMaterial != null && roadRenderers != null)
        {
            foreach (var rend in roadRenderers)
            {
                if (rend != null) rend.material = activeLoc.roadMaterial;
            }
        }

        // 3. Небо (Skybox)
        if (activeLoc.skyboxMaterial != null)
        {
            RenderSettings.skybox = activeLoc.skyboxMaterial;
        }

        // 4. ТУМАН (ВНИМАНИЕ: fogColor, fogStartDistance, fogEndDistance — ВСЁ с маленькой буквы!)
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 15f;
        RenderSettings.fogEndDistance = 45f;
        RenderSettings.fogColor = activeLoc.fogColor;

        // 5. Передаем препятствия в спавнер
        if (WorldManager.Instance != null)
        {
            WorldManager.Instance.SetLocationObstacles(activeLoc.staticObstacles, activeLoc.carObstacles);
        }

        Debug.Log($"<color=cyan>[LOCATION]</color> Активирована локация: <b>{activeLoc.locationName}</b>");
    }
}