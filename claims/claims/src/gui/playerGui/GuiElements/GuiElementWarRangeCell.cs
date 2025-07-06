using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cairo;
using claims.src.auxialiry;
using static OpenTK.Graphics.OpenGL.GL;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.Client.NoObf;
using claims.src.gui.playerGui.structures.cellElements;
using Vintagestory.API.Config;
using System.Threading;

namespace claims.src.gui.playerGui.GuiElements
{
    public class GuiElementWarRangeCell : GuiElementTextBase, IGuiElementCell, IDisposable
    {
        public static double unscaledRightBoxWidth = 40.0;

        public ClientWarRangeCellElement cell;
        DayOfWeek DayOfWeek { get; set; }

        public bool On;

        internal double unscaledSwitchPadding = 4.0;

        internal double unscaledSwitchSize = 25.0;

        private LoadedTexture modcellTexture;
        private bool buttonToggble = true;

        ElementBounds IGuiElementCell.Bounds => Bounds;
        private List<GuiElementToggleButton> ToggleButtons = new List<GuiElementToggleButton>();
        private List<GuiElementHoverText> HoverTexts = new List<GuiElementHoverText>();

        public GuiElementWarRangeCell(ICoreClientAPI capi, ClientWarRangeCellElement cell, ElementBounds bounds, bool toggble = true)
            : base(capi, "", null, bounds)
        {
            this.buttonToggble = toggble;
            this.DayOfWeek = cell.DayOfWeek;
            this.cell = cell;
            this.Font = CairoFont.WhiteSmallishText();
            modcellTexture = new LoadedTexture(capi);

            var font = CairoFont.WhiteDetailText();
            ElementBounds bu = ElementBounds.Fixed(130, 10, 18, 18).WithParent(Bounds);
            ElementBounds currentBounds = bu;
            var savedX = currentBounds.fixedX;
            for (int j = 0; j < 3; j++)
            {
                for (int i = 0; i < 16; i++)
                {
                    int p = i + j * 16;
                    ToggleButtons.Add(new GuiElementToggleButton(capi, "claims:stairs-goal", "", font, (bool t) =>
                    {                       
                        this.cell.WarRangeArray[p] = !this.cell.WarRangeArray[p];
                    }, currentBounds, true));
                    if(this.cell.WarRangeArray[p])
                    {
                        ToggleButtons[p].On = true;
                    }
                    var ff = p % 1 * 60;
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

            //string cellName = string.Format("{0} x {1}", cell.FirstAllianceName, cell.SecondAllianceName);
            TextExtents textExtents = Font.GetTextExtents(DayOfWeek.ToString());
            textUtil.AutobreakAndDrawMultilineTextAt(context, Font, DayOfWeek.ToString(), Bounds.absPaddingX, Bounds.absPaddingY + GuiElement.scaled(10), textExtents.Width + 1.0, EnumTextOrientation.Left);
            //string expDate = TimeFunctions.getDateFromEpochSecondsWithHoursMinutes(cell.TimeStampCreated, true).ToString();
            //textExtents = Font.GetTextExtents(expDate);
           // textUtil.AutobreakAndDrawMultilineTextAt(context, CairoFont.WhiteDetailText(), expDate, Bounds.absPaddingX, Bounds.absPaddingY + GuiElement.scaled(36), textExtents.Width + 1.0, EnumTextOrientation.Left);

            //make border as button
            EmbossRoundRectangleElement(context, 0.0, 0.0, Bounds.OuterWidth, Bounds.OuterHeight, inverse: false, (int)GuiElement.scaled(4.0), 0);

            double num5 = GuiElement.scaled(unscaledSwitchSize);
            double num6 = GuiElement.scaled(unscaledSwitchPadding);
            double num7 = Bounds.absPaddingX + Bounds.InnerWidth - GuiElement.scaled(0.0) - num5 - num6;
            double num8 = Bounds.absPaddingY + Bounds.absPaddingY;

           /* capi.Gui.DrawSvg(cancelIcon, imageSurface, (int)(num7 - GuiElement.scaled(3.0)),
                                                            (int)(num8 + GuiElement.scaled(15.0)),
                                                            (int)GuiElement.scaled(30.0),
                                                            (int)GuiElement.scaled(30.0),
                                                            ColorUtil.ColorFromRgba(255, 128, 0, 255));
            if (claims.clientDataStorage.clientPlayerInfo.AllianceInfo?.Guid?.Equals(this.cell.Guid) ?? false)
            {
                capi.Gui.DrawSvg(approveIcon, imageSurface,
                (int)(num7 - GuiElement.scaled(unscaledRightBoxWidth) - GuiElement.scaled(10.0)),
                (int)(num8 + GuiElement.scaled(15.0)),
                (int)GuiElement.scaled(30.0),
                (int)GuiElement.scaled(30.0),
                ColorUtil.ColorFromRgba(0, 153, 0, 255));
            }*/


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
            if (Bounds.fixedHeight < 73.0)
            {
                Bounds.fixedHeight = 73.0;
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
                if(this.buttonToggble && it.IsPositionInside(mouseX, mouseY))
                {
                    it.OnMouseDownOnElement(api, args);
                }
            }
        }
    }
}
