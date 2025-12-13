using Core.Components;
using Core.Projectiles;
using Core.Systems.ObjectPools;
using Core.Weapons;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Zenject;

namespace Player.Logic.Weapons
{
    public class UniversalPlayerWeaponSystem : ITickable, IDisposable
    {
        private PlayerWeaponConfig _config;

        private PlayerShootingSubsystem _shootingSubsystem;
        private PlayerReloadingSubsystem _reloadingSubsystem;
        private PlayerAmmoSubsystem _ammoManager;

        private bool _shouldShoot = false;
        private bool _isInitialized = false;

        [Inject]
        public void Construct(PlayerShootingSubsystem shootingSubsystem, PlayerReloadingSubsystem reloadingSubsystem,
            PlayerAmmoSubsystem ammoManager)
        {
            _shootingSubsystem = shootingSubsystem;
            _reloadingSubsystem = reloadingSubsystem;
            _ammoManager = ammoManager;
        }

        public void Configure(PlayerWeaponConfig config)
        {
            _config = config;

            _shootingSubsystem.Configure(config, _reloadingSubsystem, _ammoManager);
            _reloadingSubsystem.Configure(config, _ammoManager);

            _shootingSubsystem.OnShotFired += ProcessFiring;
            _reloadingSubsystem.OnReloadCompleted += ProcessReloadCompletion;
            _reloadingSubsystem.OnReloadStarted += ProcessReloadStart;
            _ammoManager.OnAmmoChanged += ProcessAmmoChange;

            _isInitialized = true;
        }

        public void Tick()
        {
            if (_isInitialized == false) return;

            _reloadingSubsystem.ProcessAutoReloading();
        }

        public void SetShouldShoot(bool value)
        {
            if (_isInitialized == false) return;

            if (value == _shouldShoot) return;

            _shouldShoot = value;

            if (_shouldShoot)
            {
                _shootingSubsystem.TryStartShooting();
            }
            else
            {
                _shootingSubsystem.TryStopShooting();
            }
        }

        public void StartReload()
        {
            _reloadingSubsystem.StartReload();
        }

        public void CancelReload()
        {
            _reloadingSubsystem.CancelReload();
        }

        public void DebugInfo()
        {
            Debug.Log($"Ammo: {_ammoManager.CurrentAmmo}/{_ammoManager.MaxAmmo}, " +
                      $"Infinite: {_ammoManager.HasInfiniteAmmo}, " +
                      $"ShouldShoot: {_shouldShoot}, " +
                      $"IsReloading: {_reloadingSubsystem.IsReloading}, " +
                      $"CanShoot: {_shootingSubsystem.CanShoot}");
        }

        public void Dispose()
        {
            _shootingSubsystem.Dispose();
            _reloadingSubsystem.Dispose();

            _shootingSubsystem.OnShotFired -= ProcessFiring;
            _reloadingSubsystem.OnReloadCompleted -= ProcessReloadCompletion;
            _reloadingSubsystem.OnReloadStarted -= ProcessReloadStart;
            _ammoManager.OnAmmoChanged -= ProcessAmmoChange;
        }

        private void ProcessFiring()
        {
            if (_ammoManager.HasInfiniteAmmo == false)
            {
                _reloadingSubsystem.ProcessAutoReloading();
            }
        }

        private void ProcessReloadCompletion()
        {
            if (_shouldShoot)
            {
                _shootingSubsystem.TryStartShooting();
            }
        }

        private void ProcessReloadStart()
        {
            if (_reloadingSubsystem.ShouldBlockFire)
            {
                _shootingSubsystem.TryStopShooting();
            }
        }

        private void ProcessAmmoChange()
        {
            if (_shouldShoot && _ammoManager.CurrentAmmo >= _config.AmmoCostPerShot)
            {
                _shootingSubsystem.TryStartShooting();
            }
        }
    }
}
