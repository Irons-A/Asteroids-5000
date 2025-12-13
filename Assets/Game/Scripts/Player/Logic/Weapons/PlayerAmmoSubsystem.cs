using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player.Logic.Weapons
{
    public class PlayerAmmoSubsystem
    {
        public event Action<int, int> OnAmmoChanged;

        public int CurrentAmmo { get; private set; }
        public int MaxAmmo { get; private set; }
        public bool HasInfiniteAmmo { get; private set; }
        public bool IsEmpty => CurrentAmmo <= 0;
        public bool IsFull => CurrentAmmo >= MaxAmmo;

        public void ResetAmmo(int maxAmmo)
        {
            MaxAmmo = maxAmmo;
            CurrentAmmo = maxAmmo;

            OnAmmoChanged?.Invoke(CurrentAmmo, MaxAmmo);
        }

        public void SetInfiniteAmmo(bool infinite)
        {
            HasInfiniteAmmo = infinite;
        }

        public void ConsumeAmmo(int amount)
        {
            if (HasInfiniteAmmo) return;

            CurrentAmmo = Mathf.Max(0, CurrentAmmo - amount);

            OnAmmoChanged?.Invoke(CurrentAmmo, MaxAmmo);
        }

        public void AddAmmo(int amount)
        {
            CurrentAmmo = Mathf.Min(MaxAmmo, CurrentAmmo + amount);

            OnAmmoChanged?.Invoke(CurrentAmmo, MaxAmmo);
        }

        public void EmptyAmmo()
        {
            CurrentAmmo = 0;

            OnAmmoChanged?.Invoke(CurrentAmmo, MaxAmmo);
        }
    }
}
