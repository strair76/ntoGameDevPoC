using UnityEngine;

public class EnvironmentScroller : MonoBehaviour
{
    [Header("Чанки окружения")]
    [Tooltip("Массив ваших чанков (домов, деревьев, фонарей)")]
    public Transform[] environmentChunks;

    [Tooltip("Длина одного чанка по оси Z")]
    public float chunkLength = 30.0f;

    [Tooltip("Точка позади камеры, где чанк переносится вперед")]
    public float recycleZ = -30.0f;

    private void Update()
    {
        // Не двигаем фон, если игра на паузе или машина разбилась
        if (WorldManager.Instance == null || !WorldManager.Instance.isWorldActive) return;

        // Скорость фона полностью синхронизирована со скоростью дороги
        float speed = WorldManager.Instance.roadSpeed;
        float totalTrackLength = chunkLength * environmentChunks.Length;

        for (int i = 0; i < environmentChunks.Length; i++)
        {
            if (environmentChunks[i] == null) continue;

            // Двигаем чанк назад
            environmentChunks[i].position += Vector3.back * speed * Time.deltaTime;

            // Если чанк полностью проехал игрока и скрылся из виду
            if (environmentChunks[i].position.z <= recycleZ)
            {
                // Переносим его в самый конец очереди всех чанков
                environmentChunks[i].position += new Vector3(0, 0, totalTrackLength);
            }
        }
    }
}