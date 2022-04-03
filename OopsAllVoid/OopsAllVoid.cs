using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Artifacts;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Linq;

namespace OopsAllVoid
{
    //This is an example plugin that can be put in BepInEx/plugins/ExamplePlugin/ExamplePlugin.dll to test out.
    //It's a small plugin that adds a relatively simple item to the game, and gives you that item whenever you press F2.

    //This attribute specifies that we have a dependency on R2API, as we're using it to add our item to the game.
    //You don't need this if you're not using R2API in your plugin, it's just to tell BepInEx to initialize R2API before this plugin so it's safe to use R2API.
    //[BepInDependency()]

    //This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    //We will be using 2 modules from R2API: ItemAPI to add our item and LanguageAPI to add our language tokens.
    //[R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI))]

    //This is the main declaration of our plugin class. BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    //BaseUnityPlugin itself inherits from MonoBehaviour, so you can use this as a reference for what you can declare and use in your plugin class: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    public class OopsAllVoid : BaseUnityPlugin
    {
        //The Plugin GUID should be a unique ID for this plugin, which is human readable (as it is used in places like the config).
        //If we see this PluginGUID as it is on thunderstore, we will deprecate this mod. Change the PluginAuthor and the PluginName !
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "zombieseatflesh7";
        public const string PluginName = "OopsAllVoid";
        public const string PluginVersion = "1.0.0";

        public static InteractableSpawnCard duplicatorT1SpawnCard = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/Duplicator/iscDuplicator.asset").WaitForCompletion();
        public static InteractableSpawnCard duplicatorT2SpawnCard = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/DuplicatorLarge/iscDuplicatorLarge.asset").WaitForCompletion();
        public static InteractableSpawnCard duplicatorT3SpawnCard = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/DuplicatorMilitary/iscDuplicatorMilitary.asset").WaitForCompletion();
        public static InteractableSpawnCard duplicatorBossSpawnCard = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/DuplicatorWild/iscDuplicatorWild.asset").WaitForCompletion();
        public static InteractableSpawnCard scrapperSpawnCard = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/Scrapper/iscScrapper.asset").WaitForCompletion();
        public static InteractableSpawnCard smallDamageChestSpawnCard = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/CategoryChest/iscCategoryChestDamage.asset").WaitForCompletion();
        public static InteractableSpawnCard smallHealingChestSpawnCard = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/CategoryChest/iscCategoryChestHealing.asset").WaitForCompletion();
        public static InteractableSpawnCard smallUtilityChestSpawnCard = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/CategoryChest/iscCategoryChestUtility.asset").WaitForCompletion();
        public static InteractableSpawnCard largeDamageChestSpawnCard = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/DLC1/CategoryChest2/iscCategoryChest2Damage.asset").WaitForCompletion();
        public static InteractableSpawnCard largeHealingChestSpawnCard = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/DLC1/CategoryChest2/iscCategoryChest2Healing.asset").WaitForCompletion();
        public static InteractableSpawnCard largeUtilityChestSpawnCard = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/DLC1/CategoryChest2/iscCategoryChest2Utility.asset").WaitForCompletion();

        public void Awake()
        {
            Log.Init(Logger);

            On.RoR2.Run.Start += (orig, self) =>
            {
                List<PickupDropTable> list = typeof(PickupDropTable).GetFieldValue<List<PickupDropTable>>("instancesList");
                for(int i = 0; i < list.Count; i++)
                {
                    foreach (BasicPickupDropTable dropTable in list.OfType<BasicPickupDropTable>())
                    {
                        ConvertDropTable(dropTable);
                    }
                }
                typeof(PickupDropTable).InvokeMethod("RegenerateAll", Run.instance);
                orig(self);
            };

            On.RoR2.BossGroup.DropRewards += (orig, self) =>
            {
                List<PickupIndex> bossDrops = self.GetFieldValue<List<PickupIndex>>("bossDrops");
                List<PickupDropTable> bossDropTables = self.GetFieldValue<List<PickupDropTable>>("bossDropTables");
                Xoroshiro128Plus rng = self.GetFieldValue<Xoroshiro128Plus>("rng");
                for (int i = 0; i < bossDrops.Count; i++)
                {
                    bossDrops[i] = rng.NextElementUniform<PickupIndex>(Run.instance.availableVoidBossDropList);
                }
                for (int i = 0; i < bossDropTables.Count; i++)
                {
                    bossDrops.Add(rng.NextElementUniform<PickupIndex>(Run.instance.availableVoidBossDropList));
                }
                self.SetFieldValue<List<PickupDropTable>>("bossDropTables", new List<PickupDropTable>());
                orig(self);
            };

            SceneDirector.onGenerateInteractableCardSelection += OnGenerateInteractableCardSelection;

            Log.LogInfo(nameof(Awake) + " done.");
        }

        private static void ConvertDropTable(BasicPickupDropTable dropTable)
        {
            dropTable.voidTier1Weight = dropTable.tier1Weight + dropTable.voidTier1Weight;
            dropTable.voidTier2Weight = dropTable.tier2Weight + dropTable.voidTier2Weight;
            dropTable.voidTier3Weight = dropTable.tier3Weight + dropTable.voidTier3Weight;
            dropTable.tier1Weight = 0f;
            dropTable.tier2Weight = 0f;
            dropTable.tier3Weight = 0f;
            
        }

        private static void OnGenerateInteractableCardSelection(SceneDirector sceneDirector, DirectorCardCategorySelection dccs)
        {
            dccs.RemoveCardsThatFailFilter(new System.Predicate<DirectorCard>(InteractableFilter));
        }

        private static bool InteractableFilter(DirectorCard card)
        {
            if(card.spawnCard == duplicatorT1SpawnCard
                || card.spawnCard == duplicatorT2SpawnCard
                || card.spawnCard == duplicatorT3SpawnCard
                || card.spawnCard == duplicatorBossSpawnCard
                || card.spawnCard == scrapperSpawnCard
                || card.spawnCard == smallDamageChestSpawnCard
                || card.spawnCard == smallHealingChestSpawnCard
                || card.spawnCard == smallUtilityChestSpawnCard
                || card.spawnCard == largeDamageChestSpawnCard
                || card.spawnCard == largeHealingChestSpawnCard
                || card.spawnCard == largeUtilityChestSpawnCard)
            {
                return false;
            }
            return true;
        }
    }
}
