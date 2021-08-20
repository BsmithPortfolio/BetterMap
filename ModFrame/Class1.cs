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
            On.Minimap.CenterMap += CenterMapHook;
        }

        private void CenterMapHook(On.Minimap.orig_CenterMap orig, Minimap self, Vector3 centerpoint)
        {
            self.WorldToMapPoint(centerpoint, out var mx, out var my);
            Rect uvRect = self.m_mapImageSmall.uvRect;
            uvRect.width = self.m_smallZoom;
            uvRect.height = self.m_smallZoom;
            uvRect.center = new Vector2(mx, my);
            self.m_mapImageSmall.uvRect = uvRect;
            RectTransform rectTransform = self.m_mapImageLarge.transform as RectTransform;
            float num = rectTransform.rect.width / rectTransform.rect.height;
            Rect uvRect2 = self.m_mapImageSmall.uvRect;
            uvRect2.width = self.m_largeZoom * num;
            uvRect2.height = self.m_largeZoom;
            uvRect2.center = new Vector2(mx, my);
            self.m_mapImageLarge.uvRect = uvRect2;
            if (self.m_mode == Minimap.MapMode.Large)
            {
                self.m_mapImageLarge.material.SetFloat("_zoom",self.m_largeZoom);
                self.m_mapImageLarge.material.SetFloat("_pixelSize", 200f / self.m_largeZoom);
                self.m_mapImageLarge.material.SetVector("_mapCenter", centerpoint);
            }
            else
            {
                self.m_mapImageSmall.material.SetFloat("_zoom", self.m_smallZoom);
                self.m_mapImageSmall.material.SetFloat("_pixelSize", 200f / self.m_smallZoom);
                self.m_mapImageSmall.material.SetVector("_mapCenter", centerpoint);
            }
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
                __instance.m_smallRoot = BetterMapper.internalMapRoot; 
                __instance.m_biomeNameSmall = BetterMapper.internalbiometext;
                __instance.m_smallMarker = BetterMapper.internalsmallmarker;
                BetterMapper.internalsmallmap.material = UnityEngine.Object.Instantiate(__instance.m_mapImageSmall.material);
                BetterMapper.internalsmallmap.maskable = true;
                __instance.m_mapImageSmall = BetterMapper.internalsmallmap; //this line still bothers me because the awake function pulls the material from m_small (see valheim line where it instantiates the material from the stock thing) m_mapImageSmall.material = UnityEngine.Object.Instantiate(m_mapImageSmall.material);
                __instance.m_smallShipMarker = BetterMapper.internalsmallship;
                __instance.m_pinRootSmall = BetterMapper.internalpinroot;
               // __instance.m_windMarker = BetterMapper.internalWindDir;
            }

        }
        
        
    }
}