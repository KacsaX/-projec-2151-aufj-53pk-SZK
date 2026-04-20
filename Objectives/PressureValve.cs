using UnityEngine;
using UnityEngine.InputSystem;

public class ValveController : MonoBehaviour
{
    public Animator animator;
    public float rotateAnimDuration = 1.0f;
    public string controlledPanelName = "Pressure";
    private bool playerInRange = false;
    private bool isRotating = false;

    void Update()
    {
        if (!playerInRange || isRotating) return;

        ReactorControlLogic logic = FindObjectOfType<ReactorControlLogic>();
        int currentValue = logic != null ? logic.GetCurrentValue(controlledPanelName) : 0;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (currentValue < 100)
                StartCoroutine(RotateRightSequence());
            else
                Debug.Log($"{controlledPanelName} is at maximum (100), cannot rotate right.");
        }
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            if (currentValue > 0)
                StartCoroutine(RotateLeftSequence());
            else
                Debug.Log($"{controlledPanelName} is at minimum (0), cannot rotate left.");
        }
    }

    private System.Collections.IEnumerator RotateRightSequence()
    {
        isRotating = true;
        animator.ResetTrigger("RotateRight");
        animator.ResetTrigger("Static");
        animator.SetTrigger("RotateRight");
        Debug.Log("Animator trigger: RotateRight");

        ReactorControlLogic logic = FindObjectOfType<ReactorControlLogic>();
        int currentValue = logic != null ? logic.GetCurrentValue(controlledPanelName) : 0;
        int newValue = Mathf.Clamp(currentValue + 10, 0, 100);
        if (logic != null)
            logic.SetPanelValue(controlledPanelName, newValue);

        Debug.Log($"{controlledPanelName} increased to {newValue}");

        yield return new WaitForSeconds(rotateAnimDuration);

        animator.SetTrigger("Static");
        Debug.Log("Animator trigger: Static");

        yield return new WaitForSeconds(0.1f);
        isRotating = false;
    }

    private System.Collections.IEnumerator RotateLeftSequence()
    {
        isRotating = true;
        animator.ResetTrigger("RotateLeft");
        animator.ResetTrigger("Static");
        animator.SetTrigger("RotateLeft");
        Debug.Log("Animator trigger: RotateLeft");

        ReactorControlLogic logic = FindObjectOfType<ReactorControlLogic>();
        int currentValue = logic != null ? logic.GetCurrentValue(controlledPanelName) : 0;
        int newValue = Mathf.Clamp(currentValue - 10, 0, 100);
        if (logic != null)
            logic.SetPanelValue(controlledPanelName, newValue);

        Debug.Log($"{controlledPanelName} decreased to {newValue}");

        yield return new WaitForSeconds(rotateAnimDuration);

        animator.SetTrigger("Static");
        Debug.Log("Animator trigger: Static");

        yield return new WaitForSeconds(0.1f);
        isRotating = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}