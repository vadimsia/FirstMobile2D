using UnityEngine;
using Resources.Scripts.Player;

namespace Resources.Scripts.Data
{
    public enum PerkType
    {
        ManaRegenDelayReduction,    // уменьшения времени на восстановление маны
        MaxManaIncrease,            // увеличение маны
        MoveSpeedIncrease,          // скорость передвижения
        EvasionChanceIncrease,      // шанс уклонения
        FairyPullRangeIncrease      // притягивание фей
    }

    public enum PerkQuality { Small, Medium, Large }

    [CreateAssetMenu(fileName = "PerkDefinition", menuName = "GameSettings/Perk Definition", order = 2)]
    public class PerkDefinition : ScriptableObject
    {
        public PerkType Type;
        public PerkQuality Quality;
        public float Value;  // проценты для MoveSpeed и FairyPullRange

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
                PerkType.FairyPullRangeIncrease =>
                    $"+{Value:F1}% к радиусу притягивания фей от базового ({Quality})",
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
                case PerkType.FairyPullRangeIncrease:
                    // здесь мы увеличиваем радиус притягивания на Value процентов
                    stats.ModifyPullRangePercent(Value);
                    break;
            }
        }
    }
}
