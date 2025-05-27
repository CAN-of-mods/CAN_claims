using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cairo;
using claims.src.gui.playerGui.structures;
using Vintagestory.API.Client;
using static claims.src.gui.playerGui.CANClaimsGui;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;

namespace claims.src.gui.playerGui.GuiElements
{
    public class GuiElementCitySummonCell : GuiElementTextBase, IGuiElementCell, IDisposable
    {
        public static int counter = 0;
        private List<GuiElementButtonWithAdditionalText> buttons = new List<GuiElementButtonWithAdditionalText>();
        private List<GuiElementStaticText> texts = new List<GuiElementStaticText>();
        private GuiElementToggleButton addRankButton;
        private GuiElementToggleButton setPointName;
        public GuiElementRichtext richTextElem;
        public enum HighlightedTexture
        {
            FIRST, SECOND, THIRD
        }
        public static double unscaledRightBoxWidth = 40.0;

        private SummonCellElement summonCell;

        private bool showModifyIcons = true;

        public bool On;

        internal int leftHighlightTextureId;

        internal int middleHighlightTextureId;

        internal int rightHighlightTextureId;

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
        public GuiElementCitySummonCell(ICoreClientAPI capi, SummonCellElement summonCell, ElementBounds bounds)
            : base(capi, "", null, bounds)
        {
            this.summonCell = summonCell;
            this.Font = CairoFont.WhiteSmallishText();
            modcellTexture = new LoadedTexture(capi);

            this.capi = capi;

            TextExtents textExtents = CairoFont.WhiteMediumText().GetTextExtents(summonCell.Name);
            var font = CairoFont.WhiteDetailText();
            ElementBounds bu = ElementBounds.Fixed(0, 0, textExtents.Width + 40, 25).WithParent(Bounds);
            if (summonCell.Name.Length > 0)
            {                
                texts.Add(new GuiElementStaticText(capi, summonCell.Name, EnumTextOrientation.Left, bu, CairoFont.ButtonText()));
            }
            
            this.text = Lang.Get("claims:gui-prison-cell-coords",
                                    (summonCell.SpawnPosition.X - capi.World.DefaultSpawnPosition.AsBlockPos.X).ToString(),
                                    (summonCell.SpawnPosition.Y - capi.World.DefaultSpawnPosition.AsBlockPos.Y).ToString(),
                                    (summonCell.SpawnPosition.Z - capi.World.DefaultSpawnPosition.AsBlockPos.Z).ToString());
            textExtents = CairoFont.WhiteMediumText().GetTextExtents(this.text);
            double unScaledButtonCellHeight = 35.0;
            var height = unScaledButtonCellHeight;

            var offY = (height - font.UnscaledFontsize) / 2.0;
            double offsetY = 45;
            double offsetX = 15;
            var labelTextBounds = ElementBounds.Fixed(offsetX, offsetY, textExtents.Width, height).WithParent(Bounds);
            this.richTextElem = new GuiElementRichtext(capi, VtmlUtil.Richtextify(capi, this.text, CairoFont.WhiteMediumText().WithFontSize(15)), labelTextBounds);
            var addRankBounds = bu.RightCopy().WithFixedSize(35, 35);
            addRankBounds.fixedY += 5;
            addRankButton = new GuiElementToggleButton(capi, "claims:dodging", "", font, (bool t) =>
            {
                if (t)
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/c summon use " + this.summonCell.Name, EnumChatType.Macro, "");
                }
            }, addRankBounds);
            var setNameBounds = addRankBounds.RightCopy().WithFixedSize(35, 35);
            setPointName = new GuiElementToggleButton(capi, "claims:highlighter", "", font, (bool t) =>
            {
                if (t)
                {
                    claims.CANCityGui.CreateNewCityState = EnumUpperWindowSelectedState.CITY_SUMMON_NEED_NAME;
                    claims.CANCityGui.selectedPos = this.summonCell.SpawnPosition;
                    claims.CANCityGui.BuildUpperWindow();                    
                }
            }, setNameBounds);

            

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
                    it.ComposeElements(context, imageSurface);
                }
            }
            richTextElem.Compose();
            addRankButton.ComposeElements(context, imageSurface);
            setPointName.ComposeElements(context, imageSurface);
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
            richTextElem.RenderInteractiveElements(deltaTime);
            addRankButton.RenderInteractiveElements(deltaTime);
            setPointName.RenderInteractiveElements(deltaTime);
        }

        public override void Dispose()
        {
            base.Dispose();
            foreach (var it in buttons)
            {
                it.Dispose();
            }
            addRankButton.Dispose();
            setPointName.Dispose();
            modcellTexture?.Dispose();
            api.Render.GLDeleteTexture(leftHighlightTextureId);
            api.Render.GLDeleteTexture(middleHighlightTextureId);
            api.Render.GLDeleteTexture(rightHighlightTextureId);
            api.Render.GLDeleteTexture(switchOnTextureId);
            richTextElem.Dispose();
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
            addRankButton.OnMouseUpOnElement(api, args);
            setPointName.OnMouseUpOnElement(api, args);

            int mouseX = api.Input.MouseX;
            int mouseY = api.Input.MouseY;
            Vec2d vec2d = Bounds.PositionInside(mouseX, mouseY);
            api.Gui.PlaySound("menubutton_press");
            if (vec2d.X > Bounds.InnerWidth - GuiElement.scaled(GuiElementMainMenuCell.unscaledRightBoxWidth) * 2 &&
                    vec2d.X < Bounds.InnerWidth - GuiElement.scaled(GuiElementMainMenuCell.unscaledRightBoxWidth))
            {
                OnMouseDownOnCellMiddle?.Invoke(elementIndex);
                args.Handled = true;
            }
            else if (vec2d.X > Bounds.InnerWidth - GuiElement.scaled(GuiElementMainMenuCell.unscaledRightBoxWidth))
            {
                OnMouseDownOnCellRight?.Invoke(elementIndex);
                args.Handled = true;
            }
            else
            {
                OnMouseDownOnCellLeft?.Invoke(elementIndex);
                args.Handled = true;
            }
        }

        public void OnMouseMoveOnElement(MouseEvent args, int elementIndex)
        {
            if (buttons == null) return;
            foreach (var it in buttons)
            {
                it.OnMouseMove(api, args);
            }
            addRankButton.OnMouseMove(api, args);
            setPointName.OnMouseMove(api, args);
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
            if (addRankButton.Bounds.PointInside(x, y))
            {
                addRankButton.OnMouseDownOnElement(api, args);
            }
            if (setPointName.Bounds.PointInside(x, y))
            {
                setPointName.OnMouseDownOnElement(api, args);
            }
        }
    }
}
