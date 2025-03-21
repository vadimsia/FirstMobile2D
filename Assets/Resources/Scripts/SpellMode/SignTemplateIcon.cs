using System.Collections.Generic;
using UnityEngine;

namespace Resources.Scripts.SpellMode
{
    [CreateAssetMenu(fileName = "SignTemplateIcon", menuName = "SignTemplates/Icon Template")]
    public class SignTemplateIcon : ScriptableObject
    {
        public string id;
        public Sprite icon;

        [Tooltip("Normalized sign contour points")]
        public List<Vector2> points;

        public GameObject skillPrefab;

        [Tooltip("Required mana cost to use this sign")]
        public float manaCost = 10f; // Configurable in the Inspector for each sign
    }
}