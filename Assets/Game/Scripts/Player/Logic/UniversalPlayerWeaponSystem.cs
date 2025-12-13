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
        private bool _wasShootingLastFrame = false;
        private bool _isInitialized = false;

        // DI через Zenject
        [Inject]
        public void Construct(PlayerShootingSubsystem shootingSubsystem, PlayerReloadingSubsystem reloadingSubsystem,
            AmmoManager ammoManager)
        {
            _shootingSubsystem = shootingSubsystem;
            _reloadingSubsystem = reloadingSubsystem;
            _ammoManager = ammoManager;
        }

        private void SetupSubsystemEvents()
        {
            // При выстреле проверяем автоперезарядку
            if (_shootingSubsystem is PlayerShootingSubsystem shooting)
            {
                shooting.OnShotFired += () =>
                {
                    if (!_ammoManager.HasInfiniteAmmo)
                    {
                        _reloadingSubsystem.ProcessAutoReloading();
                    }
                };
            }

            // При завершении перезарядки пытаемся возобновить стрельбу
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
                    // Если перезарядка блокирует стрельбу и мы стреляем - останавливаем
                    if (reloading.ShouldBlockFire)
                    {
                        _shootingSubsystem.TryStopShooting();
                    }
                };
            }

            // При изменении количества патронов пытаемся возобновить стрельбу
            _ammoManager.OnAmmoChanged += (current, max) =>
            {
                if (_shouldShoot && _ammoManager.CurrentAmmo >= _config?.AmmoCostPerShot)
                {
                    _shootingSubsystem.TryStartShooting();
                }
            };
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
            // Обработка начала/остановки стрельбы при изменении состояния кнопки
            if (_shouldShoot != _wasShootingLastFrame)
            {
                if (_shouldShoot)
                {
                    _shootingSubsystem.TryStartShooting();
                }
                else
                {
                    _shootingSubsystem.TryStopShooting();
                }

                _wasShootingLastFrame = _shouldShoot;
            }

            // Проверка автоперезарядки
            _reloadingSubsystem.ProcessAutoReloading();

            // Тик подсистем
            _shootingSubsystem.Tick();
            _reloadingSubsystem.Tick();
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
    }
}
