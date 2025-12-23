using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;

namespace UI
{
    public class PlayerUIView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _coordinatesText;
        [SerializeField] private TMP_Text _angleText;
        [SerializeField] private TMP_Text _immediateSpeedText;
        [SerializeField] private TMP_Text _laserAmmoText;
        [SerializeField] private TMP_Text _laserCooldownText;
        
        [Header("Health")]
        //[SerializeField] private Image[] _hearts;
        [SerializeField] private GameObject _heartPrefab;
        [SerializeField] private Transform _healthContainer;
        
        private PlayerUIViewModel _viewModel;
        private CompositeDisposable _disposables = new CompositeDisposable();

        [Inject]
        public void Init(PlayerUIViewModel viewModel)
        {
            _viewModel = viewModel;
            _viewModel.Coordinates.Subscribe(SetCoordinates).AddTo(_disposables);
            _viewModel.ShipAngle.Subscribe(SetShipAngle).AddTo(_disposables);
            _viewModel.ImmediateSpeed.Subscribe(SetImmediateSpeed).AddTo(_disposables);
            _viewModel.LaserCooldown.Subscribe(SetLaserCooldown).AddTo(_disposables);
            _viewModel.LaserAmmo.Subscribe(SetAmmo).AddTo(_disposables);
            //_viewModel.Health.Subscribe(SetHealthView).AddTo(_disposables);
            //InitHealth(viewModel);
        }

        // private void InitHealth(PlayerUIViewModel viewModel)
        // {
        //     var hearts = new Image[viewModel.Health.Value];
        //     for (var i = 0; i < viewModel.Health.Value; i++)
        //     {
        //         var heart = Instantiate(_heartPrefab, _healthContainer);
        //         hearts[i] = heart.GetComponent<Image>();;
        //     }
        //     _hearts = hearts;
        // }
        //
        //
        // private void SetHealthView(int health)
        // {
        //     for (var i = 0; i < _hearts.Length; i++)
        //     {
        //         _hearts[i].gameObject.SetActive(i < health);
        //     }
        // }
        
        private void SetLaserCooldown(float cooldown)
        {
            _laserCooldownText.text = $"Время восстановления заряда лазера : {cooldown} ";
        }
        private void SetImmediateSpeed(int speed)
        {
            _immediateSpeedText.text = $"Мнгновенная скорость : {speed}";
        }
        private void SetAmmo(int ammo)
        {
            _laserAmmoText.text = $"Осталось {ammo} зарядов лазера";
        }
        private void SetShipAngle(float angle)
        {
            _angleText.text = $"Поворот корябля : {angle:F1} ";
        }
        private void SetCoordinates(Vector2 pos)
        {
            _coordinatesText.text = $"Координаты: X: {pos.x:F1}, Y: {pos.y:F1}";
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
    }
}
