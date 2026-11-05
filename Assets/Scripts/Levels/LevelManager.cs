using System.Collections.Generic;
using UnityEngine;

namespace AntiGravityGolf
{
    /// <summary>
    /// Manages the 18-hole mini-golf park where ALL holes are always visible simultaneously.
    /// Advancing to a new hole teleports the ball to that hole's tee and activates that hole's cup.
    /// Also supports the dedicated Practice Putting Green mode near the entrance.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private GolfBallController ball;
        [SerializeField] private GolfCameraController camController;

        [Header("18-Hole Park — All Always Visible")]
        [SerializeField] private List<GameObject> levelContainers = new List<GameObject>();
        [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

        [Header("Practice Green")]
        [SerializeField] private Transform practiceSpawnPoint;

        private int activeLevelIndex = 0;
        private bool isPractice = false;

        public GolfBallController Ball => ball;
        public int ActiveLevelIndex => activeLevelIndex;
        public bool IsPractice => isPractice;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (ball == null)
                ball = FindObjectOfType<GolfBallController>();
            if (camController == null)
                camController = FindObjectOfType<GolfCameraController>();

            // Ensure ALL hole containers are active — the full course is always visible
            foreach (var container in levelContainers)
            {
                if (container != null) container.SetActive(true);
            }

            int startingLevel = GameManager.Instance != null ? GameManager.Instance.CurrentLevelIndex : 0;
            if (GameManager.Instance != null && GameManager.Instance.IsPracticeMode)
            {
                LoadPracticeGreen();
            }
            else
            {
                LoadLevel(startingLevel);
            }
        }

        public void RegisterLevel(GameObject container, Transform spawn)
        {
            if (!levelContainers.Contains(container))
            {
                levelContainers.Add(container);
                spawnPoints.Add(spawn);
            }
        }

        public void SetPracticeSpawn(Transform spawn)
        {
            practiceSpawnPoint = spawn;
        }

        /// <summary>
        /// Move to a specific hole: keep all containers visible,
        /// teleport ball to that hole's tee and reset that hole's state.
        /// </summary>
        public void LoadLevel(int index)
        {
            isPractice = false;
            activeLevelIndex = Mathf.Clamp(index, 0, Mathf.Max(0, levelContainers.Count - 1));

            foreach (var container in levelContainers)
            {
                if (container != null) container.SetActive(true);
            }

            if (GravityManager.Instance != null)
                GravityManager.Instance.ResetToDefault(instant: true);

            Vector3 spawnPos = Vector3.zero;
            Vector3 lookDir = Vector3.forward;

            // 1. Locate the exact Tee on the active course
            Transform activeTee = null;
            if (activeLevelIndex < levelContainers.Count && levelContainers[activeLevelIndex] != null)
            {
                foreach (Transform t in levelContainers[activeLevelIndex].GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == "TeeSpot" || t.name == "TeeArea" || t.name == "TeeMat")
                    {
                        activeTee = t;
                        break;
                    }
                }
            }

            if (activeTee != null)
            {
                spawnPos = new Vector3(activeTee.position.x, activeTee.position.y + 0.19f, activeTee.position.z);
            }
            else if (activeLevelIndex < spawnPoints.Count && spawnPoints[activeLevelIndex] != null)
            {
                spawnPos = spawnPoints[activeLevelIndex].position;
            }
            else if (activeLevelIndex < levelContainers.Count && levelContainers[activeLevelIndex] != null)
            {
                spawnPos = levelContainers[activeLevelIndex].transform.position + Vector3.up * 0.2f;
            }

            // 2. Aim direction towards the cup down the fairway
            GolfHole targetHole = null;
            if (activeLevelIndex < levelContainers.Count && levelContainers[activeLevelIndex] != null)
            {
                targetHole = levelContainers[activeLevelIndex].GetComponentInChildren<GolfHole>(true);
            }

