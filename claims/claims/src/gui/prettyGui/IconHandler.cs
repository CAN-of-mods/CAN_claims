using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace claims.src.gui.prettyGui
{
    public class IconHandler
    {
        public ICoreClientAPI capi;
        public Dictionary<string, int> texturesDict;
        public IconHandler(ICoreClientAPI capi)
        {
            this.capi = capi;
            texturesDict = new Dictionary<string, int>();
        }
        public int GetOrLoadIcon(string iconName)
        {
            if(texturesDict.ContainsKey(iconName))
            {
                return texturesDict[iconName];
            }
            var assetPath = new AssetLocation($"claims:textures/icons/{iconName}.svg");
            var asset = capi.Assets.TryGet(assetPath);

            var newTexture = capi.Gui.LoadSvgWithPadding(assetPath, 250, 250, 0, -900000000);
            if(newTexture == null)
            {
                return 0;
            }
            this.texturesDict[iconName] = newTexture.TextureId;
            return newTexture?.TextureId ?? 0;
        }
    }
}
