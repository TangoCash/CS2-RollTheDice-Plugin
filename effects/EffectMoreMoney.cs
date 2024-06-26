using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectMoreMoney : EffectBaseRegular, IEffectParameter
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "More Money".__("effect_name_more_money");
    public override string Description { get; set; } = "Your money is increased by {mark}{0}".__("effect_description_more_money");
    public override double Probability { get; set; }  = 3;
    public override bool ShowDescriptionOnRoll { get; set; } = false;
    public Dictionary<string, string> RawParameters { get; set; } = new();

    public override void Initialize()
    {
        RawParameters.Add("moneySummand", "1000");
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        if (playerController == null || 
            playerController.PlayerPawn.Value is null ||
            playerController.InGameMoneyServices is null
        )  return;

        if(!RawParameters.TryGetValue("moneySummand", out var moneyStr))
            return;

        if(!int.TryParse(moneyStr, out var moneyInt))
            return;

        playerController.InGameMoneyServices.Account += moneyInt;
        playerController.RefreshUI();
        
        PrintDescription(playerController, "effect_description_more_money", moneyStr);
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
    }
}