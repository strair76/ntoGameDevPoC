using UnityEngine;
using UnityEngine.InputSystem;
using KinematicCharacterController;

/// <summary>
/// Контроллер персонажа KCC с поддержкой 1-го и 3-го лица.
/// </summary>
[RequireComponent(typeof(KinematicCharacterMotor))]
public class PlayerCharacterController : MonoBehaviour, ICharacterController
{
    [Header("Ссылки (References)")]
    public KinematicCharacterMotor Motor;
    public Animator CharacterAnimator;
    public PlayerCameraController CameraController;
    public Transform PlayerCamera;

    [Header("Режимы управления")]
    public bool OrientToCamera = false;

    [Header("Параметры скорости")]
    public float WalkSpeed = 2.0f;
    public float RunSpeed = 6.0f;
    
    [Range(0.0f, 1f)]
    public float AirControlMultiplier = 0.8f;

    public float Acceleration = 30f;
    public float RotationSpeed = 300f; 

    [Header("Физика и механика прыжков")]
    public float Gravity = 21f;
    public float JumpSpeed = 7.3f;

    public float CoyoteTime = 0.15f;
    public float JumpCooldown = 0.1f;

    [Header("Звуки")]
    public AudioClip LandingAudioClip;
    public AudioClip[] FootstepAudioClips;
    [Range(0f, 1f)] public float FootstepAudioVolume = 0.5f;

    // Внутренние переменные
    private Vector3 _moveInputVector;
    private Vector3 _lookDirection;
    private bool _jumpRequested = false;
    private bool _isSprinting = false;
    private float _currentDesiredSpeed;
    
    private float _coyoteTimeTimer = 0f;
    private float _jumpCooldownTimer = 0f;
    private bool _wasGroundedLastFrame = true;

    // Хэши Аниматора
    private static readonly int AnimSpeed = Animator.StringToHash("Speed");
    private static readonly int AnimGrounded = Animator.StringToHash("Grounded");
    private static readonly int AnimFreeFall = Animator.StringToHash("FreeFall");
    private static readonly int AnimJumpTrigger = Animator.StringToHash("Jump");

    private void Awake()
    {
        if (Motor == null) Motor = GetComponent<KinematicCharacterMotor>();
        if (CharacterAnimator == null) CharacterAnimator = GetComponentInChildren<Animator>();
        if (CameraController == null) CameraController = GetComponent<PlayerCameraController>();

        KinematicCharacterSystem.EnsureCreation();
        if (KinematicCharacterSystem.Settings != null)
        {
            KinematicCharacterSystem.Settings.Interpolate = true;
        }

        Motor.CharacterController = this;
    }

    private void Start()
    {
        if (PlayerCamera == null && Camera.main != null)
        {
            PlayerCamera = Camera.main.transform;
        }
    }

    private void Update()
    {
        HandleInput();
        UpdateAnimator();
    }

    private void HandleInput()
    {
        if (_jumpCooldownTimer > 0f) _jumpCooldownTimer -= Time.deltaTime;

        if (Motor.GroundingStatus.IsStableOnGround)
        {
            _coyoteTimeTimer = CoyoteTime;
        }
        else
        {
            _coyoteTimeTimer -= Time.deltaTime;
        }

        Vector2 input = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) input.y += 1f;
            if (Keyboard.current.sKey.isPressed) input.y -= 1f;
            if (Keyboard.current.dKey.isPressed) input.x += 1f;
            if (Keyboard.current.aKey.isPressed) input.x -= 1f;

            bool isShiftPressed = Keyboard.current.leftShiftKey.isPressed;
            _isSprinting = isShiftPressed && input.sqrMagnitude > 0f;

