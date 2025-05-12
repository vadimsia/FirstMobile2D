using UnityEngine;
using Resources.Scripts.Player;

namespace Resources.Scripts.Data
{
    public enum PerkType
    {
        ManaRegenDelayReduction,
        MaxManaIncrease,
        MoveSpeedIncrease,
        EvasionChanceIncrease
    }

    public enum PerkQuality { Small, Medium, Large }

    [CreateAssetMenu(fileName = "PerkDefinition", menuName = "GameSettings/Perk Definition", order = 2)]
    public class PerkDefinition : ScriptableObject
    {
        public PerkType Type;
        public PerkQuality Quality;
        public float Value;  // теперь интерпретируется как проценты для MoveSpeed

        public string GetDescription()
        {
            return Type switch
            {
                PerkType.ManaRegenDelayReduction =>
                    $"–{Value:F1}s задержки регена маны ({Quality})",
                PerkType.MaxManaIncrease =>
                    $"+{Value} к макс. мане ({Quality})",
                PerkType.MoveSpeedIncrease =>
                    $"+{Value:F1}% к скорости передвижения ({Quality})",
                PerkType.EvasionChanceIncrease =>
                    $"+{Value:F1}% к шансу уклонения ({Quality})",
                _ => ""
            };
        }

        public void Apply(PlayerStatsHandler stats)
        {
            switch (Type)
            {
                case PerkType.ManaRegenDelayReduction:
                    stats.ModifyManaRegenDelay(Value);
                    break;
                case PerkType.MaxManaIncrease:
                    stats.ModifyMaxMana(Value);
                    break;
                case PerkType.MoveSpeedIncrease:
                    stats.ModifyMoveSpeedPercent(Value);
                    break;
                case PerkType.EvasionChanceIncrease:
                    stats.ModifyEvasion(Value);
                    break;
            }
        }
    }
}