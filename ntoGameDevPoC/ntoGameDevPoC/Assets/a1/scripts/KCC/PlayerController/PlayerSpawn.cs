using System.Collections;
using UnityEngine;
using KinematicCharacterController;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Настройки смерти")]
    [Tooltip("Время задержки перед возрождением (в секундах)")]
    [SerializeField] private float respawnDelay = 2.0f;

    [Tooltip("Дочерний объект с мешами персонажа (например, Visuals или Model)")]
    [SerializeField] private GameObject characterModel;

    private Vector3 _lastCheckpointPosition;
    private Quaternion _lastCheckpointRotation;
    private KinematicCharacterMotor _motor;
    private PlayerCameraController _cameraController;
    private bool _isDead = false;

    public bool IsDead => _isDead;

    private void Awake()
    {
        _motor = GetComponent<KinematicCharacterMotor>();
        _cameraController = GetComponent<PlayerCameraController>();
        if (_cameraController == null) _cameraController = GetComponentInChildren<PlayerCameraController>();

        _lastCheckpointPosition = transform.position;
        _lastCheckpointRotation = transform.rotation;
    }

    public void SetCheckpoint(Vector3 position, Quaternion rotation)
    {
        if (_isDead) return;

        _lastCheckpointPosition = position;
        _lastCheckpointRotation = rotation;
        Debug.Log($"[Checkpoint] Сохранена новая точка: {position}");
    }

    public void DieAndRespawn()
    {
        if (_isDead) return;
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        _isDead = true;

        // 1. Скрываем визуалку персонажа
        if (characterModel != null)
        {
            characterModel.SetActive(false);
        }

        // 2. Отключаем физику KCC
        if (_motor != null)
        {
            _motor.BaseVelocity = Vector3.zero;
            _motor.enabled = false;
        }

        // 3. Замораживаем управление мышью и вращение камеры
        if (_cameraController != null)
        {
            _cameraController.enabled = false;
        }

        // 4. Ждем 2 секунды
        yield return new WaitForSeconds(respawnDelay);

        // 5. Перемещаем на чекпоинт и включаем физику
        if (_motor != null)
        {
            _motor.enabled = true;
            _motor.SetPositionAndRotation(_lastCheckpointPosition, _lastCheckpointRotation);
            _motor.BaseVelocity = Vector3.zero;
        }
        else
        {
            transform.position = _lastCheckpointPosition;
            transform.rotation = _lastCheckpointRotation;
        }

        // 6. Разблокируем управление мышью
        if (_cameraController != null)
        {
            _cameraController.enabled = true;
        }

        // 7. Возвращаем видимость персонажа
        if (characterModel != null)
        {
            characterModel.SetActive(true);
        }

        _isDead = false;
    }
}