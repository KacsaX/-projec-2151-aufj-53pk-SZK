using UnityEngine;
using UnityEngine.InputSystem;

public class ButtonInteraction : MonoBehaviour
{
    public Animator animator;
    public string pressTrigger = "ButtonPress";
    public string resetTrigger = "ButtonReturn";
    public float animationDuration = 0.5f;
    private bool canPress = true;
    public enum ButtonType { Letter, Number, Remove, Submit }
    public ButtonType type;
    public string value;
    public MonoBehaviour inputController;

    void Update()
    {
        if (canPress && IsPlayerLookingAtMe() && Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartCoroutine(PressButtonCoroutine());
        }
    }

private System.Collections.IEnumerator PressButtonCoroutine()
{
    canPress = false;
    animator.ResetTrigger(resetTrigger);
    animator.ResetTrigger(pressTrigger);
    animator.SetTrigger(pressTrigger);
    Debug.Log("Button pressed!");
    yield return new WaitForSeconds(animationDuration);
    animator.ResetTrigger(pressTrigger);
    animator.SetTrigger(resetTrigger);
    Debug.Log("Button reset!");

    // Call input logic
    if (inputController == null)
        inputController = FindObjectOfType<RadiationPanelInput>();

switch (type)
{
    case ButtonType.Letter:
        var letterInput = inputController as RadiationPanelInput;
        if (letterInput != null) letterInput.AddLetter(value);
        Debug.Log($"inputController type: {inputController?.GetType().Name}");
        var calibrationInput = inputController as CalibrationPanelInput;
        if (calibrationInput != null)
        {
            Debug.Log($"Button pressed: {value} (calling AddState)");
            calibrationInput.AddState(value);
        }
        break;
    case ButtonType.Number:
        var numberInput = inputController as RadiationPanelInput;
        if (numberInput != null) numberInput.AddNumber(value);
        break;
    case ButtonType.Remove:
        var removeInput = inputController as RadiationPanelInput;
        if (removeInput != null) removeInput.RemoveLast();

        var calibrationRemove = inputController as CalibrationPanelInput;
        if (calibrationRemove != null) calibrationRemove.RemoveLast();
        break;
    case ButtonType.Submit:
        var radInput = inputController as RadiationPanelInput;
        if (radInput != null) radInput.Submit();

        var freqInput = inputController as FrequencyKnobController;
        if (freqInput != null) freqInput.Submit();

        var calibrationSubmit = inputController as CalibrationPanelInput;
        if (calibrationSubmit != null) calibrationSubmit.Submit();
        break;
}

    yield return new WaitForSeconds(0.1f);
    canPress = true;
}

    private bool IsPlayerLookingAtMe()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(ray, out RaycastHit hit, 3f))
        {
            return hit.collider != null && hit.collider.gameObject == this.gameObject;
        }
        return false;
    }
}