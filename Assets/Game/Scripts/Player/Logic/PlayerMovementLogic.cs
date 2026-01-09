using System;
using Core.Components;
using Core.Configuration;
using Core.Configuration.Player;
using Core.Physics;
using Player.Presentation;
using Player.UserInput;
using UnityEngine;
using Zenject;

namespace Player.Logic
{
    public class PlayerMovementLogic : IFixedTickable, IDisposable
    {
        private const float MinEngineParticlesToggleDelay = 0.1f;
        
        private readonly PlayerShipSettings _shipSettings;
        private readonly UncontrollabilityLogic _uncontrollabilityLogic;
        
        private CollisionHandler _playerCollisionHandler;
        private CustomPhysics _playerPhysics;
        private ParticleSystem _playerEngineParticles;
        private PlayerPresentation _playerPresentation;
        
        private float _lastParticleToggleTime = 0f;
        
        private bool _isConfigured = false;
        
        public bool IsUncontrollable => _uncontrollabilityLogic.IsUncontrollable;

        public PlayerMovementLogic(JsonConfigProvider configProvider, UncontrollabilityLogic uncontrollabilityLogic)
        {
            _shipSettings = configProvider.PlayerShipSettingsRef;
            _uncontrollabilityLogic = uncontrollabilityLogic;
        }

        public void Configure(CollisionHandler collisionHandler, CustomPhysics playerPhysics,
            PlayerPresentation playerPresentation)
        {
            _playerPhysics = playerPhysics;
            _playerPresentation = playerPresentation;
            
            _playerPhysics.SetMovableObject(_playerPresentation, _shipSettings.PlayerMass);
            
            _playerCollisionHandler = collisionHandler;
            _playerCollisionHandler.OnRicochetCalled += _playerPhysics.ApplyRicochet;
            
            _playerEngineParticles = _playerPresentation.EngineParticles;
            _playerEngineParticles.Stop();
            
            _uncontrollabilityLogic.Configure(_shipSettings.UncontrollabilityDuration);

            _isConfigured = true;
        }
        
        public void FixedTick()
        {
            if (_isConfigured)
            {
                _playerPhysics.ProcessPhysics();
            }
        }

        public void MovePlayer(PlayerMovementState movementState)
        {
            if (_uncontrollabilityLogic.IsUncontrollable || _isConfigured == false) return;
            
            if (movementState == PlayerMovementState.Accelerating)
            {
                _playerPhysics.ApplyAcceleration(_shipSettings.AccelerationSpeed, _shipSettings.MaxSpeed);

                if (Time.time - _lastParticleToggleTime > MinEngineParticlesToggleDelay)
                {
                    _playerEngineParticles.Play();
                    _lastParticleToggleTime = Time.time;
                }
            }
            else if (movementState == PlayerMovementState.Decelerating)
            {
                _playerPhysics.ApplyDeceleration(_shipSettings.DecelerationSpeed);
            }

            if (movementState != PlayerMovementState.Accelerating)
            {
                if (Time.time - _lastParticleToggleTime > MinEngineParticlesToggleDelay)
                {
                    ResetEngineParticlesToggleTime();
                }
            }
        }

        public void StopMovement()
        {
            if (_isConfigured == false) return;
            
            _playerPhysics.Stop();
        }

        public void StartUncontrollabilityPeriod()
        {
            if (_isConfigured == false) return;
            
            _uncontrollabilityLogic.StartUncontrollabilityPeriod();
        }

        public void StopUncontrollabilityPeriod()
        {
            if (_isConfigured == false) return;
            
            _uncontrollabilityLogic.StopUncontrollabilityPeriod();
        }

        public void ResetEngineParticlesToggleTime()
        {
            _playerEngineParticles.Stop();
            _lastParticleToggleTime = Time.time;
        }

        public void Dispose()
        {
            if (_playerCollisionHandler != null)
            {
                _playerCollisionHandler.OnRicochetCalled -= _playerPhysics.ApplyRicochet;
            }
        }
    }
}
