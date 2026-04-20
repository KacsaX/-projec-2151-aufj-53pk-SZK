using UnityEngine;
using System.Collections.Generic;

public class CalibrationComboGenerator : MonoBehaviour
{
    public string[] states = { "Prime", "Test", "Boost", "Stabilize", "Vent", "Lock" };
    public Dictionary<List<string>, int> combos = new Dictionary<List<string>, int>();

    void Start()
    {
        GenerateCombos();
        LogCombos();
    }

    public void GenerateCombos()
    {
        combos.Clear();
        List<int> possibleValues = new List<int>();
        for (int v = 0; v <= 100; v += 10)
            possibleValues.Add(v);

        // Generate 10 unique combos
        HashSet<string> usedCombos = new HashSet<string>();
        for (int i = 0; i < 10; i++)
        {
            List<string> combo = new List<string>();
            while (combo.Count < 3)
            {
                string state = states[Random.Range(0, states.Length)];
                combo.Add(state);
            }
            string comboKey = string.Join("-", combo);
            if (usedCombos.Contains(comboKey))
            {
                i--;
                continue;
            }
            usedCombos.Add(comboKey);

            int value = possibleValues[i];
            combos.Add(combo, value);
        }
    }

    public void LogCombos()
    {
        Debug.Log("Calibration Combos:");
        foreach (var kvp in combos)
        {
            Debug.Log($"{string.Join(", ", kvp.Key)} = {kvp.Value}");
        }
    }
}