using System;
using UnityEngine;

namespace AntiGravityGolf
{
    [Serializable]
    public class LevelData
    {
        public string levelName = "Forest Canopy";
        public int par = 3;
        [TextArea(2, 4)]
        public string description = "Pine-lined fairway, timber launch ramp & mystical Zero-G gap.";
        public Vector3 spawnPosition = Vector3.zero;
        public float initialCameraYaw = 0f;
    }
}
