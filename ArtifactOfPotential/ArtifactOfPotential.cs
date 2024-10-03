using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Artifacts;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine.Bindings;
using UnityEngine.UIElements;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace ArtifactOfPotential
{
    [BepInDependency(R2API.R2API.PluginGUID)]
    //[BepInDependency("com.KingEnderBrine.InLobbyConfig")]

    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    //[R2APISubmoduleDependency(nameof())]

    public class ArtifactOfPotential : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "zombieseatflesh7";
        public const string PluginName = "ArtifactOfPotential";
        public const string PluginVersion = "1.2.2";

        public static PluginInfo PInfo { get; private set; }

        public void Awake()
        {
            Log.Init(Logger);

            PInfo = Info;
            InitConfig();
            Asset.Init();
            PotentialArtifact.Init();
        }

        private void InitConfig()
        {
            Settings.AnyTierMode = Config.Bind<bool>("Any Tier Mode", "Any Tier Mode", false, "Similar to Simulacrum. An alternate mode where the options you choose from can be of different tiers, as long as that tier CAN be dropped. Affects: chests, shrines, etc. Ignores: multishops, lunar pods, boss events, etc. Synergizes with Eulogy Zero. Does not affect void items.");
            Settings.AnyTierModeChoiceCount = Config.Bind<int>("Any Tier Mode", "Any Tier Mode - Choice Count", 3, "The number of choices you get from void potentials when \"Any Tier Mode\" is enabled. Does not affect void items.");
            Settings.AnyTierModeVoid = Config.Bind<bool>("Any Tier Mode", "Any Tier Mode - Void", true, "Similar to Any Tier Mode, but only affects void items.");
            Settings.AnyTierModeVoidChoiceCount = Config.Bind<int>("Any Tier Mode", "Any Tier Mode - Void - Choice Count", 3, "The number of choices you get from void tier void potentials when \"Any Tier Mode - Void\" is enabled. Only affects void items.");

            Settings.ChestsAffected = Config.Bind<bool>("Item Sources Affected", "Chests", true, "Whether or not chests should drop void potentials.");
            Settings.ShrineOfChanceAffected = Config.Bind<bool>("Item Sources Affected", "Shrines of chance", true, "Whether or not shrines of chance should drop void potentials.");
            Settings.HiddenMultishopsAffected = Config.Bind<bool>("Item Sources Affected", "Mystery Multishops", true, "Whether or not mystery multishops should drop void potentials. Works even if the \"Multishops\" config is disabled.");
            Settings.MultishopsAffected = Config.Bind<bool>("Item Sources Affected", "Multishops", false, "Whether or not multishops should drop void potentials. Includes mystery multishopsm even if that config is disabled");
            //Settings.PrintersAffected = Config.Bind<bool>("Item Sources Affected", "Printers and Cauldrons", true, "Whether or not printers and cauldrons should drop void potentials. This feature is currently unfinished.");
            Settings.ShopsAffected = Config.Bind<bool>("Item Sources Affected", "Shops", false, "Whether or not shops should drop void potentials. This includes: The bazaar shop, printers, and cauldrons. This setting will change in the future.");
            Settings.BossAffected = Config.Bind<bool>("Item Sources Affected", "Boss", true, "Whether or not bosses should drop void potentials. This includes: the teleporter event, Alloy Worship Unit, Aurelionite, and any other \"boss\" event.");
            Settings.SacrificeAffected = Config.Bind<bool>("Item Sources Affected", "Artifact of Sacrifice", true, "Whether or not Artifact of Sacrifice should drop void potentials.");

            Settings.Tier1ChoiceCount = Config.Bind<int>("Number of Options by Tier", "Common Options", 3, "The number of choices you get from common tier void potentials. Set to 1 to disable void potentials for this tier.");
            Settings.Tier2ChoiceCount = Config.Bind<int>("Number of Options by Tier", "Uncommon Options", 3, "The number of choices you get from uncommon tier void potentials. Set to 1 to disable void potentials for this tier.");
            Settings.Tier3ChoiceCount = Config.Bind<int>("Number of Options by Tier", "Legendary Options", 3, "The number of choices you get from legendary tier void potentials. Set to 1 to disable void potentials for this tier.");
            Settings.EquipmentChoiceCount = Config.Bind<int>("Number of Options by Tier", "Equipment Options", 3, "The number of choices you get from Equipment tier void potentials. Set to 1 to disable void potentials for this tier.");
            Settings.BossChoiceCount = Config.Bind<int>("Number of Options by Tier", "Boss Options", 1, "The number of choices you get from boss tier void potentials. Set to 1 to disable void potentials for this tier. Boss items are special, because they are based on the boss that is killed. Setting this value above 1 will allow you to get boss items that are hard to find normally, such as charged perforators or planulas.");
            Settings.LunarChoiceCount = Config.Bind<int>("Number of Options by Tier", "Lunar Options", 3, "The number of choices you get from lunar tier void potentials. Set to 1 to disable void potentials for this tier.");
            Settings.Void1ChoiceCount = Config.Bind<int>("Number of Options by Tier", "Common Void Options", 3, "The number of choices you get from common void tier void potentials. Set to 1 to disable void potentials for this tier.");
            Settings.Void2ChoiceCount = Config.Bind<int>("Number of Options by Tier", "Uncommon Void Options", 3, "The number of choices you get from uncommon void tier void potentials. Set to 1 to disable void potentials for this tier.");
            Settings.Void3ChoiceCount = Config.Bind<int>("Number of Options by Tier", "Legendary Void Options", 3, "The number of choices you get from legendary void tier void potentials. Set to 1 to disable void potentials for this tier.");
            Settings.VoidBossChoiceCount = Config.Bind<int>("Number of Options by Tier", "Boss Void Options", 3, "The number of choices you get from boss void tier void potentials. Set to 1 to disable void potentials for this tier. This option is redundant as there is only one boss void item at the time of this mod versions upload.");

        }
    }
    public static class PotentialArtifact
    {
        public static ArtifactDef Potential;

        public static GameObject voidPotentialPrefab = null;
        public static GameObject commandCubePrefab = null;
        public static Xoroshiro128Plus rng = null;
        public static PickupDropTable dropTable = null;

        public enum PickupType
        {
            Ignore, NoDropTable, BasicDropTable, BossDropTable
        }
        public static PickupType nextPickup = PickupType.Ignore;

        public static void Init()
        {
            Potential = ScriptableObject.CreateInstance<ArtifactDef>();
            Potential.cachedName = "ArtifactOfPotential";
            Potential.nameToken = "Artifact of Potential";
            Potential.descriptionToken = "Most items become Void Potentials.";
            Potential.smallIconSelectedSprite = Asset.AssetBundle.LoadAsset<Sprite>("texArtifactPotentialEnabled.png");
            Potential.smallIconDeselectedSprite = Asset.AssetBundle.LoadAsset<Sprite>("texArtifactPotentialDisabled.png");
            ContentAddition.AddArtifactDef(Potential);

            voidPotentialPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/OptionPickup/OptionPickup.prefab").WaitForCompletion();
            commandCubePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Command/CommandCube.prefab").WaitForCompletion();
            
            RunArtifactManager.onArtifactEnabledGlobal += OnArtifactEnabled;
            RunArtifactManager.onArtifactDisabledGlobal += OnArtifactDisabled;
        }

        private static void OnArtifactEnabled(RunArtifactManager runArtifactManager, ArtifactDef artifactDef)
        {
            if (artifactDef != Potential)
            {
                return;
            }
            if (NetworkServer.active)
            {
                if(Settings.ChestsAffected.Value)
                    On.RoR2.ChestBehavior.BaseItemDrop += ChestBehavior_BaseItemDrop;
                if(Settings.ShrineOfChanceAffected.Value)
                    On.RoR2.ShrineChanceBehavior.AddShrineStack += ShrineChanceBehavior_AddShrineStack;
                if(Settings.BossAffected.Value)
                    On.RoR2.BossGroup.DropRewards += BossGroup_DropRewards;
                if(Settings.HiddenMultishopsAffected.Value || Settings.MultishopsAffected.Value || Settings.ShopsAffected.Value)
                    On.RoR2.ShopTerminalBehavior.DropPickup += ShopTerminalBehavior_DropPickup;
                if(Settings.SacrificeAffected.Value)
                    On.RoR2.Artifacts.SacrificeArtifactManager.OnServerCharacterDeath += SacrificeArtifactManager_OnServerCharacterDeath;

                On.RoR2.PickupDropletController.CreatePickupDroplet_PickupIndex_Vector3_Vector3 += PickupDropletController_CreatePickupDroplet_PickupIndex_Vector3_Vector3;
                On.RoR2.PickupDropletController.CreatePickupDroplet_CreatePickupInfo_Vector3_Vector3 += PickupDropletController_CreatePickupDroplet_CreatePickupInfo_Vector3_Vector3;
                On.RoR2.GenericPickupController.CreatePickup += GenericPickupController_CreatePickup;
            }
        }

        private static void OnArtifactDisabled(RunArtifactManager runArtifactManager, ArtifactDef artifactDef)
        {
            if (artifactDef != Potential)
            {
                return;
            }
            On.RoR2.ChestBehavior.BaseItemDrop -= ChestBehavior_BaseItemDrop;
            On.RoR2.ShrineChanceBehavior.AddShrineStack -= ShrineChanceBehavior_AddShrineStack;
            On.RoR2.BossGroup.DropRewards -= BossGroup_DropRewards;
            On.RoR2.ShopTerminalBehavior.DropPickup -= ShopTerminalBehavior_DropPickup;
            On.RoR2.Artifacts.SacrificeArtifactManager.OnServerCharacterDeath -= SacrificeArtifactManager_OnServerCharacterDeath;
            On.RoR2.PickupDropletController.CreatePickupDroplet_PickupIndex_Vector3_Vector3 -= PickupDropletController_CreatePickupDroplet_PickupIndex_Vector3_Vector3;
            On.RoR2.PickupDropletController.CreatePickupDroplet_CreatePickupInfo_Vector3_Vector3 -= PickupDropletController_CreatePickupDroplet_CreatePickupInfo_Vector3_Vector3;
            On.RoR2.GenericPickupController.CreatePickup -= GenericPickupController_CreatePickup;
        }

        private static GenericPickupController GenericPickupController_CreatePickup(On.RoR2.GenericPickupController.orig_CreatePickup orig, ref GenericPickupController.CreatePickupInfo createPickupInfo)
        {
            if (createPickupInfo.prefabOverride == commandCubePrefab)
            {
                //This is a bit hacky but it's the only way I could find to make artifact prefab to work.
                //Basically cutting out a portion of the decompiled code to make it work.
                //Looking for mesh renderer child that doesn't exist in commandcube.prefab - Valkarin
                Log.LogDebug("Prefab Overrid is CommandCube");
                GameObject gameObject = Object.Instantiate(createPickupInfo.prefabOverride ?? GenericPickupController.pickupPrefab, createPickupInfo.position, createPickupInfo.rotation);
                GenericPickupController component = gameObject.GetComponent<GenericPickupController>();
                if ((bool)component)
                {
                    component.NetworkpickupIndex = createPickupInfo.pickupIndex;
                    component.chestGeneratedFrom = createPickupInfo.chest;
                }
                PickupIndexNetworker component2 = gameObject.GetComponent<PickupIndexNetworker>();
                if ((bool)component2)
                {
                    component2.NetworkpickupIndex = createPickupInfo.pickupIndex;
                }
                PickupPickerController component3 = gameObject.GetComponent<PickupPickerController>();
                if ((bool)component3 && createPickupInfo.pickerOptions != null)
                {
                    component3.SetOptionsServer(createPickupInfo.pickerOptions);
                }
                NetworkServer.Spawn(gameObject);
                return component;
            }
            else
            {
                orig(ref createPickupInfo);
            }
            return null;

        }

        private static void ChestBehavior_BaseItemDrop(On.RoR2.ChestBehavior.orig_BaseItemDrop orig, ChestBehavior self)
        {
            rng = self.GetFieldValue<Xoroshiro128Plus>("rng");
            dropTable = self.dropTable;
            nextPickup = PickupType.BasicDropTable;
            orig(self);
            rng = null;
            dropTable = null;
            nextPickup = PickupType.Ignore;
        }

        private static void ShrineChanceBehavior_AddShrineStack(On.RoR2.ShrineChanceBehavior.orig_AddShrineStack orig, ShrineChanceBehavior self, Interactor activator)
        {
            rng = self.GetFieldValue<Xoroshiro128Plus>("rng");
            dropTable = self.dropTable;
            nextPickup = PickupType.BasicDropTable;
            orig(self, activator);
            rng = null;
            dropTable = null;
            nextPickup = PickupType.Ignore;
        }

        private static void SacrificeArtifactManager_OnServerCharacterDeath(On.RoR2.Artifacts.SacrificeArtifactManager.orig_OnServerCharacterDeath orig, DamageReport damageReport)
        {
            rng = typeof(SacrificeArtifactManager).GetFieldValue<Xoroshiro128Plus>("treasureRng");
            dropTable = typeof(SacrificeArtifactManager).GetFieldValue<PickupDropTable>("dropTable");
            nextPickup = PickupType.BasicDropTable;
            orig(damageReport);
            rng = null;
            dropTable = null;
            nextPickup = PickupType.Ignore;
        }

        private static void ShopTerminalBehavior_DropPickup(On.RoR2.ShopTerminalBehavior.orig_DropPickup orig, ShopTerminalBehavior self)
        {
            if (self.serverMultiShopController != null && (Settings.MultishopsAffected.Value || (Settings.HiddenMultishopsAffected.Value && self.GetFieldValue<bool>("hidden"))))
            {
                rng = self.serverMultiShopController.GetFieldValue<Xoroshiro128Plus>("rng");
                nextPickup = PickupType.NoDropTable;
                orig(self);
                rng = null;
                nextPickup = PickupType.Ignore;
                return;
            }
            if (self.serverMultiShopController == null && Settings.ShopsAffected.Value)
            {
                rng = self.GetFieldValue<Xoroshiro128Plus>("rng");
                dropTable = self.dropTable;
                nextPickup = PickupType.BasicDropTable;
                orig(self);
                rng = null;
                dropTable = null;
                nextPickup = PickupType.Ignore;
                return;
            }
            orig(self);
        }
        
        private static void BossGroup_DropRewards(On.RoR2.BossGroup.orig_DropRewards orig, BossGroup self)
        {
            rng = self.GetFieldValue<Xoroshiro128Plus>("rng");
            nextPickup = PickupType.BossDropTable;
            orig(self);
            rng = null;
            nextPickup = PickupType.Ignore;
            for (int i = 0; i < bossDropsByTier.Length; i++)
            {
                bossDropsByTier[i] = null;
            }
        }

        public static PickupIndex[][] bossDropsByTier = new PickupIndex[10][];

        private static void PickupDropletController_CreatePickupDroplet_PickupIndex_Vector3_Vector3(On.RoR2.PickupDropletController.orig_CreatePickupDroplet_PickupIndex_Vector3_Vector3 orig, PickupIndex pickupIndex, Vector3 position, Vector3 velocity)
        {
            switch (nextPickup)
            {
                case PickupType.Ignore:
                    orig(pickupIndex, position, velocity);
                    break;
                case PickupType.NoDropTable:
                    nextPickup = PickupType.Ignore;
                    PickupDropletController.CreatePickupDroplet(CreatePickupInfo_NoDropTable(pickupIndex, position), position, velocity);
                    break;
                case PickupType.BasicDropTable:
                    nextPickup = PickupType.Ignore;
                    PickupDropletController.CreatePickupDroplet(CreatePickupInfo_BasicPickupDropTable(pickupIndex, position), position, velocity);
                    break;
                case PickupType.BossDropTable:
                    PickupDropletController.CreatePickupDroplet(CreatePickupInfo_BossDropTable(pickupIndex, position), position, velocity);
                    break;
            }
        }

        private static void PickupDropletController_CreatePickupDroplet_CreatePickupInfo_Vector3_Vector3(On.RoR2.PickupDropletController.orig_CreatePickupDroplet_CreatePickupInfo_Vector3_Vector3 orig, GenericPickupController.CreatePickupInfo pickupInfo, Vector3 position, Vector3 velocity)
        {
            if (pickupInfo.pickerOptions != null && pickupInfo.pickerOptions.Length > 0)
            {
                orig(pickupInfo, position, velocity);
                return;
            }

            GenericPickupController.CreatePickupInfo newPickupInfo;
            switch (nextPickup)
            {
                case PickupType.NoDropTable:
                    nextPickup = PickupType.Ignore;
                    newPickupInfo = CreatePickupInfo_NoDropTable(pickupInfo.pickupIndex, pickupInfo.position);
                    newPickupInfo.chest = pickupInfo.chest;
                    newPickupInfo.artifactFlag = pickupInfo.artifactFlag;
                    pickupInfo = newPickupInfo;
                    break;
                case PickupType.BasicDropTable:
                    nextPickup = PickupType.Ignore;
                    newPickupInfo = CreatePickupInfo_BasicPickupDropTable(pickupInfo.pickupIndex, pickupInfo.position);
                    newPickupInfo.chest = pickupInfo.chest;
                    newPickupInfo.artifactFlag = pickupInfo.artifactFlag;
                    pickupInfo = newPickupInfo;
                    break;
            }
            orig(pickupInfo, position, velocity);
        }

        private static GenericPickupController.CreatePickupInfo CreatePickupInfo_NoDropTable(PickupIndex pickupIndex, Vector3 position)
        {
            Log.LogInfo("Creating pickup without drop table");

            GenericPickupController.CreatePickupInfo pickupInfo = new GenericPickupController.CreatePickupInfo
            {
                position = position,
                rotation = Quaternion.identity,
                pickupIndex = pickupIndex
            };

            int tier = GetTier(pickupIndex);
            PickupIndex[] choices = null;
            PickupIndex[] choices2 = null;

            choices2 = GetUniqueItemsOfSameTier(Settings.GetChoiceCountByTier(tier) - 1, pickupIndex, rng);
            int num = choices2.Length;
            if (num == 0)
                return pickupInfo;

            choices = new PickupIndex[num + 1];

            choices[0] = pickupIndex;
            for (int i = 0; i < num; i++)
                choices[i + 1] = choices2[i];

            pickupInfo.pickerOptions = PickupPickerController.GenerateOptionsFromArray(choices);
            pickupInfo.prefabOverride = (choices.Length > 3) ? commandCubePrefab : voidPotentialPrefab;
            //Sets the pickupIndex to the Tier. This is how it is done in the Void Field area. - Valkarin
            if (choices.Length <= 3 && PickupCatalog.itemTierToPickupIndex.TryGetValue(PickupCatalog.GetPickupDef(pickupIndex).itemTier, out var value))
            {
                Log.LogInfo("Item Tier to Pickup Idex: " + value);
                pickupInfo.pickupIndex = value;
            }
            else if (tier == 4)
            {
                //Couldn't get equipment to use the pickupOption prefab - Valkarin
                pickupInfo.prefabOverride = commandCubePrefab;
            }
            return pickupInfo;
        }

        private static GenericPickupController.CreatePickupInfo CreatePickupInfo_BasicPickupDropTable(PickupIndex pickupIndex, Vector3 position)
        {
            Log.LogInfo("Creating pickup from basic drop table");

            GenericPickupController.CreatePickupInfo pickupInfo = new GenericPickupController.CreatePickupInfo
            {
                position = position,
                rotation = Quaternion.identity,
                pickupIndex = pickupIndex
            };

            if (dropTable == null)
            {
                Log.LogInfo("The droptable is null! This is usually the result of an error.");
                rng = null;
                return pickupInfo;
            }
            int tier = GetTier(pickupIndex);
            PickupIndex[] choices = null;
            PickupIndex[] choices2 = null;
            int num = 0;
            if ((Settings.AnyTierMode.Value && tier <= 6) || (Settings.AnyTierModeVoid.Value && tier >= 7)) //Any Tier Mode or Any Tier Mode Viod is true
            {
                WeightedSelection<PickupIndex> selection = ((BasicPickupDropTable)dropTable).GetFieldValue<WeightedSelection<PickupIndex>>("selector");
                for (int i = 0; i < selection.Count; i++)
                {
                    if (selection.GetChoice(i).value == pickupIndex)
                    {
                        selection.RemoveChoice(i);
                        i--;
                    }
                }
                num = Mathf.Min((tier <= 6) ? (Settings.AnyTierModeChoiceCount.Value - 1) : (Settings.AnyTierModeVoidChoiceCount.Value - 1), selection.Count);
                if (num == 0)
                {
                    return pickupInfo;
                }
                choices = new PickupIndex[num + 1];
                choices2 = dropTable.GenerateUniqueDrops(num, rng);
            }
            else if (tier == 6) //is lunar tier (for eulogy zero)
            {
                choices2 = GetUniqueItemsOfSameTier(Settings.GetChoiceCountByTier(tier) - 1, pickupIndex, rng);
                num = choices2.Length;
                if (num == 0)
                {
                    return pickupInfo;
                }
                choices = new PickupIndex[num + 1];
            }
            else //is not lunar tier
            {
                dropTable.canDropBeReplaced = false;
                WeightedSelection<PickupIndex> selection = ((BasicPickupDropTable)dropTable).GetFieldValue<WeightedSelection<PickupIndex>>("selector");
                for (int i = 0; i < selection.Count; i++)
                {
                    if (GetTier(selection.GetChoice(i).value) != tier || selection.GetChoice(i).value == pickupIndex)
                    {
                        selection.RemoveChoice(i);
                        i--;
                    }
                }
                num = Mathf.Min(Settings.GetChoiceCountByTier(tier) - 1, selection.Count);
                if (num == 0)
                {
                    return pickupInfo;
                }
                choices = new PickupIndex[num + 1];
                choices2 = dropTable.GenerateUniqueDrops(num, rng);
                dropTable.canDropBeReplaced = true;
                dropTable.InvokeMethod("Regenerate", Run.instance);
            }

            choices[0] = pickupIndex;
            for (int i = 0; i < num; i++)
            {
                choices[i + 1] = choices2[i];
            }
            pickupInfo.pickerOptions = PickupPickerController.GenerateOptionsFromArray(choices);
            pickupInfo.prefabOverride = (choices.Length > 3) ? commandCubePrefab : voidPotentialPrefab;
            //see line 334
            if (choices.Length <= 3 && PickupCatalog.itemTierToPickupIndex.TryGetValue(PickupCatalog.GetPickupDef(pickupIndex).itemTier, out var value))
            {
                Log.LogInfo("Item Tier to Pickup Idex: " + value);
                pickupInfo.pickupIndex = value;
            } 
            else if (tier == 4)
            {
                pickupInfo.prefabOverride = commandCubePrefab;
            }
            return pickupInfo;
        }

        private static GenericPickupController.CreatePickupInfo CreatePickupInfo_BossDropTable(PickupIndex pickupIndex, Vector3 position)
        {
            Log.LogInfo("Creating pickup from boss drop table");

            GenericPickupController.CreatePickupInfo pickupInfo = new GenericPickupController.CreatePickupInfo
            {
                position = position,
                rotation = Quaternion.identity,
                pickupIndex = pickupIndex
            };

            int tier = GetTier(pickupIndex);
            PickupIndex[] choices = null;
            int num = 0;

            if (tier == 6) //lunar tier (for eulogy zero)
            {
                PickupIndex[] bossDropsLunar = GetUniqueItemsOfSameTier(Settings.GetChoiceCountByTier(tier) - 1, pickupIndex, rng);
                if (bossDropsLunar == null || bossDropsLunar.Length == 0)
                {
                    return pickupInfo;
                }
                num = bossDropsLunar.Length;
                choices = new PickupIndex[num + 1];
                choices[0] = pickupIndex;
                for (int i = 0; i < num; i++)
                {
                    choices[i + 1] = bossDropsLunar[i];
                }
            }
            else
            {
                if (bossDropsByTier[tier - 1] == null)
                {
                    bossDropsByTier[tier - 1] = GetUniqueItemsOfSameTier(Settings.GetChoiceCountByTier(tier) - 1, pickupIndex, rng);
                }
                if (bossDropsByTier[tier - 1].Length == 0)
                {
                    return pickupInfo;
                }
                num = bossDropsByTier[tier - 1].Length;
                choices = new PickupIndex[num + 1];
                choices[0] = pickupIndex;
                for (int i = 0; i < num; i++)
                {
                    choices[i + 1] = bossDropsByTier[tier - 1][i];
                }
            }

            /*else if (tier == 2 || tier == 3) //uncommon/legendary tier
            {
                if (bossDropsStandard == null)
                {
                    bossDropsStandard = GetUniqueItemsOfSameTier(Settings.GetChoiceCountByTier(tier) - 1, pickupIndex, rng);
                }
                if(bossDropsStandard == null || bossDropsStandard.Length == 0)
                {
                    orig(pickupIndex, position, velocity);
                    return;
                }
                num = bossDropsStandard.Length;
                choices = new PickupIndex[num + 1];
                choices[0] = pickupIndex;
                for (int i = 0; i < num; i++)
                {
                    choices[i + 1] = bossDropsStandard[i];
                }
            }
            else if (tier == 5) //boss tier
            {
                if (bossDropsBoss == null)
                {
                    bossDropsBoss = GetUniqueItemsOfSameTier(Settings.GetChoiceCountByTier(tier) - 1, pickupIndex, rng);
                }
                if (bossDropsBoss == null || bossDropsBoss.Length == 0)
                {
                    orig(pickupIndex, position, velocity);
                    return;
                }
                num = bossDropsBoss.Length;
                choices = new PickupIndex[num + 1];
                choices[0] = pickupIndex;
                for (int i = 0; i < num; i++)
                {
                    choices[i + 1] = bossDropsBoss[i];
                }
            }*/

            pickupInfo.pickerOptions = PickupPickerController.GenerateOptionsFromArray(choices);
            pickupInfo.prefabOverride = (choices.Length > 3) ? commandCubePrefab : voidPotentialPrefab;
            //see line 334
            if (choices.Length <= 3 && PickupCatalog.itemTierToPickupIndex.TryGetValue(PickupCatalog.GetPickupDef(pickupIndex).itemTier, out var value))
            {
                Log.LogInfo("Item Tier to Pickup Idex: " + value);
                pickupInfo.pickupIndex = value;
            }
            else if (tier == 4)
            {
                pickupInfo.prefabOverride = commandCubePrefab;
            }
            return pickupInfo;
        }

        private static int GetTier(PickupIndex pickupIndex)
        {
            switch (PickupCatalog.GetPickupDef(pickupIndex).itemTier)
            {
                case ItemTier.Tier1:
                    return 1;
                case ItemTier.Tier2:
                    return 2;
                case ItemTier.Tier3:
                    return 3;
                case ItemTier.Boss:
                    return 5;
                case ItemTier.Lunar:
                    return 6;
                case ItemTier.VoidTier1:
                    return 7;
                case ItemTier.VoidTier2:
                    return 8;
                case ItemTier.VoidTier3:
                    return 9;
                case ItemTier.VoidBoss:
                    return 10;
                default:
                    if (PickupCatalog.GetPickupDef(pickupIndex).isLunar)
                    {
                        return 6;
                    }
                    return 4;
            }
        }

        private static PickupIndex[] GetUniqueItemsOfSameTier(int num, PickupIndex pickupIndex, Xoroshiro128Plus rng)
        {
            List<PickupIndex> list = null;
            switch (GetTier(pickupIndex))
            {
                case 1:
                    list = Run.instance.availableTier1DropList;
                    break;
                case 2:
                    list = Run.instance.availableTier2DropList;
                    break;
                case 3:
                    list = Run.instance.availableTier3DropList;
                    break;
                case 4:
                    list = Run.instance.availableEquipmentDropList;
                    break;
                case 5:
                    list = Run.instance.availableBossDropList;
                    break;
                case 6:
                    list = Run.instance.availableLunarCombinedDropList;
                    break;
                case 7:
                    list = Run.instance.availableVoidTier1DropList;
                    break;
                case 8:
                    list = Run.instance.availableVoidTier2DropList;
                    break;
                case 9:
                    list = Run.instance.availableVoidTier3DropList;
                    break;
                case 10:
                    list = Run.instance.availableVoidBossDropList;
                    break;
            }
            WeightedSelection<PickupIndex> selection = new WeightedSelection<PickupIndex>(8);
            for(int i = 0; i < list.Count; i++)
            {
                if(list[i] != pickupIndex)
                    selection.AddChoice(list[i], 1f);
            }
            return typeof(PickupDropTable).InvokeMethod<PickupIndex[]>("GenerateUniqueDropsFromWeightedSelection", num, rng, selection);
        }
    }
}
