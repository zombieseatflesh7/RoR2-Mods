using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Configuration;

namespace ArtifactOfPotential
{
    public static class Settings
    {
        //Any Tier Mode
        public static ConfigEntry<bool> AnyTierMode;
        public static ConfigEntry<int> AnyTierModeChoiceCount;
        public static ConfigEntry<bool> AnyTierModeVoid;
        public static ConfigEntry<int> AnyTierModeVoidChoiceCount;
        public static ConfigEntry<bool> AnyTierModeDoppelganger;
        public static ConfigEntry<int> AnyTierModeDoppelgangerCount;

        //Item Sources Affected
        public static ConfigEntry<bool> ChestsAffected;
        public static ConfigEntry<bool> ShrineOfChanceAffected;
        public static ConfigEntry<bool> HiddenMultishopsAffected;
        public static ConfigEntry<bool> MultishopsAffected;
        public static ConfigEntry<bool> PrintersAffected;
        public static ConfigEntry<bool> ShopsAffected;
        public static ConfigEntry<bool> BossAffected;
        public static ConfigEntry<bool> SacrificeAffected;
        public static ConfigEntry<bool> DoppelgangersAffected;
        public static ConfigEntry<bool> SonorousWhispersAffected;

        //Boss



        //Number of Options
        public static ConfigEntry<int> Tier1ChoiceCount;
        public static ConfigEntry<int> Tier2ChoiceCount;
        public static ConfigEntry<int> Tier3ChoiceCount;
        public static ConfigEntry<int> EquipmentChoiceCount;
        public static ConfigEntry<int> BossChoiceCount;
        public static ConfigEntry<int> LunarChoiceCount;
        public static ConfigEntry<int> Void1ChoiceCount;
        public static ConfigEntry<int> Void2ChoiceCount;
        public static ConfigEntry<int> Void3ChoiceCount;
        public static ConfigEntry<int> VoidBossChoiceCount;

        public static int GetChoiceCountByTier(int tier)
        {
            switch (tier)
            {
                case 1:
                    return Tier1ChoiceCount.Value;
                case 2:
                    return Tier2ChoiceCount.Value;
                case 3:
                    return Tier3ChoiceCount.Value;
                case 4:
                    return EquipmentChoiceCount.Value;
                case 5:
                    return BossChoiceCount.Value;
                case 6:
                    return LunarChoiceCount.Value;
                case 7:
                    return Void1ChoiceCount.Value;
                case 8:
                    return Void2ChoiceCount.Value;
                case 9:
                    return Void3ChoiceCount.Value;
                case 10:
                    return VoidBossChoiceCount.Value;
            }
            return 1;
        }
    }
}
