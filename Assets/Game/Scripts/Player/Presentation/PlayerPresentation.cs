using Core.Physics;
using Player.Logic;
using Player.UserInput;
using UnityEngine;
using Zenject;

namespace Player.Presentation
{
    public class PlayerPresentation : MovableObject, IInitializable
    {
        [SerializeField] private Transform[] _bulletFirepoints;
        [SerializeField] private Transform[] _laserFirepoints;
        [SerializeField] private ParticleSystem _engineParticles;
        
        private PlayerLogic _playerLogic;
        private InputDetector _inputDetector;
        
        public Transform[] BulletFirepoints => _bulletFirepoints;
        public Transform[] LaserFirepoints => _laserFirepoints;
        public ParticleSystem EngineParticles => _engineParticles;

        [Inject]
        private void Construct(PlayerLogic playerLogic, InputDetector inputDetector)
        {
            _playerLogic = playerLogic;
            
            _inputDetector = inputDetector;
        }
        
        private void Awake()
        {
            _shouldTeleport = true;
        }

        public void Initialize()
        {
            _playerLogic.Configure(this);
            _inputDetector.SetPlayerLogic(_playerLogic);
            _inputDetector.SetCamera();
        }
    }
}
