using System.Collections.Generic;
using UnityEngine;

namespace AntiGravityGolf
{
    /// <summary>
    /// Procedurally constructs rich natural environments and hole-specific scenery:
    /// - Diverse 3D tree varieties (Pine/Evergreen, Oak/Deciduous, Birch, Flowering Cherry Blossom)
    /// - Bushes, flowerbeds (red poppies, yellow marigolds, purple hydrangeas), grass tufts
    /// - Decorative wooden park benches, post-and-rail fences, and granite boulder clusters
    /// - Beautiful blue ponds with animated water, floating lily pads, shoreline rocks, reeds, and water hazard triggers
    /// - Hole-specific themed scenery kept safely outside playable pathways
    /// </summary>
    public static class ParkEnvironmentBuilder
    {
        // Shared Materials
        private static Material matOakFoliage;
        private static Material matPineFoliage;
        private static Material matBirchFoliage;
        private static Material matCherryFoliage;
        private static Material matBarkBrown;
        private static Material matBirchBark;
        private static Material matBushGreen;
        private static Material matFlowerRed;
        private static Material matFlowerYellow;
        private static Material matFlowerPurple;
        private static Material matFlowerWhite;
        private static Material matLilyPad;
        private static Material matReeds;
        private static Material matCattail;
        private static Material matPondWater;
        private static Material matGraniteRock;
        private static Material matBenchWood;
        private static Material matBenchIron;
        private static Material matFenceWood;

        private static bool isInitialized = false;

        public static void InitializeMaterials()
        {
            if (isInitialized && matOakFoliage != null) return;

            Shader standardShader = Shader.Find("Standard") ?? Shader.Find("Mobile/Diffuse") ?? Shader.Find("Sprites/Default");

            // Foliage
            matOakFoliage = CreateMat(standardShader, "Mat_OakFoliage", new Color(0.18f, 0.58f, 0.22f), 0.35f);
            matPineFoliage = CreateMat(standardShader, "Mat_PineFoliage", new Color(0.10f, 0.38f, 0.20f), 0.25f);
            matBirchFoliage = CreateMat(standardShader, "Mat_BirchFoliage", new Color(0.32f, 0.72f, 0.28f), 0.35f);
            matCherryFoliage = CreateMat(standardShader, "Mat_CherryBlossom", new Color(0.96f, 0.65f, 0.76f), 0.40f);

            // Trunks & Wood
            matBarkBrown = CreateMat(standardShader, "Mat_BarkBrown", new Color(0.38f, 0.24f, 0.14f), 0.20f);
            matBirchBark = CreateMat(standardShader, "Mat_BirchBark", new Color(0.92f, 0.90f, 0.85f), 0.45f);
            matBenchWood = CreateMat(standardShader, "Mat_BenchWood", new Color(0.62f, 0.36f, 0.18f), 0.60f);
            matBenchIron = CreateMat(standardShader, "Mat_BenchIron", new Color(0.16f, 0.16f, 0.18f), 0.70f);
            matFenceWood = CreateMat(standardShader, "Mat_FenceWood", new Color(0.55f, 0.42f, 0.28f), 0.30f);

            // Shrubbery & Flowers
            matBushGreen = CreateMat(standardShader, "Mat_BushGreen", new Color(0.24f, 0.64f, 0.26f), 0.30f);
            matFlowerRed = CreateMat(standardShader, "Mat_FlowerRed", new Color(0.92f, 0.15f, 0.18f), 0.50f);
            matFlowerYellow = CreateMat(standardShader, "Mat_FlowerYellow", new Color(0.98f, 0.84f, 0.16f), 0.50f);
            matFlowerPurple = CreateMat(standardShader, "Mat_FlowerPurple", new Color(0.65f, 0.25f, 0.85f), 0.50f);
            matFlowerWhite = CreateMat(standardShader, "Mat_FlowerWhite", new Color(0.96f, 0.96f, 0.96f), 0.50f);

            // Water & Wetland
            matPondWater = new Material(standardShader) { name = "Mat_PondWater" };
            matPondWater.color = new Color(0.12f, 0.58f, 0.78f, 0.88f);
            if (matPondWater.HasProperty("_Glossiness")) matPondWater.SetFloat("_Glossiness", 0.95f);
            if (matPondWater.HasProperty("_Mode"))
            {
                matPondWater.SetFloat("_Mode", 3);
                matPondWater.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                matPondWater.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                matPondWater.SetInt("_ZWrite", 0);
                matPondWater.EnableKeyword("_ALPHABLEND_ON");
                matPondWater.renderQueue = 3000;
            }

            matLilyPad = CreateMat(standardShader, "Mat_LilyPad", new Color(0.25f, 0.70f, 0.28f), 0.45f);
            matReeds = CreateMat(standardShader, "Mat_Reeds", new Color(0.30f, 0.55f, 0.22f), 0.25f);
            matCattail = CreateMat(standardShader, "Mat_Cattail", new Color(0.40f, 0.22f, 0.12f), 0.30f);

            // Rocks
            matGraniteRock = CreateMat(standardShader, "Mat_GraniteRock", new Color(0.50f, 0.52f, 0.50f), 0.40f);

            isInitialized = true;
        }

