
using UnityEngine;
using UnityEngine.InputSystem;

public class ShieldOrb : MonoBehaviour
{
    [Header("Sanity System")]
    public SanitySystem sanitySystem;

    [Header("Input System")]
    public InputActionReference activateShieldAction;

    private void OnEnable()
    {
        if (activateShieldAction != null)
        {
            activateShieldAction.action.performed += OnShieldPressed;
            activateShieldAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (activateShieldAction != null)
        {
            activateShieldAction.action.performed -= OnShieldPressed;
            activateShieldAction.action.Disable();
        }
    }

    private void OnShieldPressed(InputAction.CallbackContext context)
    {
        if (sanitySystem != null)
        {
            sanitySystem.ActivateShield();
            Debug.Log("Shield Activated!");
        }
    }
}
