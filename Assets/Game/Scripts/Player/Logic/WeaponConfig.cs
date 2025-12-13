using Core.Projectiles;
using Core.Systems.ObjectPools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player.Logic
{
    public class WeaponConfig
    {
        public PoolableObjectType ProjectileType { get; private set; }
        public Transform[] FirePoints { get; private set; }
        public float ProjectileSpeed { get; private set; }
        public bool ProjectileDelayedDestruction { get; private set; }
        public float DestroyProjectileAfter { get; private set; }
        public int ProjectileDamage { get; private set; }
        public DamagerAffiliation ProjectileAffiliation { get; private set; }
        public DamagerDurability ProjectileDurability { get; private set; }
        public bool ShouldSetFirepointAsProjectileParent { get; private set; }
        public float FireRateInterval { get; private set; }
        public int MaxAmmo { get; private set; }
        public int AmmoCostPerShot { get; private set; }
        public bool HasInfiniteAmmo { get; private set; }
        public float ReloadLength { get; private set; }
        public int AmmoPerReload { get; private set; }
        public bool ShouldAutoReloadOnLessThanMaxAmmo { get; private set; }
        public bool ShouldAutoReloadOnNoAmmo { get; private set; }
        public bool ShouldDepleteAmmoOnReload { get; private set; }
        public bool ShouldBlockFireWhileReload { get; private set; }

        public void Configure(PoolableObjectType projectileType, Transform[] firepoints, float projectileSpeed,
            bool projectileDelayedDestruction, float destroyProjectileAfter, int projectileDamage,
            DamagerAffiliation projectileAffiliation, DamagerDurability projectileDurability,
            bool shouldSetFirepointAsProjectileParent, float fireRateInterval, int maxAmmo, int ammoCostPerShot,
            bool hasInfiniteAmmo, float reloadLength, int ammoPerReload, bool shouldAutoReloadOnLessThanMaxAmmo,
            bool shouldAutoReloadOnNoAmmo, bool shouldDepleteAmmoOnReload, bool shouldBlockFireWhileReaload)
        {
            ProjectileType = projectileType;
            FirePoints = firepoints;
            ProjectileSpeed = projectileSpeed;
            ProjectileDelayedDestruction = projectileDelayedDestruction;
            DestroyProjectileAfter = destroyProjectileAfter;
            ProjectileDamage = projectileDamage;
            ProjectileAffiliation = projectileAffiliation;
            ProjectileDurability = projectileDurability;
            ShouldSetFirepointAsProjectileParent = shouldSetFirepointAsProjectileParent;
            FireRateInterval = fireRateInterval;
            MaxAmmo = maxAmmo;
            AmmoCostPerShot = ammoCostPerShot;
            HasInfiniteAmmo = hasInfiniteAmmo;
            ReloadLength = reloadLength;
            AmmoPerReload = ammoPerReload;
            ShouldAutoReloadOnLessThanMaxAmmo = shouldAutoReloadOnLessThanMaxAmmo;
            ShouldAutoReloadOnNoAmmo = shouldAutoReloadOnNoAmmo;
            ShouldDepleteAmmoOnReload = shouldDepleteAmmoOnReload;
            ShouldBlockFireWhileReload = shouldBlockFireWhileReaload;
        }
    }
}
