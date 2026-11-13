
using UnityEngine;
using UnityEngine.UI;

public class SanitySystem : MonoBehaviour
{
    [Header("Sanity Settings")]
    public float maxSanity = 100f;
    public float currentSanity = 100f;

    [Header("Sanity Drain")]
    public float darknessDrainRate = 2f;
    public float ghostDrainRate = 5f;
    public float regenerationRate = 3f;

    [Header("Sanity UI")]
    public Slider sanityBar;

    [Header("Environment")]
    public bool isInDarkness = false;
    public bool ghostNearby = false;

    [Header("Shield Orb")]
    public bool shieldActive = false;
    public float shieldDuration = 5f;
    public float shieldCooldown = 10f;

    private float shieldTimer = 0f;
    private float cooldownTimer = 0f;

    void Start()
    {
        currentSanity = maxSanity;

        if (sanityBar != null)
        {
            sanityBar.maxValue = maxSanity;
            sanityBar.value = currentSanity;
        }
    }

    void Update()
    {
        HandleSanity();
        HandleShield();
        UpdateSanityUI();
    }

    void HandleSanity()
    {
        if (shieldActive)
            return;

        float drain = 0f;

        if (isInDarkness)
            drain += darknessDrainRate;

        if (ghostNearby)
            drain += ghostDrainRate;

        if (drain > 0)
        {
            currentSanity -= drain * Time.deltaTime;
        }
        else
        {
            currentSanity += regenerationRate * Time.deltaTime;
        }

        currentSanity = Mathf.Clamp(currentSanity, 0f, maxSanity);

        if (currentSanity <= 0)
        {
            OnSanityDepleted();
        }
    }

    void HandleShield()
    {
        if (shieldActive)
        {
            shieldTimer -= Time.deltaTime;

            if (shieldTimer <= 0)
            {
                shieldActive = false;
                Debug.Log("Shield expired!");
            }
        }

        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    public void ActivateShield()
    {
        if (shieldActive || cooldownTimer > 0)
            return;

        shieldActive = true;
        shieldTimer = shieldDuration;
        cooldownTimer = shieldCooldown;

        Debug.Log("Shield Orb Activated!");
    }

    void UpdateSanityUI()
    {
        if (sanityBar != null)
        {
            sanityBar.value = currentSanity;
        }
    }

    void OnSanityDepleted()
    {
        Debug.Log("Sanity Depleted! Player is losing their mind.");
    }

    public bool IsShieldProtecting()
    {
        return shieldActive;
    }
}
