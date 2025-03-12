using System.Runtime.Serialization;

namespace Resources.Scripts.Misc
{
    public enum ETag {
        [EnumMember(Value = "Player")]
        Player,
        [EnumMember(Value = "Fairy")]
        Fairy,
        [EnumMember(Value = "Enemy")]
        Enemy,
    }
}