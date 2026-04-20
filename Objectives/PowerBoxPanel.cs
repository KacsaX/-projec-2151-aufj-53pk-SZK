using UnityEngine;
using UnityEngine.InputSystem;

public class PowerBoxPanelController : MonoBehaviour
{
    public Animator animator;
    private bool isOpen = false;
    private bool playerInRange = false;

    void Update()
    {
        if (!playerInRange) return;

        if (Keyboard.current.eKey.wasPressedThisFrame && !isOpen)
        {
            animator.ResetTrigger("ClosePanel");
            animator.SetTrigger("OpenPanel");
            isOpen = true;
            Debug.Log("Panel opened.");
        }
        else if (Keyboard.current.qKey.wasPressedThisFrame && isOpen)
        {
            animator.ResetTrigger("OpenPanel");
            animator.SetTrigger("ClosePanel");
            isOpen = false;
            Debug.Log("Panel closed.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"OnTriggerEnter: {other.name} with tag {other.tag}");
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Player entered panel range.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"OnTriggerExit: {other.name} with tag {other.tag}");
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("Player exited panel range.");
        }
    }
}