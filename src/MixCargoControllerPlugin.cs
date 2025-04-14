using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using BepInEx;
using CommonAPI;
using CommonAPI.Systems.ModLocalization;
using crecheng.DSPModSave;
using HarmonyLib;
using Steamworks;

namespace MixCargoController
{
    [BepInDependency(CommonAPIPlugin.GUID)]
    [BepInDependency(DSPModSavePlugin.MODGUID)]
    [BepInPlugin(GUID, NAME, VERSION)]
    [CommonAPISubmoduleDependency(nameof(LocalizationModule))]
    public class MixCargoControllerPlugin :BaseUnityPlugin, IModCanSave
    {
        public const string NAME = "MixCargoControllerPlugin";
        public const string GUID = "com.GniMaerd.MixCargoControllerPlugin";
        public const string VERSION = "0.1.0";
        public const int VERSIONINT = 100;
        public static int versionWhenLoading = VERSIONINT;

        public void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(UIBeltWindowPatcher));
            Harmony.CreateAndPatchAll(typeof(BeltCargoCountLogic));
            Harmony.CreateAndPatchAll(typeof(PathChangePatcher));

            Localizations.AddLocalizations();
        }

        public void Export(BinaryWriter w)
        {
            RuntimeData.Export(w);
        }

        public void Import(BinaryReader r)
        {
            UIBeltWindowPatcher.Init();
            RuntimeData.Import(r);

        }

        public void IntoOtherSave()
        {
            UIBeltWindowPatcher.Init();
            RuntimeData.IntoOtherSave();
        }
    }
}
