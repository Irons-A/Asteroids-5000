using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemies.Presentation
{
    public class UFOPresentation : EnemyPresentation
    {
        [field: SerializeField] public Transform Firepoint { get; private set; }
    }
}
