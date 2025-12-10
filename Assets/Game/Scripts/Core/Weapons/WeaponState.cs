using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Weapons
{
    [Flags]
    public enum WeaponState
    {
        Idle,
        Shooting,
        Reloading
    }
}