            if (targetHole != null)
            {
                Vector3 toCup = targetHole.transform.position - spawnPos;
                toCup.y = 0f;
                if (toCup.sqrMagnitude > 0.1f) lookDir = toCup.normalized;
            }
            else if (activeLevelIndex < spawnPoints.Count && spawnPoints[activeLevelIndex] != null)
            {
                lookDir = spawnPoints[activeLevelIndex].forward;
            }

            // Stop and reset ALL holes and checkpoints across the entire park
            GolfHole[] allHoles = FindObjectsOfType<GolfHole>(true);
            foreach (var h in allHoles)
            {
                h.StopAllCoroutines();
                h.ResetHole();
            }

            Checkpoint[] allCheckpoints = FindObjectsOfType<Checkpoint>(true);
            foreach (var cp in allCheckpoints)
            {
                cp.ResetCheckpoint();
            }

            lookDir.y = 0f;
            if (lookDir.sqrMagnitude < 0.01f) lookDir = Vector3.forward;
            lookDir.Normalize();

            if (ball != null)
            {
                ball.TeleportTo(spawnPos);
                ball.SetAimDirection(lookDir);
                ball.SetCheckpoint(spawnPos, Vector3.down * 9.81f, GravityMode.Normal);
            }

            if (camController != null)
            {
                if (ball != null) camController.Target = ball.transform;
                float targetYaw = Mathf.Atan2(lookDir.x, lookDir.z) * Mathf.Rad2Deg;
                camController.SetYaw(targetYaw);
                camController.SnapToTarget();
            }
        }

        public void LoadPracticeGreen()
        {
            isPractice = true;
            Vector3 practicePos = practiceSpawnPoint != null 
                ? practiceSpawnPoint.position 
                : new Vector3(20.8f, 0.18f, -20.2f);

            Vector3 lookDir = practiceSpawnPoint != null ? practiceSpawnPoint.forward : new Vector3(-1f, 0f, 1f).normalized;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude < 0.01f) lookDir = new Vector3(-1f, 0f, 1f).normalized;
            lookDir.Normalize();

            if (ball != null)
            {
                ball.TeleportTo(practicePos);
                ball.SetAimDirection(lookDir);
                ball.SetCheckpoint(practicePos, Vector3.down * 9.81f, GravityMode.Normal);
            }

            if (camController != null)
            {
                if (ball != null) camController.Target = ball.transform;
                float targetYaw = Mathf.Atan2(lookDir.x, lookDir.z) * Mathf.Rad2Deg;
                camController.SetYaw(targetYaw);
                camController.SnapToTarget();
            }
        }

        public void ResetPracticeBall()
        {
            Vector3 practicePos = practiceSpawnPoint != null 
                ? practiceSpawnPoint.position 
                : new Vector3(20.8f, 0.18f, -20.2f);

            Vector3 lookDir = practiceSpawnPoint != null ? practiceSpawnPoint.forward : new Vector3(-1f, 0f, 1f).normalized;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude < 0.01f) lookDir = new Vector3(-1f, 0f, 1f).normalized;
            lookDir.Normalize();

            if (ball != null)
            {
                ball.TeleportTo(practicePos);
                ball.SetAimDirection(lookDir);
            }

            if (camController != null)
            {
                float targetYaw = Mathf.Atan2(lookDir.x, lookDir.z) * Mathf.Rad2Deg;
                camController.SetYaw(targetYaw);
                camController.SnapToTarget();
            }
        }

        public void RestartFullCourse()
        {
            foreach (var container in levelContainers)
            {
                if (container == null) continue;
                GolfHole[] holes = container.GetComponentsInChildren<GolfHole>(true);
                foreach (var h in holes) h.ResetHole();
                Checkpoint[] cps = container.GetComponentsInChildren<Checkpoint>(true);
                foreach (var cp in cps) cp.ResetCheckpoint();
            }
            LoadLevel(0);
        }
    }
}
