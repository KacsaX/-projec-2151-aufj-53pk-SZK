using UnityEngine;
using System.Collections.Generic;

public class ReactorControlLogic : MonoBehaviour
{
    // Panel names
    private readonly string[] panelNames = new string[]
    {
        "Pressure",
        "Fuel",
        "Backup Generators",
        "Coolant Flow Rate",
        "Core Temperature",
        "Radiation Meter",
        "Power Output",
        "Control Rod Position",
        "Ventilation Fan Speed",
        "Battery Backup Charge",
        "Reactor Shielding Level",
        "Auxiliary Pump Speed",
        "Turbine Output Regulator",
        "Water Purity Control"
    };

    [Header("Difficulty Settings")]
    [Range(1, 14)]
    public int panelsToRuin = 3; // Number of panels to ruin

    // Correct and current values
    private int[] correctValues = new int[14];
    private int[] currentValues = new int[14];

    public void ReactorPanelsSpawn()
    {
        GenerateCorrectValues();
        GenerateCurrentValuesWithFaults();
        float stability = CalculateStability();
        LogPanelValues(stability);
    }

    void GenerateCorrectValues()
    {
        for (int i = 0; i < correctValues.Length; i++)
        {
            //new
            if (panelNames[i] == "Fuel")
            {
                int val = Random.Range(1, 21) * 5; // 5 to 100 in steps of 5
                correctValues[i] = val;
            }
            else if (panelNames[i] == "Battery Backup Charge")
            {
                int val = Random.Range(1, 21) * 5; // 5 to 100 in steps of 5
                correctValues[i] = val;
            }
            else if (panelNames[i] == "Water Purity Control")
            {
                int val = Random.Range(1, 21) * 5; // 5 to 100 in steps of 5
                correctValues[i] = val;
            }
            else if (panelNames[i] == "Pressure")
            {
                int val = Random.Range(0, 11) * 10; // 0 to 100 in steps of 10
                correctValues[i] = val;
            }
            else if (panelNames[i] == "Radiation Meter")
            {
                int val = Random.Range(0, 11) * 10; // 0 to 100 in steps of 10
                correctValues[i] = val;
            }
            else if (panelNames[i] == "Power Output")
            {
                int val = Random.Range(0, 11) * 10; // 0 to 100 in steps of 10
                correctValues[i] = val;
            }
            else if (panelNames[i] == "Reactor Shielding Level")
            {
                int val = Random.Range(0, 11) * 10; // 0 to 100 in steps of 10
                correctValues[i] = val;
            }
            else if (panelNames[i] == "Control Rod Position")
            {
                int val = Random.Range(0, 11) * 10; // 0 to 100 in steps of 10
                correctValues[i] = val;
            }
            else if (panelNames[i] == "Coolant Flow Rate")
            {
                int val = Random.Range(0, 11) * 10; // 0 to 100 in steps of 10
                correctValues[i] = val;
            }
            else if (panelNames[i] == "Auxiliary Pump Speed")
            {
                int val = Random.Range(0, 11) * 10; // 0 to 100 in steps of 10
                correctValues[i] = val;
            }
            else if (panelNames[i] == "Ventilation Fan Speed")
            {
                int val = Random.Range(0, 11) * 10; // 0 to 100 in steps of 10
                correctValues[i] = val;
            }
            else if (panelNames[i] == "Backup Generators")
            {
                int[] backupGenValues = new int[] {0, 15, 25, 50, 75, 100 };
                int val = backupGenValues[Random.Range(0, backupGenValues.Length)];
                correctValues[i] = val;
            }
            else if (panelNames[i] == "Core Temperature")
            {
                int[] coreTempValues = new int[] { 0, 15, 25, 50, 75, 100 };
                int val = coreTempValues[Random.Range(0, coreTempValues.Length)];
                correctValues[i] = val;
            }
            else
            {
                int val = Random.Range(10, 101);
                correctValues[i] = (val % 2 == 0) ? val : val - 1; // Ensure even
            }
        }
    }

