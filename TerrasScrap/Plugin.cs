using BepInEx;
using HarmonyLib;
using LethalLevelLoader;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Windows.Speech;

namespace TerrasScrap;

[BepInPlugin(Plugin.pluginGUID, Plugin.pluginName, Plugin.pluginVersion)]
public class Plugin : BaseUnityPlugin
{
    private const string pluginGUID = "terraformer9x.TerrasScrap";
    private const string pluginName = "Terra's Scrap";
    private const string pluginVersion = "1.0.2";

    private readonly Harmony harmony = new(pluginGUID);
    public static Plugin Instance;

    private void Awake()
    {
        Instance ??= this;

        Logger.LogInfo($"Plugin {pluginName} {pluginVersion}  is loaded!");

        harmony.PatchAll(typeof(StartOfRoundPatch));
    }

    public static void Log(string msg) => Instance.Logger.LogInfo(msg);
    public static void LogError(string msg) => Instance.Logger.LogError(msg);
    public static void LogDebug(string msg) => Instance.Logger.LogDebug(msg);
}

internal class StartOfRoundPatch
{
    private static bool executed = false;

    [HarmonyPatch(typeof(StartOfRound), "Awake")]
    [HarmonyPostfix]
    private static void StartOfRoundPostfix(ref StartOfRound __instance)
    {
        if (executed) return; executed = true;
        Dictionary<string, List<ItemGroup>> itemGroups = [];
        foreach(ItemGroup itemGroup in Resources.FindObjectsOfTypeAll<ItemGroup>())
        {
            if(!itemGroups.ContainsKey(itemGroup.name))
            {
                List<ItemGroup> itemGroupList = [itemGroup];
                itemGroups.Add(itemGroup.name, itemGroupList);
            }
            else
            {
                itemGroups[itemGroup.name].Add(itemGroup);
            }
        }
        List<ExtendedItem> modExtendedItems = LethalLevelLoader.PatchedContent.ExtendedItems.Where(x => x.ModName == "Terra's Scrap").ToList();
        Dictionary<string, Item> modItems = [];
        foreach (ExtendedItem extendedItem in modExtendedItems)
        {
            if (extendedItem.Item != null && !modItems.ContainsKey(extendedItem.Item.name) && !modItems.ContainsValue(extendedItem.Item))
            {
                modItems.Add(extendedItem.Item.name, extendedItem.Item);
            }
        }
        modItems["TrafficCone"].spawnPositionTypes.AddRange(itemGroups["GeneralItemClass"]);
        modItems["Tuba"].spawnPositionTypes.AddRange(itemGroups["GeneralItemClass"]);
        modItems["WineBottle"].spawnPositionTypes.AddRange(itemGroups["TabletopItems"]);
        modItems["Fan"].spawnPositionTypes.AddRange(itemGroups["GeneralItemClass"]);
        modItems["Fan"].spawnPositionTypes.AddRange(itemGroups["TabletopItems"]);
    }
}