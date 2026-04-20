using UnityEngine;

public class PanelValueChanger : MonoBehaviour
{
    public string panelName;
    private bool isTurnedOff = false;

    // Call this to flag as turned off (e.g., when value is 0)
    public void CheckTurnedOff(int currentValue)
    {
        isTurnedOff = (currentValue == 0);
    }

    // Call this when the player tries to change the value
    public void ChangeValue(int newValue)
    {
        if (isTurnedOff)
        {
            Debug.Log($"{panelName} is turned off! Press the activation button first.");
            return;
        }

        ReactorControlLogic logic = FindObjectOfType<ReactorControlLogic>();
        if (logic != null)
        {
            logic.SetPanelValue(panelName, newValue);
            CheckTurnedOff(newValue);
        }
    }

    // Call this when the player presses the activation button
    public void ActivatePanel()
    {
        isTurnedOff = false;
        Debug.Log($"{panelName} activated! You can now change values.");
    }
}