    void GenerateCurrentValuesWithFaults()
    {
        // Copy correct values
        for (int i = 0; i < correctValues.Length; i++)
            currentValues[i] = correctValues[i];

        HashSet<int> ruinedIndices = new HashSet<int>();
        int attempts = 0;
        float stability = 100f;

        while (ruinedIndices.Count < panelsToRuin && attempts < 100)
        {
            int idx = Random.Range(0, currentValues.Length);
            if (ruinedIndices.Contains(idx)) continue;

            int ruinedValue;
            if (panelNames[idx] == "Pressure")
            {
                ruinedValue = Random.Range(0, 11) * 10; // Steps of 10
            }
            
            else if (panelNames[idx] == "Fuel")
            {
                ruinedValue = Random.Range(1, 21) * 5; // 5 to 100 in steps of 5
            }
            else if (panelNames[idx] == "Water Purity Control")
            {
                ruinedValue = Random.Range(1, 21) * 5; // 5 to 100 in steps of 5
            }
            else if (panelNames[idx] == "Battery Backup Charge")
            {
                ruinedValue = Random.Range(1, 21) * 5; // 5 to 100 in steps of 5
            }
            else if (panelNames[idx] == "Radiation Meter")
            {
                ruinedValue = Random.Range(0, 11) * 10; // 0 to 100 in steps of 10
            }
            else if (panelNames[idx] == "Power Output")
            {
                ruinedValue = Random.Range(0, 11) * 10; // 0 to 100 in steps of 10
            }
            else if (panelNames[idx] == "Reactor Shielding Level")
            {
                ruinedValue = Random.Range(0, 11) * 10; // 0 to 100 in steps of 10
            }
            else if (panelNames[idx] == "Control Rod Position")
            {
                ruinedValue = Random.Range(0, 11) * 10; // 0 to 100 in steps of 10
            }
            else if (panelNames[idx] == "Coolant Flow Rate")
            {
                ruinedValue = Random.Range(0, 11) * 10; // 0 to 100 in steps of 10
            }
            else if (panelNames[idx] == "Auxiliary Pump Speed")
            {
                ruinedValue = Random.Range(0, 11) * 10; // 0 to 100 in steps of 10
            }
            else if (panelNames[idx] == "Ventilation Fan Speed")
            {
                ruinedValue = Random.Range(0, 11) * 10; // 0 to 100 in steps of 10
            }
            else if (panelNames[idx] == "Backup Generators")
            {
                int[] backupGenValues = new int[] {0, 15, 25, 50, 75, 100 };
                ruinedValue = backupGenValues[Random.Range(0, backupGenValues.Length)];
            }
            else if (panelNames[idx] == "Core Temperature")
            {
                int[] coreTempValues = new int[] { 0, 15, 25, 50, 75, 100 };
                ruinedValue = coreTempValues[Random.Range(0, coreTempValues.Length)];
            }
            else
            {
                ruinedValue = (correctValues[idx] < 50) ? 100 : 0;
                ruinedValue = (ruinedValue % 2 == 0) ? ruinedValue : ruinedValue - 1;
            }

            int originalValue = currentValues[idx];
            currentValues[idx] = ruinedValue;
            ruinedIndices.Add(idx);

            stability = CalculateStability();

            if (stability < 40f)
            {
                currentValues[idx] = originalValue;
                ruinedIndices.Remove(idx);
            }

            attempts++;
        }
        foreach (var box in FindObjectsOfType<PowerBoxController>())
            box.SyncSwitchesWithLogic();
    }

    public float CalculateStability()
    {
        int total = 0;
        for (int i = 0; i < correctValues.Length; i++)
        {
            int diff = Mathf.Abs(currentValues[i] - correctValues[i]);
            int score = Mathf.Max(0, 100 - diff);
            total += score;
        }
        return total / (float)(correctValues.Length);
    }

    void LogPanelValues(float stability)
    {
        Debug.Log("Panel Values:");
        for (int i = 0; i < panelNames.Length; i++)
        {
            Debug.Log($"{panelNames[i]}: Correct={correctValues[i]}, Current={currentValues[i]}");
        }
        Debug.Log($"Balance (Stability): {stability:0}%");
    }

    public void UpdatePanelDisplays()
    {
        for (int i = 0; i < panelNames.Length; i++)
        {
            string panelName = panelNames[i];
            int value = currentValues[i];

            // keresse meg és frissítse a gauge-ot, ha van
            GameObject gaugeObj = GameObject.Find(panelName);
            if (gaugeObj != null)
            {
                GaugeController gauge = gaugeObj.GetComponent<GaugeController>();
                if (gauge != null)
                    gauge.SetGauge(value / 100f);
            }

            // keresse meg és frissítse a screen-t, ha van
            GameObject screenObj = GameObject.Find(panelName);
            if (screenObj != null)
            {
                ScreenController screen = screenObj.GetComponent<ScreenController>();
                if (screen != null)
                    screen.SetValue(value);
            }
        }
    }
    public void UpdateBalanceDisplay(float stability)
    {
        Debug.Log($"UpdateBalanceDisplay called. Stability: {stability}");

        GameObject balanceObj = GameObject.Find("Balance");
        if (balanceObj != null)
        {
            Debug.Log("Balance object found.");
            ScreenController screen = balanceObj.GetComponent<ScreenController>();
            if (screen != null)
            {
                Debug.Log("ScreenController found on Balance. Updating text.");
                screen.SetValueText($"{Mathf.RoundToInt(stability)}%");
            }
            else
            {
                Debug.LogWarning("ScreenController NOT found on Balance!");
            }
        }
        else
        {
            Debug.LogWarning("Balance object NOT found!");
        }
    }

    public void SetPanelValue(string panelName, int newValue)
    {
        for (int i = 0; i < panelNames.Length; i++)
        {
            if (panelNames[i] == panelName)
            {
                if (panelNames[i] == "Fuel")
                {
                    newValue = Mathf.Clamp(Mathf.RoundToInt(newValue / 5f) * 5, 0, 100); // Steps of 5
                }
                else if (panelNames[i] == "Pressure")
                {
                    newValue = Mathf.Clamp(Mathf.RoundToInt(newValue / 10f) * 10, 0, 100); // Steps of 10
                }
                else
                {
                    newValue = (newValue % 2 == 0) ? newValue : newValue; // Even
                    newValue = Mathf.Clamp(newValue, 0, 100);
                }

                currentValues[i] = newValue;
                Debug.Log($"SetPanelValue: {panelNames[i]} updated to {newValue} (called by {panelName})");
                UpdatePanelDisplays();
                float stability = CalculateStability();
                UpdateBalanceDisplay(stability);
                return;
            }
        }
        Debug.LogWarning($"Panel name '{panelName}' not found!");
    }
    
    public int GetCurrentValue(string panelName)
    {
        for (int i = 0; i < panelNames.Length; i++)
        {
            if (panelNames[i] == panelName)
            {
                //Debug.Log($"GetCurrentValue: {panelName} = {currentValues[i]}");
                return currentValues[i];
            }
        }
        Debug.LogWarning($"GetCurrentValue: Panel name '{panelName}' not found!");
        return 0;
    }
}