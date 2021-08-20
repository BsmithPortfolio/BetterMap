using System;
using System.Linq;
using System.Reflection;
using BepInEx;
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
        private Camera minimapcam;
        private const string ModName = "BetterMaps";
        private const string ModVersion = "1.0";
        private const string ModGUID = "com.zarboz.BetterMaps";
        internal static bool cameramade;
        public void Awake()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Harmony harmony = new(ModGUID);
            harmony.PatchAll(assembly);
            LoadAssets();
            On.Minimap.UpdatePlayerMarker += Minimap_UpdatePlayerMarker;
            On.Minimap.CenterMap += Minimap_CenterMap;
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
               // __instance.m_windMarker = BetterMapper.internalWindDir;
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
        private void Minimap_UpdatePlayerMarker(On.Minimap.orig_UpdatePlayerMarker orig, Minimap self, Player player, Quaternion playerRot)
        {
            if (self.m_mode == Minimap.MapMode.Small)
            {
                self.m_smallMarker.rotation = Quaternion.Euler(0, 0, 0);
                Ship controlledShip = player.GetControlledShip();
                if ((bool)controlledShip)
                {
                    self.m_smallShipMarker.gameObject.SetActive(value: true);
                    float yawShip = controlledShip.GetShipYawAngle();
                    self.m_smallShipMarker.transform.rotation = Quaternion.Euler(0, 0, yawShip);
                }
                else
                {
                    self.m_smallShipMarker.gameObject.SetActive(value: false);
                }
            } else
            {
                orig(self, player, playerRot);
            }
        }
        
        
        private void Minimap_CenterMap(On.Minimap.orig_CenterMap orig, Minimap self, Vector3 centerPoint)
        {
            self.m_mapImageSmall.transform.rotation = Quaternion.Euler(0, 0, Player.m_localPlayer.m_eye.transform.rotation.eulerAngles.y);
            self.m_pinRootSmall.transform.rotation = Quaternion.Euler(0, 0, Player.m_localPlayer.m_eye.transform.rotation.eulerAngles.y);
            for (int i = 0; i < self.m_pinRootSmall.childCount; i++)
            {
                self.m_pinRootSmall.transform.GetChild(i).transform.rotation = Quaternion.identity;
            }
            orig(self, centerPoint);
        }
        
    }
}