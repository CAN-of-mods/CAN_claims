using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cairo;
using Vintagestory.API.Client;
using static claims.src.gui.playerGui.CANClaimsGui;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;
using claims.src.gui.playerGui.structures.cellElements;

namespace claims.src.gui.playerGui.GuiElements
{
    public class GuiElementCityPlotsGroupCell : GuiElementTextBase, IGuiElementCell, IDisposable
    {
        private List<GuiElementButtonWithAdditionalText> buttons = new List<GuiElementButtonWithAdditionalText>();
        private List<GuiElementRichtext> texts;
        public GuiElementRichtext richTextElem;
        public enum HighlightedTexture
        {
            FIRST, SECOND, THIRD
        }
        public static double unscaledRightBoxWidth = 40.0;

        private PlotsGroupCellElement plotsGroupCell;

        private bool showModifyIcons = true;

        public bool On;

        internal int leftHighlightTextureId;

        internal int switchOnTextureId;

        internal double unscaledSwitchPadding = 4.0;

        internal double unscaledSwitchSize = 25.0;

        private LoadedTexture modcellTexture;

        private ICoreClientAPI capi;

        public Action<int> OnMouseDownOnCellLeft;
        public Action<int> OnMouseDownOnCellMiddle;
        public Action<int> OnMouseDownOnCellRight;


        ElementBounds IGuiElementCell.Bounds => Bounds;


        public override void ComposeElements(Context ctx, ImageSurface surface)
        {
            base.ComposeElements(ctx, surface);
        }
        public GuiElementCityPlotsGroupCell(ICoreClientAPI capi, PlotsGroupCellElement plotsGroupCell, ElementBounds bounds)
            : base(capi, "", null, bounds)
        {
            this.plotsGroupCell = plotsGroupCell;
            this.Font = CairoFont.WhiteSmallishText();
            modcellTexture = new LoadedTexture(capi);           
            this.capi = capi;

            TextExtents textExtents = CairoFont.WhiteMediumText().GetTextExtents(plotsGroupCell.CityName + ": " + plotsGroupCell.Name);
            var font = CairoFont.WhiteDetailText();
            texts = new(); double offsetY = 45;
            double offsetX = 15;
            ElementBounds bu = ElementBounds.Fixed(10, 10, textExtents.Width + 40, 25).WithParent(Bounds);
            texts.Add(new GuiElementRichtext(capi, VtmlUtil.Richtextify(capi, plotsGroupCell.CityName + ": " + plotsGroupCell.Name, CairoFont.WhiteMediumText().WithFontSize(25)), bu));
            if (plotsGroupCell.PlayersNames.Count > 0)
            {
                foreach (var it in plotsGroupCell.PlayersNames)
                {
                    textExtents = Font.GetTextExtents(it);
                    if ((textExtents.Width + 40 + offsetX + 20) > bounds.fixedWidth + this.Bounds.fixedX)
                    {
                        offsetX = 0;
                        offsetY += 30;
                    }
                    var tmpEB = ElementBounds.Fixed(offsetX, offsetY, textExtents.Width + 40, 25).WithParent(Bounds);
                    texts.Add(new GuiElementRichtext(capi,
                        VtmlUtil.Richtextify(capi, it, CairoFont.WhiteMediumText().WithFontSize(20)), tmpEB));
                    offsetX += textExtents.Width + 45 + 20;
                }
            }

            if (offsetY > 30)
            {
                Bounds.fixedHeight = offsetY + 60;
            }



            if (offsetY > 30)
            {
                Bounds.fixedHeight = offsetY + 60;
            }
        }
        private void Compose()
        {
            ComposeHover(HighlightedTexture.FIRST, ref leftHighlightTextureId);
            genOnTexture();
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
                    it.BeforeCalcBounds();
                    it.Compose();
                }
            }
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
                /*context.NewPath();
                context.LineTo(Bounds.InnerWidth - num * 2, 0);
                context.LineTo(Bounds.InnerWidth - num, 0);
                context.LineTo(Bounds.InnerWidth - num, Bounds.OuterHeight);
                context.LineTo(Bounds.InnerWidth - num * 2, Bounds.OuterHeight);
                context.ClosePath();*/
            }
            else
            {
                /*context.NewPath();
                context.LineTo(Bounds.InnerWidth - num, 0.0);
                context.LineTo(Bounds.OuterWidth, 0.0);
                context.LineTo(Bounds.OuterWidth, Bounds.OuterHeight);
                context.LineTo(Bounds.InnerWidth - num, Bounds.OuterHeight);
                context.ClosePath();*/
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
            foreach (var it in texts)
            {
                it.RenderInteractiveElements(deltaTime);
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
            Vec2d vec2d = Bounds.PositionInside(mouseX, mouseY);
            if (vec2d != null)
            {
                /* if (this.button.Bounds.PointInside(mouseX, mouseY))
                 {
                     this.button.SetActive(true);
                 }
                 else
                 {
                     this.button.SetActive(false);
                 }*/

            }
            else
            {
                //this.button.SetActive(false);
            }
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
        }

        public override void Dispose()
        {
            base.Dispose();
            foreach (var it in buttons)
            {
                it.Dispose();
            }
            foreach(var it in texts)
            {
                it.Dispose();
            }
            modcellTexture?.Dispose();
            api.Render.GLDeleteTexture(leftHighlightTextureId);
            api.Render.GLDeleteTexture(switchOnTextureId);
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

            int mouseX = api.Input.MouseX;
            int mouseY = api.Input.MouseY;
            claims.CANCityGui.SelectedTab = EnumSelectedTab.PlotsGroupInfoPage;
            claims.CANCityGui.selectedString = this.plotsGroupCell.Guid;
            claims.CANCityGui.BuildMainWindow();
            api.Gui.PlaySound("menubutton_press");           
        }

        public void OnMouseMoveOnElement(MouseEvent args, int elementIndex)
        {
            if (buttons == null) return;
            foreach (var it in buttons)
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
        }
    }
}
