using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace BetterMaps
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class BetterMaps : BaseUnityPlugin
    {
        private AssetBundle mapBundle;
        private static GameObject newMap;
        private static GameObject instantiatedMap;
        private const string ModName = "BetterMaps";
        private const string ModVersion = "1.0";
        private const string ModGUID = "com.OdinPlus.BetterMaps";
        private static ConfigEntry<bool> MapRotation;
        public void Awake()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Harmony harmony = new(ModGUID);
            harmony.PatchAll(assembly);
            LoadAssets();
            MapRotation = Config.Bind("Better Maps", "Rotation", false, "Controls the map rotation with player view");
        }
        public void LoadAssets()
        {
            mapBundle = GetAssetBundleFromResources("bettermaps");
            newMap = mapBundle.LoadAsset<GameObject>("MiniMap");
        }
        private static AssetBundle GetAssetBundleFromResources(string filename)
        {
            var execAssembly = Assembly.GetExecutingAssembly();
            var resourceName = execAssembly.GetManifestResourceNames()
                .Single(str => str.EndsWith(filename));

            using (var stream = execAssembly.GetManifestResourceStream(resourceName))
            {
                return AssetBundle.LoadFromStream(stream);
            }
        }

        [HarmonyPatch(typeof(Minimap), nameof(Minimap.Awake))]
        public static class Mappatcher
        {
            public static void Prefix(Minimap __instance)
            {
	            Instantiate(newMap, __instance.transform, false);
                //Using a prefix is imperative here due to the way that the awake function works. 
                __instance.transform.Find("small").gameObject.SetActive(false);
                __instance.m_smallRoot = BetterMapper.internalMapRoot; 
                __instance.m_biomeNameSmall = BetterMapper.internalbiometext;
                __instance.m_smallMarker = BetterMapper.internalsmallmarker;
                BetterMapper.internalsmallmap.material = Instantiate(__instance.m_mapImageSmall.material);
                BetterMapper.internalsmallmap.maskable = true;
                __instance.m_mapImageSmall = BetterMapper.internalsmallmap; 
                __instance.m_smallShipMarker = BetterMapper.internalsmallship;
                __instance.m_pinRootSmall = BetterMapper.internalpinroot;
            }

        }

        [HarmonyPatch(typeof(Minimap), nameof(Minimap.UpdateWindMarker))]
        public static class PatchWindMarker
        {
            public static void Prefix(Minimap __instance)
            {
                __instance.m_windMarker = BetterMapper.internalWindDir;
            }
        }

        [HarmonyPatch(typeof(Minimap), nameof(Minimap.UpdatePlayerMarker))]
        public static class RotationPatch
        {
            public static void Postfix(Minimap __instance, Player player, Quaternion playerRot)
            {
                if (!MapRotation.Value)
                    return;
                if (__instance.m_mode == Minimap.MapMode.Small)
                {
                    __instance.m_smallMarker.rotation = Quaternion.Euler(0, 0, 0);
                    Ship controlledShip = player.GetControlledShip();
                    if ((bool)controlledShip)
                    {
                        __instance.m_smallShipMarker.gameObject.SetActive(true);
                        float yawShip = controlledShip.GetShipYawAngle();
                        __instance.m_smallShipMarker.transform.rotation = Quaternion.Euler(0,0, yawShip);
                    }
                    else
                    {
                        __instance.m_smallShipMarker.gameObject.SetActive(false);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Minimap), nameof(Minimap.CenterMap))]
        public static class CenterMapPatch
        {
            public static void Postfix(Minimap __instance)
            {
                if (!MapRotation.Value)
                    return;
                __instance.m_mapImageSmall.transform.rotation = Quaternion.Euler(0, 0, Player.m_localPlayer.m_eye.transform.rotation.eulerAngles.y);
                __instance.m_pinRootSmall.transform.rotation = Quaternion.Euler(0, 0, Player.m_localPlayer.m_eye.transform.rotation.eulerAngles.y);
                for (int i = 0; i < __instance.m_pinRootSmall.childCount; i++)
                {
                    __instance.m_pinRootSmall.transform.GetChild(i).transform.rotation = Quaternion.identity;
                } 
            }
        }
    }
}