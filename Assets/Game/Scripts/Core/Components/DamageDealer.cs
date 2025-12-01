using Core.Projectiles;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Components
{
    public class DamageDealer : MonoBehaviour
    {
        [field: SerializeField] public int Damage { get; private set; } = 0;
        [field: SerializeField] public DamagerAffiliation Affiliation { get; private set; }
        [field: SerializeField] public DamagerDurability Durability { get; private set; }

        public void Initialize(int damage, DamagerAffiliation affiliation, DamagerDurability durability)
        {
            Damage = damage;
            Affiliation = affiliation;
            Durability = durability;
        }
    }
}
