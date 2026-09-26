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

    private GameObject currentSpawnedEnvironment; // Экземпляр заспавненного префаба на сцене

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Запускаем именно в Start(), чтобы WorldManager.Instance гарантированно уже существовал!
        int selectedIndex = PlayerPrefs.GetInt("SelectedLocation", 0);
        ApplyLocation(selectedIndex);
    }

    public void ApplyLocation(int index)
    {
        if (locations == null || locations.Length == 0) return;

        if (index < 0 || index >= locations.Length) index = 0;

        LocationData activeLoc = locations[index];

        // 1. УПРАВЛЕНИЕ ОКРУЖЕНИЕМ (ПРЕФАБ ИЛИ ОБЪЕКТ СО СЦЕНЫ)
        // Удаляем ранее заспавненный префаб предыдущей темы (если был)
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
                // Проверяем: это объект уже на сцене или это префаб из папки Project?
                if (env.scene.rootCount != 0) 
                {
                    // Объект лежит на сцене: просто включаем/выключаем
                    env.SetActive(isCurrent);
                }
                else if (isCurrent)
                {
                    // Это префаб из папки: спавним его на сцену в координаты (0, 0, 0)!
                    currentSpawnedEnvironment = Instantiate(env, Vector3.zero, Quaternion.identity);
                }
            }

            // Машинка игрока под тему (если есть)
            if (locations[i].playerCarModel != null)
            {
                locations[i].playerCarModel.SetActive(isCurrent);
            }
        }

        // 2. МАТЕРИАЛ ДОРОГИ
        if (activeLoc.roadMaterial != null && roadRenderers != null)
        {
            foreach (var rend in roadRenderers)
            {
                if (rend != null) rend.material = activeLoc.roadMaterial;
            }
        }

        // 3. НЕБО (SKYBOX)
        if (activeLoc.skyboxMaterial != null)
        {
            RenderSettings.skybox = activeLoc.skyboxMaterial;
        }

        // 4. ПЕРЕДАЕМ ПРЕПЯТСТВИЯ И МАШИНЫ В WORLD MANAGER
        if (WorldManager.Instance != null)
        {
            WorldManager.Instance.SetLocationObstacles(activeLoc.staticObstacles, activeLoc.carObstacles);
        }
        else
        {
            Debug.LogWarning("[LocationManager] WorldManager.Instance еще не готов или отсутствует на сцене!");
        }

        Debug.Log($"<color=cyan>[LOCATION]</color> Успешно загружена локация: <b>{activeLoc.locationName}</b>");
    }
}