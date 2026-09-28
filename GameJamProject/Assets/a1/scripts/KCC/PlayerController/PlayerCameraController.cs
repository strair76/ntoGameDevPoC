using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_2023_2_OR_NEWER || CINEMACHINE_V3
using Unity.Cinemachine;
#else
using Cinemachine;
#endif

public enum CameraMode
{
    FirstPerson,
    ThirdPerson
}

/// <summary>
/// Управление поворотом камеры, режимом вида (1-е / 3-е лицо), позицией, автоскрытием меша и раздельным FOV.
/// </summary>
public class PlayerCameraController : MonoBehaviour
{
    [Header("Режим Камеры")]
    [SerializeField] private CameraMode cameraMode = CameraMode.ThirdPerson;
    
    [Header("Настройки 3-го лица")]
    [SerializeField] private float thirdPersonDistance = 4.0f;
    [SerializeField] private Vector3 thirdPersonOffset = new Vector3(0f, 0f, 0f);
    [SerializeField] private float thirdPersonSensitivity = 1.2f;
    [Tooltip("Базовый угол обзора (FOV) от 3-го лица")]
    [SerializeField] private float thirdPersonFOV = 40f;

    [Header("Настройки 1-го лица")]
    [SerializeField] private float firstPersonDistance = 0.0f;
    [SerializeField] private Vector3 firstPersonOffset = new Vector3(0f, 0.1f, 0.2f);
    [SerializeField] private float firstPersonSensitivity = 0.6f;
    [Tooltip("Базовый угол обзора (FOV) от 1-го лица (для эффекта погружения обычно выставляют 70-85)")]
    [SerializeField] private float firstPersonFOV = 75f;

    [Header("Скрытие модели персонажа")]
    [Tooltip("Перетащите сюда все SkinnedMeshRenderer / MeshRenderer персонажа (тело, одежда, голова)")]
    [SerializeField] private Renderer[] characterRenderers;

