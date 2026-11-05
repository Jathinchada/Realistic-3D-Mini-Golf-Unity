using System.Collections.Generic;
using UnityEngine;

#if UNITY_6000_0_OR_NEWER
using BallPhysicMaterial = UnityEngine.PhysicsMaterial;
using BallPhysicCombine = UnityEngine.PhysicsMaterialCombine;
#else
using BallPhysicMaterial = UnityEngine.PhysicMaterial;
using BallPhysicCombine = UnityEngine.PhysicMaterialCombine;
#endif

namespace AntiGravityGolf
{
    /// <summary>
    /// Procedurally constructs the 5 Championship Courses across the park landscape
    /// with ZERO trees, ZERO water hazards/ponds, and fully BENDY putting paths:
    /// - Course 1: The Winding S-Bend (Serpentine curves with smooth banking corners)
    /// - Course 2: The Double Dogleg (Dual 90-degree dogleg bends along north lawn)
    /// - Course 3: The Horseshoe Loop (180-degree hairpin U-turn curve)
    /// - Course 4: The Serpentine Chicane (Winding chicane with alternating left/right bends)
    /// - Course 5: The Grand Finale Crescent (Sweeping 140-degree crescent arc)
    /// - Practice Putting Green near the entrance
    /// </summary>
    public static class CourseBuilder
    {
        #region Material Cache

        private static Material matStoneBase;
        private static Material matPuttingGreen;
        private static Material matRoughTurf;
        private static Material matSandTrap;
        private static Material matBrickRail;
        private static Material matCedarWood;
        private static Material matGravelPath;
        private static Material matWaterSurface;
        private static Material matHoleVoid;
        private static Material matFlagRed;
        private static Material matFlagStick;
        private static Material matGraniteRock;
        private static Material matEnchantedRune;
        private static Material matForestWispZone;

        private static BallPhysicMaterial turfPhysMat;
        private static BallPhysicMaterial railPhysMat;
        private static BallPhysicMaterial sandPhysMat;

        public static void InitializeMaterials()
        {
            Shader standardShader = Shader.Find("Standard") ?? Shader.Find("Mobile/Diffuse") ?? Shader.Find("Sprites/Default");

            // Smooth foundation stone (slate)
            matStoneBase = new Material(standardShader) { name = "Mat_ParkStoneBase" };
            matStoneBase.color = new Color(0.24f, 0.26f, 0.28f);
            if (matStoneBase.HasProperty("_Glossiness")) matStoneBase.SetFloat("_Glossiness", 0.60f);

            // Smooth manicured putting green turf (clean, vibrant, untextured)
            matPuttingGreen = new Material(standardShader) { name = "Mat_GolfTurfGreen" };
            matPuttingGreen.color = new Color(0.18f, 0.76f, 0.34f);
            if (matPuttingGreen.HasProperty("_Glossiness")) matPuttingGreen.SetFloat("_Glossiness", 0.70f);

            // Smooth park lawn ground
            matRoughTurf = new Material(standardShader) { name = "Mat_RoughCarpet" };
            matRoughTurf.color = new Color(0.22f, 0.58f, 0.28f);
            if (matRoughTurf.HasProperty("_Glossiness")) matRoughTurf.SetFloat("_Glossiness", 0.50f);

            // Smooth sand bunker
            matSandTrap = new Material(standardShader) { name = "Mat_SandBunker" };
            matSandTrap.color = new Color(0.88f, 0.82f, 0.65f);
            if (matSandTrap.HasProperty("_Glossiness")) matSandTrap.SetFloat("_Glossiness", 0.35f);

            // Smooth terracotta brick curb (lacquered, sleek finish)
            matBrickRail = new Material(standardShader) { name = "Mat_BrickRetainingCurb" };
            matBrickRail.color = new Color(0.82f, 0.26f, 0.20f);
            if (matBrickRail.HasProperty("_Glossiness")) matBrickRail.SetFloat("_Glossiness", 0.75f);

            // Smooth cedar timber
            matCedarWood = new Material(standardShader) { name = "Mat_CedarTimber" };
            matCedarWood.color = new Color(0.48f, 0.30f, 0.18f);
            if (matCedarWood.HasProperty("_Glossiness")) matCedarWood.SetFloat("_Glossiness", 0.65f);

            // Smooth walking path
            matGravelPath = new Material(standardShader) { name = "Mat_GravelPath" };
            matGravelPath.color = new Color(0.70f, 0.72f, 0.74f);
            if (matGravelPath.HasProperty("_Glossiness")) matGravelPath.SetFloat("_Glossiness", 0.55f);

            // Smooth water fallback
            matWaterSurface = new Material(standardShader) { name = "Mat_ParkWater" };
            matWaterSurface.color = new Color(0.16f, 0.52f, 0.68f, 0.88f);
            if (matWaterSurface.HasProperty("_Glossiness")) matWaterSurface.SetFloat("_Glossiness", 0.92f);

            // Sunken hole void
            matHoleVoid = new Material(standardShader) { name = "Mat_CupVoid" };
            matHoleVoid.color = new Color(0.04f, 0.05f, 0.06f);
            if (matHoleVoid.HasProperty("_Glossiness")) matHoleVoid.SetFloat("_Glossiness", 0.40f);

            matFlagRed = new Material(standardShader) { name = "Mat_FlagSilkRed" };
            matFlagRed.color = new Color(0.92f, 0.12f, 0.12f);
            if (matFlagRed.HasProperty("_Glossiness")) matFlagRed.SetFloat("_Glossiness", 0.70f);

            matFlagStick = new Material(standardShader) { name = "Mat_FlagStickFiber" };
            matFlagStick.color = new Color(0.98f, 0.98f, 0.98f);
            if (matFlagStick.HasProperty("_Glossiness")) matFlagStick.SetFloat("_Glossiness", 0.90f);

            matGraniteRock = new Material(standardShader) { name = "Mat_GraniteBoulder" };
            matGraniteRock.color = new Color(0.44f, 0.46f, 0.43f);
            if (matGraniteRock.HasProperty("_Glossiness")) matGraniteRock.SetFloat("_Glossiness", 0.50f);

            matEnchantedRune = new Material(standardShader) { name = "Mat_EmeraldRune" };
            matEnchantedRune.color = new Color(0.12f, 0.95f, 0.48f);
            if (matEnchantedRune.HasProperty("_EmissionColor"))
            {
                matEnchantedRune.EnableKeyword("_EMISSION");
                matEnchantedRune.SetColor("_EmissionColor", new Color(0.12f, 0.95f, 0.48f) * 1.3f);
            }

            matForestWispZone = new Material(standardShader) { name = "Mat_ForestWispZone" };
            matForestWispZone.color = new Color(0.14f, 0.88f, 0.45f, 0.22f);
            SetMaterialTransparent(matForestWispZone);

            turfPhysMat = new BallPhysicMaterial("TurfPhysMat")
            {
                dynamicFriction = 0.20f,
                staticFriction = 0.25f,
                bounciness = 0.05f,
                frictionCombine = BallPhysicCombine.Average,
                bounceCombine = BallPhysicCombine.Minimum
            };

            railPhysMat = new BallPhysicMaterial("CurbPhysMat")
            {
                dynamicFriction = 0.15f,
                staticFriction = 0.20f,
                bounciness = 0.35f,
                frictionCombine = BallPhysicCombine.Minimum,
                bounceCombine = BallPhysicCombine.Average
            };

            sandPhysMat = new BallPhysicMaterial("SandPhysMat")
            {
                dynamicFriction = 0.75f,
                staticFriction = 0.85f,
                bounciness = 0.02f,
                frictionCombine = BallPhysicCombine.Maximum,
                bounceCombine = BallPhysicCombine.Minimum
            };
        }

