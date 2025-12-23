using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class PlayerUIView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _coordinatesText;
        [SerializeField] private TMP_Text _angleText;
        [SerializeField] private Image _angleIndicator;
        [SerializeField] private TMP_Text _speedText;
        [SerializeField] private TMP_Text _laserAmmoText;
        [SerializeField] private Image _laserCooldownIndicator;

        [Header("Health")] 
        [SerializeField] private Image[] _hearts;
        [SerializeField] private GameObject _heartPrefab;
        [SerializeField] private Transform _healthContainer;

        private PlayerUIViewModel _viewModel;
        private CompositeDisposable _disposables = new CompositeDisposable();

        [Inject]
        public void Init(PlayerUIViewModel viewModel)
        {
            _viewModel = viewModel;
            _viewModel.Coordinates.Subscribe(SetCoordinates).AddTo(_disposables);
            _viewModel.PlayerAngle.Subscribe(SetPlayerAngle).AddTo(_disposables);
            _viewModel.CurrentSpeed.Subscribe(SetCurrentSpeed).AddTo(_disposables);
            _viewModel.LaserCooldown.Subscribe(SetLaserCooldown).AddTo(_disposables);
            _viewModel.LaserAmmo.Subscribe(SetAmmo).AddTo(_disposables);
            //_viewModel.Health.Subscribe(SetHealthView).AddTo(_disposables);
            //InitHealth(viewModel);
        }

        private void InitHealth(PlayerUIViewModel viewModel)
        {
            var hearts = new Image[viewModel.Health.Value];
            for (var i = 0; i < viewModel.Health.Value; i++)
            {
                var heart = Instantiate(_heartPrefab, _healthContainer);
                hearts[i] = heart.GetComponent<Image>();;
            }
            _hearts = hearts;
        }
        
        
        private void SetHealthView(int health)
        {
            for (var i = 0; i < _hearts.Length; i++)
            {
                _hearts[i].gameObject.SetActive(i < health);
            }
        }

        private void SetLaserCooldown(float cooldown)
        {
            _laserCooldownIndicator.fillAmount = cooldown;
        }

        private void SetCurrentSpeed(float speed)
        {
            _speedText.text = $"SPEED:\n{(int)speed}0 Mph";
        }

        private void SetAmmo(int ammo)
        {
            _laserAmmoText.text = ammo.ToString();
        }

        private void SetPlayerAngle(float angle)
        {
            _angleText.text = $"{angle:F1}°";

            float amountToFill = angle / 360;
            
            _angleIndicator.fillAmount = amountToFill;
        }

        private void SetCoordinates(Vector2 pos)
        {
            _coordinatesText.text = $"X: {pos.x:F1}\nY: {pos.y:F1}";
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
    }
}