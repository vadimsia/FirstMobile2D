using UnityEngine;

namespace Resources.Scripts.Camera
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField, Range(3, 10)] int speed = 5;
        [SerializeField] Transform target;

        [SerializeField, Range(2, 10)] float projMin = 2;
        [SerializeField, Range(2, 10)] float projMax = 10;

        void Update()
        {
            UpdatePos();
            UpdateProjSize();
        }

        void UpdatePos()
        {
            // Если цель отсутствует (например, персонаж уничтожен) – выходим из метода
            if (target == null) return;

            Vector3 newPos = Vector3.Lerp(transform.position, target.position, Time.deltaTime * speed);
            newPos.z = -10;
            transform.position = newPos;
        }

        void UpdateProjSize()
        {
            if (Input.GetKey(KeyCode.Space))
            {
                UnityEngine.Camera.main.orthographicSize = Mathf.Lerp(UnityEngine.Camera.main.orthographicSize, projMax, Time.deltaTime * speed);
            }
            else
            {
                UnityEngine.Camera.main.orthographicSize = Mathf.Lerp(UnityEngine.Camera.main.orthographicSize, projMin, Time.deltaTime * speed);
            }
        }
    }
}