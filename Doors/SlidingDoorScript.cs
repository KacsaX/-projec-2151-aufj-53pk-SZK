using UnityEngine;
using System.Collections.Generic;

public class SlidingDoor : MonoBehaviour
{
    public Animator animator;
    public bool IsOpen { get; private set; } = false;

    public float interactCooldownDuration = 3f;
    private float interactCooldown = 0f;
    private int lurkerInTriggerCount = 0;
    private bool lockedOpen = false;


private HashSet<LurkerAI> lurkersSlowedThisOpen = new HashSet<LurkerAI>();


    void Update()
    {
        if (lockedOpen)
        {
            // tartsa nyitva és ne legyen interaktálható, amíg Lurker (ellenfél) jelen van
            if (!IsOpen)
                OpenDoor();
            return;
        }

        if (interactCooldown > 0f)
            interactCooldown -= Time.deltaTime;
    }

private void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Lurker"))
    {
        lurkerInTriggerCount++;
        bool needsToOpen = !IsOpen;

        if (needsToOpen)
        {
            OpenDoor();

            LurkerAI lurker = other.GetComponent<LurkerAI>();
            if (lurker != null && !lurkersSlowedThisOpen.Contains(lurker))
            {
                lurker.SlowDownForDoor(1f, 1f); // Minden nyitásnál lassítsa le a Lurkert 1x
                lurkersSlowedThisOpen.Add(lurker);
            }
        }
        lockedOpen = true;
    }
}

private void OnTriggerExit(Collider other)
{
    if (other.CompareTag("Lurker"))
    {
        lurkerInTriggerCount = Mathf.Max(0, lurkerInTriggerCount - 1);

        LurkerAI lurker = other.GetComponent<LurkerAI>();
        if (lurker != null)
        {
            lurkersSlowedThisOpen.Remove(lurker); // Minden nyitásnál engedélyezze újra a lassítást
        }

        if (lurkerInTriggerCount == 0)
        {
            interactCooldown = interactCooldownDuration;
            lockedOpen = false;
        }
    }
}

    public void OpenDoor()
    {
        animator.SetTrigger("Open");
        IsOpen = true;
    }

    public void CloseDoor()
    {
        if (interactCooldown <= 0f && !lockedOpen)
        {
            animator.SetTrigger("Close");
            IsOpen = false;
        }
    }
}