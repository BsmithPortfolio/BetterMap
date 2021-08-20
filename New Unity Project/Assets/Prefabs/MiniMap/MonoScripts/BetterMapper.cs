using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BetterMapper : MonoBehaviour
{
    [SerializeField] private GameObject MapRoot;
    [SerializeField] private RawImage MapSmall;
    [SerializeField] private Text BiomeTextSmall;
    [SerializeField] private RectTransform pinrootsmall;
    [SerializeField] private RectTransform SmallShip;
    [SerializeField] private RectTransform SmallMarker;
    [SerializeField] private RenderTexture BetterTexture;
    [SerializeField] private Image MaskImage;
    [SerializeField] private RectTransform WindDir;
    
    
    internal static GameObject internalMapRoot;
    internal static RawImage internalsmallmap;
    internal static Text internalbiometext;
    internal static RectTransform internalpinroot;
    internal static RectTransform internalsmallship;
    internal static RectTransform internalsmallmarker;
    internal static RenderTexture internalrendermaptext;
    internal static RectTransform internalWindDir;
    
    private void Awake()
    {
        internalMapRoot = MapRoot;
        internalsmallmap = MapSmall;
        internalbiometext = BiomeTextSmall;
        internalpinroot = pinrootsmall;
        internalsmallship = SmallShip;
        internalsmallmarker = SmallMarker;
        internalrendermaptext = BetterTexture;
        internalWindDir = WindDir;
    }
    
}
