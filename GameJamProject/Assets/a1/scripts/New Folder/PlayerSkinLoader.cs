using UnityEngine;

public class PlayerSkinLoader : MonoBehaviour
{
    [Header("Модельки скинов машины игрока")]
    [Tooltip("Порядок должен строго совпадать со списком в магазине (0 = Базовая, 1 = Скин за 100, 2 = Скин за 500)")]
    public GameObject[] skinModels;

    private void Awake()
    {
        ApplyEquippedSkin();
    }

    public void ApplyEquippedSkin()
    {
        if (skinModels == null || skinModels.Length == 0) return;

        // Считываем номер надетого скина из памяти
        int selectedSkinIndex = PlayerPrefs.GetInt("SelectedCarSkin", 0);

        // Защита от выхода за границы
        if (selectedSkinIndex < 0 || selectedSkinIndex >= skinModels.Length)
            selectedSkinIndex = 0;

        // Включаем выбранную модельку и выключаем остальные
        for (int i = 0; i < skinModels.Length; i++)
        {
            if (skinModels[i] != null)
            {
                skinModels[i].SetActive(i == selectedSkinIndex);
            }
        }

        Debug.Log($"<color=green>[PLAYER SKIN]</color> На машину надет скин №{selectedSkinIndex}");
    }
}