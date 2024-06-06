using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Artifacts;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace DynamicBarrierDecay
{
    //[BepInDependency()]
    [BepInDependency("com.bepis.r2api")]

    [BepInPlugin("com.zombieseatflesh7.colorfulchaoticspite", "Colorful, Choatic, Spite.", "1.0.0")]


    public class ChaosSpite : BaseUnityPlugin
    {
        public void Awake()
        {
            On.RoR2.SpiteBombController.FixedUpdate += (orig, self) =>
            {
                Vector3 vel = self.GetFieldValue<Vector3>("velocity");

                float fixedDeltaTime = Time.fixedDeltaTime;
                vel.y = vel.y + fixedDeltaTime * UnityEngine.Physics.gravity.y;
                Vector3 position = self.transform.position;

                float raycastLength = vel.magnitude * fixedDeltaTime + self.radius;
                Vector3 raycastOrigin = position;
                Vector3 raycastDirection = vel;

                RaycastHit rayHit;
                if (Physics.Raycast(raycastOrigin, raycastDirection, out rayHit, raycastLength, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                {
                    position = rayHit.point;
                    vel = vel - 2 * Vector3.Dot(vel, rayHit.normal) * rayHit.normal;
                    vel *= 0.8f;
                    self.InvokeMethod("OnBounce");
                }
                else
                {
                    position += vel * fixedDeltaTime;
                }

                self.SetFieldValue<Vector3>("velocity", vel);
                self.GetFieldValue<Rigidbody>("rb").MovePosition(position);
                self.delayBlast.position = position;
            };

            On.RoR2.SpiteBombController.Start += (orig, self) =>
            {
                self.initialVelocityY = UnityEngine.Random.Range(-25f, 25f);
                Vector3 startPosition = self.transform.position;
                float time = Trajectory.CalculateFlightDuration(startPosition.y, self.bouncePosition.y, self.initialVelocityY);
                Vector3 a = self.bouncePosition - startPosition;
                a.y = 0f;
                float magnitude = a.magnitude;
                float d = Trajectory.CalculateGroundSpeed(time, magnitude);
                Vector3 velocity = a / magnitude * d;
                velocity.y = self.initialVelocityY;
                self.SetFieldValue<Vector3>("velocity", velocity);
            };

            typeof(BombArtifactManager).SetFieldValue<float>("extraBombPerRadius", 8f);
            typeof(BombArtifactManager).SetFieldValue<int>("maxBombCount", 100);
        }
    }
}
