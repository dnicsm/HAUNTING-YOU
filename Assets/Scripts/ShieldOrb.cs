
using UnityEngine;

public class ShieldOrb : MonoBehaviour
{
    public SanitySystem sanitySystem;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (sanitySystem != null)
            {
                sanitySystem.ActivateShield();
            }
        }
    }
}
