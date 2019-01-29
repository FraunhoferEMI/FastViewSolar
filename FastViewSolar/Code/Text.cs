/* View factor / area determination for solar flux computations.
 *
 * Sprite batch text display.
 * 
 * The Fraunhofer-Gesellschaft zur Foerderung der angewandten Forschung e.V.,
 * Hansastrasse 27c, 80686 Munich, Germany (further: Fraunhofer) is the holder
 * of all proprietary rights on this computer program. You can only use this
 * computer program if you have closed a license agreement with Fraunhofer or
 * you get the right to use the computer program from someone who is authorized
 * to grant you that right. Any use of the computer program without a valid
 * license is prohibited and liable to prosecution.
 * 
 * The use of this software is only allowed under the terms and condition of the
 * General Public License version 2.0 (GPL 2.0).
 * 
 * Copyright©2019 Gesellschaft zur Foerderung der angewandten Forschung e.V. acting
 * on behalf of its Fraunhofer Institut für  Kurzzeitdynamik. All rights reserved.
 * 
 * Contact: max.gulde@emi.fraunhofer.de
 * 
 */

#region using

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Thermal
{
    public class Text : DrawableGameComponent
    {

        #region fields

        List<string> TextSun, TextAll;
        UserSettings Set;

        #region gfx

        SpriteBatch SBatch;
        SpriteFont Font;
        Texture2D Separator;
        public Texture2D ColorBar { get; set; }

        #endregion

        #endregion

        #region init

        public Text(Game Game, SpriteBatch SB, UserSettings set) : base(Game)
        {
            TextSun = new List<string>();
            TextAll = new List<string>();

            Set = set;

            // setup sprite batch
            SBatch = SB;

            // load sprite font
            Font = Game.Content.Load<SpriteFont>("Font");

            // make separating texture
            Separator = new Texture2D(GraphicsDevice, 1, 1);
            Color[] C = new Color[1];
            C[0] = Color.White;
            Separator.SetData<Color>(C);

            // make colorbar texture
            ColorBar = new Texture2D(GraphicsDevice, 1, Settings.ColorBarHeight);
            Color[] cBar = new Color[Settings.ColorBarHeight];
            for (int i = 0; i < Settings.ColorBarHeight; i++)
            {
                cBar[Settings.ColorBarHeight - i - 1] = Tools.GetColor(((float)i / (float)Settings.ColorBarHeight), 0, 1);
            }
            ColorBar.SetData(cBar);
        }

        #endregion

        #region update

        public void Update()
        {
            TextSun.Clear();
            TextAll.Clear();
        }

        public void WriteLine(string line, Views view)
        {
            switch(view)
            {
                case Views.Sun:
                    TextSun.Add(line);
                    break;
                case Views.All:
                    TextAll.Add(line);
                    break;
            }
        }

        #endregion

        #region draw

        public void Draw(bool ShowSeparator)
        {
            // define offset for viewports
            Vector2 Offset = new Vector2(Set.ScreenSizePixel, 0);
            Vector2 OffsetCommon =new Vector2(0, Set.ScreenSizePixel - 120);

            // sun viewport
            int i = 0;
            foreach (string line in TextSun)
            {
                SBatch.DrawString(Font, line, Settings.TextPosition + i * Settings.TextSpacing, Settings.TextColor);
                i++;
            }

            // common viewport
            i = 0;
            foreach (string line in TextAll)
            {
                SBatch.DrawString(Font, line, OffsetCommon + Settings.TextPosition + i * Settings.TextSpacing, Settings.TextColor);
                i++;
            }

            // draw separator
            if (ShowSeparator)
            {
                SBatch.Draw(Separator, new Rectangle((int)Offset.X - Settings.SeparatorWidth / 2, 0, Settings.SeparatorWidth, Set.ScreenSizePixel), Color.White);
            }
        }

        public void DrawText(Viewport view, string text)
        {
            SBatch.DrawString(Font, text, new Vector2(view.X, view.Y), Settings.TextColor);
        }

        public void DrawColorBar()
        {
            // color bar texture
            SBatch.Draw(ColorBar, new Rectangle(Set.ScreenSizePixel - Settings.ColorBarWidth - Settings.ColorBarPadding, Settings.ColorBarPadding, Settings.ColorBarWidth, Settings.ColorBarHeight), Color.White);
            // coloar bar text
            float xPos = Set.ScreenSizePixel - Settings.ColorBarWidth - Settings.ColorBarPadding * 3;
            float yTop = Settings.ColorBarPadding;
            float yBot = Settings.ColorBarHeight + Settings.ColorBarPadding / 2;
            // Scale
            float range = Settings.TMax - Settings.TMin;
            for (int i = 0; i < Settings.ColorBarSteps; i++)
            {
                float relPos = (float)i / (Settings.ColorBarSteps - 1);
                float T = Settings.TMin + range * relPos;
                float yPos = yBot + (yTop - yBot) * relPos;
                SBatch.DrawString(Font, T.ToString("F0") + " K", new Vector2(xPos, yPos), Settings.TextColor);
            }
        }

        #endregion
    }
}
