using UnityEngine;

public class BattleSpawner : MonoBehaviour
{
    [Header("Settings")]
    public UnitBrain unitPrefab; 
    public int redSoldierCount = 10;
    public int blueSoldierCount = 10;
    
    [Header("Spawn Areas")]
    public Transform redSpawnPoint; 
    public Transform blueSpawnPoint; 
    [Tooltip("Radio de búsqueda para encontrar suelo válido")]
    public float spawnRadius = 5f;

    void Start()
    {
        if (PathFindingManager.instance != null && PathFindingManager.instance.grid.Nodes != null)
        {
            SpawnTeams();
        }
        else
        {
            Invoke(nameof(SpawnTeams), 0.1f);
        }
    }

    void SpawnTeams()
    {
        SpawnTeam(BattleTeams.Red, redSoldierCount, redSpawnPoint.position);
        SpawnTeam(BattleTeams.Blue, blueSoldierCount, blueSpawnPoint.position);
    }

    void SpawnTeam(BattleTeams team, int count, Vector3 origin)
    {
        Vector3 leaderPos = GetValidSpawnPosition(origin);
        UnitBrain leader = Instantiate(unitPrefab, leaderPos, Quaternion.identity);
        leader.name = $"{team}_Leader";
        leader.teamID = team;
        leader.isLeader = true;
        
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = GetValidSpawnPosition(origin);
            
            UnitBrain soldier = Instantiate(unitPrefab, spawnPos, Quaternion.identity);
            soldier.name = $"{team}_Soldier_{i}";
            soldier.teamID = team;
            soldier.isLeader = false;
            soldier.myLeader = leader; 
        }
    }
    
    Vector3 GetValidSpawnPosition(Vector3 center)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 randomPoint = Random.insideUnitCircle * spawnRadius;
            Vector3 attemptPos = center + new Vector3(randomPoint.x, 0, randomPoint.y);
            
            if (PathFindingManager.instance != null)
            {
                PFNode nearestNode = PathFindingManager.instance.Closest(attemptPos);
                
                if (nearestNode != null && !nearestNode.isBlocked)
                {
                    return nearestNode.transform.position + Vector3.up * 0.5f;
                }
            }
        }
        return center;
    }
}