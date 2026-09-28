using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CarShop : MonoBehaviour
{
    [System.Serializable]
    public class SkinItem
    {
        public string skinName = "Скин";
        public int price = 0; // 0 = бесплатный

        [Header("2D Картинка машины")]
        [Tooltip("Перетащите сюда спрайт или картинку PNG/JPG")]
        public Object carPicture;

        [Header("Опционально (если есть 3D модель)")]
        public GameObject previewModel;
    }

    [Header("UI Экран для картинки машины")]
    public Image carDisplayImage;

    [Header("Список скинов в магазине")]
    public SkinItem[] skins;

    [Header("Кнопки")]
    public Button nextButton;
    public Button prevButton;
    public Button actionButton; // Главная кнопка действия

    [Header("Дочерние объекты кнопки (SetActive)")]
    [Tooltip("Объект/картинка, когда машина УЖЕ НАДЕТА (Выбрано)")]
    public GameObject equippedStateObject;

    [Tooltip("Объект/картинка, когда машина КУПЛЕНА, но не надета (Выбрать)")]
    public GameObject selectStateObject;

    [Tooltip("Объект/картинка, когда машина ПРОДАЕТСЯ и монет ХВАТАЕТ (Купить)")]
    public GameObject buyStateObject;

    [Tooltip("Объект/картинка, когда машина НЕДОСТУПНА к покупке / НЕ ХВАТАЕТ монет")]
    public GameObject lockedStateObject;

    [Header("Отображение цены скина (Отдельный текст)")]
    [Tooltip("Текстовый объект (TextMeshPro) для вывода цены")]
    public TextMeshProUGUI priceText;

    [Tooltip("Шаблон цены. Метка {price} заменится на стоимость")]
    public string priceTemplate = "Цена: <b>{price} 🪙</b>";

    [Tooltip("Текст цены, если машина уже куплена")]
    public string purchasedPriceText = "Куплено";

    [Tooltip("Текст цены, если скин бесплатный (0 монет)")]
    public string freePriceText = "Бесплатно";

    [Header("Название скина (Опционально)")]
    public TextMeshProUGUI skinNameText;

    [Header("Блокировка нажатия")]
    [Tooltip("Запретить нажимать на кнопку, если машина уже выбрана")]
    public bool lockActionButtonWhenEquipped = true;

    [Tooltip("Запретить нажимать на кнопку покупки, если не хватает монет")]
    public bool lockWhenCannotAfford = true;

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

        // 1. ОТОБРАЖЕНИЕ КАРТИНКИ МАШИНЫ
        if (carDisplayImage != null)
        {
            Sprite resolvedSprite = null;
            if (current.carPicture is Sprite sp) resolvedSprite = sp;
            else if (current.carPicture is Texture2D tex) resolvedSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

            if (resolvedSprite != null)
            {
                carDisplayImage.sprite = resolvedSprite;
                carDisplayImage.enabled = true;
                carDisplayImage.preserveAspect = true;
            }
            else
            {
                carDisplayImage.enabled = false;
            }
        }

        // 2. 3D МОДЕЛЬ (если есть)
        for (int i = 0; i < skins.Length; i++)
        {
            if (skins[i].previewModel != null)
                skins[i].previewModel.SetActive(i == currentIndex);
        }

        // 3. НАЗВАНИЕ
        if (skinNameText != null)
            skinNameText.text = current.skinName;

        // 4. СТАТУС СКИНА
        bool isUnlocked = PlayerPrefs.GetInt($"Skin_{currentIndex}_Unlocked", (currentIndex == 0 ? 1 : 0)) == 1;
        int equippedSkin = PlayerPrefs.GetInt("SelectedCarSkin", 0);
        bool isEquipped = (equippedSkin == currentIndex);
        int totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        bool canAfford = totalCoins >= current.price;

        // 5. ВЫВОД ОТДЕЛЬНОЙ СТОИМОСТИ (PRICE TEXT)
        if (priceText != null)
        {
            if (isUnlocked)
            {
                priceText.text = purchasedPriceText;
            }
            else if (current.price <= 0)
            {
                priceText.text = freePriceText;
            }
            else
            {
                priceText.text = priceTemplate.Replace("{price}", current.price.ToString());
            }
        }

        // 6. ПЕРЕКЛЮЧЕНИЕ ДОЧЕРНИХ ОБЪЕКТОВ КНОПКИ (SetActive)
        if (actionButton != null)
        {
            if (isEquipped)
            {
                // Состояние 1: Выбрано
                SetStateObjects(isEquippedActive: true, isSelectActive: false, isBuyActive: false, isLockedActive: false);
                actionButton.interactable = !lockActionButtonWhenEquipped;
            }
            else if (isUnlocked)
            {
                // Состояние 2: Куплено, можно выбрать
                SetStateObjects(isEquippedActive: false, isSelectActive: true, isBuyActive: false, isLockedActive: false);
                actionButton.interactable = true;
            }
            else if (canAfford)
            {
                // Состояние 3: Продается и монет хватает
                SetStateObjects(isEquippedActive: false, isSelectActive: false, isBuyActive: true, isLockedActive: false);
                actionButton.interactable = true;
            }
            else
            {
                // Состояние 4: Недоступно (не хватает монет)
                SetStateObjects(isEquippedActive: false, isSelectActive: false, isBuyActive: false, isLockedActive: true);
                actionButton.interactable = !lockWhenCannotAfford;
            }
        }

        // Блокировка дополнительных кнопок
        if (additionalButtonsToLockWhenEquipped != null)
        {
            foreach (var btn in additionalButtonsToLockWhenEquipped)
            {
                if (btn != null) btn.interactable = !isEquipped;
            }
        }
    }

    // Включение одного нужного объекта и выключение остальных
    private void SetStateObjects(bool isEquippedActive, bool isSelectActive, bool isBuyActive, bool isLockedActive)
    {
        if (equippedStateObject != null) equippedStateObject.SetActive(isEquippedActive);
        if (selectStateObject != null) selectStateObject.SetActive(isSelectActive);
        if (buyStateObject != null) buyStateObject.SetActive(isBuyActive);
        if (lockedStateObject != null) lockedStateObject.SetActive(isLockedActive);
    }

    public void OnActionButtonClick()
    {
        SkinItem current = skins[currentIndex];
        bool isUnlocked = PlayerPrefs.GetInt($"Skin_{currentIndex}_Unlocked", (currentIndex == 0 ? 1 : 0)) == 1;
        int totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);

        if (isUnlocked)
        {
            // Надеваем
            PlayerPrefs.SetInt("SelectedCarSkin", currentIndex);
            PlayerPrefs.Save();
        }
        else if (totalCoins >= current.price)
        {
            // Покупаем
            totalCoins -= current.price;
            PlayerPrefs.SetInt("TotalCoins", totalCoins);
            PlayerPrefs.SetInt($"Skin_{currentIndex}_Unlocked", 1);
            PlayerPrefs.SetInt("SelectedCarSkin", currentIndex);
            PlayerPrefs.Save();

            if (SceneController.Instance != null)
            {
                SceneController.Instance.UpdateRecordsUI();
            }
        }

        UpdateShopUI();
    }
}