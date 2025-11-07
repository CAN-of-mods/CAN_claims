using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace claims.src.auxialiry
{
    public static class Particles
    {
        public static void PlayerRespawnParticles(Vec3d position)
        {
            Vec3d pos = position.Clone();
            float radius = 0.5f;
            int particleCount = 50;

            for (int i = 0; i < particleCount; i++)
            {
                double angle = 2 * Math.PI * i / particleCount;
                double x = pos.X  + radius * Math.Cos(angle);
                double z = pos.Z  + radius * Math.Sin(angle);
                double y = pos.Y;

                Vec3d particlePos = new Vec3d(x, y, z);
                Vec3f velocity = new Vec3f(0, 1, 0); // Подъем вверх
                SimpleParticleProperties myParticles = new SimpleParticleProperties(1, 1, ColorUtil.ColorFromRgba(14, 227, 121, 255), new Vec3d(), new Vec3d(), new Vec3f(), new Vec3f());
                myParticles.MinPos = particlePos;
                myParticles.GravityEffect = 0;
                myParticles.AddVelocity.Set(0.1f, 0.7f, 0.1f);
                myParticles.SelfPropelled = true;
                myParticles.ParticleModel = EnumParticleModel.Cube;
                myParticles.MaxSize = 0.1f;
                myParticles.LifeLength = 2;
                claims.sapi.World.SpawnParticles(myParticles);
            }
        }
    }
}
