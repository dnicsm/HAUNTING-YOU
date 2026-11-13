
using UnityEngine;

public class DarknessDetection : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SanitySystem sanity =
                other.GetComponent<SanitySystem>();

            if (sanity != null)
            {
                sanity.isInDarkness = true;
                Debug.Log("Player entered darkness.");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SanitySystem sanity =
                other.GetComponent<SanitySystem>();

            if (sanity != null)
            {
                sanity.isInDarkness = false;
                Debug.Log("Player left darkness.");
            }
        }
    }
}