    [Header("Ссылки (References)")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private GameObject virtualCameraObject;

    [Header("Ограничения поворота")]
    [SerializeField] private float topClamp = 70.0f;
    [SerializeField] private float bottomClamp = -30.0f;
    [SerializeField] private bool lockCursor = true;

    [Header("Динамический FOV при беге")]
    [Tooltip("На сколько увеличивать FOV при спринте относительно текущего режима")]
    [SerializeField] private float sprintFOVOffset = 10f;
    [SerializeField] private float fovChangeSpeed = 5f;

    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private float _sensitivityMultiplier = 1f;
    private bool _isSprinting = false;

#if UNITY_2023_2_OR_NEWER || CINEMACHINE_V3
    private CinemachineCamera _vcam3;
#else
    private CinemachineVirtualCamera _vcam2;
#endif

    public Transform CameraPivot => cameraPivot;
    public CameraMode CurrentCameraMode
    {
        get => cameraMode;
        set
        {
            cameraMode = value;
            HandleCameraMode();
        }
    }

    private void Awake()
    {
        if (cameraPivot != null)
        {
            _cinemachineTargetYaw = cameraPivot.rotation.eulerAngles.y;
            _cinemachineTargetPitch = cameraPivot.rotation.eulerAngles.x;
        }

        InitVirtualCamera();
    }

    private void OnValidate()
    {
        InitVirtualCamera();
        HandleCameraMode();
        ApplyEditorFOV();
    }

    private void InitVirtualCamera()
    {
        if (virtualCameraObject == null) return;

#if UNITY_2023_2_OR_NEWER || CINEMACHINE_V3
        _vcam3 = virtualCameraObject.GetComponent<CinemachineCamera>();
#else
        _vcam2 = virtualCameraObject.GetComponent<CinemachineVirtualCamera>();
#endif
    }

    private void ApplyEditorFOV()
    {
        float targetFOV = (cameraMode == CameraMode.FirstPerson) ? firstPersonFOV : thirdPersonFOV;

#if UNITY_2023_2_OR_NEWER || CINEMACHINE_V3
        if (_vcam3 != null)
        {
            var lens = _vcam3.Lens;
            lens.FieldOfView = targetFOV;
            _vcam3.Lens = lens;
        }
#else
        if (_vcam2 != null)
        {
            _vcam2.m_Lens.FieldOfView = targetFOV;
        }
#endif
    }

    private void Start()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void LateUpdate()
    {
        HandleCameraMode();
        HandleCameraRotation();
        HandleFOV();
    }

    private void HandleCameraMode()
    {
        bool isFP = (cameraMode == CameraMode.FirstPerson);

        if (characterRenderers != null)
        {
            foreach (var r in characterRenderers)
            {
                if (r != null) r.enabled = !isFP;
            }
        }

        if (virtualCameraObject == null) return;

        float targetDistance = isFP ? firstPersonDistance : thirdPersonDistance;
        Vector3 targetOffset = isFP ? firstPersonOffset : thirdPersonOffset;

#if UNITY_2023_2_OR_NEWER || CINEMACHINE_V3
        if (_vcam3 == null) _vcam3 = virtualCameraObject.GetComponent<CinemachineCamera>();
        if (_vcam3 != null)
        {
            var follow = _vcam3.GetComponent<CinemachineThirdPersonFollow>();
            if (follow == null) follow = _vcam3.GetComponentInChildren<CinemachineThirdPersonFollow>();

            if (follow != null)
            {
                follow.CameraDistance = targetDistance;
                follow.ShoulderOffset = targetOffset;
            }
        }
#else
        if (_vcam2 == null) _vcam2 = virtualCameraObject.GetComponent<CinemachineVirtualCamera>();
        if (_vcam2 != null)
        {
            var follow = _vcam2.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            if (follow == null) follow = _vcam2.GetComponentInChildren<Cinemachine3rdPersonFollow>();

            if (follow != null)
            {
                follow.m_CameraDistance = targetDistance;
                follow.m_ShoulderOffset = targetOffset;
            }
        }
#endif
    }

    private void HandleCameraRotation()
    {
        if (cameraPivot == null || Mouse.current == null) return;

        Vector2 lookInput = Mouse.current.delta.ReadValue();
        
        float baseSensitivity = (cameraMode == CameraMode.FirstPerson) ? firstPersonSensitivity : thirdPersonSensitivity;
        float multiplier = baseSensitivity * _sensitivityMultiplier;

        _cinemachineTargetYaw += lookInput.x * multiplier;
        _cinemachineTargetPitch -= lookInput.y * multiplier;

        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, bottomClamp, topClamp);

        cameraPivot.rotation = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0.0f);
    }

    private void HandleFOV()
    {
        float baseFOV = (cameraMode == CameraMode.FirstPerson) ? firstPersonFOV : thirdPersonFOV;
        float targetFOV = _isSprinting ? baseFOV + sprintFOVOffset : baseFOV;

#if UNITY_2023_2_OR_NEWER || CINEMACHINE_V3
        if (_vcam3 != null)
        {
            var lens = _vcam3.Lens;
            if (!Mathf.Approximately(lens.FieldOfView, targetFOV))
            {
                lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, targetFOV, Time.deltaTime * fovChangeSpeed);
                _vcam3.Lens = lens;
            }
        }
#else
        if (_vcam2 != null)
        {
            if (!Mathf.Approximately(_vcam2.m_Lens.FieldOfView, targetFOV))
            {
                _vcam2.m_Lens.FieldOfView = Mathf.Lerp(_vcam2.m_Lens.FieldOfView, targetFOV, Time.deltaTime * fovChangeSpeed);
            }
        }
#endif
    }

    public void SetSprintingFOV(bool isSprinting) => _isSprinting = isSprinting;
    public void SetSensitivityMultiplier(float multiplier) => _sensitivityMultiplier = Mathf.Clamp01(multiplier);

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}