        private static void SetMaterialTransparent(Material mat)
        {
            if (mat == null) return;
            if (mat.HasProperty("_Mode"))
            {
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }
        }

        #endregion

        #region Park Landscape Construction (Zero Trees, Zero Water)

        public static GameObject CreateParkLandscape(Transform parent, out Transform practiceSpawn)
        {
            InitializeMaterials();

            GameObject park = new GameObject("MiniGolfParkLandscape");
            park.transform.SetParent(parent, false);

            // 1. Overall Park Ground (74m wide x 54m deep)
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "ParkLawnGround";
            ground.transform.SetParent(park.transform, false);
            ground.transform.localScale = new Vector3(74f, 1.0f, 54f);
            ground.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            ground.GetComponent<MeshRenderer>().sharedMaterial = matRoughTurf;
            ground.GetComponent<BoxCollider>().material = turfPhysMat;

            // 2. Perimeter Retaining Wall
            CreatePerimeterWall(park.transform);

            // 3. Central Pavilion (15' x 25') near bottom-center
            CreatePavilion(park.transform, new Vector3(0f, 0f, -21f));

            // 4. Practice Putting Green near lower-right entrance
            practiceSpawn = CreatePracticeGreen(park.transform);

            // 5. Aggregate Walking Pathways
            CreateGravelPathways(park.transform);

            return park;
        }

        private static void CreatePerimeterWall(Transform parent)
        {
            GameObject wallRoot = new GameObject("PerimeterRetainingWall");
            wallRoot.transform.SetParent(parent, false);

            float halfW = 36.5f;
            float halfD = 26.5f;
            float wallH = 0.75f;
            float wallT = 0.55f;

            CreateWallSegment(wallRoot.transform, new Vector3(0f, wallH * 0.5f, halfD), new Vector3(halfW * 2f, wallH, wallT));
            CreateWallSegment(wallRoot.transform, new Vector3(0f, wallH * 0.5f, -halfD), new Vector3(halfW * 2f, wallH, wallT));
            CreateWallSegment(wallRoot.transform, new Vector3(halfW, wallH * 0.5f, 0f), new Vector3(wallT, wallH, halfD * 2f));
            CreateWallSegment(wallRoot.transform, new Vector3(-halfW, wallH * 0.5f, 0f), new Vector3(wallT, wallH, halfD * 2f));

            Vector3[] corners = {
                new Vector3(-halfW, 0f, -halfD),
                new Vector3(halfW, 0f, -halfD),
                new Vector3(halfW, 0f, halfD),
                new Vector3(-halfW, 0f, halfD)
            };
            foreach (var c in corners)
            {
                GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.name = "CornerPost";
                post.transform.SetParent(wallRoot.transform, false);
                post.transform.localPosition = c + Vector3.up * (wallH * 0.5f);
                post.transform.localScale = new Vector3(1.2f, wallH * 0.5f, 1.2f);
                post.GetComponent<MeshRenderer>().sharedMaterial = matStoneBase;
            }
        }

        private static void CreateWallSegment(Transform parent, Vector3 pos, Vector3 size)
        {
            GameObject seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seg.name = "WallSegment";
            seg.transform.SetParent(parent, false);
            seg.transform.localPosition = pos;
            seg.transform.localScale = size;
            seg.GetComponent<MeshRenderer>().sharedMaterial = matStoneBase;
            seg.GetComponent<BoxCollider>().material = railPhysMat;
        }

