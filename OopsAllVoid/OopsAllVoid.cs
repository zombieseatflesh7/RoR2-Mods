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
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    public class OopsAllVoid : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "zombieseatflesh7";
        public const string PluginName = "OopsAllVoid";
        public const string PluginVersion = "1.1.0";

        public static PluginInfo PInfo { get; private set; }

        public void Awake()
        {
            Log.Init(Logger);

            PInfo = Info;
            Assets.Init();
            CorruptionArtifact.Init();
        }
    }

    public static class CorruptionArtifact
    {
        public static ArtifactDef Corruption;

        //used to filter out interactables that dont work with this mod
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

        //stores all droptables
        public static List<PickupDropTable> DropTables = null;
        //stores the weights of all droptables so they can be restored after the artifact is disabled
        public static float[][] DropTableValues = null;

        public static void Init()
        {
            Corruption = ScriptableObject.CreateInstance<ArtifactDef>();
            Corruption.cachedName = "ArtifactOfCorruption";
            Corruption.nameToken = "Artifact of Corruption";
            Corruption.descriptionToken = "Replaces all items except equipment and lunar items with void items.";
            Corruption.smallIconSelectedSprite = Assets.AssetBundle.LoadAsset<Sprite>("texArtifactCorruptionEnabled.png");
            Corruption.smallIconDeselectedSprite = Assets.AssetBundle.LoadAsset<Sprite>("texArtifactCorruptionDisabled.png");
            ContentAddition.AddArtifactDef(Corruption);

            RunArtifactManager.onArtifactEnabledGlobal += OnArtifactEnabled;
            RunArtifactManager.onArtifactDisabledGlobal += OnArtifactDisabled;
        }

        private static void OnArtifactEnabled(RunArtifactManager runArtifactManager, ArtifactDef artifactDef)
        {
            if (artifactDef != Corruption || !NetworkServer.active)
            {
                return;
            }

            DropTables = typeof(PickupDropTable).GetFieldValue<List<PickupDropTable>>("instancesList");
            DropTableValues = new float[DropTables.Count()][];
            int i = 0;
            foreach (BasicPickupDropTable dropTable in DropTables.OfType<BasicPickupDropTable>())
            {
                DropTableValues[i] = new float[6];
                DropTableValues[i][0] = dropTable.tier1Weight;
                DropTableValues[i][1] = dropTable.tier2Weight;
                DropTableValues[i][2] = dropTable.tier3Weight;
                DropTableValues[i][3] = dropTable.voidTier1Weight;
                DropTableValues[i][4] = dropTable.voidTier2Weight;
                DropTableValues[i][5] = dropTable.voidTier3Weight;

                dropTable.voidTier1Weight = dropTable.tier1Weight + dropTable.voidTier1Weight;
                dropTable.voidTier2Weight = dropTable.tier2Weight + dropTable.voidTier2Weight;
                dropTable.voidTier3Weight = dropTable.tier3Weight + dropTable.voidTier3Weight;
                dropTable.tier1Weight = 0f;
                dropTable.tier2Weight = 0f;
                dropTable.tier3Weight = 0f;
                i++;
            }
            typeof(PickupDropTable).InvokeMethod("RegenerateAll", Run.instance);

            On.RoR2.BossGroup.DropRewards += BossGroup_DropRewards;
            SceneDirector.onGenerateInteractableCardSelection += OnGenerateInteractableCardSelection;
        }

        private static void OnArtifactDisabled(RunArtifactManager runArtifactManager, ArtifactDef artifactDef)
        {
            if (artifactDef != Corruption)
            {
                return;
            }

            int i = 0;
            foreach (BasicPickupDropTable dropTable in DropTables.OfType<BasicPickupDropTable>())
            {
                dropTable.tier1Weight = DropTableValues[i][0];
                dropTable.tier2Weight = DropTableValues[i][1];
                dropTable.tier3Weight = DropTableValues[i][2];
                dropTable.voidTier1Weight = DropTableValues[i][3];
                dropTable.voidTier2Weight = DropTableValues[i][4];
                dropTable.voidTier3Weight = DropTableValues[i][5];
                i++;
            }
            DropTables = null;
            DropTableValues = null;

            On.RoR2.BossGroup.DropRewards -= BossGroup_DropRewards;
            SceneDirector.onGenerateInteractableCardSelection -= OnGenerateInteractableCardSelection;
        }

        public static void BossGroup_DropRewards(On.RoR2.BossGroup.orig_DropRewards orig, BossGroup self)
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
        }

        private static void OnGenerateInteractableCardSelection(SceneDirector sceneDirector, DirectorCardCategorySelection dccs)
        {
            dccs.RemoveCardsThatFailFilter(new System.Predicate<DirectorCard>(InteractableFilter));
        }

        private static bool InteractableFilter(DirectorCard card)
        {
            if (card.spawnCard == duplicatorT1SpawnCard
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

    public static class Assets
    {
        public static AssetBundle AssetBundle;
        public const string dllName = "OopsAllVoid.dll";
        public const string bundleName = "oavassetbundle";

        public static void Init()
        {
            AssetBundle = AssetBundle.LoadFromFile(OopsAllVoid.PInfo.Location.Replace(dllName, bundleName));
            if (AssetBundle == null)
            {
                Log.LogInfo("Failed to load AssetBundle!");
                return;
            }
        }
    }
}
