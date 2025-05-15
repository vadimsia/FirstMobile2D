using UnityEngine;
using TMPro;

namespace Resources.Scripts.UI
{
    public class EvasionText : MonoBehaviour
    {
        [SerializeField] private float floatSpeed = 1f;
        [SerializeField] private float fadeTime = 1f;

        private TMP_Text tmpText;     
        private Color originalColor;

        private void Awake()
        {
            tmpText = GetComponent<TMP_Text>();
            if (tmpText == null)
                Debug.LogError($"[{nameof(EvasionText)}] Не найден компонент TMP_Text на {gameObject.name}");
            else
                originalColor = tmpText.color;
        }

        private void Update()
        {
            transform.position += Vector3.up * floatSpeed * Time.deltaTime; // мне так удобно, отстань Rider :(

            if (tmpText == null) return;

            Color c = tmpText.color;
            c.a -= Time.deltaTime / fadeTime;
            tmpText.color = c;

            if (c.a <= 0f)
                Destroy(gameObject);
        }
    }
}