        private static Transform CreatePracticeGreen(Transform parent)
        {
            GameObject pg = new GameObject("PracticePuttingGreen");
            pg.transform.SetParent(parent, false);
            pg.transform.localPosition = new Vector3(18f, 0f, -18f);

            GameObject green = GameObject.CreatePrimitive(PrimitiveType.Cube);
            green.name = "PracticeTurf";
            green.transform.SetParent(pg.transform, false);
            green.transform.localScale = new Vector3(7.5f, 0.04f, 6.5f);
            green.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            green.GetComponent<MeshRenderer>().sharedMaterial = matPuttingGreen;
            green.GetComponent<BoxCollider>().material = turfPhysMat;

            CreateBorders(pg.transform, new Vector3(0f, 0.02f, 0f), new Vector3(7.5f, 0.04f, 6.5f), 0.35f, false, false);

            Vector3 teeLocal = new Vector3(2.0f, 0.02f, -1.5f);
            CreateTeeMat(pg.transform, teeLocal);

            CreateHole(pg.transform, new Vector3(-1.8f, 0.02f, 1.2f), 101);
            CreateHole(pg.transform, new Vector3(0.8f, 0.02f, 1.4f), 102);

            GameObject spawnObj = new GameObject("PracticeSpawnPoint");
            spawnObj.transform.SetParent(pg.transform, false);
            spawnObj.transform.localPosition = teeLocal + Vector3.up * 0.15f;
            spawnObj.transform.localRotation = Quaternion.LookRotation(new Vector3(-1f, 0f, 1f).normalized, Vector3.up);

            return spawnObj.transform;
        }

        private static void CreatePavilion(Transform parent, Vector3 pos)
        {
            GameObject pav = new GameObject("Pavilion_15x25");
            pav.transform.SetParent(parent, false);
            pav.transform.localPosition = pos;

            float width = 8.0f;
            float depth = 4.8f;
            float height = 3.6f;

            GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slab.transform.SetParent(pav.transform, false);
            slab.transform.localScale = new Vector3(width, 0.25f, depth);
            slab.transform.localPosition = new Vector3(0f, 0.125f, 0f);
            slab.GetComponent<MeshRenderer>().sharedMaterial = matGravelPath;

            float[] px = { -width * 0.45f, 0f, width * 0.45f };
            float[] pz = { -depth * 0.45f, depth * 0.45f };
            foreach (float x in px)
            {
                foreach (float z in pz)
                {
                    GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    post.transform.SetParent(pav.transform, false);
                    post.transform.localScale = new Vector3(0.35f, height * 0.5f, 0.35f);
                    post.transform.localPosition = new Vector3(x, height * 0.5f, z);
                    post.GetComponent<MeshRenderer>().sharedMaterial = matCedarWood;
                }
            }

            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.transform.SetParent(pav.transform, false);
            roof.transform.localScale = new Vector3(width + 1.0f, 0.6f, depth + 1.0f);
            roof.transform.localPosition = new Vector3(0f, height + 0.3f, 0f);
            roof.GetComponent<MeshRenderer>().sharedMaterial = matCedarWood;
        }

        private static void CreateGravelPathways(Transform parent)
        {
            GameObject paths = new GameObject("WalkingPaths");
            paths.transform.SetParent(parent, false);

            CreatePathSegment(paths.transform, new Vector3(26f, 0.02f, -22f), new Vector3(18f, 0.02f, -22f), 2.0f);
            CreatePathSegment(paths.transform, new Vector3(18f, 0.02f, -22f), new Vector3(0f, 0.02f, -21f), 2.0f);
            CreatePathSegment(paths.transform, new Vector3(0f, 0.02f, -21f), new Vector3(-18f, 0.02f, -21f), 2.0f);
        }

        private static void CreatePathSegment(Transform parent, Vector3 start, Vector3 end, float width)
        {
            Vector3 mid = (start + end) * 0.5f;
            float length = Vector3.Distance(start, end);
            Vector3 dir = (end - start).normalized;

            GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
            segment.name = "PathSegment";
            segment.transform.SetParent(parent, false);
            segment.transform.localPosition = new Vector3(mid.x, 0.001f, mid.z);
            segment.transform.localScale = new Vector3(width, 0.001f, length);
            if (dir.sqrMagnitude > 0.01f)
            {
                segment.transform.localRotation = Quaternion.LookRotation(dir, Vector3.up);
            }
            segment.GetComponent<MeshRenderer>().sharedMaterial = matGravelPath;
            Object.DestroyImmediate(segment.GetComponent<Collider>());
        }

        #endregion

        #region Hole Primitive Builders

        public static GameObject CreatePlatform(Transform parent, Vector3 localPos, Vector3 size, bool isGreen = true)
        {
            if (matStoneBase == null) InitializeMaterials();

            GameObject platform = new GameObject("Platform");
            platform.transform.SetParent(parent, false);
            platform.transform.localPosition = localPos;

            GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slab.name = "StoneSlab";
            slab.transform.SetParent(platform.transform, false);
            slab.transform.localScale = new Vector3(size.x, 0.35f, size.z);
            slab.transform.localPosition = new Vector3(0f, -0.175f, 0f);
            slab.GetComponent<MeshRenderer>().sharedMaterial = matStoneBase;

            if (isGreen)
            {
                GameObject turf = GameObject.CreatePrimitive(PrimitiveType.Cube);
                turf.name = "GolfTurf";
                turf.transform.SetParent(platform.transform, false);
                turf.transform.localScale = new Vector3(size.x - 0.2f, 0.05f, size.z - 0.2f);
                turf.transform.localPosition = new Vector3(0f, 0.025f, 0f);
                turf.GetComponent<MeshRenderer>().sharedMaterial = matPuttingGreen;
                turf.GetComponent<BoxCollider>().material = turfPhysMat;
            }

            return platform;
        }

        public static GameObject CreateRamp(Transform parent, Vector3 localPos, Vector3 size, float pitchDegrees)
        {
            if (matPuttingGreen == null) InitializeMaterials();

            GameObject ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ramp.name = "TurfRamp";
            ramp.transform.SetParent(parent, false);
            ramp.transform.localPosition = localPos;
            ramp.transform.localRotation = Quaternion.Euler(pitchDegrees, 0f, 0f);
            ramp.transform.localScale = size;
            ramp.GetComponent<MeshRenderer>().sharedMaterial = matPuttingGreen;
            ramp.GetComponent<BoxCollider>().material = turfPhysMat;

            return ramp;
        }

