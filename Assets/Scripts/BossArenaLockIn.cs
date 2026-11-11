
using UnityEngine;

public class BossArenaLockIn : MonoBehaviour
{
    [Header("Arena Doors")]
    public Animator entryDoorAnimator;
    public Animator exitDoorAnimator;

    [Header("Arena Blockers")]
    public Collider2D entryBlocker;
    public Collider2D exitBlocker;

    [Header("Boss")]
    public GameObject boss;
    public Transform bossSpawnPoint;

    [Header("Door Animation Triggers")]
    public string closeTrigger = "Close";
    public string openTrigger = "Open";

    private bool encounterStarted;
    private bool encounterCompleted;

    private void Start()
    {
        if (boss != null)
            boss.SetActive(false);

        entryBlocker.enabled = false;
        exitBlocker.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (encounterStarted || encounterCompleted)
            return;

        if (other.CompareTag("Player"))
        {
            StartEncounter();
        }
    }

    public void StartEncounter()
    {
        if (encounterStarted)
            return;

        encounterStarted = true;

        entryBlocker.enabled = true;
        exitBlocker.enabled = true;

        entryDoorAnimator.SetTrigger(closeTrigger);
        exitDoorAnimator.SetTrigger(closeTrigger);

        if (boss != null)
        {
            boss.transform.position = bossSpawnPoint.position;
            boss.SetActive(true);
        }

        Debug.Log("BOSS ENCOUNTER STARTED!");
    }

    public void CompleteEncounter()
    {
        if (!encounterStarted || encounterCompleted)
            return;

        encounterCompleted = true;

        entryBlocker.enabled = false;
        exitBlocker.enabled = false;

        entryDoorAnimator.SetTrigger(openTrigger);
        exitDoorAnimator.SetTrigger(openTrigger);

        Debug.Log("BOSS DEFEATED! ARENA UNLOCKED!");
    }
}
