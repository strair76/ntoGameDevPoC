using UnityEngine;

public class LocationManager : MonoBehaviour
{
    public static LocationManager Instance { get; private set; }

    [System.Serializable]
    public class LocationData
    {
        public string locationName = "Название локации";

        [Header("1. Визуал и окружение")]
        [Tooltip("Родительский объект с фоновыми декорациями (включится только для этой локации)")]
        public GameObject environmentRoot;

        [Tooltip("Материал для полотна дороги (асфальт, песок, снег)")]
        public Material roadMaterial;

        [Tooltip("Материал неба (Skybox)")]
        public Material skyboxMaterial;

        [Header("2. Препятствия под эту локацию")]
        [Tooltip("Статичные объекты именно для этой темы (например: бочки для города, кактусы для пустыни)")]
        public WorldManager.ObstacleConfig[] staticObstacles;

        [Tooltip("Встречные машины именно для этой темы (например: такси для города, багги для пустыни)")]
        public WorldManager.ObstacleConfig[] carObstacles;

        [Header("3. Машина игрока (Опционально)")]
        [Tooltip("Если хотите, чтобы у игрока менялась моделька под тему (например: спорткар в городе, джип в пустыне)")]
        public GameObject playerCarModel;
    }

    [Header("Список всех ваших локаций")]
    [Tooltip("0 = Первая локация, 1 = Вторая, 2 = Третья")]
    public LocationData[] locations;

    [Header("Ссылки на дорогу")]
    [Tooltip("Рендереры кусков дороги (чтобы скрипт сменил им материал)")]
    public Renderer[] roadRenderers;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Считываем номер сохраненной локации из Главного Меню
        int selectedIndex = PlayerPrefs.GetInt("SelectedLocation", 0);

        ApplyLocation(selectedIndex);
    }

    public void ApplyLocation(int index)
    {
        if (locations == null || locations.Length == 0) return;

        // Защита от выхода за границы списка
        if (index < 0 || index >= locations.Length) index = 0;

        for (int i = 0; i < locations.Length; i++)
        {
            bool isCurrent = (i == index);

            // 1. Включаем декорации выбранной локации и выключаем чужие
            if (locations[i].environmentRoot != null)
            {
                locations[i].environmentRoot.SetActive(isCurrent);
            }

            // 2. Включаем модельку машины игрока под эту тему (если назначена)
            if (locations[i].playerCarModel != null)
            {
                locations[i].playerCarModel.SetActive(isCurrent);
            }

            // Настройки активной локации
            if (isCurrent)
            {
                // Смена материала дороги
                if (locations[i].roadMaterial != null && roadRenderers != null)
                {
                    foreach (var rend in roadRenderers)
                    {
                        if (rend != null) rend.material = locations[i].roadMaterial;
                    }
                }

                // Смена неба
                if (locations[i].skyboxMaterial != null)
                {
                    RenderSettings.skybox = locations[i].skyboxMaterial;
                }

                // 3. ПЕРЕДАЕМ НОВЫЕ ПРЕПЯТСТВИЯ И МАШИНЫ В СПАВНЕР
                if (WorldManager.Instance != null)
                {
                    WorldManager.Instance.SetLocationObstacles(
                        locations[i].staticObstacles,
                        locations[i].carObstacles
                    );
                }

                Debug.Log($"<color=cyan>[LOCATION]</color> Активирована локация: <b>{locations[i].locationName}</b>");
            }
        }
    }
}