        private static Material CreateMat(Shader shader, string name, Color color, float gloss)
        {
            Material mat = new Material(shader) { name = name };
            mat.color = color;
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", gloss);
            return mat;
        }

        #region 3D Tree Varieties

        public static GameObject CreatePineTree(Transform parent, Vector3 localPos, float scale = 1.0f)
        {
            InitializeMaterials();

            GameObject tree = new GameObject("PineTree");
            tree.transform.SetParent(parent, false);
            tree.transform.localPosition = localPos;
            tree.transform.localScale = Vector3.one * scale;

            // Trunk
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.SetParent(tree.transform, false);
            trunk.transform.localScale = new Vector3(0.38f, 1.4f, 0.38f);
            trunk.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            trunk.GetComponent<MeshRenderer>().sharedMaterial = matBarkBrown;

            // 3 Stacked Conical Foliage Layers
            float[] tierY = { 2.4f, 3.8f, 5.0f };
            float[] tierRadius = { 2.2f, 1.7f, 1.1f };
            float[] tierHeight = { 1.6f, 1.5f, 1.3f };

            for (int t = 0; t < 3; t++)
            {
                GameObject cone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cone.name = $"FoliageCone_{t}";
                cone.transform.SetParent(tree.transform, false);
                cone.transform.localScale = new Vector3(tierRadius[t], tierHeight[t] * 0.5f, tierRadius[t]);
                cone.transform.localPosition = new Vector3(0f, tierY[t], 0f);
                cone.GetComponent<MeshRenderer>().sharedMaterial = matPineFoliage;
                Object.DestroyImmediate(cone.GetComponent<Collider>());
            }

            return tree;
        }

        public static GameObject CreateOakTree(Transform parent, Vector3 localPos, float scale = 1.0f)
        {
            InitializeMaterials();

            GameObject tree = new GameObject("OakTree");
            tree.transform.SetParent(parent, false);
            tree.transform.localPosition = localPos;
            tree.transform.localScale = Vector3.one * scale;

            // Trunk
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.SetParent(tree.transform, false);
            trunk.transform.localScale = new Vector3(0.50f, 1.6f, 0.50f);
            trunk.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            trunk.GetComponent<MeshRenderer>().sharedMaterial = matBarkBrown;

            // Clustered Leafy Crown (5 overlapping spheres)
            Vector3[] leafOffsets = {
                new Vector3(0f, 3.8f, 0f),
                new Vector3(0.9f, 3.4f, 0.6f),
                new Vector3(-0.8f, 3.5f, -0.5f),
                new Vector3(-0.6f, 3.3f, 0.8f),
                new Vector3(0.7f, 3.6f, -0.7f)
            };
            float[] leafRadii = { 2.6f, 2.1f, 2.0f, 1.9f, 2.1f };

            for (int i = 0; i < leafOffsets.Length; i++)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.name = $"LeafSphere_{i}";
                sphere.transform.SetParent(tree.transform, false);
                sphere.transform.localScale = Vector3.one * leafRadii[i];
                sphere.transform.localPosition = leafOffsets[i];
                sphere.GetComponent<MeshRenderer>().sharedMaterial = matOakFoliage;
                Object.DestroyImmediate(sphere.GetComponent<Collider>());
            }

            return tree;
        }

