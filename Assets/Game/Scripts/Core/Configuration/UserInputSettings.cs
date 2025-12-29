using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.Configuration
{
    public class UserInputSettings
    {
        public KeyCode PCAccelerationKey;
        public KeyCode PCDecelrationKey;
        public KeyCode PCShootBulletKey;
        public KeyCode PCShootLaserKey;
        public KeyCode PCPauseKey;

        public string GamepadHorizontalRotationAxis;
        public string GamepadVerticalRotationAxis;
        public string GamepadMovementAxisName;
        public float GamepadAccelerationAxisValue;
        public float GamepadDecelerationAxisValue;
        public string GamepadShootBulletKey;
        public string GamepadShootLaserKey;
        public KeyCode GamepadPauseKey;

        public float InputDeadzone;

        [JsonConstructor]
        public UserInputSettings(KeyCode pCAccelerationKey, KeyCode pCDecelerationKey, KeyCode pCShootBulletKey,
            KeyCode pCShootLaserKey, KeyCode pCPauseKey, string gamepadHorizontalRotationAxis,
            string gamepadVerticalRotationAxis, string gamepadMovementAxisName, float gamepadAccelerationAxisValue,
            float gamepadDecelerationAxisValue, string gamepadShootBulletKey, string gamepadShootLaserKey,
            KeyCode gamepadPauseKey, float inputDeadzone)
        {
            PCAccelerationKey = pCAccelerationKey;
            PCDecelrationKey = pCDecelerationKey;
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
