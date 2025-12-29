using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Saves
{
    [System.Serializable]
    public class PlayerSaveData
    {
        public int HighScore { get; set; } = 0;
    }
}
