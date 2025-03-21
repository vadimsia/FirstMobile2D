using System.Collections.Generic;
using UnityEngine;

namespace Resources.Scripts.SpellMode
{
    [CreateAssetMenu(fileName = "SignTemplateDatabaseIcon", menuName = "SignTemplates/Icon Template Database")]
    public class SignTemplateDatabaseIcon : ScriptableObject
    {
        // List of sign templates
        public List<SignTemplateIcon> templates = new List<SignTemplateIcon>();
    }
}