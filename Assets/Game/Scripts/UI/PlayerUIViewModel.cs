using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace UI
{
    public class PlayerUIViewModel
    {
        public IReadOnlyReactiveProperty<Vector2> Coordinates => _model.Coordinates;
        public IReadOnlyReactiveProperty<float> ShipAngle => _model.ShipAngle;
        public IReadOnlyReactiveProperty<int> LaserAmmo => _model.LaserAmmo;
        public IReadOnlyReactiveProperty<float> LaserCooldown => _model.LaserCooldown;
        public IReadOnlyReactiveProperty<int> ImmediateSpeed => _model.ImmediateSpeed;
        public IReadOnlyReactiveProperty<int> Health => _model.Health;
        
        private readonly PlayerUIModel _model;

        public PlayerUIViewModel(PlayerUIModel model)
        {
            _model = model;
        }
    }
}
