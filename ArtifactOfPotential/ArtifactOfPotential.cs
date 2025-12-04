using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Artifacts;
using System.Collections.Generic;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using static RoR2.GenericPickupController;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

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
        public const string PluginVersion = "1.3.3";

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
            Settings.SonorousWhispersAffected = Config.Bind<bool>("Item Sources Affected", "Sonorous Whispers", true, "Whether or not Sonorous Whispers should drop void potentials.");
            Settings.SacrificeAffected = Config.Bind<bool>("Item Sources Affected", "Artifact of Sacrifice", true, "Whether or not Artifact of Sacrifice should drop void potentials.");
            Settings.TemporaryItemsAffected = Config.Bind<bool>("Item Sources Affected", "Temporary Items", true, "Whether or not temporary items from any source should drop void potentials.");
            Settings.DoppelgangerChoiceCount = Config.Bind<int>("Item Sources Affected", "Artifact of Vengance - Choice Count", 5, "The number of item choices you get when killing a doppelganger from Artifact of Vengeance. Item choices can be any tier, regardless of other settings. Set to 1 to disable void potentials from this artifact.");

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
        public static GameObject currentModelObjectOverride = null;

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
                return;

            if (NetworkServer.active)
            {
                if (Settings.ChestsAffected.Value)
                    IL.RoR2.ChestBehavior.BaseItemDrop += ChestBehavior_BaseItemDrop;
                if (Settings.ShrineOfChanceAffected.Value)
                    IL.RoR2.ShrineChanceBehavior.AddShrineStack += ShrineChanceBehavior_AddShrineStack;
                if (Settings.HiddenMultishopsAffected.Value || Settings.MultishopsAffected.Value || Settings.ShopsAffected.Value)
                    IL.RoR2.ShopTerminalBehavior.DropPickup_bool += ShopTerminalBehavior_DropPickup;
                if (Settings.BossAffected.Value)
                    IL.RoR2.BossGroup.DropRewards += BossGroup_DropRewards;
                if (Settings.SacrificeAffected.Value)
                    IL.RoR2.Artifacts.SacrificeArtifactManager.OnServerCharacterDeath += SacrificeArtifactManager_OnServerCharacterDeath;
                if (Settings.DoppelgangerChoiceCount.Value > 1)
                    IL.RoR2.Artifacts.DoppelgangerInvasionManager.OnCharacterDeathGlobal += DoppelgangerInvasionManager_OnCharacterDeathGlobal;
                if (Settings.SonorousWhispersAffected.Value)
                    IL.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;

                On.RoR2.PickupDisplay.RebuildModel += PickupDisplay_RebuildModel;
                On.RoR2.GenericPickupController.CreatePickup += GenericPickupController_CreatePickup;
            }
        }

        private static void OnArtifactDisabled(RunArtifactManager runArtifactManager, ArtifactDef artifactDef)
        {
            if (artifactDef != Potential)
                return;

            IL.RoR2.ChestBehavior.BaseItemDrop -= ChestBehavior_BaseItemDrop;
            IL.RoR2.ShrineChanceBehavior.AddShrineStack -= ShrineChanceBehavior_AddShrineStack;
            IL.RoR2.ShopTerminalBehavior.DropPickup_bool -= ShopTerminalBehavior_DropPickup;
            IL.RoR2.BossGroup.DropRewards -= BossGroup_DropRewards;
            IL.RoR2.Artifacts.SacrificeArtifactManager.OnServerCharacterDeath -= SacrificeArtifactManager_OnServerCharacterDeath;
            IL.RoR2.Artifacts.DoppelgangerInvasionManager.OnCharacterDeathGlobal -= DoppelgangerInvasionManager_OnCharacterDeathGlobal;
            IL.RoR2.GlobalEventManager.OnCharacterDeath -= GlobalEventManager_OnCharacterDeath;

            On.RoR2.PickupDisplay.RebuildModel -= PickupDisplay_RebuildModel;
            On.RoR2.GenericPickupController.CreatePickup -= GenericPickupController_CreatePickup;
        }

        private static void ChestBehavior_BaseItemDrop(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(i => i.MatchCall<PickupDropletController>("CreatePickupDroplet"));
            c.Remove();
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((CreatePickupInfo pickupInfo, Vector3 position, Vector3 velocity, ChestBehavior chest) => 
            { 
                CreatePickupInfo newPickupInfo = CreatePickupInfo_Basic(pickupInfo.pickup, pickupInfo.position, chest.rng, chest.dropTable);
                newPickupInfo.chest = chest;
                newPickupInfo.artifactFlag = pickupInfo.artifactFlag;
                PickupDropletController.CreatePickupDroplet(newPickupInfo, position, velocity);
            });
        }

        private static void ShrineChanceBehavior_AddShrineStack(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(i => i.MatchCall<PickupDropletController>("CreatePickupDroplet"));
            c.Remove();
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((UniquePickup pickup, Vector3 position, Vector3 velocity, ShrineChanceBehavior shrine) =>
            {
                CreatePickupInfo pickupInfo = CreatePickupInfo_Basic(pickup, position, shrine.rng, shrine.dropTable);
                PickupDropletController.CreatePickupDroplet(pickupInfo, position, velocity);
            });
        }

        private static void ShopTerminalBehavior_DropPickup(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(i => i.MatchCall<PickupDropletController>("CreatePickupDroplet"));
            c.Remove();
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((UniquePickup pickup, Vector3 position, Vector3 velocity, bool isDuplicated, ShopTerminalBehavior shop) =>
            {
                CreatePickupInfo pickupInfo = new CreatePickupInfo
                {
                    position = position,
                    rotation = Quaternion.identity,
                    pickup = pickup,
                };
                if (shop.serverMultiShopController != null && (Settings.MultishopsAffected.Value || (Settings.HiddenMultishopsAffected.Value && shop.GetFieldValue<bool>("hidden")))) // multishops
                    pickupInfo = CreatePickupInfo_Random(pickupInfo.pickup, position, shop.serverMultiShopController.rng);
                else if (shop.serverMultiShopController == null && Settings.ShopsAffected.Value) // shops
                    pickupInfo = CreatePickupInfo_Basic(pickupInfo.pickup, position, shop.rng, shop.dropTable);
                pickupInfo.duplicated = isDuplicated;
                PickupDropletController.CreatePickupDroplet(pickupInfo, position, velocity);
            });
        }

        public static List<UniquePickup>[] bossDropsByTier = new List<UniquePickup>[12];
        private static void BossGroup_DropRewards(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.EmitDelegate(() => 
            {
                for (int i = 0; i < bossDropsByTier.Length; i++)
                    bossDropsByTier[i] = null;
            });
            c.GotoNext(i => i.MatchCall<PickupDropletController>("CreatePickupDroplet"));
            c.Remove();
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((UniquePickup pickup, Vector3 position, Vector3 velocity, BossGroup boss) =>
            {
                CreatePickupInfo pickupInfo = CreatePickupInfo_Boss(pickup, position, boss.rng);
                PickupDropletController.CreatePickupDroplet(pickupInfo, position, velocity);
            });
        }

        private static void SacrificeArtifactManager_OnServerCharacterDeath(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(i => i.MatchCall<PickupDropletController>("CreatePickupDroplet"));
            c.Remove();
            c.EmitDelegate((UniquePickup pickup, Vector3 position, Vector3 velocity) =>
            {
                CreatePickupInfo pickupInfo = CreatePickupInfo_Basic(pickup, position, SacrificeArtifactManager.treasureRng, SacrificeArtifactManager.dropTable);
                PickupDropletController.CreatePickupDroplet(pickupInfo, position, velocity);
            });
        }
        
        private static void DoppelgangerInvasionManager_OnCharacterDeathGlobal(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(i => i.MatchCall<PickupDropletController>("CreatePickupDroplet"));
            c.Remove();
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((UniquePickup pickup, Vector3 position, Vector3 velocity, DoppelgangerInvasionManager doppel) =>
            {
                CreatePickupInfo pickupInfo = CreatePickupInfo_Doppelganger(pickup, position, doppel.treasureRng, doppel.dropTable);
                PickupDropletController.CreatePickupDroplet(pickupInfo, position, velocity);
            });
        }

        //Sonorous Whispers
        private static void GlobalEventManager_OnCharacterDeath(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(i => i.MatchCall<PickupDropletController>("CreatePickupDroplet"));
            c.GotoNext(i => i.MatchCall<PickupDropletController>("CreatePickupDroplet"));
            c.Remove();
            c.EmitDelegate((UniquePickup pickup, Vector3 position, Vector3 velocity) =>
            {
                CreatePickupInfo pickupInfo = CreatePickupInfo_Basic(pickup, position, Run.instance.runRNG, GlobalEventManager.CommonAssets.dtSonorousEchoPath);
                PickupDropletController.CreatePickupDroplet(pickupInfo, position, velocity);
            });
            c.GotoNext(i => i.MatchCall<PickupDropletController>("CreatePickupDroplet"));
            c.Remove();
            c.EmitDelegate((UniquePickup pickup, Vector3 position, Vector3 velocity) =>
            {
                CreatePickupInfo pickupInfo = CreatePickupInfo_Basic(pickup, position, Run.instance.runRNG, GlobalEventManager.CommonAssets.dtSonorousEchoPath);
                PickupDropletController.CreatePickupDroplet(pickupInfo, position, velocity);
            });
        }
 
        private static CreatePickupInfo CreatePickupInfo_Random(UniquePickup pickup, Vector3 position, Xoroshiro128Plus rng)
        {
            Log.LogInfo("Creating pickup without drop table");

            CreatePickupInfo pickupInfo = new CreatePickupInfo
            {
                position = position,
                rotation = Quaternion.identity,
                pickup = pickup
            };

            int tier = GetTier(pickup);
            List<UniquePickup> choices = new List<UniquePickup>() { pickup };
            choices.AddRange(GetUniqueItemsOfSameTier(Settings.GetChoiceCountByTier(tier) - 1, pickup, rng));

            if (choices.Count == 1)
                return pickupInfo;

            pickupInfo.pickerOptions = PickupPickerController.GenerateOptionsFromList(choices);
            pickupInfo.prefabOverride = (choices.Count > 3) ? commandCubePrefab : voidPotentialPrefab;
            return pickupInfo;
        }
        
        private static CreatePickupInfo CreatePickupInfo_Basic(UniquePickup pickup, Vector3 position, Xoroshiro128Plus rng, PickupDropTable dropTable)
        {
            CreatePickupInfo pickupInfo = new CreatePickupInfo
            {
                position = position,
                rotation = Quaternion.identity,
                pickup = pickup
            };

            if (pickup.isTempItem && !Settings.TemporaryItemsAffected.Value) // Short-circuit choices if not enabled for temporary drops
                return pickupInfo;
            
            if (dropTable is not BasicPickupDropTable)
                return pickupInfo;

            Log.LogInfo("Creating choice from basic drop table");

            int tier = GetTier(pickup);
            List<UniquePickup> choices = new List<UniquePickup>() { pickup };
            int num = 0;
            if ((Settings.AnyTierMode.Value && tier <= 6) || (Settings.AnyTierModeVoid.Value && tier >= 7)) //Any Tier Mode or Any Tier Mode Viod is true
            {
                WeightedSelection<UniquePickup> selection = (dropTable as BasicPickupDropTable).selector;
                for (int i = 0; i < selection.Count; i++)
                {
                    if (selection.GetChoice(i).value.pickupIndex == pickup.pickupIndex)
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
                List<UniquePickup> choices2 = new List<UniquePickup>(num);
                dropTable.GenerateDistinctPickups(choices2, num, rng);
                choices.AddRange(choices2);
            }
            else if (tier == 6) //is lunar tier (for eulogy zero)
            {
                choices.AddRange(GetUniqueItemsOfSameTier(Settings.GetChoiceCountByTier(tier) - 1, pickup, rng));
                num = choices.Count - 1;
                if (num == 0)
                {
                    return pickupInfo;
                }
            }
            else //is not lunar tier
            {
                dropTable.canDropBeReplaced = false;
                WeightedSelection<UniquePickup> selection = (dropTable as BasicPickupDropTable).selector;
                for (int i = 0; i < selection.Count; i++)
                {
                    if (GetTier(selection.GetChoice(i).value) != tier || selection.GetChoice(i).value.pickupIndex == pickup.pickupIndex)
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
                List<UniquePickup> choices2 = new List<UniquePickup>(num);
                dropTable.GenerateDistinctPickups(choices2, num, rng);
                choices.AddRange(choices2);
                dropTable.canDropBeReplaced = true;
                dropTable.InvokeMethod("Regenerate", Run.instance);
            }
            
            pickupInfo.pickerOptions = PickupPickerController.GenerateOptionsFromList(choices);
            pickupInfo.prefabOverride = (choices.Count > 3) ? commandCubePrefab : voidPotentialPrefab;
            return pickupInfo;
        }

        private static CreatePickupInfo CreatePickupInfo_Boss(UniquePickup pickup, Vector3 position, Xoroshiro128Plus rng)
        {
            Log.LogInfo("Creating pickup from boss drop table");

            CreatePickupInfo pickupInfo = new CreatePickupInfo
            {
                position = position,
                rotation = Quaternion.identity,
                pickup = pickup
            };

            int tier = GetTier(pickup);
            List<UniquePickup> choices = new List<UniquePickup>() { pickup };

            if (tier == 6) //lunar tier (for eulogy zero)
            {
                List<UniquePickup> bossDropsLunar = GetUniqueItemsOfSameTier(Settings.GetChoiceCountByTier(tier) - 1, pickup, rng);
                if (bossDropsLunar == null || bossDropsLunar.Count == 0)
                {
                    return pickupInfo;
                }
                choices.AddRange(bossDropsLunar);
            }
            else
            {
                if (bossDropsByTier[tier - 1] == null)
                {
                    bossDropsByTier[tier - 1] = GetUniqueItemsOfSameTier(Settings.GetChoiceCountByTier(tier) - 1, pickup, rng);
                }
                if (bossDropsByTier[tier - 1].Count == 0)
                {
                    return pickupInfo;
                }
                choices.AddRange(bossDropsByTier[tier - 1]);
            }

            if (choices.Count <= 1)
                return pickupInfo;

            pickupInfo.pickerOptions = PickupPickerController.GenerateOptionsFromList(choices);
            pickupInfo.prefabOverride = (choices.Count > 3) ? commandCubePrefab : voidPotentialPrefab;
            return pickupInfo;
        }

        private static CreatePickupInfo CreatePickupInfo_Doppelganger(UniquePickup pickup, Vector3 position, Xoroshiro128Plus rng, PickupDropTable dropTable)
        {
            Log.LogInfo("Creating pickup from doppelganger drop table");

            CreatePickupInfo pickupInfo = new CreatePickupInfo
            {
                position = position,
                rotation = Quaternion.identity,
                pickup = pickup
            };

            List<UniquePickup> choices = new List<UniquePickup>() { pickup };
            int num = 0;

            //Using the Any Tier Mode logic
            WeightedSelection<UniquePickup> selection = (dropTable as DoppelgangerDropTable).selector;
            for (int i = 0; i < selection.Count; i++)
            {
                if (selection.GetChoice(i).value.pickupIndex == pickup.pickupIndex)
                {
                    selection.RemoveChoice(i);
                    i--;
                }
            }
            num = Mathf.Min(Settings.DoppelgangerChoiceCount.Value - 1, selection.Count);
            if (num == 0)
            {
                return pickupInfo;
            }
            List<UniquePickup> choices2 = new List<UniquePickup>(num);
            dropTable.GenerateDistinctPickups(choices2, num, rng);
            choices.AddRange(choices2);
            dropTable.InvokeMethod("Regenerate", Run.instance);

            pickupInfo.pickerOptions = PickupPickerController.GenerateOptionsFromList(choices);
            pickupInfo.prefabOverride = (choices.Count > 3) ? commandCubePrefab : voidPotentialPrefab;
            return pickupInfo;
        }

        private static int GetTier(UniquePickup pickup)
        {
            switch (PickupCatalog.GetPickupDef(pickup.pickupIndex).itemTier)
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
                case ItemTier.FoodTier:
                    return 11;
                case ItemTier.AssignedAtRuntime:
                    return 12;
                default:
                    if (PickupCatalog.GetPickupDef(pickup.pickupIndex).isLunar)
                    {
                        return 6;
                    }
                    return 4; // equipment tier
            }
        }

        private static List<UniquePickup> GetUniqueItemsOfSameTier(int num, UniquePickup pickup, Xoroshiro128Plus rng)
        {
            List<PickupIndex> list = null;
            switch (GetTier(pickup))
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
                if(list[i] != pickup.pickupIndex)
                    selection.AddChoice(list[i], 1f);
            }
            return PickupDropTable.GenerateDistinctFromWeightedSelection(new List<PickupIndex>(num), num, rng, selection).ConvertAll(pickupIndex => new UniquePickup(pickupIndex) { decayValue = pickup.decayValue });
        }

        private static void PickupDisplay_RebuildModel(On.RoR2.PickupDisplay.orig_RebuildModel orig, PickupDisplay self, GameObject modelObjectOverride)
        {
            if (modelObjectOverride != null && currentModelObjectOverride == null)
            {
                currentModelObjectOverride = modelObjectOverride;
            }
            else if (currentModelObjectOverride != null)
            {
                modelObjectOverride = currentModelObjectOverride;
            }

            orig(self, modelObjectOverride);

            if (currentModelObjectOverride != null)
            {

                if ((bool)self.tier1ParticleEffect)
                {
                    self.tier1ParticleEffect.SetActive(value: false);
                }
                if ((bool)self.tier2ParticleEffect)
                {
                    self.tier2ParticleEffect.SetActive(value: false);
                }
                if ((bool)self.tier3ParticleEffect)
                {
                    self.tier3ParticleEffect.SetActive(value: false);
                }
                if ((bool)self.equipmentParticleEffect)
                {
                    self.equipmentParticleEffect.SetActive(value: false);
                }
                if ((bool)self.lunarParticleEffect)
                {
                    self.lunarParticleEffect.SetActive(value: false);
                }
                if ((bool)self.voidParticleEffect)
                {
                    self.voidParticleEffect.SetActive(value: false);
                }
            }
        }

        private static GenericPickupController GenericPickupController_CreatePickup(On.RoR2.GenericPickupController.orig_CreatePickup orig, ref GenericPickupController.CreatePickupInfo createPickupInfo)
        {
            if (createPickupInfo.prefabOverride == commandCubePrefab)
            {
                //This is a bit hacky but it's the only way I could find to make artifact of command prefab to work for now.
                //Basically cutting out a portion of the decompiled code to make it work.
                //Looking for mesh renderer child that doesn't exist in commandcube.prefab
                //Most likely to cause conflict issues - Valkarin
                Log.LogDebug("Prefab Overrid is CommandCube");
                GameObject gameObject = UnityEngine.Object.Instantiate(createPickupInfo.prefabOverride ?? GenericPickupController.pickupPrefab, createPickupInfo.position, createPickupInfo.rotation);
                GenericPickupController component = gameObject.GetComponent<GenericPickupController>();
                if ((bool)component)
                {
                    component.Network_pickupState = createPickupInfo.pickup;
                    component.chestGeneratedFrom = createPickupInfo.chest;
                }
                PickupIndexNetworker component2 = gameObject.GetComponent<PickupIndexNetworker>();
                if ((bool)component2)
                {
                    component2.NetworkpickupState = createPickupInfo.pickup;
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
            currentModelObjectOverride = null;
            return null;

        }

    }
}
