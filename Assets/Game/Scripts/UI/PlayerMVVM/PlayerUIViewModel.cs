using UniRx;
using UnityEngine;

namespace UI.PlayerMVVM
{
    public class PlayerUIViewModel
    {
        private readonly PlayerUIModel _model;
        
        public IReadOnlyReactiveProperty<Vector2> Coordinates => _model.Coordinates;
        public IReadOnlyReactiveProperty<float> PlayerAngle => _model.PlayerAngle;
        public IReadOnlyReactiveProperty<int> LaserAmmo => _model.LaserAmmo;
        public IReadOnlyReactiveProperty<float> LaserCooldown => _model.LaserCooldown;
        public IReadOnlyReactiveProperty<float> CurrentSpeed => _model.CurrentSpeed;
        public IReadOnlyReactiveProperty<int> Health => _model.Health;
        public IReadOnlyReactiveProperty<int> Score => _model.Score;
        
        public PlayerUIViewModel(PlayerUIModel model)
        {
            _model = model;
        }
    }
}
