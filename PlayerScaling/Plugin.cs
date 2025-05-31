using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace PlayerScaling;

[BepInPlugin(modGUID, modName, modVersion)]
public class Plugin : BaseUnityPlugin
{
    public const string modGUID = "dev.redfops.repo.playerscaling";
    public const string modName = "Player Scaling";
    public const string modVersion = "1.2.1";

    public static int curModuleAmount = 5;

    public static ConfigEntry<bool> mapScalingEnabled;
    public static ConfigEntry<bool> downScalingEnabled;
    public static ConfigEntry<bool> difficultyScalingEnabled;
    public static ConfigEntry<float> mapScalingMultiplier;
    public static ConfigEntry<float> enemyScalingMultiplier;
    public static ConfigEntry<float> valuableScalingMultiplier;
    public static ConfigEntry<float> difficultyScalingMultiplier;
    public static ConfigEntry<float> difficultyScalingOffset;

    public static ConfigEntry<int> defaultMaxMapSize;
    public static ConfigEntry<float> maxEnemyDensity;
    public static ConfigEntry<float> downScalingMin;

    public static ConfigEntry<float> globalScalingMultiplier;
    public static ConfigEntry<int> numPlayersStartScaling;
    public static ConfigEntry<float> playerDivisor;

    

    internal static new ManualLogSource Logger;
    private readonly Harmony harmony = new Harmony(modGUID);

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;

        globalScalingMultiplier = Config.Bind("General Scaling", "Global Scaling Multiplier", 1f, new ConfigDescription("Multiplies player scaling by this number", new AcceptableValueRange<float>(0.3f, 10f)));
        numPlayersStartScaling = Config.Bind("General Scaling", "Player Scaling Minimum", 4, new ConfigDescription("Scaling only happens after the player count passes this number", new AcceptableValueRange<int>(1, 16)));
        playerDivisor = Config.Bind("General Scaling", "Player Scaling Divisor", 4f, new ConfigDescription("Number of players divided by this is the scaling factor", new AcceptableValueRange<float>(1f, 12f)));
        downScalingEnabled = Config.Bind("General Scaling", "Down Scaling Enabled", false, new ConfigDescription("Whether or not the map will be scaled down if less players than minimum"));
        downScalingMin = Config.Bind("General Scaling", "Minimum Downscaling Multiplier", 0.5f, new ConfigDescription("The lowest the downscaling factor will go, not recommended below 0.5", new AcceptableValueRange<float>(0.1f, 1f)));

        mapScalingEnabled = Config.Bind("Map Scaling", "Map Scaling Enabled", true, new ConfigDescription("Whether or not the map will be scaled to the number of players"));
        mapScalingMultiplier = Config.Bind("Map Scaling", "Map Scaling Multiplier", 1f, new ConfigDescription("Multiplies the map size by this number (including max size)", new AcceptableValueRange<float>(0.3f, 10f)));
        defaultMaxMapSize = Config.Bind("Map Scaling", "Default Max Map Size", 10, new ConfigDescription("Max map size before scaling", new AcceptableValueRange<int>(5,60)));

        enemyScalingMultiplier = Config.Bind("Enemies Scaling", "Enemies Scaling Multiplier", 1f, new ConfigDescription("Multiplies the number of enemies by this number (not including max)", new AcceptableValueRange<float>(0.3f, 10f)));
        maxEnemyDensity = Config.Bind("Enemies Scaling", "Max Enemy Density", 0.8f, new ConfigDescription("Caps number of enemies per map module", new AcceptableValueRange<float>(0.3f, 5f)));

        valuableScalingMultiplier = Config.Bind("Valuables Scaling", "Valuables Scaling Multiplier", 1f, new ConfigDescription("Multiplies the amount of valuables by this number", new AcceptableValueRange<float>(0.3f, 10f)));

        difficultyScalingEnabled = Config.Bind("Difficulty Scaling", "Difficulty Scaling Enabled", true, new ConfigDescription("Whether or not the difficulty will be scaled to the number of players"));
        difficultyScalingMultiplier = Config.Bind("Difficulty Scaling", "Difficulty Scaling Multiplier", 1f, new ConfigDescription("Multiplies the difficulty by this number", new AcceptableValueRange<float>(0.3f, 4f)));
        difficultyScalingOffset = Config.Bind("Difficulty Scaling", "Difficulty Scaling Offset", 0.085f, new ConfigDescription("Offsets the general difficulty", new AcceptableValueRange<float>(0f, 1f)));

        harmony.PatchAll(typeof(TileGenerationPatchTrans));
        harmony.PatchAll(typeof(TileGenerationPatch));
        harmony.PatchAll(typeof(DifficultyPatch));
        harmony.PatchAll(typeof(EnemyAmountPatch));
        harmony.PatchAll(typeof(ValuablePatch));
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    //This function is mostly redundant (and a bit ugly) from an old implementation, but I'll keep it around for now, mostly due to laziness.
    public static float PlayerScaling(ScalingType scalingType)
    {
        return scalingType switch
        {
            ScalingType.Valuable => valuableScalingMultiplier.Value,
            ScalingType.Enemy => enemyScalingMultiplier.Value,
            ScalingType.Map => mapScalingMultiplier.Value * (mapScalingEnabled.Value ? PlayerScaling(ScalingType.Global) : 1),
            ScalingType.Difficulty => difficultyScalingMultiplier.Value * (difficultyScalingEnabled.Value ? PlayerScaling(ScalingType.Global) : 1),
            _ => globalScalingMultiplier.Value * (((GameDirector.instance.PlayerList.Count < numPlayersStartScaling.Value) || downScalingEnabled.Value) ? 1 : Mathf.Max(GameDirector.instance.PlayerList.Count / playerDivisor.Value, downScalingMin.Value)),
        };
    }
}

public enum ScalingType{
    Global,
    Enemy,
    Map,
    Valuable,
    Difficulty,
}
