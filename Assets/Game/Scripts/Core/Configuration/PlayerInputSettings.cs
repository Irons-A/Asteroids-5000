using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Configuration
{
    public class PlayerInputSettings
    {
        public KeyCode PCAccelerationKey;
        public KeyCode PCDecelrationKey;
        public KeyCode PCShootBulletKey;
        public KeyCode PCShootLaserKey;
        public KeyCode PCPauseKey;

        public string GamepadHorizontalRotationAxis;
        public string GamepadVerticalRotationAxis;
        public KeyCode GamepadAccelerationKey;
        public KeyCode GamepadDecelrationKey;
        public KeyCode GamepadShootBulletKey;
        public KeyCode GamepadShootLaserKey;
        public KeyCode GamepadPauseKey;

        public float RotationStickDeadzone;
    }
}
