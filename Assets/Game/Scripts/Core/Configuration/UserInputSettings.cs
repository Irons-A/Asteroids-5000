using System.Collections;
using System.Collections.Generic;
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
        public float GamepadDecelrationAxisValue;
        public string GamepadShootBulletKey;
        public string GamepadShootLaserKey;
        public KeyCode GamepadPauseKey;

        public float InputDeadzone;
    }
}
