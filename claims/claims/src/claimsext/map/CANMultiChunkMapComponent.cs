using claims.src.auxialiry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using OpenTK.Graphics.OpenGL;

namespace claims.src.claimsext.map
{
    public class CANMultiChunkMapComponent: MapComponent
    {
        public static int ChunkLen = 4;

        public static LoadedTexture tmpTexture;

        public float renderZ = 50f;

        public Vec2i chunkCoord;

        public LoadedTexture Texture;

        private static int[] emptyPixels;

        private Vec3d worldPos;

        private Vec2f viewPos = new Vec2f();

        private bool[,] chunkSet = new bool[ChunkLen, ChunkLen];

        private int chunksize;

        public float TTL = MaxTTL;

        public static float MaxTTL = 15f;

        private Vec2i tmpVec = new Vec2i();

        public bool AnyChunkSet
        {
            get
            {
                for (int i = 0; i < ChunkLen; i++)
                {
                    for (int j = 0; j < ChunkLen; j++)
                    {
                        if (chunkSet[i, j])
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public bool IsChunkSet(int dx, int dz)
        {
            if (dx < 0 || dz < 0)
            {
                return false;
            }

            return chunkSet[dx, dz];
        }

        public CANMultiChunkMapComponent(ICoreClientAPI capi, Vec2i baseChunkCord)
            : base(capi)
        {
            chunkCoord = baseChunkCord;
            chunksize = capi.World.BlockAccessor.ChunkSize;
            worldPos = new Vec3d(baseChunkCord.X * chunksize, 0.0, baseChunkCord.Y * chunksize);
            if (emptyPixels == null)
            {
                int num = ChunkLen * chunksize;
                emptyPixels = new int[num * num];
            }
        }

        public void setChunk(int dx, int dz, int[] pixels)
        {
            if (dx < 0 || dx >= ChunkLen || dz < 0 || dz >= ChunkLen)
            {
                throw new ArgumentOutOfRangeException("dx/dz must be within [0," + (ChunkLen - 1) + "]");
            }

            if (tmpTexture == null || tmpTexture.Disposed)
            {
                tmpTexture = new LoadedTexture(capi, 0, chunksize, chunksize);
            }

            if (Texture == null || Texture.Disposed)
            {
                int num = ChunkLen * chunksize;
                Texture = new LoadedTexture(capi, 0, num, num);
                capi.Render.LoadOrUpdateTextureFromRgba(emptyPixels, linearMag: false, 0, ref Texture);
            }
            capi.Render.LoadOrUpdateTextureFromRgba(pixels, linearMag: false, 0, ref tmpTexture);
            //GL.Enable((EnableCap)3042);

            //GL.BlendFunc(BlendingFactor.One, BlendingFactor.Zero);
            //GL.BlendEquation(BlendEquationMode.FuncSubtract);
            //capi.Render.GlToggleBlend(blend: false);
            //tmpTexture?.Dispose();
            capi.Render.GLDisableDepthTest();
            capi.Render.RenderTextureIntoTexture(tmpTexture, 0f, 0f, chunksize, chunksize, Texture, chunksize * dx, chunksize * dz, -1);
            capi.Render.BindTexture2d(Texture.TextureId);
            capi.Render.GlGenerateTex2DMipmaps();
            chunkSet[dx, dz] = true;
        }

        public void unsetChunk(int dx, int dz)
        {
            if (dx < 0 || dx >= ChunkLen || dz < 0 || dz >= ChunkLen)
            {
                throw new ArgumentOutOfRangeException("dx/dz must be within [0," + (ChunkLen - 1) + "]");
            }
            chunkSet[dx, dz] = false;
        }

        public override void Render(GuiElementMap map, float dt)
        {
            map.TranslateWorldPosToViewPos(worldPos, ref viewPos);
            capi.Render.Render2DTexture(Texture.TextureId, (int)(map.Bounds.renderX + (double)viewPos.X), (int)(map.Bounds.renderY + (double)viewPos.Y), (int)((float)Texture.Width * map.ZoomLevel), (int)((float)Texture.Height * map.ZoomLevel), renderZ);
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public void ActuallyDispose()
        {
            Texture.Dispose();
            tmpTexture.Dispose();
        }

        public bool IsVisible(HashSet<Vec2i> curVisibleChunks)
        {
            for (int i = 0; i < ChunkLen; i++)
            {
                for (int j = 0; j < ChunkLen; j++)
                {
                    tmpVec.Set(chunkCoord.X + i, chunkCoord.Y + j);
                    if (curVisibleChunks.Contains(tmpVec))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static void DisposeStatic()
        {
            tmpTexture?.Dispose();
            emptyPixels = null;
            tmpTexture = null;
        }
    }
}
