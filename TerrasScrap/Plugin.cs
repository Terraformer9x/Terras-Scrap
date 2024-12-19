using BepInEx;
using HarmonyLib;
using LethalLevelLoader;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;

namespace TerrasScrap;

[BepInPlugin(Plugin.pluginGUID, Plugin.pluginName, Plugin.pluginVersion)]
public class Plugin : BaseUnityPlugin
{
    private const string pluginGUID = "terraformer9x.TerrasScrap";
    private const string pluginName = "Terra's Scrap";
    private const string pluginVersion = "1.0.0";

    private readonly Harmony harmony = new(pluginGUID);
    public static Plugin Instance;

    private void Awake()
    {
        Instance ??= this;

        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

        harmony.PatchAll(typeof(StartOfRoundPatch));
    }

    public static void Log(string msg) => Instance.Logger.LogInfo(msg);
    public static void LogError(string msg) => Instance.Logger.LogError(msg);
    public static void LogDebug(string msg) => Instance.Logger.LogDebug(msg);
}

internal class StartOfRoundPatch
{
    [HarmonyPatch(typeof(StartOfRound), "Awake")]
    [HarmonyPostfix]
    private static void StartOfRoundPostfix(ref StartOfRound __instance)
    {
        List<ExtendedItem> modExtendedItems = LethalLevelLoader.PatchedContent.ExtendedItems.Where(x => x.ModName == "Terra's Scrap").ToList();
        Dictionary<string, ItemGroup> itemGroups = [];
        Dictionary<string, Item> modItems = [];
        foreach (ItemGroup itemGroup in Resources.FindObjectsOfTypeAll<ItemGroup>())
        {
            if (itemGroup != null && !itemGroups.ContainsKey(itemGroup.name) && !itemGroups.ContainsValue(itemGroup))
            {
                itemGroups.Add(itemGroup.name, itemGroup);
            }
        }
        foreach (ExtendedItem extendedItem in modExtendedItems)
        {
            if (extendedItem.Item != null && !modItems.ContainsKey(extendedItem.Item.name) && !modItems.ContainsValue(extendedItem.Item))
            {
                modItems.Add(extendedItem.Item.name, extendedItem.Item);
            }
        }
        modItems["TrafficCone"].spawnPositionTypes.Add(itemGroups["GeneralItemClass"]);
        modItems["Tuba"].spawnPositionTypes.Add(itemGroups["GeneralItemClass"]);
        modItems["WineBottle"].spawnPositionTypes.Add(itemGroups["TabletopItems"]);
        modItems["Fan"].spawnPositionTypes.Add(itemGroups["GeneralItemClass"]);
        modItems["Fan"].spawnPositionTypes.Add(itemGroups["TabletopItems"]);
    }
}