            if (Keyboard.current.spaceKey.wasPressedThisFrame && _coyoteTimeTimer > 0f && _jumpCooldownTimer <= 0f)
            {
                _jumpRequested = true;
                _coyoteTimeTimer = 0f;
                _jumpCooldownTimer = JumpCooldown;

                if (CharacterAnimator != null)
                {
                    CharacterAnimator.SetTrigger(AnimJumpTrigger);
                }
            }
        }

        if (input.sqrMagnitude > 1f) input.Normalize();

        _currentDesiredSpeed = _isSprinting ? RunSpeed : WalkSpeed;

        if (CameraController != null)
        {
            CameraController.SetSprintingFOV(_isSprinting);
        }

        Vector3 cameraForward = Vector3.forward;
        Vector3 cameraRight = Vector3.right;

        if (PlayerCamera != null)
        {
            cameraForward = Vector3.ProjectOnPlane(PlayerCamera.forward, Motor.CharacterUp).normalized;
            cameraRight = Vector3.ProjectOnPlane(PlayerCamera.right, Motor.CharacterUp).normalized;
            if (cameraForward.sqrMagnitude < 0.001f) cameraForward = Vector3.forward;
            if (cameraRight.sqrMagnitude < 0.001f) cameraRight = Vector3.right;
        }

        _moveInputVector = (cameraForward * input.y) + (cameraRight * input.x);

        bool isFirstPerson = CameraController != null && CameraController.CurrentCameraMode == CameraMode.FirstPerson;

        if (OrientToCamera || isFirstPerson)
        {
            _lookDirection = cameraForward;
        }
        else
        {
            if (_moveInputVector.sqrMagnitude > 0.001f)
            {
                _lookDirection = _moveInputVector;
            }
            else
            {
                _lookDirection = transform.forward;
            }
        }
    }

    // =========================================================================
    // Физика KCC
    // =========================================================================

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        if (Motor.GroundingStatus.IsStableOnGround)
        {
            Vector3 targetVelocity = _moveInputVector * _currentDesiredSpeed;
            currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, Acceleration * deltaTime);

            if (_jumpRequested)
            {
                currentVelocity += Motor.CharacterUp * JumpSpeed;
                Motor.ForceUnground();
                _jumpRequested = false;
            }
        }
        else
        {
            if (_jumpRequested)
            {
                currentVelocity = (_moveInputVector * _currentDesiredSpeed * AirControlMultiplier) + Motor.CharacterUp * JumpSpeed;
                Motor.ForceUnground();
                _jumpRequested = false;
            }
            else
            {
                Vector3 targetVelocity = _moveInputVector * (_currentDesiredSpeed * AirControlMultiplier);
                Vector3 horizontalVelocity = Vector3.ProjectOnPlane(currentVelocity, Motor.CharacterUp);
                horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, Acceleration * AirControlMultiplier * deltaTime);

                currentVelocity = horizontalVelocity + (currentVelocity.y - Gravity * deltaTime) * Motor.CharacterUp;
            }
        }
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        if (_lookDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_lookDirection, Motor.CharacterUp);
            currentRotation = Quaternion.Slerp(currentRotation, targetRotation, 1f - Mathf.Exp(-RotationSpeed * 0.018f * deltaTime));
        }
    }

    public void BeforeCharacterUpdate(float deltaTime) { }
    public void PostGroundingUpdate(float deltaTime) { }
    public void AfterCharacterUpdate(float deltaTime) { }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }
    public void OnDiscreteCollisionDetected(Collider hitCollider) { }
    public bool IsColliderValidForCollisions(Collider coll) => true;

    // =========================================================================
    // Анимации
    // =========================================================================

    private void UpdateAnimator()
    {
        if (CharacterAnimator == null) return;

        CharacterAnimator.speed = 1f;

        Vector3 horizontalVelocity = Vector3.ProjectOnPlane(Motor.Velocity, Motor.CharacterUp);
        float currentSpeed = horizontalVelocity.magnitude;

        CharacterAnimator.SetFloat(AnimSpeed, currentSpeed);

        bool isGrounded = Motor.GroundingStatus.IsStableOnGround || _jumpRequested;

        if (isGrounded && !_wasGroundedLastFrame)
        {
            _jumpCooldownTimer = JumpCooldown;
        }
        _wasGroundedLastFrame = isGrounded;

        CharacterAnimator.SetBool(AnimGrounded, isGrounded);
        CharacterAnimator.SetBool(AnimFreeFall, !isGrounded && Motor.Velocity.y < -0.1f);
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight < 0.5f) return;

        if (FootstepAudioClips != null && FootstepAudioClips.Length > 0)
        {
            var index = Random.Range(0, FootstepAudioClips.Length);
            AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.position, FootstepAudioVolume);
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (LandingAudioClip != null)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.position, FootstepAudioVolume);
        }
    }
}