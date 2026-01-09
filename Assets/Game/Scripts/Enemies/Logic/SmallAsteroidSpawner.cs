using Core.Systems.ObjectPools;
using Enemies.Signals;
using UnityEngine;
using Zenject;

namespace Enemies.Logic
{
    public class SmallAsteroidSpawner
    {
        private readonly PoolAccessProvider _objectPool;
        private readonly SignalBus _signalBus;
        
        private int _minSmallAsteroidSpawnAmount;
        private int _maxSmallAsteroidSpawnAmount;

        public SmallAsteroidSpawner(PoolAccessProvider objectPool, SignalBus signalBus)
        {
            _objectPool = objectPool;
            _signalBus = signalBus;
        }
        
        public void SpawnSmallAsteroids(int minSpawnAmount, int maxSpawnAmount, Vector3 spawnPosition)
        {
            int asteroidsToSpawn = Random.Range(minSpawnAmount, maxSpawnAmount + 1);
            
            for (int i = 0; i < asteroidsToSpawn; i++)
            {
                PoolableObject smallAsteroid = _objectPool.GetFromPool(PoolableObjectType.SmallAsteroid);
                
                smallAsteroid.transform.position = spawnPosition;
                
                _signalBus.TryFire(new EnemySpawnedSignal());
            }
        }
    }
}
