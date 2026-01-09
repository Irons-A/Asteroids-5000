using System;

namespace Player.Logic.Weapons
{
    public class UniversalPlayerWeaponSystem : IDisposable
    {
        private readonly PlayerShootingSubsystem _shootingSubsystem;
        private readonly PlayerReloadingSubsystem _reloadingSubsystem;
        private readonly PlayerAmmoSubsystem _ammoManager;
        
        private PlayerWeaponConfig _config;

        private bool _shouldShoot = false;
        private bool _isInitialized = false;

        public int CurrentAmmo => _ammoManager.CurrentAmmo;
        public float ReloadProgress => _reloadingSubsystem.ReloadProgress;
        
        public UniversalPlayerWeaponSystem(PlayerShootingSubsystem shootingSubsystem, PlayerReloadingSubsystem reloadingSubsystem,
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

        public void UpdateState()
        {
            if (_isInitialized)
            {
                _reloadingSubsystem.ProcessAutoReloading();
            }
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
