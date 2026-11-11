using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
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

    [Header("Hearts & Health")]
    [SerializeField] private int maxHearts = 3;
    [field: SerializeField] public int CurrentHearts { get; private set; }

    [Header("Shield Cores")]
    [SerializeField] private int maxShieldCores = 3;
    [field: SerializeField] public int CurrentShieldCores { get; private set; }

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRegenRate = 15f;
    [SerializeField] private float staminaRegenDelay = 1.2f;
    [field: SerializeField] public float CurrentStamina { get; private set; }
    private float staminaRegenTimer;

    [Header("Sanity")]
    [SerializeField] private float maxSanity = 100f;
    [SerializeField] private float sanityDrainRate = 1f;
    [field: SerializeField] public float CurrentSanity { get; private set; }

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform playerVisual;

    [Header("State (Read-Only Debug)")]
    [field: SerializeField] public State CurrentState { get; private set; } = State.Normal;
    public bool IsRunning { get; private set; }
    public bool IsBusy => CurrentState != State.Normal;

    private Vector2 moveInput;
    private Vector2 facingDirection = Vector2.down;
    private Vector2 dodgeDirection;

    private float activeMoveSpeed;
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

    private void Awake()
    {
        CurrentHearts = maxHearts;
        CurrentStamina = maxStamina;
        CurrentSanity = maxSanity;
        CurrentShieldCores = 0;

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (playerVisual != null)
        {
            originalVisualPosition = playerVisual.localPosition;
            if (animator == null) animator = playerVisual.GetComponent<Animator>();
        }

        ConfigureRigidbody();
    }

    private void ConfigureRigidbody()
    {
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Update()
    {
        if (CurrentState == State.Dead)
        {
            HandleDeadInput();
            return;
        }

        HandleInput();
        UpdateJumpVisual();
        UpdateDodgeState();
        UpdateStamina();
        UpdateSanity();
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

    // =====================================
    // INPUT & MOVEMENT
    // =====================================

    private void HandleInput()
    {
        if (CurrentState == State.Hurt || CurrentState == State.Burning || CurrentState == State.Dodging)
        {
            moveInput = Vector2.zero;
            IsRunning = false;
            return;
        }

        float rawX = Input.GetAxisRaw("Horizontal");
        float rawY = Input.GetAxisRaw("Vertical");

        if (rawX != 0)
            moveInput = new Vector2(Mathf.Sign(rawX), 0f);
        else if (rawY != 0)
            moveInput = new Vector2(0f, Mathf.Sign(rawY));
        else
            moveInput = Vector2.zero;

        if (moveInput != Vector2.zero)
            facingDirection = moveInput;

        bool holdingRunKey = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool isMoving = moveInput != Vector2.zero;

        if (holdingRunKey && isMoving && CurrentStamina > 0f)
        {
            IsRunning = true;
            activeMoveSpeed = runSpeed;
            CurrentStamina = Mathf.Max(CurrentStamina - runStaminaCost * Time.deltaTime, 0f);
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
            else if (Input.GetKeyDown(KeyCode.CapsLock))
                StartDodge();
        }
    }

    private void HandleDeadInput()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            TryRevive();
        }
    }

    // =====================================
    // STAMINA SYSTEM
    // =====================================

    private void UpdateStamina()
    {
        if (staminaRegenTimer > 0)
        {
            staminaRegenTimer -= Time.deltaTime;
            return;
        }

        if (CurrentStamina < maxStamina && CurrentState != State.Dodging && !IsRunning)
        {
            CurrentStamina = Mathf.Min(CurrentStamina + staminaRegenRate * Time.deltaTime, maxStamina);
        }
    }

    public bool UseStamina(float amount)
    {
        if (CurrentStamina >= amount)
        {
            CurrentStamina -= amount;
            staminaRegenTimer = staminaRegenDelay;
            return true;
        }
        return false;
    }

    public void RestoreStamina(float amount)
    {
        CurrentStamina = Mathf.Min(CurrentStamina + amount, maxStamina);
    }

    // =====================================
    // SANITY SYSTEM
    // =====================================

    private void UpdateSanity()
    {
        if (sanityDrainRate <= 0f || CurrentState == State.Dead) return;

        CurrentSanity = Mathf.Max(CurrentSanity - sanityDrainRate * Time.deltaTime, 0f);

        if (CurrentSanity <= 0f)
        {
            OnSanityDepleted();
        }
    }

    public void DrainSanity(float amount)
    {
        CurrentSanity = Mathf.Max(CurrentSanity - amount, 0f);
        Debug.Log($"<color=purple>[SANITY LOST]</color> Current Sanity: {CurrentSanity:F1}/{maxSanity}");

        if (CurrentSanity <= 0f) OnSanityDepleted();
    }

    public void RestoreSanity(float amount)
    {
        CurrentSanity = Mathf.Min(CurrentSanity + amount, maxSanity);
        Debug.Log($"<color=magenta>[SANITY RESTORED]</color> Current Sanity: {CurrentSanity:F1}/{maxSanity}");
    }

    private void OnSanityDepleted()
    {
        // Custom effect when sanity reaches 0
    }

    // =====================================
    // JUMP SYSTEM
    // =====================================

    public void StartJump()
    {
        if (IsBusy) return;

        if (!UseStamina(jumpStaminaCost))
        {
            Debug.LogWarning("<color=yellow>[JUMP FAILED]</color> Not enough stamina!");
            return;
        }

        CurrentState = State.Jumping;
        jumpTimer = 0f;

        SetAnimBool(IsJumpingHash, true);
    }

    private void UpdateJumpVisual()
    {
        if (CurrentState != State.Jumping) return;

        jumpTimer += Time.deltaTime;

        if (jumpTimer >= jumpDuration)
        {
            AnimationEvent_EndJump();
        }
    }

    public void AnimationEvent_EndJump()
    {
        if (CurrentState != State.Jumping) return;

        CurrentState = State.Normal;
        jumpTimer = 0f;

        SetAnimBool(IsJumpingHash, false);
    }

    // =====================================
    // DODGE SYSTEM
    // =====================================

    public void StartDodge()
    {
        if (IsBusy || Time.time < lastDodgeTime + dodgeCooldown) return;

        if (!UseStamina(dodgeStaminaCost))
        {
            Debug.LogWarning("<color=yellow>[DODGE FAILED]</color> Not enough stamina!");
            return;
        }

        lastDodgeTime = Time.time;
        dodgeDirection = moveInput != Vector2.zero ? moveInput : facingDirection;
        CurrentState = State.Dodging;
        dodgeTimer = 0f;
        IsRunning = false;

        Debug.Log($"<color=cyan>[DODGE STARTED]</color> Direction: {dodgeDirection} | Stamina Left: {CurrentStamina:F1}");

        if (animator != null)
        {
            animator.SetFloat(DodgeXHash, dodgeDirection.x);
            animator.SetFloat(DodgeYHash, dodgeDirection.y);
            animator.SetBool(IsDodgingHash, true);
        }
    }

    private void UpdateDodgeState()
    {
        if (CurrentState != State.Dodging) return;

        dodgeTimer += Time.deltaTime;

        if (dodgeTimer >= dodgeDuration)
        {
            AnimationEvent_EndDodge();
        }
    }

    public void AnimationEvent_EndDodge()
    {
        if (CurrentState != State.Dodging) return;

        CurrentState = State.Normal;
        rb.linearVelocity = Vector2.zero;
        SetAnimBool(IsDodgingHash, false);

        Debug.Log("<color=green>[DODGE ENDED]</color> State reset to Normal.");
    }

    // =====================================
    // DAMAGE, SHIELD, DEATH & REVIVE
    // =====================================

    public void TakeDamage() => ProcessIncomingHit(State.Hurt, IsHurtHash);
    public void TakeFireDamage() => ProcessIncomingHit(State.Burning, IsBurningHash);

    private void ProcessIncomingHit(State hurtState, int animHash)
    {
        if (IsBusy) return;

        if (CurrentShieldCores > 0)
        {
            CurrentShieldCores--;
            Debug.Log($"<color=blue>[SHIELD CORE ABSORBED HIT]</color> Shield Cores left: {CurrentShieldCores}/{maxShieldCores}");
            TriggerDamageState(hurtState, animHash);
            return;
        }

        Debug.Log("<color=red>[SHIELD BROKEN - TAKING DAMAGE]</color>");
        TriggerDamageState(hurtState, animHash);
    }

    private void TriggerDamageState(State state, int animHash)
    {
        CurrentState = state;
        moveInput = Vector2.zero;
        IsRunning = false;
        rb.linearVelocity = Vector2.zero;
        SetAnimBool(animHash, true);
    }

    public void AnimationEvent_EndHurt() => EndDamageState(State.Hurt, IsHurtHash);
    public void AnimationEvent_EndBurn() => EndDamageState(State.Burning, IsBurningHash);

    private void EndDamageState(State state, int animHash)
    {
        if (CurrentState != state) return;

        CurrentState = State.Normal;
        SetAnimBool(animHash, false);
    }

    public void RestoreShieldCore(int amount = 1)
    {
        CurrentShieldCores = Mathf.Min(CurrentShieldCores + amount, maxShieldCores);
        Debug.Log($"<color=blue>[SHIELD RESTORED]</color> Current Shield Cores: {CurrentShieldCores}/{maxShieldCores}");
    }

    public void Die()
    {
        if (CurrentState == State.Dead) return;

        CurrentState = State.Dead;
        moveInput = Vector2.zero;
        IsRunning = false;
        rb.linearVelocity = Vector2.zero;

        if (playerVisual != null)
            playerVisual.localPosition = originalVisualPosition;

        SetAnimBool(IsDeadHash, true);
        Debug.Log($"<color=red>[PLAYER DIED]</color> Hearts left: {CurrentHearts}. Press 'R' to spend a heart and revive.");
    }

    public bool TryRevive()
    {
        if (CurrentState != State.Dead) return false;

        if (CurrentHearts > 0)
        {
            CurrentHearts--;
            CurrentStamina = maxStamina;
            CurrentState = State.Normal;
            SetAnimBool(IsDeadHash, false);

            Debug.Log($"<color=green>[PLAYER REVIVED]</color> Spent 1 heart. Hearts remaining: {CurrentHearts}");
            return true;
        }

        Debug.LogWarning("<color=red>[REVIVE FAILED]</color> No hearts remaining! Game Over.");
        return false;
    }

    public void AddHeart(int amount = 1)
    {
        CurrentHearts = Mathf.Min(CurrentHearts + amount, maxHearts);
        Debug.Log($"<color=yellow>[HEART GAINED]</color> Current hearts: {CurrentHearts}/{maxHearts}");
    }

    // =====================================
    // ANIMATION & HELPERS
    // =====================================

    private void UpdateAnimations()
    {
        if (animator == null) return;

        animator.SetFloat(MoveXHash, moveInput.x);
        animator.SetFloat(MoveYHash, moveInput.y);
        animator.SetFloat(LastMoveXHash, facingDirection.x);
        animator.SetFloat(LastMoveYHash, facingDirection.y);
        animator.SetBool(IsMovingHash, moveInput != Vector2.zero);
        animator.SetBool(IsRunningHash, IsRunning);
    }

    private void SetAnimBool(int parameterHash, bool value)
    {
        if (animator != null)
            animator.SetBool(parameterHash, value);
    }
}