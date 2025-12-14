using System.Collections;
using System.Collections.Generic;
using Core.Physics;
using Core.Systems.ObjectPools;
using UnityEngine;

namespace Enemies
{
    [RequireComponent(typeof(PoolableObject))]
    public class EnemyPresentation : MovableObject
    {
        protected override void Awake()
        {
            base.Awake();
            _shouldTeleport = false;
        }

        protected virtual void SetAngle(float angle)
        {
            transform.rotation =  Quaternion.Euler(0, 0, angle);
        }
    }
}
