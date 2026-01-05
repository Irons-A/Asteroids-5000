using Core.Physics;
using Player.Logic;
using Player.UserInput;
using UnityEngine;
using Zenject;

namespace Player.Presentation
{
    public class PlayerPresentation : MovableObject, IInitializable
    {
        private PlayerLogic _playerLogic;
        private InputDetector _inputDetector;
        
        [field: SerializeField] public Transform[] BulletFirepoints { get; private set; }
        [field: SerializeField] public Transform[] LaserFirepoints { get; private set; }
        [field: SerializeField] public ParticleSystem EngineParticles { get; private set; }

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
