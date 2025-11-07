using System;
using System.Collections.Generic;
using Cairo;
using claims.src.gui.playerGui.structures.cellElements;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using static claims.src.gui.playerGui.CANClaimsGui;

namespace claims.src.gui.playerGui.GuiElements
{
    internal class GuiElementCityRanks : GuiElementTextBase, IGuiElementCell, IDisposable
    {
        private List<GuiElementButtonWithAdditionalText> buttons = new List<GuiElementButtonWithAdditionalText>();
        private GuiElementToggleButton addRankButton;
        private GuiElementStaticText rankName;
        public GuiElementRichtext richTextElem;
        public enum HighlightedTexture
        {
            FIRST, SECOND, THIRD
        }
        public static double unscaledRightBoxWidth = 40.0;

        private CityRankCellElement rankCell;

        private bool showModifyIcons = true;

        public bool On;

        internal int leftHighlightTextureId;

        internal int middleHighlightTextureId;

        internal int rightHighlightTextureId;

        internal int switchOnTextureId;

        internal double unscaledSwitchPadding = 4.0;

        internal double unscaledSwitchSize = 25.0;

        private LoadedTexture modcellTexture;

        private static IAsset cancelIcon;
        private static IAsset approveIcon;

        private ICoreClientAPI capi;

        ElementBounds IGuiElementCell.Bounds => Bounds;


        public override void ComposeElements(Context ctx, ImageSurface surface)
        {
            base.ComposeElements(ctx, surface);
        }
        public GuiElementCityRanks(ICoreClientAPI capi, CityRankCellElement rankCell, ElementBounds bounds)
            : base(capi, "", null, bounds)
        {
            this.rankCell = rankCell;
            this.Font = CairoFont.WhiteSmallishText();
            modcellTexture = new LoadedTexture(capi);

            cancelIcon = capi.Assets.Get(new AssetLocation("claims:textures/icons/cancel.svg"));
            approveIcon = capi.Assets.Get(new AssetLocation("claims:textures/icons/check-mark.svg"));

            this.capi = capi;
            this.text = rankCell.Name;



  
            
            //button.SetOrientation(CairoFont.ButtonText().Orientation);
           // ElementBounds textBounds = ElementBounds.Fixed(0.0, 0.0, 900.0, 100.0).WithEmptyParent();
            //double unScaledTextCellHeight = 25.0;
            double unScaledButtonCellHeight = 35.0;
            var height = unScaledButtonCellHeight;
            var font = CairoFont.WhiteDetailText();
            var offY = (height - font.UnscaledFontsize) / 2.0;
            TextExtents textExtents = CairoFont.WhiteMediumText().GetTextExtents(rankCell.Name);
            var labelTextBounds = ElementBounds.Fixed(0.0, 0.0, textExtents.Width + 10, height).WithParent(Bounds);
            this.richTextElem = new GuiElementRichtext(capi, VtmlUtil.Richtextify(capi, this.rankCell.Name, CairoFont.WhiteMediumText()), labelTextBounds);
            var addRankBounds = labelTextBounds.RightCopy().WithFixedSize(25, 25);
            addRankBounds.fixedY += 5;
            addRankButton = new GuiElementToggleButton(capi, "plus", "", font, (bool t) =>
            {
                if (t)
                {
                    claims.CANCityGui.CreateNewCityState = EnumUpperWindowSelectedState.CITY_RANK_ADD;
                    claims.CANCityGui.firstValueCollected = this.rankCell.Name;
                    claims.CANCityGui.BuildUpperWindow();
                }
            }, addRankBounds);

            double offsetY = 35;
            double offsetX = 0;
            if (rankCell.Citizens.Count > 0)
            {
                foreach (var it in rankCell.Citizens)
                {
                    textExtents = Font.GetTextExtents(it);
                    if ((textExtents.Width + 40 + offsetX + 20) > bounds.fixedWidth + this.Bounds.fixedX)
                    {
                        offsetX = 0;
                        offsetY += 30;
                    }
                    var bu = ElementBounds.Fixed(offsetX, offsetY, textExtents.Width + 40, 25).WithParent(Bounds);
                    buttons.Add(new GuiElementButtonWithAdditionalText(capi, it, this.Font, this.Font, new ActionConsumable(() =>
                    {
                        claims.CANCityGui.CreateNewCityState = EnumUpperWindowSelectedState.CITY_RANK_REMOVE_CONFIRM;
                        claims.CANCityGui.firstValueCollected = this.rankCell.Name;
                        claims.CANCityGui.secondValueCollected = it;
                        claims.CANCityGui.BuildUpperWindow();
                        return true;
                    }), bu));
                    offsetX += textExtents.Width + 45 + 20;
                }

            }

            if (offsetY > 30)
            {
                Bounds.fixedHeight = offsetY + 60;
            }


        }
        private void Compose()
        {
            ComposeHover(HighlightedTexture.FIRST, ref leftHighlightTextureId);
            ComposeHover(HighlightedTexture.SECOND, ref middleHighlightTextureId);
            ComposeHover(HighlightedTexture.THIRD, ref rightHighlightTextureId);
            genOnTexture();
            ImageSurface imageSurface = new ImageSurface(Format.Argb32, Bounds.OuterWidthInt, Bounds.OuterHeightInt);
            Context context = new Context(imageSurface);
            double num = GuiElement.scaled(unscaledRightBoxWidth);
            Bounds.CalcWorldBounds();

            
            
            //textUtil.AutobreakAndDrawMultilineTextAt(context, Font, this.rankCell.RankName, Bounds.absPaddingX, Bounds.absPaddingY + GuiElement.scaled(5), textExtents.Width + 1.0, EnumTextOrientation.Left);

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
            richTextElem.Compose();
            addRankButton.ComposeElements(context, imageSurface);
            generateTexture(imageSurface, ref modcellTexture);
            ComposeElements(context, imageSurface);
            context.Dispose();
            imageSurface.Dispose();
        }

