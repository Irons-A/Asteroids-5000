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
        [SerializeField] public Transform[] BulletFirepoints;

        protected override void Awake()
        {
            base.Awake();
            _shouldTeleport = true;
        }
    }
}
