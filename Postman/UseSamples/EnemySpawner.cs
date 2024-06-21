namespace Vampire.Game.Systems.WaveSystem.EnemySpawner
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Enemy;
    using Enemy.Messages;
    using Poseidon.Postman;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using Random = UnityEngine.Random;

    public class EnemySpawner : MonoBehaviour
    {
        #region Inspector
        [SerializeField] private List<Transform> spawnPoints; // List of points where enemies can spawn
        private Dictionary<int, Queue<EnemyFacade>> waveEnemiesQueueMapping; // Mapping of wave index to queue of enemy instances
        #endregion
    
        #region Public API
    
        // Check if the spawner is empty for a specific wave
        public bool SpawnerEmpty(int waveIndex)
        {
            return waveEnemiesQueueMapping[waveIndex].Count == 0;
        }
    
        // Initialize the spawner with enemy templates for each wave
        public Task InitializeSpawner(Dictionary<int, List<EnemyFacade>> waveEnemiesTemplatesMapping)
        {
            waveEnemiesQueueMapping = new Dictionary<int, Queue<EnemyFacade>>();
            foreach (var waveEnemiesData in waveEnemiesTemplatesMapping)
            {
                waveEnemiesQueueMapping.Add(waveEnemiesData.Key, new Queue<EnemyFacade>());
                foreach (var templateEnemy in waveEnemiesData.Value)
                {
                    // Select a random spawn point and instantiate the enemy
                    var randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
                    var enemyInstance = Instantiate(templateEnemy);
                    enemyInstance.transform.parent = randomSpawnPoint;
                    enemyInstance.transform.localPosition = Vector3.zero;
                    enemyInstance.transform.localRotation = Quaternion.identity;
    
                    // Deactivate enemy and enqueue it
                    enemyInstance.gameObject.SetActive(false);
                    waveEnemiesQueueMapping[waveEnemiesData.Key].Enqueue(enemyInstance);
                }
            }
            return Task.CompletedTask;
        }
    
        // Spawn a specified number of enemies for a given wave
        public List<EnemyFacade> SpawnEnemies(int waveIndex, int count)
        {
            var spawnedEnemies = new List<EnemyFacade>();
            for (var index = 0; index < count; index++)
            {
                if (waveEnemiesQueueMapping[waveIndex].Count == 0)
                {
                    return new List<EnemyFacade>();
                }
    
                // Dequeue an enemy, activate it, initialize it, and add to the spawned list
                var enemy = waveEnemiesQueueMapping[waveIndex].Dequeue();
                enemy.gameObject.SetActive(true);
                enemy.InitializeOnSpawn();
                spawnedEnemies.Add(enemy);
    
                // Notify about the spawned enemy
                Postman.Send(new OnEnemySpawnedOnMap(enemy));
            }
    
            return spawnedEnemies;
        }
    
        // Dispose of all remaining enemies and clear the spawner
        public void Dispose()
        {
            foreach (var waveData in waveEnemiesQueueMapping)
            {
                while (waveData.Value.Count > 0)
                {
                    var enemyLeft = waveData.Value.Dequeue();
                    Destroy(enemyLeft.gameObject);
                }
            }
            waveEnemiesQueueMapping.Clear();
        }
        #endregion
    
        #region Utils
    
        // Validate the spawn points in the inspector
        private void OnValidate()
        {
            if (spawnPoints == null || spawnPoints.Count == 0)
            {
                GetSpawnPoints();
            }
        }
    
        // Retrieve and set all child transforms as spawn points
        [Button]
        private void GetSpawnPoints()
        {
            spawnPoints = GetComponentsInChildren<Transform>().ToList();
            spawnPoints.Remove(transform); // Exclude the transform of the spawner itself
        }
    
        #endregion
    }
}