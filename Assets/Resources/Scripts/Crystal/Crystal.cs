using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Resources.Scripts.Crystal
{
    public class CrystalController : MonoBehaviour
    {
        [Header("Light Settings")]
        [SerializeField] private Light2D crystalLight;
        [SerializeField, Tooltip("Base size of the crystal light.")] private float size = 0.5f;
        [SerializeField, Tooltip("Multiplier for the light radius.")] private float radiusMultiplier = 10f;

        [Header("Animation Settings")]
        [SerializeField, Tooltip("Toggle whether to update the light size dynamically.")] private bool dynamicUpdate;
        [SerializeField, Tooltip("Speed at which the light size updates.")] private float updateSpeed = 2f;

        private void Start()
        {
            // Initialize the light radius based on the current size
            UpdateSize(size);
        }

        private void Update()
        {
            if (dynamicUpdate)
            {
                // Oscillate the size between 0.5 and 1.5 using PingPong for demonstration
                float newSize = 0.5f + Mathf.PingPong(Time.time * updateSpeed, 1f);
                UpdateSize(newSize);
            }
        }
        
        private void UpdateSize(float newSize)
        {
            size = newSize;
            if (crystalLight != null)
            {
                crystalLight.pointLightOuterRadius = size * radiusMultiplier;
            }
        }
    }
}