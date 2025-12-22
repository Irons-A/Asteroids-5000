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
        protected PoolableObject PoolableObject;
        protected CollisionHandler CollisionHandler;
        private Transform _targetTransform;

        public event Action<Transform> OnTargetTransformChanged;
        
        
        protected virtual void Awake()
        {
            _shouldTeleport = false;
        }

        public void SetTargetTransform(Transform targetTransform)
        {
            _targetTransform = targetTransform;
            OnTargetTransformChanged?.Invoke(_targetTransform);
        }

        public void SetAngle(float angle, bool shouldRandomize = false, bool setAngleToPlayer = false)
        {
            float angleToSet = angle;

            if (setAngleToPlayer && _targetTransform != null)
            {
                Vector3 direction = _targetTransform.position - transform.position;
                angleToSet = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            }
            
            if (shouldRandomize)
            {
                angleToSet = Random.Range(0, 360);
            }
            
            transform.rotation =  Quaternion.Euler(0, 0, angleToSet);
        }

        protected void OnDestroy()
        {
            OnTargetTransformChanged = null;
        }
    }
}
