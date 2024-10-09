using BepInEx;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;

namespace LysateBeacons
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
	
	public class LysateBeacons : BaseUnityPlugin
	{
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "zombieseatflesh7";
        public const string PluginName = "LysateBeacons";
        public const string PluginVersion = "1.0.1";

        public static Dictionary<CaptainSupplyDropController, StockData> beaconStocks = null;

        public void Awake()
        {
            //Log.Init(Logger); not needed

            Stage.onServerStageBegin += OnServerStageBegin;
            On.RoR2.CaptainSupplyDropController.FixedUpdate += CaptainSupplyDropController_FixedUpdate;
        }

        private static void OnServerStageBegin(Stage stage)
        {
            beaconStocks = new Dictionary<CaptainSupplyDropController, StockData>();
        }

        private static void CaptainSupplyDropController_FixedUpdate(On.RoR2.CaptainSupplyDropController.orig_FixedUpdate orig, CaptainSupplyDropController self)
        {
            StockData data = null;
            if (!beaconStocks.ContainsKey(self))
            {
                data = new StockData();
                data.lysateStacks = self.GetFieldValue<CharacterBody>("characterBody").inventory.GetItemCount(DLC1Content.Items.EquipmentMagazineVoid);
                data.extraStocks = data.lysateStacks;
                beaconStocks.Add(self, data);
                orig(self);
            }
            data = beaconStocks[self];
            int num = self.GetFieldValue<CharacterBody>("characterBody").inventory.GetItemCount(DLC1Content.Items.EquipmentMagazineVoid) - data.lysateStacks;
            if (num != 0)
            {
                data.lysateStacks += num;
                data.extraStocks += num;
            }
            if (data.extraStocks > 0 && self.supplyDrop1Skill.stock == 0)
            {
                self.supplyDrop1Skill.AddOneStock();
                data.extraStocks--;
            }
            if (data.extraStocks > 0 && self.supplyDrop2Skill.stock == 0)
            {
                self.supplyDrop2Skill.AddOneStock();
                data.extraStocks--;
            }
            if (data.extraStocks < 0 && self.supplyDrop2Skill.stock > 0)
            {
                self.supplyDrop2Skill.RemoveAllStocks();
                data.extraStocks++;
            }
            if (data.extraStocks < 0 && self.supplyDrop1Skill.stock > 0)
            {
                self.supplyDrop1Skill.RemoveAllStocks();
                data.extraStocks++;
            }
            orig(self);
        }
    }

    public class StockData
    {
        public int lysateStacks = 0;
        public int extraStocks = 0;
    }
}
