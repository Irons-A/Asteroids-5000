using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.PlayerMVVM
{
    public class PlayerUIView : MonoBehaviour
    {
        private readonly CompositeDisposable _disposables = new();
        
        [SerializeField] private TMP_Text _coordinatesText;
        [SerializeField] private TMP_Text _angleText;
        [SerializeField] private Image _angleIndicator;
        [SerializeField] private TMP_Text _speedText;
        [SerializeField] private TMP_Text _laserAmmoText;
        [SerializeField] private Image _laserCooldownIndicator;
        [SerializeField] private TMP_Text _scoreText;

        [Header("Health")] 
        [SerializeField] private Image[] _lives;

        private PlayerUIViewModel _viewModel;

        [Inject]
        private void Construct(PlayerUIViewModel viewModel)
        {
            _viewModel = viewModel;
            _viewModel.Coordinates.Subscribe(SetCoordinates).AddTo(_disposables);
            _viewModel.PlayerAngle.Subscribe(SetPlayerAngle).AddTo(_disposables);
            _viewModel.CurrentSpeed.Subscribe(SetCurrentSpeed).AddTo(_disposables);
            _viewModel.LaserCooldown.Subscribe(SetLaserCooldown).AddTo(_disposables);
            _viewModel.LaserAmmo.Subscribe(SetAmmo).AddTo(_disposables);
            _viewModel.Health.Subscribe(SetHealthView).AddTo(_disposables);
            _viewModel.Score.Subscribe(SetScore).AddTo(_disposables);
        }
        
        private void SetHealthView(int health)
        {
            for (var i = 0; i < _lives.Length; i++)
            {
                _lives[i].gameObject.SetActive(i < health);
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

        private void SetScore(int score)
        {
            _scoreText.text = $"SCORE:\n{score}";
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
    }
}