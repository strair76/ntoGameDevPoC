using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CarShop : MonoBehaviour
{
    [System.Serializable]
    public class SkinItem
    {
        public string skinName = "Скин";
        public int price = 0; // 0 = бесплатный базовый скин
        [Tooltip("3D-моделька или визуал этой машины в витрине магазина")]
        public GameObject previewModel;
    }

    [Header("Список скинов в магазине")]
    public SkinItem[] skins;

    [Header("Кнопки навигации")]
    public Button nextButton;
    public Button prevButton;
    public Button actionButton; // Кнопка "Купить" / "Выбрать"

    [Header("Текстовые элементы UI")]
    public TextMeshProUGUI actionButtonText;
    public TextMeshProUGUI skinNameText;
    public TextMeshProUGUI balanceText;

    private int currentIndex = 0;

    private void Start()
    {
        // Базовый скин (индекс 0) ВСЕГДА куплен по умолчанию
        PlayerPrefs.SetInt("Skin_0_Unlocked", 1);

        // Загружаем текущий надетый скин
        int equippedSkin = PlayerPrefs.GetInt("SelectedCarSkin", 0);
        currentIndex = equippedSkin;

        // Привязываем клики к кнопкам
        if (nextButton != null) nextButton.onClick.AddListener(NextSkin);
        if (prevButton != null) prevButton.onClick.AddListener(PrevSkin);
        if (actionButton != null) actionButton.onClick.AddListener(OnActionButtonClick);

        UpdateShopUI();
    }

    public void NextSkin()
    {
        currentIndex = (currentIndex + 1) % skins.Length;
        UpdateShopUI();
    }

    public void PrevSkin()
    {
        currentIndex--;
        if (currentIndex < 0) currentIndex = skins.Length - 1;
        UpdateShopUI();
    }

    private void UpdateShopUI()
    {
        if (skins == null || skins.Length == 0) return;

        SkinItem current = skins[currentIndex];

        // 1. Включаем 3D-модель выбранного скина в витрине и выключаем остальные
        for (int i = 0; i < skins.Length; i++)
        {
            if (skins[i].previewModel != null)
            {
                skins[i].previewModel.SetActive(i == currentIndex);
            }
        }

        // 2. Название скина
        if (skinNameText != null)
        {
            skinNameText.text = current.skinName;
        }

        // 3. Баланс монет
        int totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        if (balanceText != null)
        {
            balanceText.text = $"Монеты: <b>{totalCoins}</b>";
        }

        // 4. Проверяем статус: куплен ли скин и надет ли он
        bool isUnlocked = PlayerPrefs.GetInt($"Skin_{currentIndex}_Unlocked", (currentIndex == 0 ? 1 : 0)) == 1;
        int equippedSkin = PlayerPrefs.GetInt("SelectedCarSkin", 0);
        bool isEquipped = (equippedSkin == currentIndex);

        if (actionButton != null && actionButtonText != null)
        {
            if (isEquipped)
            {
                actionButtonText.text = "<color=#00FF88>ВЫБРАНО</color>";
                actionButton.interactable = false; // Кнопка неактивна, машина уже надета
            }
            else if (isUnlocked)
            {
                actionButtonText.text = "ВЫБРАТЬ";
                actionButton.interactable = true;
            }
            else // Еще не куплен
            {
                actionButtonText.text = $"КУПИТЬ: {current.price} 🪙";
                // Активна только если хватает монет
                actionButton.interactable = (totalCoins >= current.price);
            }
        }
    }

    public void OnActionButtonClick()
    {
        SkinItem current = skins[currentIndex];
        bool isUnlocked = PlayerPrefs.GetInt($"Skin_{currentIndex}_Unlocked", (currentIndex == 0 ? 1 : 0)) == 1;
        int totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);

        // Если скин уже куплен — надеваем его
        if (isUnlocked)
        {
            PlayerPrefs.SetInt("SelectedCarSkin", currentIndex);
            PlayerPrefs.Save();
            Debug.Log($"<color=cyan>[ГАРАЖ]</color> Выбран скин: <b>{current.skinName}</b>");
        }
        // Если еще не куплен — покупаем
        else if (totalCoins >= current.price)
        {
            // Списываем монеты
            totalCoins -= current.price;
            PlayerPrefs.SetInt("TotalCoins", totalCoins);

            // Помечаем скин купленным и сразу надеваем
            PlayerPrefs.SetInt($"Skin_{currentIndex}_Unlocked", 1);
            PlayerPrefs.SetInt("SelectedCarSkin", currentIndex);
            PlayerPrefs.Save();

            Debug.Log($"<color=yellow>[ПОКУПКА]</color> Куплен скин: <b>{current.skinName}</b> за {current.price} монет!");
        }

        UpdateShopUI();
    }
}