using System;
using AchEngine.DI;
using Unity.Entities;
using UnityEngine;

namespace AchEngine.ECS
{
    [AddComponentMenu("AchEngine/ECS/ECS Installer")]
    public sealed class AchEngineEcsInstaller : AchEngineInstaller
    {
        [SerializeField] private string _worldName;

        public override void Install(IServiceBuilder builder)
        {
            var world = ResolveWorld();
            builder.RegisterInstance<IEcsWorld>(new EcsWorld(world));
        }

        private World ResolveWorld()
        {
            if (string.IsNullOrWhiteSpace(_worldName))
            {
                var defaultWorld = World.DefaultGameObjectInjectionWorld;
                if (defaultWorld == null)
                {
                    throw new InvalidOperationException("Default ECS world is not available.");
                }

                return defaultWorld;
            }

            foreach (var world in World.All)
            {
                if (world.Name == _worldName)
                {
                    return world;
                }
            }

            throw new InvalidOperationException($"ECS world '{_worldName}' was not found.");
        }
    }
}
