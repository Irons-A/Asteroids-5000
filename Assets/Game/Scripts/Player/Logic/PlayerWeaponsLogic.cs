using System.Collections;
using System.Collections.Generic;
using Core.Configuration;
using Core.Configuration.Player;
using Core.Systems;
using Core.Systems.ObjectPools;
using Player.Logic.Weapons;
using Player.Presentation;
using UnityEngine;
using Zenject;

namespace Player.Logic
{
    public class PlayerWeaponsLogic : ITickable
    {
        private readonly PlayerWeaponsSettings _weaponsSettings;
        private readonly UniversalPlayerWeaponSystem _bulletWeaponSystem;
        private readonly PlayerWeaponConfig _bulletWeaponConfig;
        private readonly UniversalPlayerWeaponSystem _laserWeaponSystem;
        private readonly PlayerWeaponConfig _laserWeaponConfig;
        private readonly PlayerMovementLogic _movementLogic;
        
        private PlayerPresentation _playerPresentation;
        
        private bool _isConfigured = false;

        public int CurrentLaserAmmo => _laserWeaponSystem.CurrentAmmo;
        public float CurrentLaserCooldownProgress => _laserWeaponSystem.ReloadProgress;

        public PlayerWeaponsLogic(JsonConfigProvider configProvider, UniversalPlayerWeaponSystem bulletWeaponSystem,
            PlayerWeaponConfig bulletWeaponConfig, UniversalPlayerWeaponSystem laserWeaponSystem,
            PlayerWeaponConfig laserWeaponConfig, PlayerMovementLogic movementLogic)
        {
            _weaponsSettings = configProvider.PlayerWeaponsSettingsRef;
            _bulletWeaponSystem = bulletWeaponSystem;
            _bulletWeaponConfig = bulletWeaponConfig;
            _laserWeaponSystem = laserWeaponSystem;
            _laserWeaponConfig = laserWeaponConfig;
            _movementLogic = movementLogic;
        }

        public void Configure(PlayerPresentation playerPresentation)
        {
            _playerPresentation = playerPresentation;
            
            ConfigureBulletWeaponSystem();
            ConfigureLaserWeaponSystem();
            
            _isConfigured =  true;
        }

        public void Tick()
        {
            if (_isConfigured)
            {
                _bulletWeaponSystem.UpdateState();
                _laserWeaponSystem.UpdateState();
            }
        }
        
        public void ShootBullets(bool value)
        {
            if (_movementLogic.IsUncontrollable || _playerPresentation.isActiveAndEnabled == false 
                                                || _isConfigured == false)
            {
                value = false;
            }
            
            _bulletWeaponSystem.SetShouldShoot(value);
        }

        public void ShootLaser(bool value)
        {
            if (_movementLogic.IsUncontrollable || _playerPresentation.isActiveAndEnabled == false 
                                                || _isConfigured == false)
            {
                value = false;
            }
            
            _laserWeaponSystem.SetShouldShoot(value);
        }

        public void RefillAmmo()
        {
            if (_isConfigured == false) return;
            
            _bulletWeaponSystem.RefillAmmo();
            _laserWeaponSystem.RefillAmmo();
        }
        
        private void ConfigureBulletWeaponSystem()
        {
            _bulletWeaponConfig.Configure(
                projectileType: PoolableObjectType.PlayerBullet,
                firepoints: _playerPresentation.BulletFirepoints,
                projectileSpeed: _weaponsSettings.BulletSpeed,
                projectileDelayedDestruction: false,
                destroyProjectileAfter: 0,
                projectileDamage: _weaponsSettings.BulletDamage,
                projectileAffiliation: EntityAffiliation.Ally,
                projectileDurability: EntityDurability.Fragile,
                shouldSetFirepointAsProjectileParent: false,
                fireRateInterval: _weaponsSettings.BulletFireRateInterval,
                maxAmmo: 0,
                ammoCostPerShot: 0,
                hasInfiniteAmmo: true,
                reloadLength: 0,
                ammoPerReload: 0,
                shouldAutoReloadOnLessThanMaxAmmo: false,
                shouldAutoReloadOnNoAmmo: false,
                shouldDepleteAmmoOnReload: false,
                shouldBlockFireWhileReload: false);

            _bulletWeaponSystem.Configure(_bulletWeaponConfig);
        }

        private void ConfigureLaserWeaponSystem()
        {
            _laserWeaponConfig.Configure(
                projectileType: PoolableObjectType.PlayerLaser,
                firepoints: _playerPresentation.LaserFirepoints,
                projectileSpeed: 0,
                projectileDelayedDestruction: true,
                destroyProjectileAfter: _weaponsSettings.LaserDuration,
                projectileDamage: _weaponsSettings.LaserDamage,
                projectileAffiliation: EntityAffiliation.Ally,
                projectileDurability: EntityDurability.Undestructable,
                shouldSetFirepointAsProjectileParent: true,
                fireRateInterval: _weaponsSettings.LaserFireRateInterval,
                maxAmmo: _weaponsSettings.MaxLaserCharges,
                ammoCostPerShot: _weaponsSettings.LaserAmmoPerShot,
                hasInfiniteAmmo: false,
                reloadLength: _weaponsSettings.LaserCooldown,
                ammoPerReload: _weaponsSettings.LaserAmmoPerReload,
                shouldAutoReloadOnLessThanMaxAmmo: true,
                shouldAutoReloadOnNoAmmo: true,
                shouldDepleteAmmoOnReload: false,
                shouldBlockFireWhileReload: false);

            _laserWeaponSystem.Configure(_laserWeaponConfig);
        }
    }
}