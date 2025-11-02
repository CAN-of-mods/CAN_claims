using System;
using System.Collections.Generic;
using Cairo;
using Vintagestory.API.Client;
using claims.src.gui.playerGui.structures.cellElements;
using Vintagestory.API.Common;

namespace claims.src.gui.playerGui.GuiElements
{
    public class GuiElementTwoWarRangesCell : GuiElementTextBase, IGuiElementCell, IDisposable
    {
        public static double unscaledRightBoxWidth = 40.0;

        public ClientTwoWarRangesCellElement Cell;
        DayOfWeek DayOfWeek { get; set; }

        public bool On;

        internal double unscaledSwitchPadding = 4.0;

        internal double unscaledSwitchSize = 25.0;

        private LoadedTexture modcellTexture;

        ElementBounds IGuiElementCell.Bounds => Bounds;
        private List<CANGuiElementToggleButton> ToggleButtons = new();
        private List<GuiElementHoverText> HoverTexts = new();
        private List<GuiElementRichtext> texts;
        public GuiElementTwoWarRangesCell(ICoreClientAPI capi, ClientTwoWarRangesCellElement cell, ElementBounds bounds)
            : base(capi, "", null, bounds)
        {
            this.DayOfWeek = cell.DayOfWeek;
            this.Cell = cell;
            this.Font = CairoFont.WhiteSmallishText();
            modcellTexture = new LoadedTexture(capi);

            var font = CairoFont.WhiteDetailText();
            ElementBounds bu = ElementBounds.Fixed(130, 10, 18, 18).WithParent(Bounds);
            ElementBounds currentBounds = bu;
           // ElementBounds textBounds = bu.FlatCopy();
            //textBounds.fixedOffsetX -= 20;
            currentBounds.fixedOffsetY += 10;
            var savedX = currentBounds.fixedX;
            string our = "Our";
            TextExtents textExtents = CairoFont.WhiteMediumText().GetTextExtents(our);
            ElementBounds textBounds = ElementBounds.Fixed(10, 10, textExtents.Width + 40, 25).WithParent(Bounds);
            textBounds.fixedOffsetX -= 20;
            texts = new List<GuiElementRichtext>();

            texts.Add(new GuiElementRichtext(capi, VtmlUtil.Richtextify(capi, our, CairoFont.WhiteMediumText().WithFontSize(25)), textBounds));
            for (int j = 0; j < 3; j++)
            {
                for (int i = 0; i < 16; i++)
                {
                    int p = i + j * 16;
                    ToggleButtons.Add(new CANGuiElementToggleButton(capi, "claims:stairs-goal", (bool t) =>
                    {
                        this.Cell.OurWarRangeArray[p] = !this.Cell.OurWarRangeArray[p];
                    }, currentBounds, true, true));
                    if (this.Cell.OurWarRangeArray[p])
                    {
                        ToggleButtons[p].On = true;
                    }
                    HoverTexts.Add(new GuiElementHoverText(capi, string.Format("{0:00}:{1:00} - {2:00}:{3:00}", Math.Floor(p * 0.5), (p * 0.5) % 1 * 60, Math.Floor((p + 1) * 0.5), ((p + 1) * 0.5) % 1 * 60), font, 120, currentBounds));
                    currentBounds = currentBounds.RightCopy();
                }
                currentBounds = currentBounds.BelowCopy();
                currentBounds.fixedX = savedX;
            }
            currentBounds.fixedOffsetY += 10;
            //currentBounds.BelowCopy(10, 15);
            for (int j = 0; j < 3; j++)
            {
                for (int i = 0; i < 16; i++)
                {
                    int p = i + j * 16;
                    ToggleButtons.Add(new CANGuiElementToggleButton(capi, "claims:stairs-goal", (bool t) =>
                    {
                        this.Cell.EnemyWarRangeArray[p] = !this.Cell.EnemyWarRangeArray[p];
                    }, currentBounds, true));
                    if (this.Cell.EnemyWarRangeArray[p])
                    {
                        ToggleButtons[p + 48].On = true;                       
                    }
                    ToggleButtons[p + 48].Toggleable = false;
                    HoverTexts.Add(new GuiElementHoverText(capi, string.Format("{0:00}:{1:00} - {2:00}:{3:00}", Math.Floor(p * 0.5), (p * 0.5) % 1 * 60, Math.Floor((p + 1) * 0.5), ((p + 1) * 0.5) % 1 * 60), font, 120, currentBounds));
                    currentBounds = currentBounds.RightCopy();
                }
                currentBounds = currentBounds.BelowCopy();
                currentBounds.fixedX = savedX;
            }
        }

