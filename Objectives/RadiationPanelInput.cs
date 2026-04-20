using UnityEngine;
using TMPro;

public class RadiationPanelInput : MonoBehaviour
{
    public TMP_Text codeDisplay;
    public RadiationPanelCodeGenerator codeGenerator;
    private string currentInput = "";
    public string controlledPanelName = "Radiation Meter";

    public void AddLetter(string letter)
    {
        if (currentInput.Length < 2)
        {
            currentInput += letter;
            UpdateDisplay();
        }
    }

    public void AddNumber(string number)
    {
        if (currentInput.Length == 1)
        {
            currentInput += number;
            UpdateDisplay();
        }
    }

    public void RemoveLast()
    {
        if (currentInput.Length > 0)
        {
            currentInput = currentInput.Substring(0, currentInput.Length - 1);
            UpdateDisplay();
        }
    }

    public void Submit()
    {
        if (currentInput.Length == 2 && codeGenerator != null)
        {
            if (codeGenerator.codeCombinations.ContainsKey(currentInput))
            {
                int value = codeGenerator.codeCombinations[currentInput];
                Debug.Log($"Submitted {currentInput}: Setting {controlledPanelName} to {value}");

                
                ReactorControlLogic logic = FindObjectOfType<ReactorControlLogic>();
                if (logic != null)
                {
                    logic.SetPanelValue(controlledPanelName, value);
                }
            }
            else
            {
                Debug.Log($"Submitted {currentInput}: Invalid code!");
            }
        }
        else
        {
            Debug.Log("Code must be a letter and a number (e.g., d2)");
        }

        currentInput = "";
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (codeDisplay != null)
            codeDisplay.text = currentInput;
    }
}