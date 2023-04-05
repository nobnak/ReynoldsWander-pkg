using Gist2.Extensions.ComponentExt;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using static ReynoldsWander.Boid;
using Random = Unity.Mathematics.Random;

namespace ReynoldsWander {

    [BurstCompile]
    public class Boid {

        protected Random rand;
        protected Tuner currTuner = new Tuner();

        public Boid() {
            rand = Random.CreateFromIndex(31);
        }

        #region interface
        public Tuner CurrTuner {
            get => currTuner.DeepCopy();
            set {
                var copy = value.DeepCopy();
                currTuner = Validate(copy);
            }
        }

        public float3 GetWanderForce(ref WanderData data, Coordinates coord) {
            GetWanderForce(currTuner.wander, coord, ref data, ref rand, out var wander_force);
            return wander_force;
        }
        #endregion

        #region static
        [BurstCompile]
        public static void RandomOnCircle(
            ref Random rand,
            out float2 o
            ) {
            var r = rand.NextFloat() * CIRCLE_RAD;
            o = new float2(math.cos(r), math.sin(r));
        }
        [BurstCompile]
        public static void GetWanderForce(
            in WanderTuner wander,
            in Coordinates coord,
            ref WanderData data,
            ref Random rand,
            out float3 target
            ) {
            var forward = coord.Forward;
            var right = coord.Right;
            var dt = Time.deltaTime;

            var wt_local = data.wanderTarget;
            RandomOnCircle(ref rand, out var r);
            wt_local += dt * wander.jitter * r;
            wt_local = math.normalizesafe(wt_local);
            wt_local *= wander.radius;
            data.wanderTarget = wt_local;

            target = wt_local.x * right + (wt_local.y + wander.distance) * forward;
        }
        #endregion

        #region methods
        private Tuner Validate(Tuner copy) {
            var wander = copy.wander;
            wander.distance = math.max(wander.distance, 1e-2f);
            wander.radius = math.clamp(wander.radius, 0f, wander.distance - 1e-2f);
            return copy;
        }

        #endregion

        #region declarations
        public const float CIRCLE_RAD = 2f * math.PI;

        [BurstCompile]
        public struct Coordinates {
            public float3 Position;
            public float3 Forward;
            public float3 Right;
            public float3 Upward;

            public static implicit operator Coordinates(Transform tr) {
                return new Coordinates() {
                    Position = tr.position,
                    Forward = tr.up,
                    Right = tr.right,
                    Upward = -tr.forward,
                };
            }
        }
        [BurstCompile]
        public struct WanderData {
            public static readonly float2 INIT_WANDER_TARGET = new float2(0f, 1f);

            public float2 wanderTarget;

            public static WanderData Create() => new WanderData() { wanderTarget = INIT_WANDER_TARGET };
        }

        [System.Serializable]
        [BurstCompile]
        public struct WanderTuner {
            public float jitter;
            public float radius;
            public float distance;
        }
        [System.Serializable]
        public class Tuner {
            public WanderTuner wander = new WanderTuner();
        }
        #endregion
    }
}