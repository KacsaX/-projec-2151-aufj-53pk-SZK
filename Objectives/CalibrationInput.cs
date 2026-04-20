using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CalibrationPanelInput : MonoBehaviour
{
    public TMP_Text[] screens;
    public CalibrationComboGenerator comboGenerator;
    private List<string> currentCombo = new List<string>();
    public string controlledPanelName = "Auxiliary Pump Speed";

    // Called by ButtonInteraction when a button is pressed
    
public void AddState(string state)
{
    Debug.Log($"AddState called with: {state}");
    if (currentCombo.Count < 3)
    {
        currentCombo.Add(state);
        UpdateScreens();
    }
}

    public void RemoveLast()
    {
        if (currentCombo.Count > 0)
        {
            currentCombo.RemoveAt(currentCombo.Count - 1);
            UpdateScreens();
        }
    }

    public void Submit()
    {
        if (currentCombo.Count == 3 && comboGenerator != null)
        {
            foreach (var kvp in comboGenerator.combos)
            {
                if (IsComboMatch(kvp.Key, currentCombo))
                {
                    int value = kvp.Value;
                    Debug.Log($"[{controlledPanelName}] Submitted combo: {string.Join(", ", currentCombo)}. Setting value to {value}");

                    ReactorControlLogic logic = FindObjectOfType<ReactorControlLogic>();
                    if (logic != null)
                        logic.SetPanelValue(controlledPanelName, value);

                    currentCombo.Clear();
                    UpdateScreens();
                    return;
                }
            }
            Debug.Log($"[{controlledPanelName}] Submitted combo: {string.Join(", ", currentCombo)}. Invalid combo!");
        }
        else
        {
            Debug.Log("Enter 3 states before submitting.");
        }
        currentCombo.Clear();
        UpdateScreens();
    }

    private void UpdateScreens()
    {
        for (int i = 0; i < screens.Length; i++)
        {
            if (i < currentCombo.Count)
                screens[i].text = currentCombo[i];
            else
                screens[i].text = "";
        }
    }

    private bool IsComboMatch(List<string> comboA, List<string> comboB)
    {
        if (comboA.Count != comboB.Count) return false;
        for (int i = 0; i < comboA.Count; i++)
            if (comboA[i] != comboB[i]) return false;
        return true;
    }
    
}