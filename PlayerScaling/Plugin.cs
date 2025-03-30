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
    public const string modVersion = "1.1.0";

    public static ConfigEntry<bool> mapScalingEnabled;
    public static ConfigEntry<bool> enemyScalingEnabled;
    public static ConfigEntry<bool> valuableScalingEnabled;
    public static ConfigEntry<bool> difficultyScalingEnabled;
    public static ConfigEntry<float> mapScalingMultiplier;
    public static ConfigEntry<float> enemyScalingMultiplier;
    public static ConfigEntry<float> valuableScalingMultiplier;
    public static ConfigEntry<float> difficultyScalingMultiplier;
    public static ConfigEntry<float> valuableScalingOffset;
    public static ConfigEntry<float> difficultyScalingOffset;

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

        mapScalingEnabled = Config.Bind("Map Scaling", "Map Scaling Enabled", true, new ConfigDescription("Whether or not the map will be scaled to the number of players"));
        mapScalingMultiplier = Config.Bind("Map Scaling", "Map Scaling Multiplier", 1f, new ConfigDescription("Multiplies the map size by this number", new AcceptableValueRange<float>(0.3f, 10f)));

        enemyScalingEnabled = Config.Bind("Enemies Scaling", "Enemies Scaling Enabled", true, new ConfigDescription("Whether or not the number of enemies will be scaled to the number of players"));
        enemyScalingMultiplier = Config.Bind("Enemies Scaling", "Enemies Scaling Multiplier", 1f, new ConfigDescription("Multiplies the number of enemies by this number", new AcceptableValueRange<float>(0.3f, 10f)));

        valuableScalingEnabled = Config.Bind("Valuables Scaling", "Valuables Scaling Enabled", true, new ConfigDescription("Whether or not the amount of valuables will be scaled to the number of players"));
        valuableScalingMultiplier = Config.Bind("Valuables Scaling", "Valuables Scaling Multiplier", 1f, new ConfigDescription("Multiplies the amount of valuables by this number", new AcceptableValueRange<float>(0.3f, 10f)));
        valuableScalingOffset = Config.Bind("Valuables Scaling", "Valuables Scaling Offset", 0.1f, new ConfigDescription("Offsets the difficulty of the Valuable spawner", new AcceptableValueRange<float>(0f, 1f)));

        difficultyScalingEnabled = Config.Bind("Difficulty Scaling", "Difficulty Scaling Enabled", true, new ConfigDescription("Whether or not the difficulty will be scaled to the number of players"));
        difficultyScalingMultiplier = Config.Bind("Difficulty Scaling", "Difficulty Scaling Multiplier", 1f, new ConfigDescription("Multiplies the difficulty by this number", new AcceptableValueRange<float>(0.3f, 4f)));
        difficultyScalingOffset = Config.Bind("Difficulty Scaling", "Difficulty Scaling Offset", 0.085f, new ConfigDescription("Offsets the general difficulty", new AcceptableValueRange<float>(0f, 1f)));

        harmony.PatchAll(typeof(TileGenerationPatchTrans));
        harmony.PatchAll(typeof(TileGenerationPatch));
        harmony.PatchAll(typeof(DifficultyPatch));
        harmony.PatchAll(typeof(EnemyAmountPatch));
        harmony.PatchAll(typeof(ValuablePatchTrans));
        harmony.PatchAll(typeof(ValuablePatch));
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    public static float PlayerScaling(ScalingType scalingType)
    {
        return scalingType switch
        {
            ScalingType.Valuable => valuableScalingMultiplier.Value * (valuableScalingEnabled.Value ? PlayerScaling(ScalingType.Global) : 1),
            ScalingType.Enemy => enemyScalingMultiplier.Value * (enemyScalingEnabled.Value ? PlayerScaling(ScalingType.Global) : 1),
            ScalingType.Map => mapScalingMultiplier.Value * (mapScalingEnabled.Value ? PlayerScaling(ScalingType.Global) : 1),
            ScalingType.Difficulty => difficultyScalingMultiplier.Value * (difficultyScalingEnabled.Value ? PlayerScaling(ScalingType.Global) : 1),
            _ => globalScalingMultiplier.Value * ((GameDirector.instance.PlayerList.Count < numPlayersStartScaling.Value) ? 1 : GameDirector.instance.PlayerList.Count / playerDivisor.Value),
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