        public static void CreateBorders(Transform parent, Vector3 center, Vector3 size, float railHeight = 0.40f, bool openFront = false, bool openBack = false)
        {
            if (matBrickRail == null) InitializeMaterials();

            float halfX = size.x * 0.5f;
            float halfZ = size.z * 0.5f;
            float railThick = 0.22f;

            GameObject left = GameObject.CreatePrimitive(PrimitiveType.Cube);
            left.name = "BrickCurb_Left";
            left.transform.SetParent(parent, false);
            left.transform.localPosition = center + new Vector3(-halfX, railHeight * 0.5f, 0f);
            left.transform.localScale = new Vector3(railThick, railHeight, size.z);
            left.GetComponent<MeshRenderer>().sharedMaterial = matBrickRail;
            left.GetComponent<BoxCollider>().material = railPhysMat;

            GameObject right = GameObject.CreatePrimitive(PrimitiveType.Cube);
            right.name = "BrickCurb_Right";
            right.transform.SetParent(parent, false);
            right.transform.localPosition = center + new Vector3(halfX, railHeight * 0.5f, 0f);
            right.transform.localScale = new Vector3(railThick, railHeight, size.z);
            right.GetComponent<MeshRenderer>().sharedMaterial = matBrickRail;
            right.GetComponent<BoxCollider>().material = railPhysMat;

            if (!openBack)
            {
                GameObject back = GameObject.CreatePrimitive(PrimitiveType.Cube);
                back.name = "BrickCurb_Back";
                back.transform.SetParent(parent, false);
                back.transform.localPosition = center + new Vector3(0f, railHeight * 0.5f, -halfZ);
                back.transform.localScale = new Vector3(size.x + railThick, railHeight, railThick);
                back.GetComponent<MeshRenderer>().sharedMaterial = matBrickRail;
                back.GetComponent<BoxCollider>().material = railPhysMat;
            }

            if (!openFront)
            {
                GameObject front = GameObject.CreatePrimitive(PrimitiveType.Cube);
                front.name = "BrickCurb_Front";
                front.transform.SetParent(parent, false);
                front.transform.localPosition = center + new Vector3(0f, railHeight * 0.5f, halfZ);
                front.transform.localScale = new Vector3(size.x + railThick, railHeight, railThick);
                front.GetComponent<MeshRenderer>().sharedMaterial = matBrickRail;
                front.GetComponent<BoxCollider>().material = railPhysMat;
            }
        }

        public static GameObject CreateHole(Transform parent, Vector3 localPos, int holeNumber = 1)
        {
            if (matHoleVoid == null) InitializeMaterials();

            GameObject holeObj = new GameObject($"GolfHole_{holeNumber}");
            holeObj.transform.SetParent(parent, false);
            holeObj.transform.localPosition = localPos;

            // 1. Reasonable Mini-Golf Sunken Cup Well Cavity
            GameObject cupWell = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cupWell.name = "SunkenCupWell";
            cupWell.transform.SetParent(holeObj.transform, false);
            cupWell.transform.localScale = new Vector3(0.58f, 0.14f, 0.58f);
            cupWell.transform.localPosition = new Vector3(0f, -0.08f, 0f);
            cupWell.GetComponent<MeshRenderer>().sharedMaterial = matHoleVoid;
            Object.DestroyImmediate(cupWell.GetComponent<Collider>());

            // 2. Sunken Cup Bottom Floor
            GameObject cupBottom = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cupBottom.name = "SunkenCupFloor";
            cupBottom.transform.SetParent(holeObj.transform, false);
            cupBottom.transform.localScale = new Vector3(0.54f, 0.015f, 0.54f);
            cupBottom.transform.localPosition = new Vector3(0f, -0.15f, 0f);
            cupBottom.GetComponent<MeshRenderer>().sharedMaterial = matHoleVoid;
            Object.DestroyImmediate(cupBottom.GetComponent<Collider>());

            // 3. Smooth Collar Ring (Flush with putting turf, zero bump)
            GameObject collarRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            collarRing.name = "SunkenCupCollarRing";
            collarRing.transform.SetParent(holeObj.transform, false);
            collarRing.transform.localScale = new Vector3(0.68f, 0.002f, 0.68f);
            collarRing.transform.localPosition = new Vector3(0f, 0.021f, 0f);
            collarRing.GetComponent<MeshRenderer>().sharedMaterial = matStoneBase;
            Object.DestroyImmediate(collarRing.GetComponent<Collider>());

            // 4. Smooth Inner Rim Sleeve
            GameObject innerRim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            innerRim.name = "SunkenCupInnerRim";
            innerRim.transform.SetParent(holeObj.transform, false);
            innerRim.transform.localScale = new Vector3(0.59f, 0.002f, 0.59f);
            innerRim.transform.localPosition = new Vector3(0f, 0.019f, 0f);
            innerRim.GetComponent<MeshRenderer>().sharedMaterial = matFlagStick;
            Object.DestroyImmediate(innerRim.GetComponent<Collider>());

            SphereCollider trigger = holeObj.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 0.45f;
            trigger.center = new Vector3(0f, 0.02f, 0f);

            // FlagStick anchored at the bottom of the sunken cup
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "FlagStick";
            pole.transform.SetParent(holeObj.transform, false);
            pole.transform.localScale = new Vector3(0.035f, 1.35f, 0.035f);
            pole.transform.localPosition = new Vector3(0f, 1.15f, 0f);
            pole.GetComponent<MeshRenderer>().sharedMaterial = matFlagStick;
            Object.DestroyImmediate(pole.GetComponent<Collider>());

            GameObject flag = GameObject.CreatePrimitive(PrimitiveType.Cube);
            flag.name = "GolfFlagBanner";
            flag.transform.SetParent(holeObj.transform, false);
            flag.transform.localScale = new Vector3(0.50f, 0.30f, 0.025f);
            flag.transform.localPosition = new Vector3(0.28f, 2.25f, 0f);
            flag.GetComponent<MeshRenderer>().sharedMaterial = matFlagRed;
            Object.DestroyImmediate(flag.GetComponent<Collider>());

            GolfHole gh = holeObj.AddComponent<GolfHole>();

            GameObject psObj = new GameObject("CelebrationParticles");
            psObj.transform.SetParent(holeObj.transform, false);
            psObj.transform.localPosition = new Vector3(0f, 0.15f, 0f);
            ParticleSystem ps = psObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.playOnAwake = false;
            main.startLifetime = 1.6f;
            main.startSpeed = 5.2f;
            main.startSize = 0.18f;
            main.startColor = new Color(1.0f, 0.88f, 0.25f);
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 40) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 28f;

#if UNITY_EDITOR
            var ghSo = new UnityEditor.SerializedObject(gh);
            ghSo.FindProperty("cupCenter").objectReferenceValue = cupBottom.transform;
            ghSo.FindProperty("celebrationParticles").objectReferenceValue = ps;
            ghSo.FindProperty("maxSinkSpeed").floatValue = 8.5f;
            ghSo.FindProperty("sinkProximity").floatValue = 0.38f;
            ghSo.FindProperty("funnelStrength").floatValue = 16f;
            ghSo.ApplyModifiedProperties();
#endif

