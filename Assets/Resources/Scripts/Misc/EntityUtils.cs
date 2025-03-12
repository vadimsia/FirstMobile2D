using UnityEngine;

namespace Resources.Scripts.Misc
{
    public abstract class EntityUtils
    {
        public static void MakeDash(Transform transform, Vector3 direction) {
            transform.Translate(direction.normalized);
        }
    }
}