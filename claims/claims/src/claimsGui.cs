using System.Drawing;
using System.Numerics;
using Cairo;
using claims.src.gui.prettyGui;
using ImGuiNET;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using VSImGui;
using VSImGui.API;

namespace claims.src
{
    public class claimsGui: ModSystem
    {
        public ICoreClientAPI capi;
        public PrettyGuiState prettyGuiState = new PrettyGuiState();
        public static LoadedTexture myTex = null;
        public static int active_button = 0;
        public static int svgid = 0;
        private ImGuiModSystem imguiSys;
        private IconHandler iconHandler;
        private TabDrawHandler tabDrawHandler;
        public EnumSelectedTab selectedTab = EnumSelectedTab.CITY;
        public EnumSecondaryWindowTab secondaryWindowTab = EnumSecondaryWindowTab.NONE;
        public override double ExecuteOrder()
        {
            return 1;
        }
        public override void StartClientSide(ICoreClientAPI api)
        {
            /*[ "claims:qaitbay-citadel", "claims:magnifying-glass",
            "claims:price-tag", "claims:flat-platform",
                                                               "claims:prisoner", "claims:magic-portal",
                                                               "claims:huts-village"        ]*/
            this.capi = api;
            api.Input.RegisterHotKey("prettycangui", "Pretty CAN Claims GUI", GlKeys.P, HotkeyType.GUIOrOtherControls);
            api.Input.SetHotKeyHandler("prettycangui", new ActionConsumable<KeyCombination>(this.SwitchGui));
            this.imguiSys = api.ModLoader.GetModSystem<ImGuiModSystem>();
            iconHandler = new IconHandler(api);
            tabDrawHandler = new TabDrawHandler(api, iconHandler);
            api.Event.LevelFinalize += () =>
            {
                api.ModLoader.GetModSystem<ImGuiModSystem>().Draw += Draw;
            };
        }
        private bool SwitchGui(KeyCombination comb)
        {
            if (this.prettyGuiState.IsOpen)
            {
                this.prettyGuiState.IsOpen = false;
            }
            else
            {
                this.prettyGuiState.IsOpen = true;
                this.imguiSys.Show();
            }
            return true;
        }
        private void OpenGui()
        {
            if (this.prettyGuiState.IsOpen)
            {
                return;
            }
            this.prettyGuiState.IsOpen = true;
            this.imguiSys.Show();
        }
        private CallbackGUIStatus Draw(float deltaSeconds)
        {
            if(!this.prettyGuiState.IsOpen)
            {
                return CallbackGUIStatus.Closed;
            }

            ImGuiWindowFlags flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar
                 | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoInputs;
            ImGuiWindowFlags flags1 =
                 ImGuiWindowFlags.NoScrollWithMouse;
            //ImGui.Begin("effectBox", flags);
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.252f, 0.161f, 0.016f, 1f));
            ImGui.Begin("Claims", flags1);

            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.4f, 0.8f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.3f, 0.5f, 0.9f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.1f, 0.3f, 0.7f, 1.0f));
            /*if (ImGui.Button("fffff8232", new Vector2(750, 40)))
            {
                // Действие при нажатии
            }*/

            ImGui.PopStyleColor(3);


            /*ImGui.SetNextWindowSize(new Vector2(500, 300), ImGuiCond.FirstUseEver);
            var displaySize = ImGui.GetIO().DisplaySize;
            var windowSize = new Vector2(500, 300);
            var windowPos = new Vector2(
                (displaySize.X - windowSize.X) * 0.5f,
                (displaySize.Y - windowSize.Y) * 0.5f
            );
            float roll = capi.World.Player.CameraRoll * GameMath.RAD2DEG;
            ImGui.SliderFloat("Roll", ref roll, -90, 90);*/

          /*  if (claimsGui.myTex == null)
            {
                ImageSurface surface = new ImageSurface(0, 1, 1);
                Context context = new Context(surface);
                context.SetSourceRGBA(0.2, 1.0, 1.0, 0.1);
                context.Paint();
                claimsGui.myTex = new LoadedTexture(this.capi);
                capi.Gui.LoadOrUpdateCairoTexture(surface, false, ref myTex);
                context.Dispose();
                surface.Dispose();
            }*/

            /* var assetPath = new AssetLocation($"canmarket:textures/icons/pickaxe.svg");
             var asset = capi.Assets.TryGet(assetPath);

             IAsset svgAsset = capi.Assets.Get("canmarket:textures/icons/pickaxe.svg");
             if (svgid == 0)
             {
                 var tee = capi.Gui.LoadSvgWithPadding(assetPath, 250, 250, 0, -900000000);
                 svgid = tee.TextureId;
             }*/
            /*[ "claims:qaitbay-citadel", "claims:magnifying-glass",
             "claims:price-tag", "claims:flat-platform",
                                                                "claims:prisoner", "claims:magic-portal",
                                                                "claims:huts-village"        ]*/
            // var textureId = capi.Render.GetOrLoadTexture(assetPath);
            string[] labels = { "qaitbay-citadel", "magnifying-glass", "price-tag", "flat-platform", "prisoner", "magic-portal", "huts-village" };
            //int te = capi.Assets
            for (int i = 0; i < 7; i++)
            {
                
                ImGui.PushID(i);

                if (active_button == i)
                    ImGui.PushStyleColor(ImGuiCol.Button, ImGui.GetStyle().Colors[23]);

                if (ImGui.ImageButton("", this.iconHandler.GetOrLoadIcon(labels[i]), new Vector2(60)))
                {
                    active_button = i;
                    selectedTab = (EnumSelectedTab)i;
                }

                if (active_button == i)
                    ImGui.PopStyleColor();

                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip(labels[i]);
                }

                ImGui.PopID();
                ImGui.SameLine();
            }
            ImGui.NewLine();
            ImGui.Separator();
            this.tabDrawHandler.DrawTab(this.selectedTab);
            ImGui.End();
            var c = ImGui.GetBackgroundDrawList();
            // c.AddImage(myTex.TextureId, new(50), new(600));
            //ImGui.Image(tt.TextureId,new(200));
            //capi.World.Player.CameraRoll = roll * GameMath.DEG2RAD;
            /*if (ImGui.ImageButton("", myTex.TextureId, new Vector2(40)))
            {
                // действие
            }*/
            /*ImGui.PopFont();
            var f2 = ImGui.GetIO().Fonts.Fonts[6];
            ImGui.PushFont(f2);
            var p = ImGui.GetFont();
            //ImGui.PushFont
            ImGui.RadioButton("hello", false);
            byte[] f = new byte[16];
            ImGui.InputText("here", f, 16);
            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.ShowUserGuide();*/
            if (this.secondaryWindowTab != EnumSecondaryWindowTab.NONE)
            {
                var mainPos = ImGui.GetWindowPos();
                var mainSize = ImGui.GetWindowSize();
                ImGui.SetNextWindowPos(
                    mainPos + new Vector2(mainSize.X + 10, 0),
                    ImGuiCond.Always
                );

                ImGui.SetNextWindowSize(
                    new Vector2(220, mainSize.Y),
                    ImGuiCond.Always
                );

                ImGui.Begin("Side", ImGuiWindowFlags.NoResize);
                ImGui.Text("Goood");
                ImGui.End();
            }

            ImGui.End();
            return CallbackGUIStatus.GrabMouse;
        }
    }
}
