using UnityEngine;
using UnityEngine.AI;

public class MonsterSpawner : MonoBehaviour
{
    public GameObject monsterPrefab;
    public Transform mapRoot;

    public void SpawnMonster()
    {
        Transform player = FindPlayerTransform();
        if (player == null)
        {
            Debug.LogWarning("MonsterSpawner: Player not found in scene.");
            return;
        }

        Vector3 spawnPos = GetFurthestNavMeshPoint(player.position);
        if (spawnPos != Vector3.zero)
        {
            Instantiate(monsterPrefab, spawnPos, Quaternion.identity, mapRoot);
        }
        else
        {
            Debug.LogWarning("MonsterSpawner: Could not find a valid NavMesh position to spawn monster far from player.");
        }
    }

    Vector3 GetFurthestNavMeshPoint(Vector3 playerPos)
    {
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
        Vector3 furthest = Vector3.zero;
        float maxDist = 0f;

        foreach (var vertex in triangulation.vertices)
        {
            float dist = Vector3.Distance(playerPos, vertex);
            if (dist > maxDist)
            {
                NavMeshHit hit;
                
                if (NavMesh.SamplePosition(vertex, out hit, 1f, NavMesh.AllAreas))
                {
                    maxDist = dist;
                    furthest = hit.position;
                }
            }
        }

        return furthest;
    }

    Transform FindPlayerTransform()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        return playerObj != null ? playerObj.transform : null;
    }
}