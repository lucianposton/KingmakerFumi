using Kingmaker;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Persistence;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityModManagerNet;
using Newtonsoft.Json;
using Kingmaker.UI.SettingsUI;

namespace PenultimateAzlanti
{
    public static class Main
    {
        public class Settings
        {
            public Settings()
            {
                EnableKeepAzlanti = true;
                BackupPreviousAzlantiOnAutoSave = true;
                BackupAzlantiOnAutoSave = true;
            }

            public bool EnableKeepAzlanti { get; set; }
            public bool BackupPreviousAzlantiOnAutoSave { get; set; }
            public bool BackupAzlantiOnAutoSave { get; set; }
        }

        public static UnityModManager.ModEntry.ModLogger logger;
        private static Harmony12.HarmonyInstance harmony;
        private static Settings settings = new Settings();

        internal static bool Load(UnityModManager.ModEntry modEntry)
        {
            logger = modEntry.Logger;

            harmony = Harmony12.HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(typeof(Main).Assembly);

            try {
                using (StreamReader file = File.OpenText(UnityModManager.modsPath + @"/PenultimateAzlanti/settings.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    settings = (Settings)serializer.Deserialize(file, typeof(Settings));
                }
            }
            catch (Exception e)
            {
                logger.Log($"Failed to load custom settings.json due to {e.GetType().ToString()}. Using defaults.");
            }

            return true;
        }

        [Harmony12.HarmonyPatch(typeof(GameOverIronmanController), nameof(GameOverIronmanController.Activate))]
        public static class KeepSavePatch
        {
            private static bool Prefix()
            {
                if (settings.EnableKeepAzlanti)
                {
                    logger.Log("Prevented Iron Man loss.");
                    LoadingProcess.Instance.ResetManualLoadingScreen();
                    return false;
                }
                return true;
            }
        }

        [Harmony12.HarmonyPatch(typeof(SaveManager), nameof(SaveManager.SaveRoutine))]
        public static class BackupPreviousSavePatch
        {
            private static void Prefix(SaveInfo saveInfo)
            {
                //Main.Logger.Log("Saving game... " + (saveInfo.Type == SaveInfo.SaveType.IronMan).ToString() + ":" + SettingsRoot.Instance.OnlyOneSave.CurrentValue.ToString());
                if (settings.BackupPreviousAzlantiOnAutoSave && (saveInfo.Type == SaveInfo.SaveType.IronMan || SettingsRoot.Instance.OnlyOneSave.CurrentValue))
                {
                    string copy = saveInfo.FolderName + ".previous.bak";
                    try
                    {
                        System.IO.File.Copy(saveInfo.FolderName, copy, true);
                        logger.Log("Backup of previous Iron Man savegame created: " + copy);
                    }
                    catch (Exception e)
                    {
                        logger.Log("Backup of previous Iron Man savegame failed: " + e.ToString());
                    }
                }
            }
        }

        [Harmony12.HarmonyPatch]
        public static class BackupSavePatch
        {
            static System.Reflection.MethodBase TargetMethod() {
                return typeof(SaveManager).GetMethod("SerializeAndSaveThread", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            }

            private static void Postfix(SaveInfo saveInfo)
            {
                if (settings.BackupAzlantiOnAutoSave && (saveInfo.Type == SaveInfo.SaveType.IronMan || SettingsRoot.Instance.OnlyOneSave.CurrentValue))
                {
                    string copy = saveInfo.FolderName + ".bak";
                    try
                    {
                        System.IO.File.Copy(saveInfo.FolderName, copy, true);
                        logger.Log("Backup of Iron Man savegame created: " + copy);
                    }
                    catch (Exception e)
                    {
                        logger.Log("Backup of Iron Man savegame failed: " + e.ToString());
                    }
                }
            }
        }
    }
}