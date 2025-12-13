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

namespace Player.Logic
{
    public class UniversalPlayerWeaponSystem : ITickable, IDisposable
    {
        private WeaponConfig _config;

        private PlayerShootingSubsystem _shootingSubsystem;
        private PlayerReloadingSubsystem _reloadingSubsystem;
        private AmmoManager _ammoManager;

        private bool _shouldShoot = false;
        private bool _isInitialized = false;

        [Inject]
        public void Construct(PlayerShootingSubsystem shootingSubsystem, PlayerReloadingSubsystem reloadingSubsystem,
            AmmoManager ammoManager)
        {
            _shootingSubsystem = shootingSubsystem;
            _reloadingSubsystem = reloadingSubsystem;
            _ammoManager = ammoManager;
        }

        public void Configure(WeaponConfig config)
        {
            _config = config;

            _shootingSubsystem.Configure(config, _reloadingSubsystem, _ammoManager);
            _reloadingSubsystem.Configure(config, _ammoManager);

            SetupSubsystemEvents();

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
        }

        private void SetupSubsystemEvents()
        {
            if (_shootingSubsystem is PlayerShootingSubsystem shooting)
            {
                shooting.OnShotFired += () =>
                {
                    if (_ammoManager.HasInfiniteAmmo == false)
                    {
                        _reloadingSubsystem.ProcessAutoReloading();
                    }
                };
            }

            if (_reloadingSubsystem is PlayerReloadingSubsystem reloading)
            {
                reloading.OnReloadCompleted += () =>
                {
                    if (_shouldShoot)
                    {
                        _shootingSubsystem.TryStartShooting();
                    }
                };

                reloading.OnReloadStarted += () =>
                {
                    if (reloading.ShouldBlockFire)
                    {
                        _shootingSubsystem.TryStopShooting();
                    }
                };
            }

            _ammoManager.OnAmmoChanged += (current, max) =>
            {
                if (_shouldShoot && _ammoManager.CurrentAmmo >= _config?.AmmoCostPerShot)
                {
                    _shootingSubsystem.TryStartShooting();
                }
            };
        }
    }
}
