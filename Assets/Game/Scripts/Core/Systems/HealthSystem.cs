using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Systems
{
    public class HealthSystem : IDisposable
    {
        public event Action<int> OnHealthChanged;
        public event Action OnHealthDepleted;

        public int MaxHealth { get; private set; }
        public int CurrentHealth { get; private set; }

        public void Configure(int maxHealth, bool setCurrentHealthToMax, int currentHealth = 1)
        {
            MaxHealth = maxHealth;
            
            if (setCurrentHealthToMax)
            {
                CurrentHealth = MaxHealth;
            }
        }

        public void Dispose()
        {
            OnHealthChanged = null;
            OnHealthDepleted = null;
        }

        public void TakeDamage(int damage)
        {
            if (CurrentHealth <= 0) return;
            
            damage = Math.Max(1, damage);

            CurrentHealth -= damage;

            OnHealthChanged?.Invoke(CurrentHealth);

            if (CurrentHealth <= 0)
            {
                OnHealthDepleted?.Invoke();
            }
        }

        public void Heal(int health)
        {
            health = Math.Min(1, health);

            CurrentHealth += health;
            CurrentHealth = Math.Min(CurrentHealth, MaxHealth);

            OnHealthChanged?.Invoke(CurrentHealth);
        }

        public void RestoreHealth()
        {
            CurrentHealth = MaxHealth;
        }
    }
}