        public static GameObject CreateBirchTree(Transform parent, Vector3 localPos, float scale = 1.0f)
        {
            InitializeMaterials();

            GameObject tree = new GameObject("BirchTree");
            tree.transform.SetParent(parent, false);
            tree.transform.localPosition = localPos;
            tree.transform.localScale = Vector3.one * scale;

            // Slender white trunk
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.SetParent(tree.transform, false);
            trunk.transform.localScale = new Vector3(0.28f, 2.4f, 0.28f);
            trunk.transform.localPosition = new Vector3(0f, 2.4f, 0f);
            trunk.GetComponent<MeshRenderer>().sharedMaterial = matBirchBark;

            // Tall oval canopy
            GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy.name = "BirchCanopy";
            canopy.transform.SetParent(tree.transform, false);
            canopy.transform.localScale = new Vector3(2.0f, 3.4f, 2.0f);
            canopy.transform.localPosition = new Vector3(0f, 4.8f, 0f);
            canopy.GetComponent<MeshRenderer>().sharedMaterial = matBirchFoliage;
            Object.DestroyImmediate(canopy.GetComponent<Collider>());

            return tree;
        }

        public static GameObject CreateCherryBlossomTree(Transform parent, Vector3 localPos, float scale = 1.0f)
        {
            InitializeMaterials();

            GameObject tree = new GameObject("CherryBlossomTree");
            tree.transform.SetParent(parent, false);
            tree.transform.localPosition = localPos;
            tree.transform.localScale = Vector3.one * scale;

            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.SetParent(tree.transform, false);
            trunk.transform.localScale = new Vector3(0.42f, 1.5f, 0.42f);
            trunk.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            trunk.transform.localRotation = Quaternion.Euler(4f, 0f, 3f);
            trunk.GetComponent<MeshRenderer>().sharedMaterial = matBarkBrown;

            // Soft pink blossom canopy
            Vector3[] petalOffsets = {
                new Vector3(0f, 3.6f, 0f),
                new Vector3(0.8f, 3.3f, 0.5f),
                new Vector3(-0.7f, 3.4f, -0.4f),
                new Vector3(0.2f, 4.0f, -0.6f)
            };
            float[] petalRadii = { 2.4f, 1.9f, 1.8f, 1.9f };

            for (int i = 0; i < petalOffsets.Length; i++)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.name = $"BlossomSphere_{i}";
                sphere.transform.SetParent(tree.transform, false);
                sphere.transform.localScale = Vector3.one * petalRadii[i];
                sphere.transform.localPosition = petalOffsets[i];
                sphere.GetComponent<MeshRenderer>().sharedMaterial = matCherryFoliage;
                Object.DestroyImmediate(sphere.GetComponent<Collider>());
            }

