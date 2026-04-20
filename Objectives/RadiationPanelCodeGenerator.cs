using UnityEngine;
using System.Collections.Generic;

public class RadiationPanelCodeGenerator : MonoBehaviour
{
    private string[] buttonLabels = new string[] { "a", "b", "c", "d", "e", "f" };
    private int[] numberLabels = new int[] { 1, 2, 3, 4, 5, 6 };
    private int[] possibleValues = new int[] { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };

    // raktározza a generált kódot: kulcs = "d2", érték = 10
    public Dictionary<string, int> codeCombinations = new Dictionary<string, int>();

    void Start()
    {
        GenerateCode();
        LogCode();
    }

    public void GenerateCode()
    {
        codeCombinations.Clear();
        List<string> allCombos = new List<string>();

        // generál minden lehetséges gomb-szám kombinációt
        foreach (string btn in buttonLabels)
            foreach (int num in numberLabels)
                allCombos.Add($"{btn}{num}");

        // keveri a kombinációkat és kiválaszt 11 egyedit
        for (int i = 0; i < 11; i++)
        {
            int comboIdx = Random.Range(0, allCombos.Count);
            string combo = allCombos[comboIdx];
            allCombos.RemoveAt(comboIdx);

            int value = possibleValues[i];

            codeCombinations.Add(combo, value);
        }
    }

    public void LogCode()
    {
        Debug.Log("Generated Radiation Panel Code:");
        foreach (var kvp in codeCombinations)
        {
            Debug.Log($"{kvp.Key} = {kvp.Value}");
        }
    }
}