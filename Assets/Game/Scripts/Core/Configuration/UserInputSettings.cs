using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.Configuration
{
    public class UserInputSettings
    {
        public readonly KeyCode PCAccelerationKey;
        public readonly KeyCode PCDecelerationKey;
        public readonly KeyCode PCShootBulletKey;
        public readonly KeyCode PCShootLaserKey;
        public readonly KeyCode PCPauseKey;

        public readonly string GamepadHorizontalRotationAxis;
        public readonly string GamepadVerticalRotationAxis;
        public readonly string GamepadMovementAxisName;
        public readonly float GamepadAccelerationAxisValue;
        public readonly float GamepadDecelerationAxisValue;
        public readonly string GamepadShootBulletKey;
        public readonly string GamepadShootLaserKey;
        public readonly KeyCode GamepadPauseKey;

        public readonly float InputDeadzone;

        [JsonConstructor]
        public UserInputSettings(KeyCode pCAccelerationKey, KeyCode pCDecelerationKey, KeyCode pCShootBulletKey,
            KeyCode pCShootLaserKey, KeyCode pCPauseKey, string gamepadHorizontalRotationAxis,
            string gamepadVerticalRotationAxis, string gamepadMovementAxisName, float gamepadAccelerationAxisValue,
            float gamepadDecelerationAxisValue, string gamepadShootBulletKey, string gamepadShootLaserKey,
            KeyCode gamepadPauseKey, float inputDeadzone)
        {
            PCAccelerationKey = pCAccelerationKey;
            PCDecelerationKey = pCDecelerationKey;
            PCShootBulletKey = pCShootBulletKey;
            PCShootLaserKey = pCShootLaserKey;
            PCPauseKey = pCPauseKey;
            
            GamepadHorizontalRotationAxis = gamepadHorizontalRotationAxis;
            GamepadVerticalRotationAxis = gamepadVerticalRotationAxis;
            GamepadMovementAxisName = gamepadMovementAxisName;
            GamepadAccelerationAxisValue = gamepadAccelerationAxisValue;
            GamepadDecelerationAxisValue = gamepadDecelerationAxisValue;
            GamepadShootBulletKey = gamepadShootBulletKey;
            GamepadShootLaserKey = gamepadShootLaserKey;
            GamepadPauseKey = gamepadPauseKey;
            
            InputDeadzone = inputDeadzone;
        }
    }
}
