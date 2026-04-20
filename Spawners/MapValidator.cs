using UnityEngine;
using System.Collections.Generic;
//Nincs használva, viszont fontos szerepet játszott a mapgenerálás debuggolás folyamatában
public class MapValidator : MonoBehaviour
{
    public string roomTag = "Room";
    public string wallTag = "Wall";

public float minClashVolume = 0.5f;

public void ValidateMap()
{
    GameObject[] rooms = GameObject.FindGameObjectsWithTag(roomTag);
    bool clashFound = false;

    for (int i = 0; i < rooms.Length; i++)
    {
        Collider[] roomColliders = rooms[i].GetComponentsInChildren<Collider>();
        foreach (var col in roomColliders)
        {
            if (!col.gameObject.CompareTag(wallTag)) continue;

            Collider[] hits = Physics.OverlapBox(col.bounds.center, col.bounds.extents, col.transform.rotation);
            foreach (var hit in hits)
            {
                if (hit == col) continue; // Ignore self
                if (hit.gameObject.CompareTag(wallTag))
                {
                    
                    Transform parent = hit.transform;
                    bool isOtherRoom = false;
                    while (parent != null)
                    {
                        if (parent.gameObject != rooms[i] && parent.CompareTag(roomTag))
                        {
                            isOtherRoom = true;
                            break;
                        }
                        parent = parent.parent;
                    }
                    if (isOtherRoom)
                    {
                        Bounds a = col.bounds;
                        Bounds b = hit.bounds;
                        if (a.Intersects(b))
                        {
                            Vector3 min = Vector3.Max(a.min, b.min);
                            Vector3 max = Vector3.Min(a.max, b.max);
                            Vector3 size = max - min;
                            float centerDist = Vector3.Distance(a.center, b.center);
                            float minExtent = Mathf.Min(a.extents.magnitude, b.extents.magnitude);

                            
                            if (centerDist < minExtent * 0.5f ||
                                size.x > a.size.x * 0.8f ||
                                size.y > a.size.y * 0.8f ||
                                size.z > a.size.z * 0.8f)
                            {
                                Debug.LogWarning($"GAME-BREAKING CLASH: {rooms[i].name} and {parent.name} at {col.bounds.center} (centerDist: {centerDist}, overlap: {size})");
                                clashFound = true;
                            }
                        }
                    }
                }
            }
        }
    }

    if (!clashFound)
        Debug.Log("No game-breaking room clashes detected!");
}
}