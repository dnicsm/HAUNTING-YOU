
using UnityEngine;

public class GhostDetection : MonoBehaviour
{
    public SanitySystem playerSanity;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerSanity = other.GetComponent<SanitySystem>();

            if (playerSanity != null)
            {
                playerSanity.ghostNearby = true;
                Debug.Log("Ghost detected! Sanity draining.");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerSanity != null)
            {
                playerSanity.ghostNearby = false;
                playerSanity = null;
                Debug.Log("Ghost no longer nearby.");
            }
        }
    }
}
