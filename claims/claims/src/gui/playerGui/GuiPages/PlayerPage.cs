using System;
using claims.src.auxialiry;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using static claims.src.gui.playerGui.CANClaimsGui;

namespace claims.src.gui.playerGui.GuiPages
{
    public static class PlayerPage
    {
        public static void BuildPlayerPage(CANClaimsGui gui, ElementBounds currentBounds, ElementBounds lineBounds)
        {
            var compo = gui.SingleComposer;
            var playerTabFont = CairoFont.ButtonText().WithFontSize(20).WithOrientation(EnumTextOrientation.Left);
            currentBounds = currentBounds.BelowCopy(0, 40);
            currentBounds.fixedY += 25;
            currentBounds.fixedWidth /= 2;
            currentBounds.WithAlignment(EnumDialogArea.LeftTop);

            var clientInfo = claims.clientDataStorage.clientPlayerInfo;
            compo.AddStaticText(Lang.Get("claims:gui-friends", clientInfo.Friends.Count),
                        playerTabFont,
                        currentBounds, "friends");

            compo.AddHoverText(StringFunctions.concatStringsWithDelim(clientInfo.Friends, ','),
                                        playerTabFont.WithOrientation(EnumTextOrientation.Center),
                                        (int)currentBounds.fixedWidth, currentBounds);

            ElementBounds addFriendBounds = currentBounds.RightCopy();
            addFriendBounds.WithFixedWidth(25).WithFixedHeight(25);
            ElementBounds removeFriendBounds = addFriendBounds.RightCopy();
            compo.AddIconButton("plus", (bool t) =>
            {
                if (t)
                {
                    gui.CreateNewCityState = EnumUpperWindowSelectedState.ADD_FRIEND_NEED_NAME;
                    gui.BuildUpperWindow();
                }
            }, addFriendBounds);

            compo.AddIconButton("line", (bool t) =>
            {
                if (t)
                {
                    gui.CreateNewCityState = EnumUpperWindowSelectedState.REMOVE_FRIEND;
                    gui.BuildUpperWindow();
                }
            }, removeFriendBounds);
            /*==============================================================================================*/
            /*=====================================UNDER 2 LINE=============================================*/
            /*==============================================================================================*/
            var line2Bounds = currentBounds.BelowCopy(0, 20).WithFixedHeight(5).WithFixedWidth(lineBounds.fixedWidth);
            line2Bounds.fixedX = 0;
            line2Bounds.fixedY = gui.mainBounds.fixedHeight * 0.85;
            compo.AddInset(line2Bounds);

            ElementBounds nextIconBounds = line2Bounds.BelowCopy().WithFixedSize(48, 48).WithAlignment(EnumDialogArea.LeftTop);
            nextIconBounds.fixedX = 0;
            nextIconBounds.fixedY = gui.mainBounds.fixedHeight * 0.90;



            compo.AddIconButton("claims:village", new Action<bool>((b) =>
            {
                gui.SelectedTab = EnumSelectedTab.CitiesListPage;
                gui.BuildMainWindow();
                return;
            }), nextIconBounds);
        }
    }
}
