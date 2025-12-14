using System.Collections;
using System.Collections.Generic;
using Core.Configuration;
using Core.Physics;
using UnityEngine;
using Zenject;

namespace Enemies.Logic
{
    public class BigAsteroidLogic
    {
        private EnemyType Type;
        private EnemyPresentation Presentation;
        private Transform EnemyTransform;
        private EnemySettings Settings;
        private CustomPhysics EnemyPhysics;

        [Inject]
        private void Construct(JsonConfigProvider configProvider, EnemyPresentation presentation,
            Transform enemyTransform, CustomPhysics physics)
        {
            Settings = configProvider.EnemySettingsRef;
            Presentation = presentation;
            enemyTransform = presentation.transform;
            EnemyPhysics = physics;
            EnemyPhysics.SetMovableObject(presentation);
        }
    }
}
