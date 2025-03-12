using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Resources.Scripts.Crystal
{
    public class CrystalController : MonoBehaviour
    {
        [SerializeField] private Light2D crystalLight;
        [SerializeField] private float size = 0.5f;

        private void Start()
        {
            UpdateSize(size);
        }

        private void UpdateSize(float newSize)
        {
            size = newSize;
            crystalLight.pointLightOuterRadius = size * 10f;
        }
    }
}
