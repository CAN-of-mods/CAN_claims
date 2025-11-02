using System;
using Cairo;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace claims.src.gui.playerGui.GuiElements
{
    public class GuiElementButtonWithAdditionalText : GuiElementControl
    {
        private GuiElementStaticText normalText;

        private GuiElementStaticText pressedText;

        private LoadedTexture normalTexture;

        private LoadedTexture activeTexture;

        private LoadedTexture hoverIconTexture;

        private LoadedTexture normalIconTexture;

        private ActionConsumable onClick;
        private ElementBounds buttonBounds;

        private bool isOver;

        private EnumButtonStyle buttonStyle;

        private bool active;

        private bool currentlyMouseDownOnElement;

        public bool PlaySound = true;

        public static double Padding = 2.0;

        private double textOffsetY;

        public bool Visible = true;
        public double DerankAreaSize = 40;

        public override bool Focusable => true;
        private static IAsset cancelIcon;
        private ICoreClientAPI capi;

        public string Text
        {
            get
            {
                return normalText.GetText();
            }
            set
            {
                normalText.Text = value;
                pressedText.Text = value;
            }
        }

        //
        // Souhrn:
        //     Creates a button with text.
        //
        // Parametry:
        //   capi:
        //     The Client API
        //
        //   text:
        //     The text of the button.
        //
        //   font:
        //     The font of the text.
        //
        //   hoverFont:
        //     The font of the text when the player is hovering over the button.
        //
        //   onClick:
        //     The event fired when the button is clicked.
        //
        //   bounds:
        //     The bounds of the button.
        //
        //   style:
        //     The style of the button.
        public GuiElementButtonWithAdditionalText(ICoreClientAPI capi, string text, CairoFont font, CairoFont hoverFont, ActionConsumable onClick, ElementBounds bounds, EnumButtonStyle style = EnumButtonStyle.Normal)
            : base(capi, bounds)
        {
            hoverIconTexture = new LoadedTexture(capi);
            activeTexture = new LoadedTexture(capi);
            normalTexture = new LoadedTexture(capi);
            normalIconTexture = new LoadedTexture(capi);
            buttonStyle = style;
            normalText = new GuiElementStaticText(capi, text, EnumTextOrientation.Center, bounds.CopyOnlySize().WithFixedWidth(bounds.fixedWidth - this.DerankAreaSize), font);
            normalText.AutoBoxSize(onlyGrow: true);
            pressedText = new GuiElementStaticText(capi, text, EnumTextOrientation.Center, bounds.CopyOnlySize().WithFixedWidth(bounds.fixedWidth - this.DerankAreaSize), hoverFont);
            if (cancelIcon == null)
            {
                cancelIcon = capi.Assets.Get(new AssetLocation("claims:textures/icons/cancel.svg"));
            }
            this.onClick = onClick;
            this.capi = capi;
            this.DerankAreaSize = Math.Min((int)(bounds.fixedHeight) + 2, 40);
        }

        //
        // Souhrn:
        //     Sets the orientation of the text both when clicked and when idle.
        //
        // Parametry:
        //   orientation:
        //     The orientation of the text.
        public void SetOrientation(EnumTextOrientation orientation)
        {
            
            /*normalText.orientation = orientation;
            pressedText.orientation = orientation;*/
        }

        public override void BeforeCalcBounds()
        {
            normalText.AutoBoxSize(onlyGrow: true);
            Bounds.fixedWidth = normalText.Bounds.fixedWidth;
            Bounds.fixedHeight = normalText.Bounds.fixedHeight;
            pressedText.Bounds = normalText.Bounds.CopyOnlySize();
        }

        public override void ComposeElements(Context ctxStatic, ImageSurface surfaceStatic)
        {
            Bounds.CalcWorldBounds();
            normalText.Bounds.CalcWorldBounds();
            ImageSurface imageSurface = new ImageSurface(Format.Argb32, (int)(Bounds.OuterWidth), (int)Bounds.OuterHeight);
            Context context = genContext(imageSurface);
            ComposeButton(context, imageSurface);         
            generateTexture(imageSurface, ref normalTexture);
            context.Clear();
            if (buttonStyle != 0)
            {
                context.SetSourceRGBA(0.0, 0.0, 0.0, 0.4);
                context.Rectangle(0.0, 0.0, Bounds.OuterWidth, Bounds.OuterHeight);
                context.Fill();
            }
            //capi.Gui.DrawSvg(cancelIcon, imageSurface, (int)(Bounds.OuterWidth - this.DerankAreaSize), (int)0, (int)GuiElement.scaled(30.0), (int)GuiElement.scaled(Bounds.OuterHeight), ColorUtil.ColorFromRgba(246, 72, 14, 255));
            pressedText.Bounds.fixedY += textOffsetY;
            pressedText.ComposeElements(context, imageSurface);
            pressedText.Bounds.fixedY -= textOffsetY;
            generateTexture(imageSurface, ref activeTexture);
            context.Clear();
            if (buttonStyle != 0)
            {
                context.SetSourceRGBA(1.0, 1.0, 1.0, 0.1);
                context.Rectangle(Bounds.OuterWidth - this.DerankAreaSize, 0.0, this.DerankAreaSize, Bounds.fixedHeight);
                
                context.Fill(); 
            }
            capi.Gui.DrawSvg(cancelIcon, imageSurface, (int)(Bounds.OuterWidth - this.DerankAreaSize), (int)0, (int)(Bounds.InnerHeight), (int)(Bounds.InnerHeight), ColorUtil.ColorFromRgba(246, 72, 14, 255));

            generateTexture(imageSurface, ref hoverIconTexture);
            context.Dispose();
            imageSurface.Dispose();
            imageSurface = new ImageSurface(Format.Argb32, (int)(Bounds.OuterWidth), (int)Bounds.OuterHeight);
            context = genContext(imageSurface);
            /*if (buttonStyle != 0)
            {
                context.SetSourceRGBA(0.0, 0.0, 0.0, 0.4);
                context.Rectangle(0.0, 0.0, 2.0, 2.0);
                context.Fill();
            }*/
            capi.Gui.DrawSvg(cancelIcon, imageSurface, (int)(Bounds.OuterWidth - this.DerankAreaSize), (int)0, (int)(Bounds.InnerHeight), (int)(Bounds.InnerHeight), ColorUtil.ColorFromRgba(255, 128, 0, 255));
            generateTexture(imageSurface, ref normalIconTexture);
            context.Dispose();
            imageSurface.Dispose();
            buttonBounds = this.Bounds.FlatCopy().WithParent(this.Bounds).WithFixedPosition(Bounds.fixedX + Bounds.InnerWidth - this.DerankAreaSize, Bounds.fixedY);

            //buttonBounds.WithFixedSize(40, 40).WithFixedPosition(Bounds.fixedX, Bounds.fixedY);
            buttonBounds.absInnerHeight = this.Bounds.absInnerHeight;
            buttonBounds.absInnerWidth = this.DerankAreaSize;
            buttonBounds.absFixedX = this.Bounds.InnerWidth - this.DerankAreaSize;
        }

        private void ComposeButton(Context ctx, ImageSurface surface)
        {
            double num = GuiElement.scaled(2.5);
            if (buttonStyle == EnumButtonStyle.Normal || buttonStyle == EnumButtonStyle.Small)
            {
                num = GuiElement.scaled(1.5);
            }

            if (buttonStyle != 0)
            {
                GuiElement.Rectangle(ctx, Bounds.OuterWidth - this.DerankAreaSize, 0.0, Bounds.OuterWidth, Bounds.OuterHeight);
                ctx.SetSourceRGBA(23.0 / 85.0, 52.0 / 255.0, 12.0 / 85.0, 0.8);
                ctx.Fill();
            }

           /* if (buttonStyle == EnumButtonStyle.MainMenu)
            {
                GuiElement.Rectangle(ctx, 0.0, 0.0, Bounds.OuterWidth, num);
                ctx.SetSourceRGBA(1.0, 1.0, 1.0, 0.15);
                ctx.Fill();
            }

            if (buttonStyle == EnumButtonStyle.Normal || buttonStyle == EnumButtonStyle.Small)
            {
                GuiElement.Rectangle(ctx, 0.0, 0.0, Bounds.OuterWidth - num, num);
                ctx.SetSourceRGBA(1.0, 1.0, 1.0, 0.15);
                ctx.Fill();
                GuiElement.Rectangle(ctx, 0.0, 0.0 + num, num, Bounds.OuterHeight - num);
                ctx.SetSourceRGBA(1.0, 1.0, 1.0, 0.15);
                ctx.Fill();
            }*/

            //surface.BlurPartial(2.0, 5);
            FontExtents fontExtents = normalText.Font.GetFontExtents();
            TextExtents textExtents = normalText.Font.GetTextExtents(normalText.GetText());
            double num2 = 0.0 - fontExtents.Ascent - textExtents.YBearing;
            textOffsetY = (num2 + (normalText.Bounds.InnerHeight + textExtents.YBearing) / 2.0) / (double)RuntimeEnv.GUIScale;
            normalText.Bounds.fixedY += textOffsetY;
            normalText.ComposeElements(ctx, surface);
            normalText.Bounds.fixedY -= textOffsetY;
            Bounds.CalcWorldBounds();
            if (buttonStyle == EnumButtonStyle.MainMenu)
            {
                GuiElement.Rectangle(ctx, 0.0, 0.0 + Bounds.OuterHeight - num, Bounds.OuterWidth, num);
                ctx.SetSourceRGBA(0.0, 0.0, 0.0, 0.2);
                ctx.Fill();
            }

            if (buttonStyle == EnumButtonStyle.Normal || buttonStyle == EnumButtonStyle.Small)
            {
                GuiElement.Rectangle(ctx, 0.0 + num, 0.0 + Bounds.OuterHeight - num, Bounds.OuterWidth - 2.0 * num, num);
                ctx.SetSourceRGBA(0.0, 0.0, 0.0, 0.2);
                ctx.Fill();
                GuiElement.Rectangle(ctx, 0.0 + Bounds.OuterWidth - num, 0.0, num, Bounds.OuterHeight);
                ctx.SetSourceRGBA(0.0, 0.0, 0.0, 0.2);
                ctx.Fill();
            }
        }

        public override void RenderInteractiveElements(float deltaTime)
        {
            if (Visible)
            {
                api.Render.Render2DTexturePremultipliedAlpha(normalTexture.TextureId, Bounds);
                if(!isOver)
                {
                    api.Render.Render2DTexturePremultipliedAlpha(normalIconTexture.TextureId, Bounds);
                }
                if (!enabled)
                {
                    //api.Render.Render2DTexturePremultipliedAlpha(disabledTexture.TextureId, Bounds);
                }
                else if (active || currentlyMouseDownOnElement)
                {
                    //api.Render.Render2DTexturePremultipliedAlpha(activeTexture.TextureId, Bounds);
                }
                else if (isOver)
                {
                    api.Render.Render2DTexturePremultipliedAlpha(hoverIconTexture.TextureId, Bounds);
                }
            }
        }

        public override void OnKeyDown(ICoreClientAPI api, KeyEvent args)
        {
            if (!Visible || !base.HasFocus || args.KeyCode != 49)
            {
                return;
            }

            args.Handled = true;
            if (enabled)
            {
                if (PlaySound)
                {
                    api.Gui.PlaySound("menubutton_press");
                }

                args.Handled = onClick();
            }
        }

        public override void OnMouseMove(ICoreClientAPI api, MouseEvent args)
        {
            bool num = isOver;
            setIsOver();
            if (!num && isOver && PlaySound)
            {
                api.Gui.PlaySound("menubutton");
            }
        }

        protected void setIsOver()
        {
            isOver = Visible && enabled && buttonBounds.PointInside(api.Input.MouseX, api.Input.MouseY);
        }

        public override void OnMouseDownOnElement(ICoreClientAPI api, MouseEvent args)
        {
            if (Visible && enabled)
            {
                base.OnMouseDownOnElement(api, args);
                currentlyMouseDownOnElement = true;
                if (PlaySound)
                {
                    api.Gui.PlaySound("menubutton_down");
                }

                setIsOver();
            }
        }

        public override void OnMouseUp(ICoreClientAPI api, MouseEvent args)
        {
            if (Visible)
            {
                if (currentlyMouseDownOnElement && !Bounds.PointInside(args.X, args.Y) && !active && PlaySound)
                {
                    api.Gui.PlaySound("menubutton_up");
                }

                base.OnMouseUp(api, args);
                currentlyMouseDownOnElement = false;
            }
        }

        public override void OnMouseUpOnElement(ICoreClientAPI api, MouseEvent args)
        {
            if (enabled && currentlyMouseDownOnElement && Bounds.PointInside(args.X, args.Y) && (args.Button == EnumMouseButton.Left || args.Button == EnumMouseButton.Right))
            {
                args.Handled = onClick();
            }

            currentlyMouseDownOnElement = false;
        }

        //
        // Souhrn:
        //     Sets the button as active or inactive.
        //
        // Parametry:
        //   active:
        //     Active == clickable
        public void SetActive(bool active)
        {
            this.active = active;
        }

        public override void Dispose()
        {
            base.Dispose();
            hoverIconTexture?.Dispose();
            activeTexture?.Dispose();
            pressedText?.Dispose();
            normalIconTexture?.Dispose();
            normalTexture?.Dispose();
            normalText.Dispose();
        }
    }
}
