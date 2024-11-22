using System.Reflection;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using Preach.CS2.Plugins.RollTheDiceV2.Effects;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;

public class EffectManager
{
    private RollTheDice _plugin;
    public Dictionary<ulong, EffectBase>? PlyActiveEffect = new();
    public EffectManager(RollTheDice plugin)
    {
        _plugin = plugin;
    }

    public void ResetState()
    {
        PlyActiveEffect?.Clear();
    }

    public void RemoveOrResetPlyActiveEffects(CCSPlayerController plyController)
    {
        if(!plyController.IsValidPly())
            return;

        var activeEffect = plyController.GetEffect();

        if(activeEffect == null)
            return;

        activeEffect.OnRemove(plyController);
        PlyActiveEffect!.Remove(plyController.SteamID);
    }

    public void RemoveActiveEffect(CCSPlayerController plyController)
    {
        if(!plyController.IsValidPly())
            return;

        var plyID = plyController.SteamID;

        if(!PlyActiveEffect!.ContainsKey(plyID))
            return;

        PlyActiveEffect.Remove(plyID);
    }

    public void AddActiveEffect(CCSPlayerController plyController, EffectBase effect)
    {
        if(!plyController.IsValidPly())
            return;

        var plyID = plyController.SteamID;

        if(PlyActiveEffect!.ContainsKey(plyID))
            return;

        PlyActiveEffect.Add(plyID, effect);
    }

    public EffectBase? GetActiveEffect(CCSPlayerController plyController)
    {
        var plyID = plyController.SteamID;

        if(!PlyActiveEffect!.ContainsKey(plyID))
            return null;

        return PlyActiveEffect[plyID];
    }

    #region Events

    public HookResult HandlePlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        CCSPlayerController? plyController = @event.Userid;
        if (plyController == null)  return HookResult.Continue;

        RemoveOrResetPlyActiveEffects(plyController);

        return HookResult.Continue;
    }

    public HookResult HandlePlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        CCSPlayerController? plyController = @event.Userid;
        if (plyController == null)  return HookResult.Continue;

        RemoveOrResetPlyActiveEffects(plyController);

        return HookResult.Continue;
    }

    public HookResult HandleRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        CounterStrikeSharp.API.Utilities.GetPlayers().ForEach(plyController => 
        {
            RemoveOrResetPlyActiveEffects(plyController);
        });

        return HookResult.Continue;
    }

    public HookResult HandlePlayerHurt(EventPlayerHurt @event, GameEventInfo eventInfo)
    {
        var victim = @event.Userid;
        var attacker = @event.Attacker;

        attacker?.GetEventEffect<EventPlayerHurt>()?.OnEvent(@event, eventInfo);
        victim?.GetEventEffect<EventPlayerHurt>()?.OnEvent(@event, eventInfo);

        return HookResult.Continue;
    }

    public HookResult HandleRoundFreezeEnd (EventRoundFreezeEnd @event, GameEventInfo eventInfo)
    {
        foreach (var player in CounterStrikeSharp.API.Utilities.GetPlayers())
        {
            player?.GetEffect()?.OnRoundFreezeEnd(player);
        }
        return HookResult.Continue;
    }

    public HookResult HandleBuytimeEnd (EventBuytimeEnded @event, GameEventInfo eventInfo)
    {
        foreach (var player in CounterStrikeSharp.API.Utilities.GetPlayers())
        {
            player?.GetEffect()?.OnBuytimeEnd(player);
        }
        return HookResult.Continue;
    }

    public HookResult HandleItemPickup(EventItemPickup @event, GameEventInfo eventInfo)
    {
        CCSPlayerController? plyController = @event.Userid;
        if (plyController == null || !plyController.IsValid || plyController.IsBot)
        {
            return HookResult.Continue;
        }
        plyController?.GetEffect()?.OnItemPickup(plyController, @event);
        return HookResult.Continue;
    }
    public HookResult HandleWeaponFire(EventWeaponFire @event, GameEventInfo eventInfo)
    {
        CCSPlayerController? plyController = @event.Userid;
        if (plyController == null || !plyController.IsValid || plyController.IsBot)
        {
            return HookResult.Continue;
        }
        plyController?.GetEffect()?.OnWeaponFire(plyController, @event);
        return HookResult.Continue;
    }

    public HookResult HandleBombPlanted(EventBombPlanted @event, GameEventInfo eventInfo)
    {
        CCSPlayerController? plyController = @event.Userid;
        var ElementPlantedC4 = GetPlantedC4();
        if (plyController == null || !plyController.IsValid || plyController.IsBot)
        {
            return HookResult.Continue;
        }

        if (plyController.GetEffect().TranslationName.Equals("chicken") && ElementPlantedC4 != null)
        {
            Server.NextFrame(() =>
            {
                ElementPlantedC4.SetModel("models/props/de_dust/hr_dust/dust_soccerball/dust_soccer_ball001.vmdl");
            });
            foreach (var player in CounterStrikeSharp.API.Utilities.GetPlayers())
                    player.ExecuteClientCommand($"play sounds/ambient/creatures/chicken_panic_03");
        }

        return HookResult.Continue;
    }
    public CPlantedC4? GetPlantedC4()
    {
        var PlantedC4 = CounterStrikeSharp.API.Utilities.FindAllEntitiesByDesignerName<CPlantedC4>("planted_c4");

        if (PlantedC4 == null || !PlantedC4.Any())
            return null;

        return PlantedC4.FirstOrDefault();
    }
    public void OnGameFrame ()
    {
        EffectThirdPerson.OnGameFrame();
        EffectChicken.OnGameFrame();
        EffectDoubleJump.OnGameFrame();
    }
    #endregion
}