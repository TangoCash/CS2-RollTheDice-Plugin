
using System.Collections;
using System.Text.Json;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Events;
using Preach.CS2.Plugins.RollTheDiceV2;
using Preach.CS2.Plugins.RollTheDiceV2.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using System.Runtime.InteropServices;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;

namespace Preach.CS2.Plugins.RollTheDiceV2.Utilities;

internal static class Helpers
{
    public static JsonElement? GetJsonElement(EffectBase? effect, string key)
    {
        var effectsList = RollTheDice.Instance!.EffectConfig!.Data.Effects;

        if(!effectsList.TryGetValue(effect!.Name, out Dictionary<string, object>? effectData))
            return null;
            
        if(!effectData.TryGetValue(key, out object? value) || !(value is JsonElement valueElement))
        {
            Log.PrintServerConsole($"Failed to get '{key}' for effect '{effect.Name}' from config.json", LogType.ERROR);
            return null;
        }

        return valueElement;
    }

    public static int GetDamageInRangePlyHealth(EventPlayerHurt @event, float scale = 1)
    {
        CCSPlayerController? victimController = @event.Userid;
        if (victimController == null || victimController.PlayerPawn.Value == null)   return 0;

        // Count damage that is lower than or equal the victim's health
        float damageAmount = @event.DmgHealth; 
        int victimHealth = victimController.PlayerPawn.Value.Health;
        damageAmount = victimHealth < 0 ? damageAmount+victimHealth : damageAmount;

        return (int)(damageAmount*scale);
    }

    public static string GetModel (CCSPlayerController player)
    {
        return player.PlayerPawn.Value?.CBodyComponent?.SceneNode?.GetSkeletonInstance().ModelState.ModelName ?? string.Empty;
    }

    public static bool IsFreezeTime ()
    {
        return CounterStrikeSharp.API.Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules?.FreezePeriod ?? false;
    }

    public static float CalculateDistance (Vector point1, Vector point2)
    {
        float dx = point2.X - point1.X;
        float dy = point2.Y - point1.Y;
        float dz = point2.Z - point1.Z;

        return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    public static void DropWeapons(CCSPlayerController? player)
    {
        if (player == null || !player.IsValid || player.IsBot || player.PlayerPawn.Value?.WeaponServices is null)
        {
            return;
        }

        var act_weapon = player.PlayerPawn.Value?.WeaponServices!.ActiveWeapon;

        while (!(act_weapon.Value.DesignerName.Contains("bayonet") || act_weapon.Value.DesignerName.Contains("knife") || act_weapon.Value.DesignerName.Contains("weapon_c4")))
        {
            player.DropActiveWeapon();
            act_weapon = player.PlayerPawn.Value?.WeaponServices!.ActiveWeapon;
        }
    }
}