            return tree;
        }

        #endregion

        #region Bushes, Flowers & Decorative Props

        public static GameObject CreateBush(Transform parent, Vector3 localPos, float scale = 1.0f)
        {
            InitializeMaterials();

            GameObject bush = new GameObject("Bush");
            bush.transform.SetParent(parent, false);
            bush.transform.localPosition = localPos;

            Vector3[] clusterOffsets = {
                new Vector3(0f, 0.45f * scale, 0f),
                new Vector3(0.35f * scale, 0.35f * scale, 0.2f * scale),
                new Vector3(-0.3f * scale, 0.35f * scale, -0.2f * scale)
            };

            for (int i = 0; i < clusterOffsets.Length; i++)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.name = $"BushNode_{i}";
                sphere.transform.SetParent(bush.transform, false);
                sphere.transform.localScale = new Vector3(0.9f * scale, 0.75f * scale, 0.9f * scale);
                sphere.transform.localPosition = clusterOffsets[i];
                sphere.GetComponent<MeshRenderer>().sharedMaterial = matBushGreen;
                Object.DestroyImmediate(sphere.GetComponent<Collider>());
            }

            return bush;
        }

        public static GameObject CreateFlowerBed(Transform parent, Vector3 localPos, Vector2 size, int count = 8)
        {
            InitializeMaterials();

            GameObject bed = new GameObject("FlowerBed");
            bed.transform.SetParent(parent, false);
            bed.transform.localPosition = localPos;

            // Border curb
            GameObject baseRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            baseRing.name = "BedBorder";
            baseRing.transform.SetParent(bed.transform, false);
            baseRing.transform.localScale = new Vector3(size.x, 0.05f, size.y);
            baseRing.transform.localPosition = new Vector3(0f, 0.025f, 0f);
            baseRing.GetComponent<MeshRenderer>().sharedMaterial = matGraniteRock;
            Object.DestroyImmediate(baseRing.GetComponent<Collider>());

            Material[] flowers = { matFlowerRed, matFlowerYellow, matFlowerPurple, matFlowerWhite };

            for (int i = 0; i < count; i++)
            {
                float angle = (float)i / count * Mathf.PI * 2f;
                float r = Random.Range(0.2f, 0.45f);
                float x = Mathf.Cos(angle) * (size.x * r);
                float z = Mathf.Sin(angle) * (size.y * r);

                GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stem.name = "FlowerStem";
                stem.transform.SetParent(bed.transform, false);
                stem.transform.localScale = new Vector3(0.04f, 0.12f, 0.04f);
                stem.transform.localPosition = new Vector3(x, 0.12f, z);
                stem.GetComponent<MeshRenderer>().sharedMaterial = matBushGreen;
                Object.DestroyImmediate(stem.GetComponent<Collider>());

                GameObject blossom = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                blossom.name = "Petals";
                blossom.transform.SetParent(bed.transform, false);
                blossom.transform.localScale = new Vector3(0.22f, 0.16f, 0.22f);
                blossom.transform.localPosition = new Vector3(x, 0.24f, z);
                blossom.GetComponent<MeshRenderer>().sharedMaterial = flowers[i % flowers.Length];
                Object.DestroyImmediate(blossom.GetComponent<Collider>());
            }

            return bed;
        }

        public static GameObject CreateParkBench(Transform parent, Vector3 localPos, Quaternion localRot)
        {
            InitializeMaterials();

            GameObject bench = new GameObject("ParkBench");
            bench.transform.SetParent(parent, false);
            bench.transform.localPosition = localPos;
            bench.transform.localRotation = localRot;

            float width = 1.8f;
            float seatHeight = 0.52f;
            float seatDepth = 0.55f;

            // Wooden Seat Plank
            GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seat.name = "BenchSeat";
            seat.transform.SetParent(bench.transform, false);
            seat.transform.localScale = new Vector3(width, 0.08f, seatDepth);
            seat.transform.localPosition = new Vector3(0f, seatHeight, 0f);
            seat.GetComponent<MeshRenderer>().sharedMaterial = matBenchWood;

            // Wooden Backrest
            GameObject back = GameObject.CreatePrimitive(PrimitiveType.Cube);
            back.name = "BenchBackrest";
            back.transform.SetParent(bench.transform, false);
            back.transform.localScale = new Vector3(width, 0.38f, 0.07f);
            back.transform.localPosition = new Vector3(0f, seatHeight + 0.32f, -seatDepth * 0.45f);
            back.transform.localRotation = Quaternion.Euler(-8f, 0f, 0f);
            back.GetComponent<MeshRenderer>().sharedMaterial = matBenchWood;

            // Iron Legs (Left & Right)
            float[] legX = { -width * 0.44f, width * 0.44f };
            foreach (float lx in legX)
            {
                GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leg.name = "IronLeg";
                leg.transform.SetParent(bench.transform, false);
                leg.transform.localScale = new Vector3(0.08f, seatHeight, seatDepth * 0.85f);
                leg.transform.localPosition = new Vector3(lx, seatHeight * 0.5f, 0f);
                leg.GetComponent<MeshRenderer>().sharedMaterial = matBenchIron;
            }

            return bench;
        }

        public static GameObject CreateWoodenFence(Transform parent, Vector3 start, Vector3 end)
        {
            InitializeMaterials();

            GameObject fence = new GameObject("WoodenFence");
            fence.transform.SetParent(parent, false);

            float length = Vector3.Distance(start, end);
            Vector3 dir = (end - start).normalized;
            Quaternion rot = (dir.sqrMagnitude > 0.01f) ? Quaternion.LookRotation(dir, Vector3.up) : Quaternion.identity;

            int postCount = Mathf.Max(2, Mathf.RoundToInt(length / 2.2f) + 1);
            for (int p = 0; p < postCount; p++)
            {
                float t = (float)p / (postCount - 1);
                Vector3 postPos = Vector3.Lerp(start, end, t);

                GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.name = $"FencePost_{p}";
                post.transform.SetParent(fence.transform, false);
                post.transform.localPosition = postPos + Vector3.up * 0.5f;
                post.transform.localScale = new Vector3(0.14f, 0.5f, 0.14f);
                post.GetComponent<MeshRenderer>().sharedMaterial = matFenceWood;
            }

            // Top and Bottom Cross Rails
            float[] railHeights = { 0.42f, 0.78f };
            Vector3 mid = (start + end) * 0.5f;

            for (int r = 0; r < railHeights.Length; r++)
            {
                GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rail.name = $"FenceRail_{r}";
                rail.transform.SetParent(fence.transform, false);
                rail.transform.localPosition = mid + Vector3.up * railHeights[r];
                rail.transform.localRotation = rot;
                rail.transform.localScale = new Vector3(0.08f, 0.08f, length);
                rail.GetComponent<MeshRenderer>().sharedMaterial = matFenceWood;
            }

            return fence;
        }

        public static GameObject CreateBoulder(Transform parent, Vector3 localPos, Vector3 scale, Quaternion rotation)
        {
            InitializeMaterials();

            GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rock.name = "GraniteBoulder";
            rock.transform.SetParent(parent, false);
            rock.transform.localPosition = localPos;
            rock.transform.localScale = scale;
            rock.transform.localRotation = rotation;
            rock.GetComponent<MeshRenderer>().sharedMaterial = matGraniteRock;

            return rock;
        }

        #endregion

        #region Ponds & Water Hazards

        public static GameObject CreatePond(Transform parent, Vector3 center, Vector2 size, bool withHazard = true)
        {
            InitializeMaterials();

            GameObject pond = new GameObject("LandscapePond");
            pond.transform.SetParent(parent, false);
            pond.transform.localPosition = center;

            // 1. Shimmering Blue Water Surface
            GameObject waterObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            waterObj.name = "WaterSurface";
            waterObj.transform.SetParent(pond.transform, false);
            waterObj.transform.localScale = new Vector3(size.x, 0.02f, size.y);
            waterObj.transform.localPosition = new Vector3(0f, 0.015f, 0f);
            waterObj.GetComponent<MeshRenderer>().sharedMaterial = matPondWater;
            waterObj.AddComponent<WaterSurfaceAnimation>();

            // Trigger Hazard Collider
            Object.DestroyImmediate(waterObj.GetComponent<Collider>());
            if (withHazard)
            {
                BoxCollider waterCol = waterObj.AddComponent<BoxCollider>();
                waterCol.isTrigger = true;
                waterCol.size = new Vector3(1.0f, 1.8f, 1.0f);
                waterCol.center = new Vector3(0f, -0.4f, 0f);
                waterObj.AddComponent<WaterHazard>();
            }

            // 2. Shoreline River Stones Ring
            int stoneCount = Mathf.Max(10, (int)((size.x + size.y) * 2.2f));
            for (int i = 0; i < stoneCount; i++)
            {
                float angle = (float)i / stoneCount * Mathf.PI * 2f;
                float rX = (size.x * 0.5f) + Random.Range(-0.15f, 0.25f);
                float rZ = (size.y * 0.5f) + Random.Range(-0.15f, 0.25f);
                float x = Mathf.Cos(angle) * rX;
                float z = Mathf.Sin(angle) * rZ;

                GameObject stone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                stone.name = "ShoreStone";
                stone.transform.SetParent(pond.transform, false);
                float stoneScale = Random.Range(0.45f, 0.85f);
                stone.transform.localScale = new Vector3(stoneScale, stoneScale * 0.55f, stoneScale * 0.85f);
                stone.transform.localPosition = new Vector3(x, stoneScale * 0.22f, z);
                stone.transform.localRotation = Random.rotation;
                stone.GetComponent<MeshRenderer>().sharedMaterial = matGraniteRock;
                Object.DestroyImmediate(stone.GetComponent<Collider>());
            }

            // 3. Floating Lily Pads
            int lilyCount = Random.Range(3, 6);
            for (int l = 0; l < lilyCount; l++)
            {
                float lx = Random.Range(-size.x * 0.32f, size.x * 0.32f);
                float lz = Random.Range(-size.y * 0.32f, size.y * 0.32f);

                GameObject lily = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                lily.name = "LilyPad";
                lily.transform.SetParent(pond.transform, false);
                lily.transform.localScale = new Vector3(0.55f, 0.005f, 0.55f);
                lily.transform.localPosition = new Vector3(lx, 0.022f, lz);
                lily.GetComponent<MeshRenderer>().sharedMaterial = matLilyPad;
                Object.DestroyImmediate(lily.GetComponent<Collider>());
            }

            // 4. Cattails / Reeds along edge
            int reedClusters = 3;
            for (int c = 0; c < reedClusters; c++)
            {
                float reedAngle = Random.Range(0f, Mathf.PI * 2f);
                float rx = Mathf.Cos(reedAngle) * (size.x * 0.44f);
                float rz = Mathf.Sin(reedAngle) * (size.y * 0.44f);

                for (int r = 0; r < 4; r++)
                {
                    Vector3 rPos = new Vector3(rx + Random.Range(-0.25f, 0.25f), 0.55f, rz + Random.Range(-0.25f, 0.25f));
                    GameObject reed = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    reed.name = "CattailReed";
                    reed.transform.SetParent(pond.transform, false);
                    reed.transform.localScale = new Vector3(0.04f, 0.55f, 0.04f);
                    reed.transform.localPosition = rPos;
                    reed.GetComponent<MeshRenderer>().sharedMaterial = matReeds;
                    Object.DestroyImmediate(reed.GetComponent<Collider>());

                    GameObject cattailHead = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    cattailHead.name = "CattailHead";
                    cattailHead.transform.SetParent(pond.transform, false);
                    cattailHead.transform.localScale = new Vector3(0.08f, 0.12f, 0.08f);
                    cattailHead.transform.localPosition = rPos + Vector3.up * 0.35f;
                    cattailHead.GetComponent<MeshRenderer>().sharedMaterial = matCattail;
                    Object.DestroyImmediate(cattailHead.GetComponent<Collider>());
                }
            }

            return pond;
        }

        #endregion

        #region Hole-Specific Environment Construction

        /// <summary>
        /// Hole 1: Peaceful garden with cherry trees, birch trees, flowers, park bench, and wooden fence.
        /// </summary>
        public static void DecorateHole1_Garden(Transform holeRoot)
        {
            GameObject decor = new GameObject("Hole1_GardenScenery");
            decor.transform.SetParent(holeRoot, false);

            // Flowering Cherry Blossom Trees along outer boundary
            CreateCherryBlossomTree(decor.transform, new Vector3(31.5f, 0f, -14f), 1.15f);
            CreateCherryBlossomTree(decor.transform, new Vector3(31.0f, 0f, -2f), 1.10f);
            CreateBirchTree(decor.transform, new Vector3(31.5f, 0f, 9f), 1.05f);
            CreateBirchTree(decor.transform, new Vector3(21.5f, 0f, 13f), 0.95f);

            // Flowerbeds flanking the fairway outside the rails
            CreateFlowerBed(decor.transform, new Vector3(30.0f, 0f, -8f), new Vector2(2.4f, 2.0f), 10);
            CreateFlowerBed(decor.transform, new Vector3(30.2f, 0f, 3f), new Vector2(2.2f, 1.8f), 8);
            CreateFlowerBed(decor.transform, new Vector3(22.0f, 0f, -6f), new Vector2(2.0f, 1.8f), 8);

            // Peaceful Wooden Park Bench facing the garden green
            CreateParkBench(decor.transform, new Vector3(30.5f, 0f, -18f), Quaternion.Euler(0f, -90f, 0f));

            // Rustic Garden Fence along the outer east border
            CreateWoodenFence(decor.transform, new Vector3(33.0f, 0f, -19f), new Vector3(33.0f, 0f, 12f));

            // Soft Leafy Bushes
            CreateBush(decor.transform, new Vector3(29.5f, 0f, -12f), 1.1f);
            CreateBush(decor.transform, new Vector3(29.8f, 0f, -4f), 1.0f);
            CreateBush(decor.transform, new Vector3(30.0f, 0f, 6f), 1.2f);
        }

        /// <summary>
        /// Hole 2: Curved pathway surrounded by greenery, oak trees, weeping willows, and a serene small pond.
        /// </summary>
        public static void DecorateHole2_CurvedPond(Transform holeRoot)
        {
            GameObject decor = new GameObject("Hole2_CurvedPondScenery");
            decor.transform.SetParent(holeRoot, false);

            // Serene Pond on the inner bend of the curve
            CreatePond(decor.transform, new Vector3(3.0f, 0f, 12.5f), new Vector2(8.0f, 5.5f), withHazard: true);

            // Lush Oak Trees & Deciduous trees around the curve
            CreateOakTree(decor.transform, new Vector3(19.5f, 0f, 22.5f), 1.15f);
            CreateOakTree(decor.transform, new Vector3(8.0f, 0f, 24.5f), 1.25f);
            CreateOakTree(decor.transform, new Vector3(-3.0f, 0f, 24.0f), 1.10f);
            CreateBirchTree(decor.transform, new Vector3(-14.5f, 0f, 19.5f), 1.05f);

            // Bushes and river boulders along outer north curve
            CreateBush(decor.transform, new Vector3(14.0f, 0f, 22.0f), 1.2f);
            CreateBush(decor.transform, new Vector3(1.0f, 0f, 23.0f), 1.3f);
            CreateBush(decor.transform, new Vector3(-8.0f, 0f, 20.0f), 1.1f);

            CreateBoulder(decor.transform, new Vector3(9.5f, 0.4f, 15.0f), new Vector3(1.4f, 1.0f, 1.2f), Quaternion.Euler(15f, 30f, 0f));
            CreateBoulder(decor.transform, new Vector3(-3.5f, 0.4f, 14.5f), new Vector3(1.6f, 1.1f, 1.3f), Quaternion.Euler(10f, -45f, 0f));

            // Wooden Fence along the high north scenic boundary
            CreateWoodenFence(decor.transform, new Vector3(18.0f, 0f, 24.0f), new Vector3(-12.0f, 0f, 24.0f));
        }

        /// <summary>
        /// Hole 3: S-shaped track with evergreen pine trees, dense bushes, and decorative granite rock clusters.
        /// </summary>
        public static void DecorateHole3_S_Track(Transform holeRoot)
        {
            GameObject decor = new GameObject("Hole3_S_TrackScenery");
            decor.transform.SetParent(holeRoot, false);

            // Tall Alpine Pine Trees in the grove
            CreatePineTree(decor.transform, new Vector3(-29.5f, 0f, 19.0f), 1.30f);
            CreatePineTree(decor.transform, new Vector3(-30.5f, 0f, 8.0f), 1.25f);
            CreatePineTree(decor.transform, new Vector3(-30.0f, 0f, -3.0f), 1.20f);
            CreatePineTree(decor.transform, new Vector3(-28.5f, 0f, -14.0f), 1.35f);
            CreateOakTree(decor.transform, new Vector3(-16.5f, 0f, 15.0f), 1.10f);

            // Decorative Granite Rock Formations in the S-curve pockets
            CreateBoulder(decor.transform, new Vector3(-24.5f, 0.6f, 14.0f), new Vector3(2.2f, 1.4f, 1.8f), Quaternion.Euler(20f, 40f, -10f));
            CreateBoulder(decor.transform, new Vector3(-17.0f, 0.5f, 4.5f), new Vector3(2.4f, 1.3f, 1.9f), Quaternion.Euler(-15f, 60f, 20f));
            CreateBoulder(decor.transform, new Vector3(-29.0f, 0.7f, -4.0f), new Vector3(2.6f, 1.6f, 2.1f), Quaternion.Euler(10f, -30f, 15f));
            CreateBoulder(decor.transform, new Vector3(-18.5f, 0.5f, -12.0f), new Vector3(1.8f, 1.1f, 1.5f), Quaternion.Euler(25f, 75f, 0f));

            // Dense Forest Bushes
            CreateBush(decor.transform, new Vector3(-28.0f, 0f, 14.0f), 1.3f);
            CreateBush(decor.transform, new Vector3(-17.5f, 0f, 9.0f), 1.2f);
            CreateBush(decor.transform, new Vector3(-28.5f, 0f, 2.0f), 1.4f);
            CreateBush(decor.transform, new Vector3(-18.0f, 0f, -6.0f), 1.2f);

            // Rustic boundary fence
            CreateWoodenFence(decor.transform, new Vector3(-31.5f, 0f, 20.0f), new Vector3(-31.5f, 0f, -15.0f));
        }

        /// <summary>
        /// Hole 4: C-shaped track surrounding a beautiful landscaped pond with water lilies, reeds, and birch trees.
        /// </summary>
        public static void DecorateHole4_C_Pond(Transform holeRoot)
        {
            GameObject decor = new GameObject("Hole4_C_PondScenery");
            decor.transform.SetParent(holeRoot, false);

            // Beautiful Landscaped Pond inside the curve of the C-shape
            CreatePond(decor.transform, new Vector3(-10.5f, 0f, -5.5f), new Vector2(7.0f, 8.0f), withHazard: true);

            // Birch & Weeping Trees surrounding the outer C-bend
            CreateBirchTree(decor.transform, new Vector3(-1.0f, 0f, 1.0f), 1.15f);
            CreateBirchTree(decor.transform, new Vector3(-1.0f, 0f, -11.0f), 1.10f);
            CreateOakTree(decor.transform, new Vector3(-16.5f, 0f, 11.0f), 1.15f);
            CreateOakTree(decor.transform, new Vector3(-16.0f, 0f, -22.0f), 1.20f);

            // Flowering shrubs and flowerbeds around pond
            CreateFlowerBed(decor.transform, new Vector3(-1.5f, 0f, -5.0f), new Vector2(2.2f, 2.0f), 8);
            CreateBush(decor.transform, new Vector3(-2.0f, 0f, 3.0f), 1.2f);
            CreateBush(decor.transform, new Vector3(-2.0f, 0f, -14.0f), 1.2f);

            // Wooden Park Bench facing the landscaped pond
            CreateParkBench(decor.transform, new Vector3(-1.0f, 0f, -5.5f), Quaternion.Euler(0f, -90f, 0f));

            // Decorative boulders along outer edge
            CreateBoulder(decor.transform, new Vector3(-1.5f, 0.4f, -0.5f), new Vector3(1.5f, 0.9f, 1.3f), Quaternion.Euler(10f, 20f, 0f));
            CreateBoulder(decor.transform, new Vector3(-1.5f, 0.4f, -9.5f), new Vector3(1.6f, 1.0f, 1.4f), Quaternion.Euler(-10f, 50f, 0f));
        }

        /// <summary>
        /// Hole 5: Elevated wooden bridge crossing a central pond with tall pines, water lilies, and shoreline scenery.
        /// </summary>
        public static void DecorateHole5_ElevatedBridge(Transform holeRoot)
        {
            GameObject decor = new GameObject("Hole5_ElevatedBridgeScenery");
            decor.transform.SetParent(holeRoot, false);

            // Central Scenic Pond directly underneath the wooden bridge
            CreatePond(decor.transform, new Vector3(10.0f, 0f, -1.0f), new Vector2(10.5f, 8.5f), withHazard: true);

            // Surrounding Pine & Oak Trees
            CreatePineTree(decor.transform, new Vector3(15.5f, 0f, -9.0f), 1.25f);
            CreatePineTree(decor.transform, new Vector3(15.5f, 0f, 6.0f), 1.30f);
            CreatePineTree(decor.transform, new Vector3(4.5f, 0f, -7.0f), 1.15f);
            CreateOakTree(decor.transform, new Vector3(4.5f, 0f, 5.0f), 1.20f);
            CreateBirchTree(decor.transform, new Vector3(10.0f, 0f, 16.5f), 1.10f);

            // Scenic Park Bench overlooking the lake and bridge
            CreateParkBench(decor.transform, new Vector3(15.5f, 0f, -1.0f), Quaternion.Euler(0f, -90f, 0f));

            // Bushes & Shoreline boulders
            CreateBush(decor.transform, new Vector3(15.0f, 0f, -5.0f), 1.2f);
            CreateBush(decor.transform, new Vector3(15.0f, 0f, 2.5f), 1.1f);
            CreateBush(decor.transform, new Vector3(5.0f, 0f, -1.0f), 1.3f);

            CreateBoulder(decor.transform, new Vector3(14.5f, 0.5f, -7.0f), new Vector3(1.8f, 1.1f, 1.5f), Quaternion.Euler(15f, 35f, 0f));
            CreateBoulder(decor.transform, new Vector3(5.5f, 0.5f, -5.0f), new Vector3(1.9f, 1.2f, 1.6f), Quaternion.Euler(-20f, 60f, 10f));
        }

        #endregion
    }
}
