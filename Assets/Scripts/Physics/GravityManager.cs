using System;
using System.Collections.Generic;
using UnityEngine;

namespace AntiGravityGolf
{
    public enum GravityMode
    {
        Normal,
        ZeroGravity,
        Directional,
        Inverted
    }

    /// <summary>
    /// Core anti-gravity orchestrator. Controls customized gravity vectors,
    /// smooth transitions, zero-gravity physics, and zone stack tracking.
    /// </summary>
    public class GravityManager : MonoBehaviour
    {
        public static GravityManager Instance { get; private set; }

        [Header("Default Gravity Settings")]
        [SerializeField] private Vector3 defaultGravity = new Vector3(0f, -9.81f, 0f);
        [SerializeField] private float transitionSpeed = 8f; // Slerp/Lerp speed between gravity vectors
        [SerializeField] private float zeroGDamping = 0.35f;  // Slight atmospheric resistance in zero-g

        [Header("State")]
        [SerializeField] private GravityMode currentMode = GravityMode.Normal;
        [SerializeField] private Vector3 currentGravity = new Vector3(0f, -9.81f, 0f);
        [SerializeField] private Vector3 targetGravity = new Vector3(0f, -9.81f, 0f);

        // Active zone stack for nested/overlapping anti-gravity zones
        private readonly List<AntiGravityZone> activeZones = new List<AntiGravityZone>();

        public event Action<Vector3, GravityMode> OnGravityChanged;

        public GravityMode CurrentMode => currentMode;
        public Vector3 CurrentGravity => currentGravity;
        public Vector3 TargetGravity => targetGravity;
        public Vector3 EffectiveUpVector => currentGravity.sqrMagnitude > 0.01f ? -currentGravity.normalized : Vector3.up;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            targetGravity = defaultGravity;
            currentGravity = defaultGravity;
        }

        private void FixedUpdate()
        {
            // Smoothly interpolate current gravity towards target gravity
            if (currentGravity != targetGravity)
            {
                currentGravity = Vector3.Lerp(currentGravity, targetGravity, Time.fixedDeltaTime * transitionSpeed);
            }
        }

        /// <summary>
        /// Registers a zone when ball enters it.
        /// </summary>
        public void EnterZone(AntiGravityZone zone)
        {
            if (!activeZones.Contains(zone))
            {
                activeZones.Add(zone);
                RecalculateZoneGravity();
            }
        }

        /// <summary>
        /// Unregisters a zone when ball exits it.
        /// </summary>
        public void ExitZone(AntiGravityZone zone)
        {
            if (activeZones.Remove(zone))
            {
                RecalculateZoneGravity();
            }
        }

        /// <summary>
        /// Overrides gravity directly (e.g. from a GravityFlipPad or custom event).
        /// </summary>
        public void SetGravity(Vector3 newGravity, GravityMode mode, bool instant = false)
        {
            targetGravity = newGravity;
            currentMode = mode;
            if (instant)
            {
                currentGravity = newGravity;
            }
            OnGravityChanged?.Invoke(targetGravity, currentMode);
        }

        /// <summary>
        /// Resets gravity to course normal.
        /// </summary>
        public void ResetToDefault(bool instant = true)
        {
            activeZones.Clear();
            SetGravity(defaultGravity, GravityMode.Normal, instant);
        }

        /// <summary>
        /// Recomputes gravity from the highest priority active zone or reverts to normal.
        /// </summary>
        private void RecalculateZoneGravity()
        {
            if (activeZones.Count == 0)
            {
                SetGravity(defaultGravity, GravityMode.Normal);
                return;
            }

            // Pick zone with highest priority (or latest if tied)
            AntiGravityZone highest = activeZones[activeZones.Count - 1];
            for (int i = 0; i < activeZones.Count; i++)
            {
                if (activeZones[i].Priority > highest.Priority)
                {
                    highest = activeZones[i];
                }
            }

            SetGravity(highest.CalculateGravityVector(), highest.Mode);
        }

        /// <summary>
        /// Applies gravity to a Rigidbody. Called by GolfBallController.
        /// </summary>
        public void ApplyGravityTo(Rigidbody rb)
        {
            if (rb == null) return;

            if (currentMode == GravityMode.ZeroGravity)
            {
                // In zero-g, apply slight velocity damping so the ball floats gracefully
                if (zeroGDamping > 0f)
                {
                    rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, zeroGDamping * 0.1f * Time.fixedDeltaTime);
                }
            }
            else
            {
                rb.AddForce(currentGravity, ForceMode.Acceleration);
            }
        }
    }
}
