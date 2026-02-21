using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace claims.src.gui.prettyGui
{
    public class ImageHandler
    {
        public ICoreClientAPI capi;
        public Dictionary<string, int> texturesDict;
        public Dictionary<string, LoadedTexture> textureDict;
        public ImageHandler(ICoreClientAPI capi)
        {
            this.capi = capi;
            texturesDict = new Dictionary<string, int>();
            textureDict = new();
        }
        public int GetOrLoadIcon(string iconName)
        {
            if (texturesDict.ContainsKey(iconName))
            {
                return texturesDict[iconName];
            }

            var assetPath = new AssetLocation($"claims:textures/icons/{iconName}.jpg");
            var asset = capi.Assets.TryGet(assetPath);

            LoadedTexture guiTex = new LoadedTexture(capi);

            capi.Render.GetOrLoadTexture(
                assetPath,
                ref guiTex
            );
            textureDict[iconName] = guiTex;
            if (guiTex == null)
            {
                return 0;
            }
            this.texturesDict[iconName] = guiTex.TextureId;
            //newTexture.Dispose();
            return guiTex?.TextureId ?? 0;
        }
    }
}
