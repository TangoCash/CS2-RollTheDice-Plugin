﻿using System.Reflection;
using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;
using Preach.CS2.Plugins.RollTheDiceV2.Config;
using CounterStrikeSharp.API.Modules.Config;

namespace Preach.CS2.Plugins.RollTheDiceV2;

public class RollTheDice : BasePlugin, IPluginConfig<GeneralConfig>
{
    public static readonly bool DEBUG = true;
    public static readonly bool UNFINISHED_EFFECTS = false;
    public override string ModuleName => "Roll The Dice";
    public override string ModuleDescription => "Roll The Dice or RTD is a plugin that allows players to roll the dice and get a random effect.";
    public override string ModuleVersion => "2.1.1";
    public override string ModuleAuthor => "Preach";
    required public GeneralConfig Config { get; set; }
    public static RollTheDice? Instance { get; private set; }
    public EffectConfig? EffectConfig { get; private set; }
    public EffectManager? EffectManager { get; private set; }
    public DiceSystem? DiceSystem { get; private set; }
    public Commands? Commands { get; private set; }
    public EventSystem? EventSystem { get; private set; }

    public override void Load(bool hotReload)
    {
        base.Load(hotReload);
        Instance = this;

        EffectManager = new EffectManager(this);
        DiceSystem = new DiceSystem(this);
        EventSystem = new EventSystem(this);
        Commands = new Commands(this);
        EffectConfig = new EffectConfig(this);

        RegisterEffects();

        EffectConfig.GetOrGenerateEffectsConfig();

        // Precache resources
        RegisterListener<Listeners.OnServerPrecacheResources>((manifest) =>
        {
            manifest.AddResource("models/chicken/chicken.vmdl");
            manifest.AddResource("models/props/de_dust/hr_dust/dust_soccerball/dust_soccer_ball001.vmdl");
        });
    }

    public override void Unload(bool hotReload)
    {
        base.Unload(hotReload);
    }

    public void ReloadConfig()
    {
        Config = ConfigManager.Load<GeneralConfig>("RollTheDice");
        EffectConfig!.LoadConfig();
        DiceSystem!.ResetState();
        EffectManager!.ResetState();
    }

    private void RegisterEffects()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        string namespaceName = "Preach.CS2.Plugins.RollTheDiceV2.Effects";

        var assemblyTypes = assembly.GetTypes();
        var effects = assemblyTypes
            .Where(type => type.IsClass && type.Namespace == namespaceName && type.IsSubclassOf(typeof(EffectBase)))
            .Where(type => !type.GetInterfaces().Contains(typeof(IEffectWorkInProgress)))
            .Concat(assemblyTypes.Where(type => UNFINISHED_EFFECTS && type.GetInterfaces().Contains(typeof(IEffectWorkInProgress))))
            .Distinct()
            .ToList();


        if (effects.Count() == 0)
            return;

        foreach (var effect in effects)
        {
            EffectBase effectInstance = (EffectBase)Activator.CreateInstance(effect)!;

            effectInstance.Initialize();

            // To invoke the translation function
            var name = effectInstance.PrettyName;

            Log.PrintServerConsole($"Registering effect: {effectInstance.Name}", LogType.INFO);
        }
    }

    public void OnConfigParsed(GeneralConfig config)
    {
        Config = config;
    }
}