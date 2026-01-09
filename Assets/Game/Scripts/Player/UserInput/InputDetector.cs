using Player.Logic;
using Player.UserInput.Strategies;
using Core.Signals;
using UnityEngine;
using Zenject;

namespace Player.UserInput
{
    public class InputDetector : ITickable
    {
        private const string MouseXAxis = "Mouse X";
        private const string MouseYAxis = "Mouse Y";
        
        private readonly KeyboardMouseInputStrategy _pcStrategy;
        private readonly GamepadInputStrategy _gamepadStrategy;
        private readonly  MobileInputStrategy _mobileStrategy;
        private readonly SignalBus _signalBus;
        
        private PlayerLogic _playerLogic;

        public IInputStrategy CurrentStrategy { get; private set; }

        public InputDetector(KeyboardMouseInputStrategy pcStrategy, GamepadInputStrategy gamepadStrategy,
            MobileInputStrategy mobileStrategy, SignalBus signalBus)
        {
            _pcStrategy = pcStrategy;
            _gamepadStrategy = gamepadStrategy;
            _mobileStrategy = mobileStrategy;
            CurrentStrategy = pcStrategy;
            
            _signalBus = signalBus;
        }

        public void Tick()
        {
            DefineInputStrategy();

            ProcessUserInput();
        }

        public void SetPlayerLogic(PlayerLogic playerLogic)
        {
            _playerLogic = playerLogic;
        }

        public void SetCamera()
        {
            _pcStrategy.SetCamera();
        }

        private void DefineInputStrategy()
        {
            if (CheckIfMobileInput())
            {
                CurrentStrategy = _mobileStrategy;
                
                return;
            }
            
            bool gamepadInput = CheckIfGamepadConnected() && CheckIfGamepadInput();
            bool pcInput = CheckIfPCInput();

            if (pcInput)
            {
                CurrentStrategy = _pcStrategy;
            }
            else if (gamepadInput)
            {
                //May not work due to Unity bug: always gets input from axis, even when there is none.
                CurrentStrategy = _gamepadStrategy;
            }
        }

        private bool CheckIfMobileInput()
        {
            #if UNITY_ANDROID || UNITY_IOS || UNITY_WSA
                    return Application.isMobilePlatform || 
                           Input.touchCount > 0 || 
                           Input.touches.Length > 0;
            #else
                        return false;
            #endif
        }

        private bool CheckIfPCInput()
        {
            bool hasKeyInput = Input.anyKeyDown;

            bool hasMouseMovement = Mathf.Abs(Input.GetAxis(MouseXAxis)) > 0.1f ||
                                   Mathf.Abs(Input.GetAxis(MouseYAxis)) > 0.1f;

            bool hasMouseClick = Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2);

            return hasKeyInput || hasMouseMovement || hasMouseClick;
        }

        private bool CheckIfGamepadInput()
        {
            return Input.GetKey(KeyCode.JoystickButton0) ||
                   Input.GetKey(KeyCode.JoystickButton1) ||
                   Input.GetKey(KeyCode.JoystickButton2) ||
                   Input.GetKey(KeyCode.JoystickButton3) ||
                   Input.GetKey(KeyCode.JoystickButton4) ||
                   Input.GetKey(KeyCode.JoystickButton5) ||
                   Input.GetKey(KeyCode.JoystickButton6) ||
                   Input.GetKey(KeyCode.JoystickButton7);
        }

        private bool CheckIfGamepadConnected()
        {
            string[] joystickNames = Input.GetJoystickNames();

            foreach (string joystickName in joystickNames)
            {
                if (!string.IsNullOrEmpty(joystickName) && joystickName.Length > 0)
                {
                    return true;
                }
            }
            return false;
        }

        private void ProcessUserInput()
        {
            if (_playerLogic == null) return;

            if (Time.timeScale > 0)
            {
                if (CurrentStrategy is KeyboardMouseInputStrategy)
                {
                    _playerLogic.RotatePlayerWithMouse(CurrentStrategy.GetRotationInput());
                }
                else
                {
                    _playerLogic.RotatePlayerTowardsStick(CurrentStrategy.GetRotationInput());
                }

                _playerLogic.MovePlayer(CurrentStrategy.GetPlayerMovementState());

                _playerLogic.ShootBullets(CurrentStrategy.IsShootingBullets());

                _playerLogic.ShootLaser(CurrentStrategy.IsShootingLaser());
            }

            if (CurrentStrategy.IsPausePressed())
            {
                _signalBus.TryFire(new PauseGameSignal());
            }
        }
    }
}
