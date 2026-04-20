using UnityEngine;

public class PowerBoxController : MonoBehaviour
{
    public int[] allowedValues = new int[] { 0, 15, 25, 50, 75, 100 }; // 6 switches
    public bool[] switchStates = new bool[6]; // true = ON, false = OFF
    public int currentIdx = 0; // Start at 0 (switch 1 ON, rest OFF)
    public Animator[] switchAnimators; // Assign in inspector, 6 elements
    public string controlledPanelName = "Backup Generators"; // Set in inspector

    private ReactorControlLogic logic;

    void Start()
    {
        logic = FindObjectOfType<ReactorControlLogic>();
        int startValue = logic != null ? logic.GetCurrentValue(controlledPanelName) : 0;
        int startIdx = System.Array.IndexOf(allowedValues, startValue);
        if (startIdx == -1) startIdx = 0; 

        // Switches up to startIdx ON, rest OFF
        for (int i = 0; i < switchStates.Length; i++)
            switchStates[i] = i <= startIdx;

        currentIdx = startIdx;
        Debug.Log($"PowerBox initialized: value={allowedValues[currentIdx]}");
        PrintSwitchStates();
    }

    public void ToggleSwitch(int index)
    {
        if (!canToggle) return; // Prevent spamming
        StartCoroutine(ToggleSwitchCoroutine(index));
    }

    private bool canToggle = true;

    private System.Collections.IEnumerator ToggleSwitchCoroutine(int index)
    {
        canToggle = false;

        Debug.Log($"ToggleSwitch called for index={index}, currentIdx={currentIdx}");

        if (index == currentIdx + 1 && !switchStates[index])
        {
            switchStates[index] = true;
            currentIdx = index;
            Debug.Log($"Switch {index + 1} turned ON, value now {allowedValues[currentIdx]}");
        }
        else if (index == currentIdx && index > 0 && switchStates[index])
        {
            switchStates[index] = false;
            currentIdx = index - 1;
            Debug.Log($"Switch {index + 1} turned OFF, value now {allowedValues[currentIdx]}");
        }
        else
        {
            Debug.Log($"Switch {index + 1} cannot be toggled out of order.");
        }

        // Update ReactorControlLogic with new value
        if (logic != null)
            logic.SetPanelValue(controlledPanelName, allowedValues[currentIdx]);

        PrintSwitchStates();

        yield return new WaitForSeconds(0.5f);

        canToggle = true;
    }

    private void PrintSwitchStates()
    {
        string states = "";
        for (int i = 0; i < switchStates.Length; i++)
        {
            states += $"[{i + 1}:{(switchStates[i] ? "ON" : "OFF")}] ";

            if (switchAnimators != null && switchAnimators.Length > i && switchAnimators[i] != null)
            {
                if (switchStates[i])
                {
                    switchAnimators[i].ResetTrigger("SwitchOff");
                    switchAnimators[i].SetTrigger("SwitchOn");
                }
                else
                {
                    switchAnimators[i].ResetTrigger("SwitchOn");
                    switchAnimators[i].SetTrigger("SwitchOff");
                }
            }
        }
        Debug.Log($"Switch states: {states} Current value: {allowedValues[currentIdx]}");
    }
    
    public void SyncSwitchesWithLogic()
    {
int startValue = logic != null ? logic.GetCurrentValue(controlledPanelName) : 0;
Debug.Log($"SyncSwitchesWithLogic for {controlledPanelName}: startValue={startValue}");
int startIdx = System.Array.IndexOf(allowedValues, startValue);
if (startIdx == -1) Debug.LogWarning($"Value {startValue} not in allowedValues for {controlledPanelName}, defaulting to 0");

        for (int i = 0; i < switchStates.Length; i++)
            switchStates[i] = i <= startIdx;

        currentIdx = startIdx;
        PrintSwitchStates();
    }
}