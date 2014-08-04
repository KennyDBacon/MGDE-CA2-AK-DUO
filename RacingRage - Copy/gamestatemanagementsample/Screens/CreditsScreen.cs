using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input.Touch;
using GameStateManagement;


namespace GameStateManagementSample
{
    /// <summary>
    /// Credits Screen
    /// </summary>
    class CreditsScreen : GameScreen
    {


        ContentManager content;
        SpriteFont gameFont;
        float creditpos;
        String creditsTitle = "Credits";

        Texture2D splogo;
        Texture2D background_pattern;

        InputAction backToMenu;

        //Contains height and width of screen.
        Vector2 ScreenDimensions = new Vector2(
            GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height,
            GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width);

        //Vector2 creditsPosition = new Vector2()

        public CreditsScreen()
        {
            EnabledGestures = GestureType.Tap;
        }



        public override void Activate(bool instancePreserved)
        {

            if (!instancePreserved)
            {
                if (content == null)
                {
                    content = new ContentManager(ScreenManager.Game.Services, "Content");
                }
                backToMenu = new InputAction(new Buttons[] { Buttons.Back }, null, true);
                gameFont = content.Load<SpriteFont>("gamefont");
                background_pattern = content.Load<Texture2D>("background");
                splogo = content.Load<Texture2D>("SPLOGO");
                

                creditpos = ScreenDimensions.Y / 2 - gameFont.MeasureString(creditsTitle).X / 2;
                TouchPanel.EnabledGestures =
                GestureType.Tap;


                // once the load has finished, we use ResetElapsedTime to tell the game's
                // timing mechanism that we have just finished a very long frame, and that
                // it should not try to catch up.
                ScreenManager.Game.ResetElapsedTime();
            }
        }


        public override void Deactivate()
        {
            base.Deactivate();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);
            creditpos--;

            if (creditpos <= -100)
            {
                
            }


        }
        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(GameTime gameTime, InputState input)
        {

            PlayerIndex player;

            foreach (GestureSample gesture in input.Gestures)
            {
                // If we have a tap
                if (gesture.GestureType == GestureType.Tap)
                {
                    LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(),
                                                        new PhoneMainMenuScreen());
                }
            }

            if (backToMenu.Evaluate(input, ControllingPlayer, out player))
            {
                LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(),
                                                        new PhoneMainMenuScreen());
            }

        }


        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {

            ScreenManager.SpriteBatch.Begin();
            string credits = "AK-DUO Studios\n\nProgrammed By \nKenny (Head)\nAdam\n\nArt by\nAdam(Head)\n\nSchool of"+
                "\nDigital Media\n&\nInfocomm Technology\n\nSound Effects from \nSoundBible.com\n\nFreesound.org\nqubodup";

            ScreenManager.SpriteBatch.Draw(background_pattern,
                new Vector2(0, 0),
                Color.White);
            
            ScreenManager.SpriteBatch.DrawString(ScreenManager.Font, credits, new Vector2(50, creditpos), Color.White);
            ScreenManager.SpriteBatch.Draw(splogo, new Vector2(50, (ScreenDimensions.Y / 2 + gameFont.MeasureString(credits).X)), Color.White);

            ScreenManager.SpriteBatch.End();

        }
    }
}
