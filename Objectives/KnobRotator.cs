using UnityEngine;

public class KnobRotator : MonoBehaviour
{
    public FrequencyKnobController controller;
    public enum KnobType { Hundreds, Tens, Units }
    public KnobType knobType;

    public int minValue = 0;
    public int maxValue = 9;
    public int currentValue = 0;

    void Update()
    {
        if (IsPlayerLookingAtMe())
        {
            if (UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
                SetValue(currentValue + 1);
            else if (UnityEngine.InputSystem.Keyboard.current.qKey.wasPressedThisFrame)
                SetValue(currentValue - 1);
        }
    }

    public void SetValue(int value)
    {
        currentValue = Mathf.Clamp(value, minValue, maxValue);
        float t = (float)(currentValue - minValue) / (maxValue - minValue);
        float angle = Mathf.Lerp(0f, 270f, t);
        transform.localEulerAngles = new Vector3(-angle, 0, 0);

        // Update the controller
        if (controller != null)
        {
            switch (knobType)
            {
                case KnobType.Hundreds:
                    controller.SetHundreds(currentValue);
                    break;
                case KnobType.Tens:
                    controller.SetTens(currentValue);
                    break;
                case KnobType.Units:
                    controller.SetUnits(currentValue);
                    break;
            }
        }
    }

    private bool IsPlayerLookingAtMe()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(ray, out RaycastHit hit, 3f))
            return hit.collider != null && hit.collider.gameObject == this.gameObject;
        return false;
    }
}