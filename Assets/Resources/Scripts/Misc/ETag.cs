using System.Runtime.Serialization;

namespace Resources.Scripts.Misc
{
    /// <summary>
    /// Enumeration for common game object tags.
    /// </summary>
    public enum ETag
    {
        [EnumMember(Value = "Player")]
        Player,
        [EnumMember(Value = "Fairy")]
        Fairy,
        [EnumMember(Value = "Enemy")]
        Enemy,
    }
}