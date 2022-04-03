using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace DynamicBarrierDecay
{
	//[BepInDependency()]

    [BepInPlugin("com.zombieseatflesh7.dynamicbarrierdecay", "DynamicBarrierDecay", "2.0.0")]

	public class DynamicBarrierDecay : BaseUnityPlugin
	{
        public static ConfigEntry<float> HalfLife;
        public static ConfigEntry<float> MinDecayRate;
        public static ConfigEntry<bool> UnlimitedBarrier;
        public static float decayRate = 0.0924196f;

        public void Awake()
        {
            Log.Init(Logger);

            InitConfigs();

            On.RoR2.CharacterBody.FixedUpdate += (orig, self) =>
            {
                orig(self);
                self.SetPropertyValue<float>("barrierDecayRate", Math.Max(MinDecayRate.Value, self.healthComponent.barrier * decayRate));
            };
            if(UnlimitedBarrier.Value)
            {
                On.RoR2.HealthComponent.AddBarrier += HealthComponent_AddBarrier;
            }
        }

        private void InitConfigs()
        {
            MinDecayRate = Config.Bind<float>("Settings", "Minimum Decay Rate", 3f, "The minimum decay rate for barrier measured in points per second. Can be a number between 0 and infinity.");
            HalfLife = Config.Bind<float>("Settings", "Barrier Half-Life", 7.5f, "The amound of time in seconds it takes for barrier to decay to half of its original amount. Lower numbers means faster barrier decay. Can be a number between 0 and infinity. Setting to 0 makes barrier disappear instantly.");
            UnlimitedBarrier = Config.Bind<bool>("Settings", "Unlimited Barrier", false, "Setting this to true will remove the barrier limit, allowing the player to get unlimited barrier.");
            if(HalfLife.Value <= 0)
            {
                decayRate = float.MaxValue;
            }
            else
            {
                decayRate = (float)(Math.Log(2) / (double)HalfLife.Value);
            }
        }

        private static void HealthComponent_AddBarrier(On.RoR2.HealthComponent.orig_AddBarrier orig, HealthComponent self, float value)
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[Server] function 'System.Void RoR2.HealthComponent::AddBarrier(System.Single)' called on client");
                return;
            }
            if (!self.alive)
            {
                return;
            }
            self.Networkbarrier = self.barrier + value;
        }
    }
}
