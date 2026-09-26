using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CarShop : MonoBehaviour
{
    [System.Serializable]
    public class SkinItem
    {
        public string skinName = "Скин";
        public int price = 0;

        [Header("2D Картинка машины")]
        [Tooltip("Перетащите сюда картинку (Sprite) машины из папки Project")]
        public Sprite carSprite;

        [Header("Опционально (если есть 3D модель)")]
        public GameObject previewModel;
    }

    [Header("UI Экран для картинки машины")]
    [Tooltip("Объект Image на вашем Canvas, куда скрипт будет подставлять спрайт выбранной машины")]
    public Image carDisplayImage;

    [Header("Список скинов в магазине")]
    public SkinItem[] skins;

    [Header("Кнопки")]
    public Button nextButton;
    public Button prevButton;
    public Button actionButton; // Кнопка "Купить" / "Выбрать"

    [Header("Текст кнопки и названия")]
    public TextMeshProUGUI actionButtonText;
    public TextMeshProUGUI skinNameText;

    [Header("Блокировка нажатия в Инспекторе")]
    [Tooltip("Запретить нажимать на кнопку действия, если эта машина уже выбрана?")]
    public bool lockActionButtonWhenEquipped = true;

    [Tooltip("Дополнительные кнопки, которые нужно заблокировать, если машина уже надета (опционально)")]
    public Button[] additionalButtonsToLockWhenEquipped;

    private int currentIndex = 0;

    private void Start()
    {
        PlayerPrefs.SetInt("Skin_0_Unlocked", 1);

        int equippedSkin = PlayerPrefs.GetInt("SelectedCarSkin", 0);
        currentIndex = equippedSkin;

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

        // 1. Отображение 2D картинки машины
        if (carDisplayImage != null)
        {
            if (current.carSprite != null)
            {
                carDisplayImage.sprite = current.carSprite;
                carDisplayImage.enabled = true;
                carDisplayImage.preserveAspect = true;
            }
            else
            {
                carDisplayImage.enabled = false;
            }
        }

        // 2. Если используется 3D витрина
        for (int i = 0; i < skins.Length; i++)
        {
            if (skins[i].previewModel != null)
                skins[i].previewModel.SetActive(i == currentIndex);
        }

        // 3. Название скина
        if (skinNameText != null)
            skinNameText.text = current.skinName;

        // 4. Проверка статуса (куплен / выбран)
        bool isUnlocked = PlayerPrefs.GetInt($"Skin_{currentIndex}_Unlocked", (currentIndex == 0 ? 1 : 0)) == 1;
        int equippedSkin = PlayerPrefs.GetInt("SelectedCarSkin", 0);
        bool isEquipped = (equippedSkin == currentIndex);
        int totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);

        if (actionButton != null && actionButtonText != null)
        {
            if (isEquipped)
            {
                actionButtonText.text = "<color=#00FF88>ВЫБРАНО</color>";
                // Блокируем кнопку через interactable
                actionButton.interactable = !lockActionButtonWhenEquipped;
            }
            else if (isUnlocked)
            {
                actionButtonText.text = "ВЫБРАТЬ";
                actionButton.interactable = true;
            }
            else
            {
                actionButtonText.text = $"КУПИТЬ: {current.price} 🪙";
                actionButton.interactable = (totalCoins >= current.price);
            }
        }

        // Блокировка дополнительных кнопок (если указаны в инспекторе)
        if (additionalButtonsToLockWhenEquipped != null)
        {
            foreach (var btn in additionalButtonsToLockWhenEquipped)
            {
                if (btn != null) btn.interactable = !isEquipped;
            }
        }
    }

    public void OnActionButtonClick()
    {
        SkinItem current = skins[currentIndex];
        bool isUnlocked = PlayerPrefs.GetInt($"Skin_{currentIndex}_Unlocked", (currentIndex == 0 ? 1 : 0)) == 1;
        int totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);

        if (isUnlocked)
        {
            PlayerPrefs.SetInt("SelectedCarSkin", currentIndex);
            PlayerPrefs.Save();
        }
        else if (totalCoins >= current.price)
        {
            totalCoins -= current.price;
            PlayerPrefs.SetInt("TotalCoins", totalCoins);
            PlayerPrefs.SetInt($"Skin_{currentIndex}_Unlocked", 1);
            PlayerPrefs.SetInt("SelectedCarSkin", currentIndex);
            PlayerPrefs.Save();

            // Просим SceneController обновить баланс монет на экране
            if (SceneController.Instance != null)
            {
                SceneController.Instance.UpdateRecordsUI();
            }
        }

        UpdateShopUI();
    }
}