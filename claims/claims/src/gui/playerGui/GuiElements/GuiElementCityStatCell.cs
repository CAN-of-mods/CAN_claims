using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cairo;
using claims.src.auxialiry;
using claims.src.gui.playerGui.structures;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.Client.NoObf;
using static claims.src.gui.playerGui.CANClaimsGui;

namespace claims.src.gui.playerGui.GuiElements
{
    public class GuiElementCityStatCell : GuiElementTextBase, IGuiElementCell, IDisposable
    {
        private List<GuiElementButtonWithAdditionalText> buttons = new List<GuiElementButtonWithAdditionalText>();
        private List<GuiElementToggleButton> ToggleButtons = new List<GuiElementToggleButton>();
        private List<GuiElementRichtext> texts;
        private List<GuiElementHoverText> HoverTexts = new List<GuiElementHoverText>();
        public enum HighlightedTexture
        {
            FIRST, SECOND, THIRD
        }
        public static double unscaledRightBoxWidth = 40.0;

        private ClientCityInfoCellElement cityStatCell;

        private bool showModifyIcons = true;

        public bool On;

        internal double unscaledSwitchPadding = 4.0;

        internal double unscaledSwitchSize = 25.0;

        private LoadedTexture modcellTexture;

        private ICoreClientAPI capi;

        ElementBounds IGuiElementCell.Bounds => Bounds;
        public override void ComposeElements(Context ctx, ImageSurface surface)
        {
            base.ComposeElements(ctx, surface);
        }
        public GuiElementCityStatCell(ICoreClientAPI capi, ClientCityInfoCellElement cityStatCell, ElementBounds bounds)
            : base(capi, "", null, bounds)
        {
            this.cityStatCell = cityStatCell;
            this.Font = CairoFont.WhiteSmallishText();
            modcellTexture = new LoadedTexture(capi);
            this.capi = capi;

            string cellName = string.Format("{0} {1}", cityStatCell.Name, (cityStatCell.AllianceName.Length > 0
                                                                                                        ? "[" + cityStatCell.AllianceName + "]"
                                                                                                        : ""));
            TextExtents textExtents = CairoFont.WhiteMediumText().GetTextExtents(cellName);
            var font = CairoFont.WhiteDetailText();
            ElementBounds bu = ElementBounds.Fixed(10, 5, bounds.fixedWidth, 25).WithParent(Bounds);
            ElementBounds currentBounds = bu;
            texts = new List<GuiElementRichtext>();

            texts.Add(new GuiElementRichtext(capi, VtmlUtil.Richtextify(capi, cellName, CairoFont.WhiteMediumText().WithFontSize(25)), currentBounds));
            currentBounds = currentBounds.BelowCopy(15, 5);
            texts.Add(new GuiElementRichtext(capi, VtmlUtil.Richtextify(capi, Lang.Get("claims:gui-mayor-name", cityStatCell.MayorName), CairoFont.WhiteMediumText().WithFontSize(15)), currentBounds));
            currentBounds = currentBounds.BelowCopy();
            currentBounds.fixedWidth /= 2;
            texts.Add(new GuiElementRichtext(capi, VtmlUtil.Richtextify(capi, Lang.Get("claims:gui-city-population", cityStatCell.CitizensAmount) + " " + Lang.Get("claims:gui-claimed-plots", cityStatCell.ClaimedPlotsAmount), CairoFont.WhiteMediumText().WithFontSize(15)), currentBounds));
            if(cityStatCell.Open)
            {
                ElementBounds joinCityBound = new ElementBounds().WithFixedSize(32, 32);
                joinCityBound.fixedX = bounds.fixedWidth - 64;
                joinCityBound.fixedY += 10;
                bounds.WithChild(joinCityBound);
                ToggleButtons.Add(new GuiElementToggleButton(capi, "claims:stairs-goal", "", font, (bool t) =>
                {
                    if (t)
                    {
                        ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                        clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/c join " + this.cityStatCell.Name, EnumChatType.Macro, "");
                    }
                }, joinCityBound));
            }
            if (cityStatCell.InvMsg.Length > 0)
            {
                ElementBounds joinCityBound = new ElementBounds().WithFixedSize(32, 32);
                joinCityBound.fixedX = bounds.fixedWidth - 96;
                joinCityBound.fixedY += 10;
                bounds.WithChild(joinCityBound);
                ToggleButtons.Add(new GuiElementToggleButton(capi, "claims:info", "", font, (bool t) =>
                {
                }, joinCityBound));
                HoverTexts.Add(new GuiElementHoverText(capi, cityStatCell.InvMsg, font, 300, joinCityBound));
            }

            currentBounds = currentBounds.BelowCopy();
            texts.Add(new GuiElementRichtext(capi, VtmlUtil.Richtextify(capi, Lang.Get("claims:gui-date-created", TimeFunctions.getDateFromEpochSeconds(cityStatCell.TimeStampCreated)), CairoFont.WhiteMediumText().WithFontSize(15)), currentBounds));

            
            textExtents = CairoFont.WhiteMediumText().GetTextExtents(this.text);
            double unScaledButtonCellHeight = 35.0;
            var height = unScaledButtonCellHeight;

            var offY = (height - font.UnscaledFontsize) / 2.0;
            double offsetY = 45;
            double offsetX = 15;
            var labelTextBounds = ElementBounds.Fixed(offsetX, offsetY, textExtents.Width, height).WithParent(Bounds);
            texts.Add(new GuiElementRichtext(capi, VtmlUtil.Richtextify(capi, this.text, CairoFont.WhiteMediumText().WithFontSize(15)), labelTextBounds));
            var addRankBounds = bu.RightCopy(65).WithFixedSize(35, 35);
            addRankBounds.fixedY += 5;
            var setNameBounds = addRankBounds.RightCopy().WithFixedSize(35, 35);


            if (offsetY > 30)
            {
                Bounds.fixedHeight = offsetY + 60;
            }
        }
        private void Compose()
        {
            ImageSurface imageSurface = new ImageSurface(Format.Argb32, Bounds.OuterWidthInt, Bounds.OuterHeightInt);
            Context context = new Context(imageSurface);
            double num = GuiElement.scaled(unscaledRightBoxWidth);
            Bounds.CalcWorldBounds();

            //make border as button
            EmbossRoundRectangleElement(context, 0.0, 0.0, Bounds.OuterWidth, Bounds.OuterHeight, inverse: false, (int)GuiElement.scaled(4.0), 0);

            double num5 = GuiElement.scaled(unscaledSwitchSize);
            double num6 = GuiElement.scaled(unscaledSwitchPadding);


            if (buttons != null)
            {
                foreach (var it in buttons)
                {
                    it.ComposeElements(context, imageSurface);
                }
            }
            if (texts != null)
            {
                foreach (var it in texts)
                {
                    it.Compose();
                }
            }

            foreach (var it in ToggleButtons)
            {
                it.ComposeElements(context, imageSurface);
            }
            foreach (var it in HoverTexts)
            {
                it.ComposeElements(context, imageSurface);
            }

            generateTexture(imageSurface, ref modcellTexture);
            ComposeElements(context, imageSurface);
            context.Dispose();
            imageSurface.Dispose();
        }
        public void UpdateCellHeight()
        {
            Bounds.CalcWorldBounds();
            foreach (var it in texts)
            {
                it.BeforeCalcBounds();
            }
            if (showModifyIcons && Bounds.fixedHeight < 73.0)
            {
                Bounds.fixedHeight = 73;
            }
        }
        public override void RenderInteractiveElements(float deltaTime)
        {
            base.RenderInteractiveElements(deltaTime);
            if (buttons != null)
            {
                foreach (var it in buttons)
                {
                    it.RenderInteractiveElements(deltaTime);
                }
            }
        }
        public void OnRenderInteractiveElements(ICoreClientAPI api, float deltaTime)
        {
            if (modcellTexture.TextureId == 0)
            {
                Compose();
            }

            api.Render.Render2DTexturePremultipliedAlpha(modcellTexture.TextureId, (int)Bounds.absX, (int)Bounds.absY, Bounds.OuterWidthInt, Bounds.OuterHeightInt);
            int mouseX = api.Input.MouseX;
            int mouseY = api.Input.MouseY;
            if (buttons != null)
            {
                foreach (var it in buttons)
                {
                    it.RenderInteractiveElements(deltaTime);
                }
            }
            foreach (var it in texts)
            {
                it.RenderInteractiveElements(deltaTime);
            }
            foreach (var it in ToggleButtons)
            {
                it.RenderInteractiveElements(deltaTime);
            }

            foreach (var it in HoverTexts)
            {
                it.RenderInteractiveElements(deltaTime);
            }
        }
        public override void Dispose()
        {
            base.Dispose();
            foreach (var it in buttons)
            {
                it.Dispose();
            }
            foreach (var it in texts)
            {
                it.Dispose();
            }
            foreach (var it in ToggleButtons)
            {
                it.Dispose();
            }
            foreach (var it in HoverTexts)
            {
                it.Dispose();
            }
            modcellTexture?.Dispose();
        }

