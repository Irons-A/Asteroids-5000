using System.Collections;
using System.Collections.Generic;
using Core.Components;
using Core.Physics;
using Core.Systems.ObjectPools;
using UnityEngine;

namespace Enemies
{
    [RequireComponent(typeof(PoolableObject))]
    [RequireComponent(typeof(CollisionHandler))]
    public class EnemyPresentation : MovableObject
    {
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
            
            transform.rotation =  Quaternion.Euler(0, 0, angle);
        }
    }
}
