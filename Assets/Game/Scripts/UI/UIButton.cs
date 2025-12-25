using System;
using System.Collections;
using System.Collections.Generic;
using UI.Signals;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    [RequireComponent(typeof(Button))]
    public class UIButton : MonoBehaviour
    {
        [SerializeField] private UIButtonType _type;
        
        private Button _button;
        private SignalBus _signalBus;

        [Inject]
        private void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnButtonClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            switch (_type)
            {
                case UIButtonType.Start:
                    _signalBus.TryFire(new StartGameSignal());
                    break;
                case UIButtonType.Exit:
                    _signalBus.TryFire(new ExitGameSignal());
                    break;
                case UIButtonType.Continue:
                    _signalBus.TryFire(new ContinueGameSignal());
                    break;
                case UIButtonType.Restart:
                    _signalBus.TryFire(new RestartGameSignal());
                    break;
                case UIButtonType.Menu:
                    _signalBus.TryFire(new GoToMenuSignal());
                    break;
                case UIButtonType.Pause:
                    _signalBus.TryFire(new PauseGameSignal());
                    break;
            }
        }
    }
}
