using System.Collections;
using System.Collections.Generic;
using Core.Systems;
using UnityEngine;

namespace Core.Components
{
    public class DamageDealer : MonoBehaviour
    {
        [field: SerializeField] public int Damage { get; private set; } = 0;
        [field: SerializeField] public EntityAffiliation Affiliation { get; private set; }
        [field: SerializeField] public EntityDurability Durability { get; private set; }

        public void Configure(int damage, EntityAffiliation affiliation, EntityDurability durability)
        {
            Damage = damage;
            Affiliation = affiliation;
            Durability = durability;
        }
    }
}