            return holeObj;
        }

        public static GameObject CreateSandBunker(Transform parent, Vector3 localPos, Vector3 size)
        {
            if (matSandTrap == null) InitializeMaterials();

            GameObject sand = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sand.name = "SandBunker";
            sand.transform.SetParent(parent, false);
            sand.transform.localPosition = localPos;
            sand.transform.localScale = size;
            sand.GetComponent<MeshRenderer>().sharedMaterial = matSandTrap;

            BoxCollider col = sand.GetComponent<BoxCollider>();
            col.material = sandPhysMat;

            return sand;
        }

        public static GameObject CreateTeeMat(Transform parent, Vector3 localPos)
        {
            GameObject teeArea = new GameObject("TeeArea");
            teeArea.transform.SetParent(parent, false);
            teeArea.transform.localPosition = localPos;

            // 1. Sturdy Dark Rubber/Stone Tee Mat Base
            GameObject mat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mat.name = "TeeMat";
            mat.transform.SetParent(teeArea.transform, false);
            mat.transform.localScale = new Vector3(1.8f, 0.015f, 1.8f);
            mat.transform.localPosition = new Vector3(0f, 0.008f, 0f);
            mat.GetComponent<MeshRenderer>().sharedMaterial = matStoneBase;
            Object.DestroyImmediate(mat.GetComponent<Collider>());

            // 2. Inner Tee Turf Insert
            GameObject turfInsert = GameObject.CreatePrimitive(PrimitiveType.Cube);
            turfInsert.name = "TeeTurfInsert";
            turfInsert.transform.SetParent(teeArea.transform, false);
            turfInsert.transform.localScale = new Vector3(1.4f, 0.018f, 1.4f);
            turfInsert.transform.localPosition = new Vector3(0f, 0.009f, 0f);
            turfInsert.GetComponent<MeshRenderer>().sharedMaterial = matRoughTurf;
            Object.DestroyImmediate(turfInsert.GetComponent<Collider>());

            // 3. Center White Tee Spot (where the ball is placed)
            GameObject spot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spot.name = "TeeSpot";
            spot.transform.SetParent(teeArea.transform, false);
            spot.transform.localScale = new Vector3(0.28f, 0.02f, 0.28f);
            spot.transform.localPosition = new Vector3(0f, 0.010f, 0f);
            spot.GetComponent<MeshRenderer>().sharedMaterial = matFlagStick;
            Object.DestroyImmediate(spot.GetComponent<Collider>());

            // 4. Red Side Tee Markers
            float[] markerX = { -0.92f, 0.92f };
            foreach (float mx in markerX)
            {
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.name = "TeeMarkerBall";
                marker.transform.SetParent(teeArea.transform, false);
                marker.transform.localScale = new Vector3(0.18f, 0.18f, 0.18f);
                marker.transform.localPosition = new Vector3(mx, 0.09f, 0f);
                marker.GetComponent<MeshRenderer>().sharedMaterial = matFlagRed;
                Object.DestroyImmediate(marker.GetComponent<Collider>());
            }

            return teeArea;
        }

        public static GameObject CreateBumper(Transform parent, Vector3 localPos)
        {
            if (matGraniteRock == null) InitializeMaterials();

            GameObject bumper = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bumper.name = "Bumper";
            bumper.transform.SetParent(parent, false);
            bumper.transform.localPosition = localPos;
            bumper.transform.localScale = new Vector3(1.3f, 0.9f, 1.3f);
            bumper.GetComponent<MeshRenderer>().sharedMaterial = matGraniteRock;

            bumper.AddComponent<Bumper>();
            return bumper;
        }

        public static GameObject CreateCheckpoint(Transform parent, Vector3 localPos)
        {
            if (matEnchantedRune == null) InitializeMaterials();

            GameObject cp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cp.name = "Checkpoint";
            cp.transform.SetParent(parent, false);
            cp.transform.localPosition = localPos;
            cp.transform.localScale = new Vector3(2.2f, 0.08f, 2.2f);
            cp.GetComponent<MeshRenderer>().sharedMaterial = matEnchantedRune;

            BoxCollider col = cp.GetComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(1f, 4f, 1f);

            cp.AddComponent<Checkpoint>();
            return cp;
        }

        #endregion

        #region Spline & Bendy Course Geometry Generation