        private void Compose()
        {
            genOnTexture();
            ImageSurface imageSurface = new ImageSurface(Format.Argb32, Bounds.OuterWidthInt, Bounds.OuterHeightInt);
            Context context = new Context(imageSurface);
            double num = GuiElement.scaled(unscaledRightBoxWidth);
            Bounds.CalcWorldBounds();

            TextExtents textExtents = Font.GetTextExtents(DayOfWeek.ToString());
            textUtil.AutobreakAndDrawMultilineTextAt(context, Font, DayOfWeek.ToString(), Bounds.absPaddingX, Bounds.absPaddingY + GuiElement.scaled(10), textExtents.Width + 1.0, EnumTextOrientation.Left);
            EmbossRoundRectangleElement(context, 0.0, 0.0, Bounds.OuterWidth, Bounds.OuterHeight, inverse: false, (int)GuiElement.scaled(4.0), 0);

            double num5 = GuiElement.scaled(unscaledSwitchSize);
            double num6 = GuiElement.scaled(unscaledSwitchPadding);
            double num7 = Bounds.absPaddingX + Bounds.InnerWidth - GuiElement.scaled(0.0) - num5 - num6;
            double num8 = Bounds.absPaddingY + Bounds.absPaddingY;

            generateTexture(imageSurface, ref modcellTexture);

            context.Dispose();
            imageSurface.Dispose();

            ImageSurface imageSurface2 = new ImageSurface(Format.Argb32, Bounds.OuterWidthInt, Bounds.OuterHeightInt);
            Context context2 = new Context(imageSurface2);
            Bounds.CalcWorldBounds();

            foreach (var it in ToggleButtons)
            {
                it.ComposeElements(context, imageSurface);
            }
            foreach (var it in HoverTexts)
            {
                it.ComposeElements(context, imageSurface);
            }
            if (texts != null)
            {
                foreach (var it in texts)
                {
                    it.Compose();
                }
            }
            context2.Dispose();
            imageSurface2.Dispose();
        }

        private void genOnTexture()
        {
            double num = GuiElement.scaled(unscaledSwitchSize - 2.0 * unscaledSwitchPadding);
            ImageSurface imageSurface = new ImageSurface(Format.Argb32, (int)num, (int)num);
            Context context = genContext(imageSurface);
            GuiElement.RoundRectangle(context, 0.0, 0.0, num, num, 2.0);
            GuiElement.fillWithPattern(api, context, GuiElement.waterTextureName);
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
            if (Bounds.fixedHeight < 150)
            {
                Bounds.fixedHeight = 150;
            }
        }

        public void OnRenderInteractiveElements(ICoreClientAPI api, float deltaTime)
        {
            if (modcellTexture.TextureId == 0)
            {
                Compose();
            }

            ElementBounds imageBounds = ElementBounds.Fixed(Bounds.absX + 10, Bounds.absY + 10, 32, 32);

            api.Render.Render2DTexturePremultipliedAlpha(modcellTexture.TextureId, (int)Bounds.absX, (int)Bounds.absY, Bounds.OuterWidthInt, Bounds.OuterHeightInt);

            foreach (var it in ToggleButtons)
            {
                it.RenderInteractiveElements(deltaTime);
            }
            foreach (var it in HoverTexts)
            {
                it.RenderInteractiveElements(deltaTime);
            }
            foreach (var it in texts)
            {
                //it.RenderInteractiveElements(deltaTime);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            modcellTexture?.Dispose();
            foreach (var it in ToggleButtons)
            {
                it.Dispose();
            }
            foreach (var it in HoverTexts)
            {
                it.Dispose();
            }
            foreach (var it in texts)
            {
                it.Dispose();
            }
        }

        public void OnMouseUpOnElement(MouseEvent args, int elementIndex)
        {
            int mouseX = api.Input.MouseX;
            int mouseY = api.Input.MouseY;
            foreach (var it in ToggleButtons)
            {
                it.OnMouseUpOnElement(api, args);
                /*if(it.IsPositionInside(mouseX, mouseY))
                {
                    it.On = !it.On;
                }*/
                
                //args.Handled = true;
                //return;
            }
            foreach (var it in HoverTexts)
            {
                it.OnMouseUpOnElement(api, args);
            }
            return;                    
        }

        public void OnMouseMoveOnElement(MouseEvent args, int elementIndex)
        {
            foreach (var it in HoverTexts)
            {
                it.OnMouseMove(api, args);
            }
        }

        public void OnMouseDownOnElement(MouseEvent args, int elementIndex)
        {
            int mouseX = api.Input.MouseX;
            int mouseY = api.Input.MouseY;
            foreach (var it in ToggleButtons)
            {
                if(it.Toggleable && it.IsPositionInside(mouseX, mouseY))
                {
                    it.OnMouseDownOnElement(api, args);
                }
            }
        }
    }
}
