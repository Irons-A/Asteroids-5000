using Core.Components;
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

        public int CurrentAmmo => _ammoManager.CurrentAmmo;
        public float ReloadProgress => _reloadingSubsystem.ReloadProgress;

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

        public void RefillAmmo()
        {
            _ammoManager.RefillAmmo();
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
        
        public void Dispose()
        {
            _shootingSubsystem.Dispose();
            _reloadingSubsystem.Dispose();
            
            _reloadingSubsystem.OnReloadCompleted -= ProcessReloadCompletion;
            _reloadingSubsystem.OnReloadStarted -= ProcessReloadStart;
            _ammoManager.OnAmmoChanged -= ProcessAmmoChange;
        }
    }
}
