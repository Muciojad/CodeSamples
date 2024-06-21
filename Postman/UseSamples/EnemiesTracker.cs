namespace Vampire.Game.Enemy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Messages;
    using Poseidon.Postman;
    using Poseidon.Postman.Interfaces;
    using UnityEngine;

    public class EnemiesTracker : MonoBehaviour, IMessageReceiver
    {
        #region Public API
        // List of currently alive enemies
        public List<EnemyFacade> AliveEnemies { get; private set; } = new List<EnemyFacade>();

        // Finds the closest enemy to a given point within an acceptable distance
        public Transform GetClosestEnemyToPoint(Vector3 point, float acceptableDistance)
        {
            foreach (var enemyFacade in AliveEnemies)
            {
                if (Vector3.Distance(point, enemyFacade.Position) <= acceptableDistance)
                {
                    return enemyFacade.BodyTransform;
                }
            }

            return null;
        }
        #endregion

        #region Private Methods
        private void OnEnable()
        {
            // Register this object as a message receiver when enabled
            Postman.RegisterReceiver(this);
        }

        private void OnDisable()
        {
            // Unregister this object as a message receiver when disabled
            Postman.UnregisterReceiver(this);
        }

        private void OnEnemyAdded(EnemyFacade enemyFacade)
        {
            // Add a new enemy to the list if it's not already there
            if (AliveEnemies.Contains(enemyFacade))
            {
                return;
            }
            AliveEnemies.Add(enemyFacade);
        }

        private void OnEnemyRemoved(EnemyFacade enemyFacade)
        {
            // Remove an enemy from the list
            AliveEnemies.Remove(enemyFacade);
        }

        #endregion

        #region Messaging
        // List of message types this object listens to
        public List<Type> ListenedTypes => new List<Type>()
        {
            typeof(OnEnemySpawnedOnMap),
            typeof(OnEnemyDeadMessage)
        };

        // Handle received messages
        public void OnMessageReceived(object message)
        {
            switch (message)
            {
                case OnEnemySpawnedOnMap enemySpawnedOnMap:
                    // Handle enemy spawn message
                    OnEnemyAdded(enemySpawnedOnMap.SpawnedEnemy);
                    break;
                case OnEnemyDeadMessage enemyDeadMessage:
                    // Handle enemy death message
                    OnEnemyRemoved(enemyDeadMessage.Enemy);
                    break;
            }
        }

        #endregion
    }
}
