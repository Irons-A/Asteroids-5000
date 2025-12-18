using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemies.Signals
{
    public class EnemyDestroyedSignal
    {
        public EnemyType Type { get; }

        public EnemyDestroyedSignal(EnemyType type)
        {
            Type = type;
        }
    }
}
