using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerHealth))]
public class PlayerController : MonoBehaviour
{
    public enum State { Normal, Jumping, Dodging, Hurt, Burning, Dead }

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 7.5f;
    [SerializeField] private float runStaminaCost = 1f;

    [Header("Jump")]
    [SerializeField] private float jumpDuration = 0.7f;
    [SerializeField] private float jumpStaminaCost = 10f;

    [Header("Dodge")]
    [SerializeField] private float dodgeSpeed = 12f;
    [SerializeField] private float dodgeCooldown = 0.8f;
    [SerializeField] private float dodgeDuration = 0.3f;
    [SerializeField] private float dodgeStaminaCost = 25f;

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRegenRate = 15f;
    [SerializeField] private float staminaRegenDelay = 1.2f;

    [field: SerializeField] public float CurrentStamina { get; private set; }

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform playerVisual;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("State (Read-Only Debug)")]
    [field: SerializeField] public State CurrentState { get; private set; } = State.Normal;

    public bool IsRunning { get; private set; }
    public bool IsBusy => CurrentState != State.Normal;
    public PlayerHealth Health => playerHealth;

    private Vector2 moveInput;
    private Vector2 facingDirection = Vector2.down;
    private Vector2 dodgeDirection;

    private float activeMoveSpeed;
    private float staminaRegenTimer;
    private float lastDodgeTime = -10f;
    private float dodgeTimer;
    private float jumpTimer;
    private Vector3 originalVisualPosition;

    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int MoveYHash = Animator.StringToHash("MoveY");
    private static readonly int LastMoveXHash = Animator.StringToHash("LastMoveX");
    private static readonly int LastMoveYHash = Animator.StringToHash("LastMoveY");
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
    private static readonly int IsJumpingHash = Animator.StringToHash("IsJumping");
    private static readonly int IsDodgingHash = Animator.StringToHash("IsDodging");
    private static readonly int DodgeXHash = Animator.StringToHash("DodgeX");
    private static readonly int DodgeYHash = Animator.StringToHash("DodgeY");
    private static readonly int IsHurtHash = Animator.StringToHash("IsHurt");
    private static readonly int IsBurningHash = Animator.StringToHash("IsBurning");
    private static readonly int IsDeadHash = Animator.StringToHash("IsDead");

    private void OnValidate()
    {
        walkSpeed = Mathf.Max(0f, walkSpeed);
        runSpeed = Mathf.Max(0f, runSpeed);
        runStaminaCost = Mathf.Max(0f, runStaminaCost);
        jumpDuration = Mathf.Max(0.01f, jumpDuration);
        jumpStaminaCost = Mathf.Max(0f, jumpStaminaCost);
        dodgeSpeed = Mathf.Max(0f, dodgeSpeed);
        dodgeCooldown = Mathf.Max(0f, dodgeCooldown);
        dodgeDuration = Mathf.Max(0.01f, dodgeDuration);
        dodgeStaminaCost = Mathf.Max(0f, dodgeStaminaCost);
        maxStamina = Mathf.Max(0f, maxStamina);
        staminaRegenRate = Mathf.Max(0f, staminaRegenRate);
        staminaRegenDelay = Mathf.Max(0f, staminaRegenDelay);
    }

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

        if (playerVisual != null)
        {
            originalVisualPosition = playerVisual.localPosition;
            if (animator == null)
                animator = playerVisual.GetComponent<Animator>();
        }

        CurrentStamina = maxStamina;
        activeMoveSpeed = walkSpeed;

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHurt += HandleHurt;
            playerHealth.OnBurned += HandleBurned;
            playerHealth.OnDied += HandleDied;
            playerHealth.OnRevived += HandleRevived;
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHurt -= HandleHurt;
            playerHealth.OnBurned -= HandleBurned;
            playerHealth.OnDied -= HandleDied;
            playerHealth.OnRevived -= HandleRevived;
        }
    }

    private void Update()
    {
        if (CurrentState == State.Dead)
        {
            if (Input.GetKeyDown(KeyCode.R) && playerHealth != null)
                playerHealth.TryRevive();
            return;
        }

        HandleInput();
        UpdateJumpState();
        UpdateDodgeState();
        UpdateStamina();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        switch (CurrentState)
        {
            case State.Dodging:
                rb.linearVelocity = dodgeDirection * dodgeSpeed;
                break;
            case State.Normal:
            case State.Jumping:
                rb.linearVelocity = moveInput * activeMoveSpeed;
                break;
            default:
                rb.linearVelocity = Vector2.zero;
                break;
        }
    }

    private void HandleInput()
    {
        if (CurrentState == State.Hurt ||
            CurrentState == State.Burning ||
            CurrentState == State.Dodging)
        {
            moveInput = Vector2.zero;
            IsRunning = false;
            return;
        }

        float rawX = Input.GetAxisRaw("Horizontal");
        float rawY = Input.GetAxisRaw("Vertical");

        if (rawX != 0f)
            moveInput = new Vector2(Mathf.Sign(rawX), 0f);
        else if (rawY != 0f)
            moveInput = new Vector2(0f, Mathf.Sign(rawY));
        else
            moveInput = Vector2.zero;

        if (moveInput != Vector2.zero)
            facingDirection = moveInput;

        bool holdingRunKey =
            Input.GetKey(KeyCode.LeftShift) ||
            Input.GetKey(KeyCode.RightShift);

        if (holdingRunKey && moveInput != Vector2.zero && CurrentStamina > 0f)
        {
            IsRunning = true;
            activeMoveSpeed = runSpeed;
            CurrentStamina = Mathf.Max(
                0f,
                CurrentStamina - runStaminaCost * Time.deltaTime);
            staminaRegenTimer = staminaRegenDelay;
        }
        else
        {
            IsRunning = false;
            activeMoveSpeed = walkSpeed;
        }

        if (CurrentState == State.Normal)
        {
            if (Input.GetKeyDown(KeyCode.Space))
                StartJump();
            else if (Input.GetKeyDown(KeyCode.P))
                StartDodge();
        }
    }

    // HEALTH EVENT HANDLERS
    private void HandleHurt()
    {
        EnterDamageState(State.Hurt, IsHurtHash);
    }

    private void HandleBurned()
    {
        EnterDamageState(State.Burning, IsBurningHash);
    }

    private void EnterDamageState(State damageState, int animationHash)
    {
        CurrentState = damageState;
        moveInput = Vector2.zero;
        IsRunning = false;
        jumpTimer = 0f;
        dodgeTimer = 0f;
        rb.linearVelocity = Vector2.zero;
        ClearActionAnimationFlags();
        SetAnimBool(animationHash, true);
    }

    private void HandleDied()
    {
        CurrentState = State.Dead;
        moveInput = Vector2.zero;
        IsRunning = false;
        activeMoveSpeed = walkSpeed;
        rb.linearVelocity = Vector2.zero;

        if (playerVisual != null)
            playerVisual.localPosition = originalVisualPosition;

        ClearActionAnimationFlags();
        SetAnimBool(IsDeadHash, true);
    }

    private void HandleRevived()
    {
        CurrentStamina = maxStamina;
        staminaRegenTimer = 0f;
        moveInput = Vector2.zero;
        IsRunning = false;
        activeMoveSpeed = walkSpeed;
        CurrentState = State.Normal;
        rb.linearVelocity = Vector2.zero;

        ClearActionAnimationFlags();
        SetAnimBool(IsDeadHash, false);
    }

    // STAMINA
    private void UpdateStamina()
    {
        if (staminaRegenTimer > 0f)
        {
            staminaRegenTimer -= Time.deltaTime;
            return;
        }

        if (CurrentStamina < maxStamina &&
            CurrentState != State.Dodging &&
            !IsRunning)
        {
            CurrentStamina = Mathf.Min(
                CurrentStamina + staminaRegenRate * Time.deltaTime,
                maxStamina);
        }
    }

    public bool UseStamina(float amount)
    {
        if (amount < 0f || CurrentStamina < amount)
            return false;

        CurrentStamina -= amount;
        staminaRegenTimer = staminaRegenDelay;
        return true;
    }

    public void RestoreStamina(float amount)
    {
        if (amount <= 0f)
            return;

        CurrentStamina = Mathf.Min(CurrentStamina + amount, maxStamina);
    }

    // JUMP
    public void StartJump()
    {
        if (IsBusy || !UseStamina(jumpStaminaCost))
            return;

        CurrentState = State.Jumping;
        jumpTimer = 0f;
        SetAnimBool(IsJumpingHash, true);
    }

    private void UpdateJumpState()
    {
        if (CurrentState != State.Jumping)
            return;

        jumpTimer += Time.deltaTime;
        if (jumpTimer >= jumpDuration)
            AnimationEvent_EndJump();
    }

    public void AnimationEvent_EndJump()
    {
        if (CurrentState != State.Jumping)
            return;

        jumpTimer = 0f;
        CurrentState = State.Normal;
        SetAnimBool(IsJumpingHash, false);
    }

    // DODGE
    public void StartDodge()
    {
        if (IsBusy || Time.time < lastDodgeTime + dodgeCooldown)
            return;

        if (!UseStamina(dodgeStaminaCost))
            return;

        lastDodgeTime = Time.time;
        dodgeDirection = moveInput != Vector2.zero ? moveInput : facingDirection;
        CurrentState = State.Dodging;
        dodgeTimer = 0f;
        IsRunning = false;

        if (animator != null)
        {
            animator.SetFloat(DodgeXHash, dodgeDirection.x);
            animator.SetFloat(DodgeYHash, dodgeDirection.y);
            animator.SetBool(IsDodgingHash, true);
        }
    }

    private void UpdateDodgeState()
    {
        if (CurrentState != State.Dodging)
            return;

        dodgeTimer += Time.deltaTime;
        if (dodgeTimer >= dodgeDuration)
            AnimationEvent_EndDodge();
    }

    public void AnimationEvent_EndDodge()
    {
        if (CurrentState != State.Dodging)
            return;

        dodgeTimer = 0f;
        CurrentState = State.Normal;
        rb.linearVelocity = Vector2.zero;
        SetAnimBool(IsDodgingHash, false);
    }

    // DAMAGE ANIMATION EVENTS
    public void AnimationEvent_EndHurt()
    {
        EndDamageState(State.Hurt, IsHurtHash);
    }

    public void AnimationEvent_EndBurn()
    {
        EndDamageState(State.Burning, IsBurningHash);
    }

    private void EndDamageState(State state, int animationHash)
    {
        if (CurrentState != state)
            return;

        CurrentState = State.Normal;
        SetAnimBool(animationHash, false);
    }

    // ANIMATION HELPERS
    private void UpdateAnimations()
    {
        if (animator == null)
            return;

        animator.SetFloat(MoveXHash, moveInput.x);
        animator.SetFloat(MoveYHash, moveInput.y);
        animator.SetFloat(LastMoveXHash, facingDirection.x);
        animator.SetFloat(LastMoveYHash, facingDirection.y);
        animator.SetBool(IsMovingHash, moveInput != Vector2.zero);
        animator.SetBool(IsRunningHash, IsRunning);
    }

    private void ClearActionAnimationFlags()
    {
        SetAnimBool(IsJumpingHash, false);
        SetAnimBool(IsDodgingHash, false);
        SetAnimBool(IsHurtHash, false);
        SetAnimBool(IsBurningHash, false);
        SetAnimBool(IsRunningHash, false);
        SetAnimBool(IsMovingHash, false);
    }

    private void SetAnimBool(int parameterHash, bool value)
    {
        if (animator != null)
            animator.SetBool(parameterHash, value);
    }
}