        private void genOnTexture()
        {
            double num = GuiElement.scaled(unscaledSwitchSize - 2.0 * unscaledSwitchPadding);
            ImageSurface imageSurface = new ImageSurface(Format.Argb32, (int)num, (int)num);
            Context context = genContext(imageSurface);
            GuiElement.RoundRectangle(context, 0.0, 0.0, num, num, 2.0);
            GuiElement.fillWithPattern(api, context, GuiElement.waterTextureName);
            generateTexture(imageSurface, ref switchOnTextureId);
            context.Dispose();
            imageSurface.Dispose();
        }

        private void ComposeHover(HighlightedTexture highlightedTexutre, ref int textureId)
        {
            ImageSurface imageSurface = new ImageSurface(Format.Argb32, (int)Bounds.OuterWidth, (int)Bounds.OuterHeight);
            Context context = genContext(imageSurface);
            double num = GuiElement.scaled(unscaledRightBoxWidth);
            if (highlightedTexutre == HighlightedTexture.FIRST)
            {
                context.NewPath();
                context.LineTo(0.0, 0.0);
                context.LineTo(Bounds.InnerWidth - num * 2, 0.0);
                context.LineTo(Bounds.InnerWidth - num * 2, Bounds.OuterHeight);
                context.LineTo(0.0, Bounds.OuterHeight);
                context.ClosePath();
            }
            else if (highlightedTexutre == HighlightedTexture.SECOND)
            {
                context.NewPath();
                context.LineTo(Bounds.InnerWidth - num * 2, 0);
                context.LineTo(Bounds.InnerWidth - num, 0);
                context.LineTo(Bounds.InnerWidth - num, Bounds.OuterHeight);
                context.LineTo(Bounds.InnerWidth - num * 2, Bounds.OuterHeight);
                context.ClosePath();
            }
            else
            {
                context.NewPath();
                context.LineTo(Bounds.InnerWidth - num, 0.0);
                context.LineTo(Bounds.OuterWidth, 0.0);
                context.LineTo(Bounds.OuterWidth, Bounds.OuterHeight);
                context.LineTo(Bounds.InnerWidth - num, Bounds.OuterHeight);
                context.ClosePath();
            }

            context.SetSourceRGBA(0.0, 0.0, 0.0, 0.15);
            context.Fill();
            generateTexture(imageSurface, ref textureId);
            context.Dispose();
            imageSurface.Dispose();
        }

        public void UpdateCellHeight()
        {
            Bounds.CalcWorldBounds();
            richTextElem.BeforeCalcBounds();
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

            if (buttons != null)
            {
                foreach (var it in buttons)
                {
                    it.RenderInteractiveElements(deltaTime);
                }
            }
            richTextElem.RenderInteractiveElements(deltaTime);
            addRankButton.RenderInteractiveElements(deltaTime);

        }

        public override void Dispose()
        {
            base.Dispose();
            foreach (var it in buttons)
            {
                it.Dispose();
            }
            addRankButton.Dispose();
            modcellTexture?.Dispose();
            api.Render.GLDeleteTexture(leftHighlightTextureId);
            api.Render.GLDeleteTexture(middleHighlightTextureId);
            api.Render.GLDeleteTexture(rightHighlightTextureId);
            api.Render.GLDeleteTexture(switchOnTextureId);
        }

        public void OnMouseUpOnElement(MouseEvent args, int elementIndex)
        {
            if (buttons == null) return;

            int x = api.Input.MouseX;
            int y = api.Input.MouseY;
            Vec2d vec2d1 = Bounds.PositionInside(x, y);
            foreach (var it in buttons)
            {
                if (it.IsPositionInside(x, y))
                {
                    it.OnMouseUpOnElement(api, args);
                    return;
                }
            }
            if(addRankButton.IsPositionInside(x, y)){
                addRankButton.OnMouseUpOnElement(api, args);
                return;
            }
            int mouseX = api.Input.MouseX;
            int mouseY = api.Input.MouseY;
            Vec2d vec2d = Bounds.PositionInside(mouseX, mouseY);
            claims.CANCityGui.SelectedTab = EnumSelectedTab.RankInfoPage;
            claims.CANCityGui.selectedString = this.rankCell.Name;
            claims.CANCityGui.BuildMainWindow();
            api.Gui.PlaySound("menubutton_press");
            api.Gui.PlaySound("menubutton_press");
        }

        public void OnMouseMoveOnElement(MouseEvent args, int elementIndex)
        {
            if (buttons == null) return;
            foreach (var it in buttons)
            {
                it.OnMouseMove(api, args);
            }
            addRankButton.OnMouseMove(api, args);
        }

        public void OnMouseDownOnElement(MouseEvent args, int elementIndex)
        {
            if (this.buttons == null) return;

            int x = api.Input.MouseX;
            int y = api.Input.MouseY;
            Vec2d vec2d1 = Bounds.PositionInside(x, y);
            foreach (var it in buttons)
            {
                if (it.IsPositionInside(x, y))
                {
                    it.OnMouseDownOnElement(api, args);
                    return;
                }
            }
            if (addRankButton.IsPositionInside(x, y))
            {
                addRankButton.OnMouseDownOnElement(api, args);
                return;
            }
        }      
    }
}
