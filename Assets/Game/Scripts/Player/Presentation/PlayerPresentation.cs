using Core.Physics;
using Core.Systems.ObjectPools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Player.Presentation
{
    public class PlayerPresentation : MovableObject
    {
        [field: SerializeField] public Transform[] BulletFirepoints { get; private set; }
        [field: SerializeField] public Transform[] LaserFirepoints { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            _shouldTeleport = true;
        }
    }
}
