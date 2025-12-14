using System.Collections;
using System.Collections.Generic;
using Core.Physics;
using UnityEngine;

namespace Enemies
{
    public class EnemyPresentation : MovableObject
    {
        protected override void Awake()
        {
            base.Awake();
            _shouldTeleport = false;
        }
    }
}
