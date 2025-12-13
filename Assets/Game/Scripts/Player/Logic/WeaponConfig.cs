using Core.Projectiles;
using Core.Systems.ObjectPools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player.Logic
{
    public class WeaponConfig
    {
        public PoolableObjectType ProjectileType { get; set; }
        public Transform[] FirePoints { get; set; }
        public float ProjectileSpeed { get; set; }
        public bool ProjectileDelayedDestruction { get; set; }
        public float DestroyProjectileAfter { get; set; }
        public int ProjectileDamage { get; set; }
        public DamagerAffiliation ProjectileAffiliation { get; set; }
        public DamagerDurability ProjectileDurability { get; set; }
        public bool ShouldSetFirepointAsProjectileParent { get; set; }
        public float FireRateInterval { get; set; }
        public int MaxAmmo { get; set; }
        public int AmmoCostPerShot { get; set; }
        public bool HasInfiniteAmmo { get; set; }
        public float ReloadLength { get; set; }
        public int AmmoPerReload { get; set; }
        public bool ShouldAutoReloadOnLessThanMaxAmmo { get; set; }
        public bool ShouldAutoReloadOnNoAmmo { get; set; }
        public bool ShouldDepleteAmmoOnReload { get; set; }
        public bool ShouldBlockFireWhileReload { get; set; }
    }
}
