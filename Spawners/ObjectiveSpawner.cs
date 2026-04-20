using UnityEngine;
using System.Collections.Generic;

public class ObjectiveSpawner : MonoBehaviour
{
    [System.Serializable]
    public class ObjectivePrefab
    {
        public string objectiveName; // pl, "PressureValve", "RadiationPanel"
        public GameObject prefab;
        public string targetTypeName;    // pl, "Panel", "Valve"
    }

    public List<ObjectivePrefab> objectives; // mind a 14 objectív listája

    public void SpawnObjectives()
    {
        HashSet<string> spawnedObjectives = new HashSet<string>();

        foreach (var obj in objectives)
        {
            Debug.Log($"ObjectiveSpawner: Searching for targets of type '{obj.targetTypeName}' for objective '{obj.objectiveName}'");

            if (spawnedObjectives.Contains(obj.objectiveName))
            {
                Debug.Log($"ObjectiveSpawner: Already spawned '{obj.objectiveName}', skipping.");
                continue;
            }

            // minden target keresése név alapján
            List<GameObject> possibleTargets = new List<GameObject>();
            foreach (GameObject go in GameObject.FindObjectsOfType<GameObject>())
            {
                if (go.name.Contains(obj.targetTypeName))
                    possibleTargets.Add(go);
            }

            // Lista shuffle randomizáláshoz
            for (int i = 0; i < possibleTargets.Count; i++)
            {
                int rnd = Random.Range(i, possibleTargets.Count);
                var temp = possibleTargets[i];
                possibleTargets[i] = possibleTargets[rnd];
                possibleTargets[rnd] = temp;
            }

            bool placed = false;
            foreach (var target in possibleTargets)
            {
                if (target.transform.childCount == 0)
                {
                    GameObject instance = Instantiate(obj.prefab, target.transform);
                    instance.name = obj.objectiveName;
                    spawnedObjectives.Add(obj.objectiveName);
                    Debug.Log($"ObjectiveSpawner: Placed '{obj.objectiveName}' on '{target.name}'");
                    placed = true;
                    break;
                }
            }
            if (!placed)
            {
                Debug.LogWarning($"ObjectiveSpawner: No available spot found for '{obj.objectiveName}' of type '{obj.targetTypeName}'");
            }
        }
    }
}