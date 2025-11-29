using Core.Physics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player.Presentation
{
    public class PlayerPresentation : MovableObject
    {
        protected override void Awake()
        {
            base.Awake();
            _shouldTeleport = true;
        }
    }
}
