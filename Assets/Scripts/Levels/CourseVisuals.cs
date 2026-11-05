using UnityEngine;

namespace AntiGravityGolf
{
    /// <summary>
    /// Constructs a clean, smooth, open 3D miniature golf environment:
    /// - Golden directional sunlight with soft shadows
    /// - Clear atmospheric morning daylight and trilight ambient illumination
    /// - Smooth manicured landscape terrain floor
    /// - Clear, unobstructed sightlines (Zero trees, zero lumps/bumps, smooth materials)
    /// </summary>
    public static class CourseVisuals
    {
        private static Material matForestFloor;

        public static void InitializeForestMaterials()
        {
            Shader standardShader = Shader.Find("Standard") ?? Shader.Find("Mobile/Diffuse") ?? Shader.Find("Sprites/Default");

            // Open park floor: smooth lush lawn green
            matForestFloor = new Material(standardShader) { name = "Mat_ParkFloor" };
            matForestFloor.color = new Color(0.22f, 0.60f, 0.28f);
            if (matForestFloor.HasProperty("_Glossiness")) matForestFloor.SetFloat("_Glossiness", 0.50f);
        }

        public static void SetupForestEnvironment(Transform parent)
        {
            InitializeForestMaterials();

            // 1. Atmosphere: Trilight natural ambient illumination
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.70f, 0.85f, 0.98f);     // Soft daylight sky blue
            RenderSettings.ambientEquatorColor = new Color(0.45f, 0.60f, 0.40f); // Grass green
            RenderSettings.ambientGroundColor = new Color(0.25f, 0.22f, 0.16f);  // Soil brown

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.75f, 0.85f, 0.88f);
            RenderSettings.fogDensity = 0.0020f; // Clear visibility

            // 2. Warm golden sunlight
            GameObject sunObj = new GameObject("ParkGoldenSun");
            sunObj.transform.SetParent(parent, false);
            Light sun = sunObj.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1.0f, 0.96f, 0.86f);
            sun.intensity = 1.30f;
            sun.shadows = LightShadows.Soft;
            sunObj.transform.rotation = Quaternion.Euler(46f, -38f, 0f);

            // 3. Open Valley Floor (Below park elevation)
            GameObject floorRoot = new GameObject("ParkValleyFloor");
            floorRoot.transform.SetParent(parent, false);
            floorRoot.transform.position = new Vector3(0f, -16f, 35f);

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "RollingGround";
            ground.transform.SetParent(floorRoot.transform, false);
            ground.transform.localScale = new Vector3(260f, 5f, 260f);
            ground.transform.localPosition = new Vector3(0f, -2.5f, 0f);
            ground.GetComponent<MeshRenderer>().sharedMaterial = matForestFloor;
        }
    }
}
