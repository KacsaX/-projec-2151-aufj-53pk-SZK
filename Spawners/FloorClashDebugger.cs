using UnityEngine;
using System.Collections.Generic;

public class FloorClashDebugger : MonoBehaviour
{
    public float seamThreshold = 0.01f;
// Nincs használatban, viszont fontos szerepet játszott a mapgenerálás debuggolás folyamatában
    void Update()
    {
        // minden "Floor" nevű objektum gyűjtése
        List<Collider> floorColliders = new List<Collider>();
        foreach (var c in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))
        {
            if (c.gameObject.name.StartsWith("Floor"))
                floorColliders.Add(c);
        }

        // Ellenőrizze az ütközéseket minden pár között
        for (int i = 0; i < floorColliders.Count; i++)
        {
            for (int j = i + 1; j < floorColliders.Count; j++)
            {
                var colA = floorColliders[i];
                var colB = floorColliders[j];

                if (colA.bounds.Intersects(colB.bounds))
                {
                    float clashDistance = Vector3.Distance(colA.bounds.center, colB.bounds.center);
                    if (clashDistance > seamThreshold)
                    {
                        Debug.Log($"[FloorClashDebugger] Clash: {colA.gameObject.name} ({colA.bounds.center}) with {colB.gameObject.name} ({colB.bounds.center}) | Distance: {clashDistance}f");
                    }
                }
            }
        }
    }
}