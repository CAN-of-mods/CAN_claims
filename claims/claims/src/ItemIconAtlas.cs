using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.Client.NoObf;

namespace claims.src;

public class ItemIconAtlas : IDisposable
{
    private readonly ICoreClientAPI _capi;
    private readonly ClientMain _game;
    private FrameBufferRef _frameBuffer;
    private readonly InventoryItemRenderer _itemRenderer;
    private readonly DummySlot _dummySlot = new();

    public int AtlasSize { get; }
    public int ItemSize { get; }
    public int ItemsPerRow { get; }

    public int TextureId => _frameBuffer?.ColorTextureIds[0] ?? -1;

    private readonly Dictionary<string, int> _itemIndexMap = new();

    public ItemIconAtlas(ICoreClientAPI capi, int atlasSize = 2048, int itemSize = 64)
    {
        _capi = capi;
        _game = (ClientMain)capi.World;
        _itemRenderer = new(_game);
        AtlasSize = atlasSize;
        ItemSize = itemSize;
        ItemsPerRow = atlasSize / itemSize;

        CreateFramebuffer();
    }

    private void CreateFramebuffer()
    {
        var framebufferAttrs = new FramebufferAttrs("imgui-item-atlas", AtlasSize, AtlasSize);
        framebufferAttrs.Attachments = new FramebufferAttrsAttachment[]
        {
            new()
            {
                AttachmentType = EnumFramebufferAttachment.ColorAttachment0,
                Texture = new()
                {
                    Width = AtlasSize, Height = AtlasSize,
                    PixelFormat = EnumTexturePixelFormat.Rgba,
                    PixelInternalFormat = EnumTextureInternalFormat.Rgba16f
                }
            },
            new()
            {
                AttachmentType = EnumFramebufferAttachment.DepthAttachment,
                Texture = new()
                {
                    Width = AtlasSize, Height = AtlasSize,
                    PixelFormat = EnumTexturePixelFormat.DepthComponent,
                    PixelInternalFormat = EnumTextureInternalFormat.DepthComponent32
                }
            }
        };

        _frameBuffer = _game.Platform.CreateFramebuffer(framebufferAttrs);
    }

    /// <summary>
    /// Rebuilds the atlas with the provided items.
    /// Call this only when the set of items to display changes.
    /// </summary>
    public void Build(IEnumerable<ItemStack> stacks)
    {
        if (_frameBuffer == null) return;

        //_itemIndexMap.Clear();

        var uniqueStacks = new List<ItemStack>();
        var seenKeys = new HashSet<string>();

        foreach (var stack in stacks)
        {
            if (stack == null || stack.Collectible == null) continue;

            string key = GetCacheKey(stack);

            if (seenKeys.Add(key))
            {
                uniqueStacks.Add(stack);
            }
        }
        /*int atlasTextureId = _game.Platform.CreatePositionInAtlas(atlas, task);
        _game.Platform.LoadFrameBuffer(_frameBuffer);*/
        _game.Platform.GlEnableDepthTest();
        _game.Platform.GlDisableCullFace();
        _game.Platform.GlToggleBlend(true);
        _game.Platform.ClearFrameBuffer(
            _frameBuffer,
            new float[] { 0, 0, 0, 0 },
            clearColorBuffers: true
        );

        _game.OrthoMode(AtlasSize, AtlasSize, true);

        for (var i = 0; i < uniqueStacks.Count; i++)
        {
            var stack = uniqueStacks[i];
            int col = i % ItemsPerRow;
            int row = i / ItemsPerRow;
            float x = col * ItemSize;
            float y = row * ItemSize;

            if (y >= AtlasSize)
            {
                _capi.Logger.Warning("ItemIconAtlas: Too many items to fit in atlas!");
                break;
            }

            _game.Platform.GlScissorFlag(true);
            _game.Platform.GlScissor(
                (int)x,
                (int)y,
                ItemSize,
                ItemSize
            );

            _dummySlot.Itemstack = stack;
            var prevSize = stack.StackSize;
            stack.StackSize = 1;

            _itemRenderer.RenderItemstackToGui(
                _dummySlot,
                x + ItemSize / 2.0,
                y + ItemSize / 2.0,
                100,
                ItemSize / 2.0f,
                -1,
                showStackSize: true
            );

            stack.StackSize = prevSize;

            _game.Platform.GlScissorFlag(false);

            _itemIndexMap[GetCacheKey(stack)] = i;
        }

        _game.PerspectiveMode();
        _game.Platform.LoadFrameBuffer(EnumFrameBuffer.Default);
    }

    private string GetCacheKey(ItemStack stack)
    {
        return stack.Collectible.Code.ToString();
    }

    public void Draw(ItemStack stack, Vector2 size, bool showTooltip = true)
    {
        if (stack == null || stack.Collectible == null) return;

        string key = GetCacheKey(stack);
        if (_itemIndexMap.TryGetValue(key, out int index))
        {
            GetUv(index, out var uv0, out var uv1);
            ImGui.Image(
                (IntPtr)TextureId,
                size,
                uv0,
                uv1
            );

            if (showTooltip && ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text(stack.GetName());
                ImGui.EndTooltip();
            }
        }
        else
        {
            Build([stack]);
            ImGui.Text("?");
        }
    }

    private void GetUv(int index, out Vector2 uv0, out Vector2 uv1)
    {
        int col = index % ItemsPerRow;
        int row = index / ItemsPerRow;

        float uStep = (float)ItemSize / AtlasSize;
        float vStep = (float)ItemSize / AtlasSize;

        float u = col * uStep;
        float v = row * vStep;

        uv0 = new(u, v);
        uv1 = new(u + uStep, v + vStep);
    }

    public void Dispose()
    {
        _game.Platform.DisposeFrameBuffer(_frameBuffer);
        _itemRenderer.Dispose();
    }
}