        private static Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            return 0.5f * (
                (2f * p1) +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3
            );
        }

        private static List<Vector3> GenerateSplinePoints(Vector3[] waypoints, int samplesPerSegment = 6)
        {
            List<Vector3> points = new List<Vector3>();
            int n = waypoints.Length;
            if (n < 2) return points;

            for (int i = 0; i < n - 1; i++)
            {
                Vector3 p0 = i > 0 ? waypoints[i - 1] : waypoints[0] + (waypoints[0] - waypoints[1]);
                Vector3 p1 = waypoints[i];
                Vector3 p2 = waypoints[i + 1];
                Vector3 p3 = (i + 2 < n) ? waypoints[i + 2] : waypoints[i + 1] + (waypoints[i + 1] - waypoints[i]);

                int count = (i == n - 2) ? samplesPerSegment + 1 : samplesPerSegment;
                for (int s = 0; s < count; s++)
                {
                    float t = (float)s / samplesPerSegment;
                    Vector3 pt = GetCatmullRomPosition(t, p0, p1, p2, p3);
                    pt.y = 0f;
                    points.Add(pt);
                }
            }
            return points;
        }

        public static GameObject CreateBendyHole(Transform parent, Vector3[] waypoints, float fairwayWidth, int holeNumber, out Vector3 teeSpawnPos, out Vector3 initialForward)
        {
            if (matPuttingGreen == null) InitializeMaterials();

            GameObject holeRoot = new GameObject($"BendyCourse_{holeNumber}");
            holeRoot.transform.SetParent(parent, false);

            List<Vector3> points = GenerateSplinePoints(waypoints, 6);
            int M = points.Count;

            Vector3[] leftPts = new Vector3[M];
            Vector3[] rightPts = new Vector3[M];

            bool isBridgeHole = (holeNumber == 5);
            float[] nodeY = new float[M];
            for (int k = 0; k < M; k++)
            {
                float prog = (float)k / (M - 1);
                if (isBridgeHole)
                {
                    if (prog < 0.22f)
                        nodeY[k] = Mathf.Lerp(0.02f, 0.55f, prog / 0.22f);
                    else if (prog <= 0.68f)
                        nodeY[k] = 0.55f;
                    else if (prog < 0.86f)
                        nodeY[k] = Mathf.Lerp(0.55f, 0.02f, (prog - 0.68f) / 0.18f);
                    else
                        nodeY[k] = 0.02f;
                }
                else
                {
                    nodeY[k] = 0.02f;
                }
            }

            for (int k = 0; k < M; k++)
            {
                Vector3 forward;
                if (k == 0) forward = (points[1] - points[0]).normalized;
                else if (k == M - 1) forward = (points[M - 1] - points[M - 2]).normalized;
                else forward = (points[k + 1] - points[k - 1]).normalized;
                forward.y = 0f;
                forward.Normalize();

                Vector3 normal = new Vector3(forward.z, 0f, -forward.x);

                float progress = (float)k / (M - 1);
                float w = fairwayWidth;
                if (progress < 0.15f)
                {
                    float t = progress / 0.15f;
                    w = Mathf.Lerp(fairwayWidth * 1.15f, fairwayWidth, t);
                }
                else if (progress > 0.75f)
                {
                    float t = (progress - 0.75f) / 0.25f;
                    w = Mathf.Lerp(fairwayWidth, fairwayWidth * 1.45f, t);
                }

                leftPts[k] = new Vector3(points[k].x - normal.x * (w * 0.5f), nodeY[k], points[k].z - normal.z * (w * 0.5f));
                rightPts[k] = new Vector3(points[k].x + normal.x * (w * 0.5f), nodeY[k], points[k].z + normal.z * (w * 0.5f));
            }

            // 1. Putting Green / Wooden Deck Surface Mesh
            GameObject turfObj = new GameObject("TurfSurface");
            turfObj.transform.SetParent(holeRoot.transform, false);

            Mesh turfMesh = new Mesh();
            turfMesh.name = $"TurfMesh_{holeNumber}";

            Vector3[] turfVerts = new Vector3[2 * M];
            Vector2[] turfUvs = new Vector2[2 * M];
            float accumDist = 0f;

            for (int k = 0; k < M; k++)
            {
                if (k > 0) accumDist += Vector3.Distance(points[k], points[k - 1]);
                turfVerts[2 * k] = leftPts[k];
                turfVerts[2 * k + 1] = rightPts[k];

                turfUvs[2 * k] = new Vector2(0f, accumDist / fairwayWidth);
                turfUvs[2 * k + 1] = new Vector2(1f, accumDist / fairwayWidth);
            }

            List<int> turfTris = new List<int>((M - 1) * 6);
            for (int k = 0; k < M - 1; k++)
            {
                int i0 = 2 * k;
                int i1 = 2 * (k + 1);
                int i2 = 2 * k + 1;
                int i3 = 2 * (k + 1) + 1;

                turfTris.Add(i0);
                turfTris.Add(i1);
                turfTris.Add(i2);

                turfTris.Add(i2);
                turfTris.Add(i1);
                turfTris.Add(i3);
            }

            turfMesh.vertices = turfVerts;
            turfMesh.uv = turfUvs;
            turfMesh.triangles = turfTris.ToArray();
            turfMesh.RecalculateNormals();
            turfMesh.RecalculateBounds();

            MeshFilter mf = turfObj.AddComponent<MeshFilter>();
            mf.sharedMesh = turfMesh;
            MeshRenderer mr = turfObj.AddComponent<MeshRenderer>();
            mr.sharedMaterial = isBridgeHole ? matCedarWood : matPuttingGreen;

            MeshCollider mc = turfObj.AddComponent<MeshCollider>();
            mc.sharedMesh = turfMesh;
            mc.material = turfPhysMat;

            // 2. Foundation Stone / Timber Base
            GameObject baseObj = new GameObject("FoundationBase");
            baseObj.transform.SetParent(holeRoot.transform, false);

            Mesh baseMesh = new Mesh();
            baseMesh.name = $"BaseMesh_{holeNumber}";
            Vector3[] baseVerts = new Vector3[2 * M];
            for (int k = 0; k < M; k++)
            {
                baseVerts[2 * k] = leftPts[k] - Vector3.up * 0.02f;
                baseVerts[2 * k + 1] = rightPts[k] - Vector3.up * 0.02f;
            }
            baseMesh.vertices = baseVerts;
            baseMesh.triangles = turfTris.ToArray();
            baseMesh.RecalculateNormals();

            MeshFilter bmf = baseObj.AddComponent<MeshFilter>();
            bmf.sharedMesh = baseMesh;
            MeshRenderer bmr = baseObj.AddComponent<MeshRenderer>();
            bmr.sharedMaterial = isBridgeHole ? matCedarWood : matStoneBase;

            // 3. Sturdy Curbs / Bridge Guide Rails along outer boundaries
            GameObject curbsRoot = new GameObject("BrickRails");
            curbsRoot.transform.SetParent(holeRoot.transform, false);

            float railHeight = isBridgeHole ? 0.48f : 0.40f;
            float railThick = 0.24f;
            Material railMat = isBridgeHole ? matCedarWood : matBrickRail;

            // Left Rails
            for (int k = 0; k < M - 1; k++)
            {
                Vector3 a = leftPts[k];
                Vector3 b = leftPts[k + 1];
                Vector3 seg = b - a;
                float len = seg.magnitude;
                if (len < 0.05f) continue;

                GameObject curb = GameObject.CreatePrimitive(PrimitiveType.Cube);
                curb.name = $"Rail_L_{k}";
                curb.transform.SetParent(curbsRoot.transform, false);
                curb.transform.localPosition = (a + b) * 0.5f + Vector3.up * (railHeight * 0.5f);
                curb.transform.localRotation = Quaternion.LookRotation(seg.normalized, Vector3.up);
                curb.transform.localScale = new Vector3(railThick, railHeight, len + 0.12f);
                curb.GetComponent<MeshRenderer>().sharedMaterial = railMat;
                curb.GetComponent<BoxCollider>().material = railPhysMat;
            }

            // Right Rails
            for (int k = 0; k < M - 1; k++)
            {
                Vector3 a = rightPts[k];
                Vector3 b = rightPts[k + 1];
                Vector3 seg = b - a;
                float len = seg.magnitude;
                if (len < 0.05f) continue;

                GameObject curb = GameObject.CreatePrimitive(PrimitiveType.Cube);
                curb.name = $"Rail_R_{k}";
                curb.transform.SetParent(curbsRoot.transform, false);
                curb.transform.localPosition = (a + b) * 0.5f + Vector3.up * (railHeight * 0.5f);
                curb.transform.localRotation = Quaternion.LookRotation(seg.normalized, Vector3.up);
                curb.transform.localScale = new Vector3(railThick, railHeight, len + 0.12f);
                curb.GetComponent<MeshRenderer>().sharedMaterial = railMat;
                curb.GetComponent<BoxCollider>().material = railPhysMat;
            }

            // Start Cap (behind tee)
            Vector3 startCapSeg = rightPts[0] - leftPts[0];
            GameObject startCap = GameObject.CreatePrimitive(PrimitiveType.Cube);
            startCap.name = "Rail_StartCap";
            startCap.transform.SetParent(curbsRoot.transform, false);
            startCap.transform.localPosition = (leftPts[0] + rightPts[0]) * 0.5f + Vector3.up * (railHeight * 0.5f);
            startCap.transform.localRotation = Quaternion.LookRotation(startCapSeg.normalized, Vector3.up);
            startCap.transform.localScale = new Vector3(railThick, railHeight, startCapSeg.magnitude + 0.25f);
            startCap.GetComponent<MeshRenderer>().sharedMaterial = railMat;
            startCap.GetComponent<BoxCollider>().material = railPhysMat;

            // End Cap (behind cup)
            Vector3 endCapSeg = rightPts[M - 1] - leftPts[M - 1];
            GameObject endCap = GameObject.CreatePrimitive(PrimitiveType.Cube);
            endCap.name = "Rail_EndCap";
            endCap.transform.SetParent(curbsRoot.transform, false);
            endCap.transform.localPosition = (leftPts[M - 1] + rightPts[M - 1]) * 0.5f + Vector3.up * (railHeight * 0.5f);
            endCap.transform.localRotation = Quaternion.LookRotation(endCapSeg.normalized, Vector3.up);
            endCap.transform.localScale = new Vector3(railThick, railHeight, endCapSeg.magnitude + 0.25f);
            endCap.GetComponent<MeshRenderer>().sharedMaterial = railMat;
            endCap.GetComponent<BoxCollider>().material = railPhysMat;

            // Bridge Support Trestles for Hole 5
            if (isBridgeHole)
            {
                GameObject trestleRoot = new GameObject("BridgeSupportTrestles");
                trestleRoot.transform.SetParent(holeRoot.transform, false);
                for (int k = 0; k < M; k += 3)
                {
                    float prog = (float)k / (M - 1);
                    if (prog >= 0.18f && prog <= 0.72f)
                    {
                        GameObject postL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        postL.name = "BridgePost_L";
                        postL.transform.SetParent(trestleRoot.transform, false);
                        postL.transform.localScale = new Vector3(0.24f, nodeY[k] * 0.5f, 0.24f);
                        postL.transform.localPosition = new Vector3(leftPts[k].x, nodeY[k] * 0.5f, leftPts[k].z);
                        postL.GetComponent<MeshRenderer>().sharedMaterial = matCedarWood;
                        Object.DestroyImmediate(postL.GetComponent<Collider>());

                        GameObject postR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        postR.name = "BridgePost_R";
                        postR.transform.SetParent(trestleRoot.transform, false);
                        postR.transform.localScale = new Vector3(0.24f, nodeY[k] * 0.5f, 0.24f);
                        postR.transform.localPosition = new Vector3(rightPts[k].x, nodeY[k] * 0.5f, rightPts[k].z);
                        postR.GetComponent<MeshRenderer>().sharedMaterial = matCedarWood;
                        Object.DestroyImmediate(postR.GetComponent<Collider>());
                    }
                }
            }

            // 4. Tee & Big Sunken Cup Placement
            Vector3 startDir = (points[1] - points[0]).normalized;
            Vector3 teePos = points[0] + startDir * 1.6f;
            teePos.y = nodeY[0];
            CreateTeeMat(holeRoot.transform, teePos);

            Vector3 endDir = (points[M - 1] - points[M - 2]).normalized;
            Vector3 cupPos = points[M - 1] - endDir * 2.2f;
            cupPos.y = nodeY[M - 1] + 0.02f;
            CreateHole(holeRoot.transform, cupPos, holeNumber);

            teeSpawnPos = teePos + Vector3.up * 0.19f;
            initialForward = startDir;

            return holeRoot;
        }

        #endregion

        #region 5 Championship Courses Construction (Gentle Curves, Flat, Zero Trees, Zero Water)

        public static GameObject BuildSpecificCourse(Transform parentRoot, int courseIndex, out Vector3 spawnPoint, out Vector3 initialForward)
        {
            InitializeMaterials();

            int courseNum = courseIndex + 1;
            GameObject course;

            switch (courseNum)
            {
                case 1:
                    // Hole 1: Peaceful garden with trees, flowering cherry/birch trees, and flowerbeds (Par 2)
                    Vector3[] c1Waypoints = new Vector3[]
                    {
                        new Vector3(26f, 0f, -18f),
                        new Vector3(26f, 0f, -8f),
                        new Vector3(26.5f, 0f, 2f),
                        new Vector3(27f, 0f, 11f)
                    };
                    course = CreateBendyHole(parentRoot, c1Waypoints, 4.0f, 1, out spawnPoint, out initialForward);
                    course.name = "Hole_1";
                    ParkEnvironmentBuilder.DecorateHole1_Garden(course.transform);
                    break;

                case 2:
                    // Hole 2: Curved pathway surrounded by greenery and a small pond (Par 3)
                    Vector3[] c2Waypoints = new Vector3[]
                    {
                        new Vector3(18f, 0f, 15f),
                        new Vector3(8f, 0f, 20f),
                        new Vector3(-2f, 0f, 19f),
                        new Vector3(-12f, 0f, 14f)
                    };
                    course = CreateBendyHole(parentRoot, c2Waypoints, 4.0f, 2, out spawnPoint, out initialForward);
                    course.name = "Hole_2";
                    ParkEnvironmentBuilder.DecorateHole2_CurvedPond(course.transform);
                    break;

                case 3:
                    // Hole 3: S-shaped track with trees, bushes and decorative rocks (Par 3)
                    Vector3[] c3Waypoints = new Vector3[]
                    {
                        new Vector3(-24f, 0f, 18f),
                        new Vector3(-20f, 0f, 9f),
                        new Vector3(-26f, 0f, 0f),
                        new Vector3(-22f, 0f, -10f)
                    };
                    course = CreateBendyHole(parentRoot, c3Waypoints, 4.0f, 3, out spawnPoint, out initialForward);
                    course.name = "Hole_3";
                    ParkEnvironmentBuilder.DecorateHole3_S_Track(course.transform);
                    break;

                case 4:
                    // Hole 4: C-shaped track surrounded by a beautiful landscaped pond (Par 3)
                    Vector3[] c4Waypoints = new Vector3[]
                    {
                        new Vector3(-12f, 0f, 7f),
                        new Vector3(-5f, 0f, -1f),
                        new Vector3(-5f, 0f, -9f),
                        new Vector3(-12f, 0f, -17f)
                    };
                    course = CreateBendyHole(parentRoot, c4Waypoints, 4.0f, 4, out spawnPoint, out initialForward);
                    course.name = "Hole_4";
                    ParkEnvironmentBuilder.DecorateHole4_C_Pond(course.transform);
                    break;

                case 5:
                default:
                    // Hole 5: Elevated wooden bridge crossing a pond with trees and scenery around it (Par 3)
                    Vector3[] c5Waypoints = new Vector3[]
                    {
                        new Vector3(10f, 0f, -15f),
                        new Vector3(10f, 0f, -6f),
                        new Vector3(10f, 0f, 4f),
                        new Vector3(10f, 0f, 13f)
                    };
                    course = CreateBendyHole(parentRoot, c5Waypoints, 4.0f, 5, out spawnPoint, out initialForward);
                    course.name = "Hole_5";
                    ParkEnvironmentBuilder.DecorateHole5_ElevatedBridge(course.transform);
                    break;
            }

            return course;
        }

        public static GameObject BuildSpecificCourse(Transform parentRoot, int courseIndex, out Vector3 spawnPoint)
        {
            Vector3 dummyForward;
            return BuildSpecificCourse(parentRoot, courseIndex, out spawnPoint, out dummyForward);
        }

        public static void Build18HolePark(Transform parentRoot, out List<GameObject> holeContainers, out List<Transform> spawnPoints, out Transform practiceSpawn)
        {
            holeContainers = new List<GameObject>();
            spawnPoints = new List<Transform>();

            // 1. Shared Landscape with Practice Green (Zero Trees, Zero Water)
            CreateParkLandscape(parentRoot, out practiceSpawn);

            // 2. The 5 Championship Bendy Courses
            GameObject holesRoot = new GameObject("AllHoles");
            holesRoot.transform.SetParent(parentRoot, false);

            for (int i = 0; i < 5; i++)
            {
                Vector3 spawnPos;
                Vector3 initialForward;
                GameObject holeObj = BuildSpecificCourse(holesRoot.transform, i, out spawnPos, out initialForward);
                holeContainers.Add(holeObj);

                GameObject spawnObj = new GameObject($"Spawn_{i + 1}");
                spawnObj.transform.SetParent(parentRoot, false);
                spawnObj.transform.position = spawnPos;
                spawnObj.transform.rotation = Quaternion.LookRotation(initialForward, Vector3.up);
                spawnPoints.Add(spawnObj.transform);
            }
        }

        #endregion
    }
}
