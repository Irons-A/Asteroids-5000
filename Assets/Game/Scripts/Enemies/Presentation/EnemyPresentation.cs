using System;
using System.Collections;
using System.Collections.Generic;
using Core.Components;
using Core.Physics;
using Core.Systems.ObjectPools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemies
{
    [RequireComponent(typeof(PoolableObject))]
    public class EnemyPresentation : MovableObject
    {
        protected PoolableObject _poolableObject;
        protected CollisionHandler _collisionHandler;
        
        public event Action OnAngleUpdated;
        
        protected virtual void Awake()
        {
            _shouldTeleport = false;
        }

        public virtual void SetAngle(float angle, bool shouldRandomize = false)
        {
            float angleToSet = angle;
            
            if (shouldRandomize)
            {
                angleToSet = Random.Range(0, 360);
            }
            
            transform.rotation =  Quaternion.Euler(0, 0, angleToSet);
            
            OnAngleUpdated?.Invoke();
        }

        protected virtual void OnDestroy()
        {
            OnAngleUpdated = null;
        }
    }
}
