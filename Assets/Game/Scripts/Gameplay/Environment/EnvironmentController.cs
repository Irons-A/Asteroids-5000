using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Environment
{
    [System.Serializable]
    public class EnvironmentController
    {
        [SerializeField] private Vector2 _fieldSize = new Vector2(38f, 22f);
        [SerializeField] private Vector2 _fieldCenter = Vector2.zero;

        public Bounds Bounds => new Bounds(_fieldCenter, _fieldSize);

        public Vector2 ClampPosition(Vector2 position)
        {
            Vector2 min = _fieldCenter - _fieldSize * 0.5f;
            Vector2 max = _fieldCenter + _fieldSize * 0.5f;

            return new Vector2(
                Mathf.Clamp(position.x, min.x, max.x),
                Mathf.Clamp(position.y, min.y, max.y)
            );
        }

        public void DrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(_fieldCenter, _fieldSize);
        }
    }
}