        public void OnMouseUpOnElement(MouseEvent args, int elementIndex)
        {
            if (buttons == null) return;

            int x = api.Input.MouseX;
            int y = api.Input.MouseY;

            foreach (var it in buttons)
            {
                it.OnMouseUpOnElement(api, args);
            }
            foreach (var it in ToggleButtons)
            {
                it.OnMouseUpOnElement(api, args);
            }
            foreach (var it in HoverTexts)
            {
                it.OnMouseUpOnElement(api, args);
            }
        }

        public void OnMouseMoveOnElement(MouseEvent args, int elementIndex)
        {
            if (buttons == null) return;
            foreach (var it in buttons)
            {
                it.OnMouseMove(api, args);
            }
            foreach (var it in ToggleButtons)
            {
                it.OnMouseMove(api, args);
            }
            foreach (var it in HoverTexts)
            {
                it.OnMouseMove(api, args);
            }
        }

        public void OnMouseDownOnElement(MouseEvent args, int elementIndex)
        {
            if (this.buttons == null) return;

            int x = api.Input.MouseX;
            int y = api.Input.MouseY;
            foreach (var it in buttons)
            {
                if (it.Bounds.PointInside(x, y))
                {
                    it.OnMouseDownOnElement(api, args);
                }
            }
            foreach (var it in ToggleButtons)
            {
                if (it.Bounds.PointInside(x, y))
                {
                    it.OnMouseDownOnElement(api, args);
                }
            }
            foreach (var it in HoverTexts)
            {
                if (it.Bounds.PointInside(x, y))
                {
                    it.OnMouseDownOnElement(api, args);
                }
            }
        }
    }
}
