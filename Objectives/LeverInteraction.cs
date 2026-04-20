using UnityEngine;
using UnityEngine.InputSystem;

public class LeverInteraction : MonoBehaviour
{
    public Animator animator;
    public bool IsOn { get; private set; } = false;
    public System.Action<bool> OnLeverToggled;

    void Update()
    {
        if (IsPlayerLookingAtMe() && Keyboard.current.eKey.wasPressedThisFrame)
        {
            SetLeverState(!IsOn);
        }
    }

    public void SetLeverState(bool on)
    {
        if (on)
        {
            animator.ResetTrigger("LeverOff");
            animator.SetTrigger("LeverOn");
        }
        else
        {
            animator.ResetTrigger("LeverOn");
            animator.SetTrigger("LeverOff");
        }
        IsOn = on;
        OnLeverToggled?.Invoke(IsOn);
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