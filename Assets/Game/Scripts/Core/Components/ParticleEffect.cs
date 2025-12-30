using System;
using System.Collections;
using System.Collections.Generic;
using Core.Systems.ObjectPools;
using UnityEngine;

namespace Core.Components
{
    [RequireComponent(typeof(ParticleSystem))]
    [RequireComponent(typeof(PoolableObject))]
    public class ParticleEffect : MonoBehaviour
    {
        private PoolableObject _poolableObject;

        private void Awake()
        {
            _poolableObject = GetComponent<PoolableObject>();
        }

        private void OnParticleSystemStopped()
        {
            _poolableObject.Despawn();
        }
    }
}