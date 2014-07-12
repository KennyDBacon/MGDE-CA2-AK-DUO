#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameStateManagement;
#endregion

namespace GameStateManagementSample
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    class GameplayScreen : GameScreen
    {
        #region Fields

        ContentManager content;
        SpriteFont gameFont;

        Vector2 playerPosition = new Vector2(200, 700);
        Vector2 enemyPosition = new Vector2(100, 100);

        Random random = new Random();

        float pauseAlpha;

        InputAction pauseAction;

        // Sprites
        Texture2D road;
        Texture2D roadEnd;
        Texture2D playerCar;
        Texture2D enemyCar;

        // Sprites positions
        Vector2 roadOneVect = new Vector2(0, 0);
        Vector2 roadTwoVect = new Vector2(0, -800);
        Vector2 roadEndVect = new Vector2(0, -8000);

        Vector2[] enemyCarsVect = new Vector2[]{new Vector2(80,0),
                                                                           new Vector2(138,0),
                                                                           new Vector2(198,0),
                                                                           new Vector2(254,0)} ;

        // Special operation
        int enemyPositionCounter = -800;
        int enemyCarNum;
        int carWidth;
        int carHeight;

        string hitText = "Miss";
        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            enemyCarNum = enemyCarsVect.Length;
            carSpawner();

            pauseAction = new InputAction(
                new Buttons[] { Buttons.Start, Buttons.Back },
                new Keys[] { Keys.Escape },
                true);
        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void Activate(bool instancePreserved)
        {
            if (!instancePreserved)
            {
                if (content == null)
                    content = new ContentManager(ScreenManager.Game.Services, "Content");

                gameFont = content.Load<SpriteFont>("gamefont");

                // A real game would probably have more content than this sample, so
                // it would take longer to load. We simulate that by delaying for a
                // while, giving you a chance to admire the beautiful loading screen.
                Thread.Sleep(1000);

                // once the load has finished, we use ResetElapsedTime to tell the game's
                // timing mechanism that we have just finished a very long frame, and that
                // it should not try to catch up.
                ScreenManager.Game.ResetElapsedTime();
            }

#if WINDOWS_PHONE
            if (Microsoft.Phone.Shell.PhoneApplicationService.Current.State.ContainsKey("PlayerPosition"))
            {
                playerPosition = (Vector2)Microsoft.Phone.Shell.PhoneApplicationService.Current.State["PlayerPosition"];
                enemyPosition = (Vector2)Microsoft.Phone.Shell.PhoneApplicationService.Current.State["EnemyPosition"];
            }
#endif
        }


        public override void Deactivate()
        {
#if WINDOWS_PHONE
            Microsoft.Phone.Shell.PhoneApplicationService.Current.State["PlayerPosition"] = playerPosition;
            Microsoft.Phone.Shell.PhoneApplicationService.Current.State["EnemyPosition"] = enemyPosition;
#endif

            base.Deactivate();
        }


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void Unload()
        {
            content.Unload();

#if WINDOWS_PHONE
            Microsoft.Phone.Shell.PhoneApplicationService.Current.State.Remove("PlayerPosition");
            Microsoft.Phone.Shell.PhoneApplicationService.Current.State.Remove("EnemyPosition");
#endif
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);

            // Gradually fade in or out depending on whether we are covered by the pause screen.
            if (coveredByOtherScreen)
                pauseAlpha = Math.Min(pauseAlpha + 1f / 32, 1);
            else
                pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);

            if (IsActive)
            {
                // Apply some random jitter to make the enemy move around.
                //const float randomization = 10;

                //enemyPosition.X += (float)(random.NextDouble() - 0.5) * randomization;
                //enemyPosition.Y += (float)(random.NextDouble() - 0.5) * randomization;

                //// Apply a stabilizing force to stop the enemy moving off the screen.
                //Vector2 targetPosition = new Vector2(
                //    ScreenManager.GraphicsDevice.Viewport.Width / 2 - gameFont.MeasureString("Insert Gameplay Here").X / 2, 
                //    200);

                //enemyPosition = Vector2.Lerp(enemyPosition, targetPosition, 0.05f);

                if (ScreenManager.resetGame == true)
                {
                    ScreenManager.resetGame = false;
                    resetGame();
                }

                roadOneVect.Y += 50;
                roadTwoVect.Y += 50;
                roadEndVect.Y += 50;

                if (roadOneVect.Y >= 800)
                    roadOneVect.Y = -800;
                else if (roadTwoVect.Y >= 800)
                    roadTwoVect.Y = -800;

                if (roadEndVect.Y == -800)
                {
                    roadOneVect.Y = 0;
                    roadTwoVect.Y = -1600;
                }

                if (roadEndVect.Y == 800)
                {
                    // ScreenManager.AddScreen(new GameOverScreen(), PlayerIndex.One);

                    // For testing
                    roadOneVect.Y = 0;
                    roadTwoVect.Y = -800;
                    roadEndVect.Y = -8000;
                }

                for (int i = 0; i < enemyCarNum; i++)
                {
                    enemyCarsVect[i].Y += 20;
                    hitTest( i );
                }

                enemyPositionCounter += 20;

                if (enemyPositionCounter >= 800)
                {
                    carSpawner();
                    enemyPositionCounter = -800;
                }
            }
        }

        private void carSpawner()
        {
            Random rand = new Random();
            int luckyNum = rand.Next(0, 6);
            int randomY = rand.Next(-800, 0);

            // Anything over -1600 won't be spawned, currently at -2000
            // First car is on the left, last car is on the right, going from 0, 1, 2, 3
            switch (luckyNum)
            {
                    // First 3 cars spawn in preset position
                    // Last car spawn randomly
                case 0:
                    for (int i = 0; i < 3; i++)
                    {
                        enemyCarsVect[i].Y = 0;
                        enemyCarsVect[i].Y = -200 * ( i + 1 );
                    }
                    enemyCarsVect[3].Y = randomY;
                    break;
                    // First car spawn randomly
                    // Last 3 cars spawn in preset position
                case 1:
                    enemyCarsVect[0].Y = randomY;

                    for (int i = 1; i < 4; i++)
                    {
                        enemyCarsVect[i].Y = 0;
                        enemyCarsVect[i].Y = -300 * i;
                    }
                    break;
                    // First and last car spawn in same random position
                    // Middle 2 cars spawn in preset position
                case 2:
                    enemyCarsVect[0].Y = randomY;
                    enemyCarsVect[1].Y = -600;
                    enemyCarsVect[2].Y = -200;
                    enemyCarsVect[3].Y = randomY;
                    break;
                    // First 2 cars spawn in same random position
                    // Last 2 cars spawn in preset position
                case 3:
                    enemyCarsVect[0].Y = randomY;
                    enemyCarsVect[1].Y = randomY;
                    enemyCarsVect[2].Y = -250;
                    enemyCarsVect[3].Y = -550;
                    break;
                    // First 2 cars spawn in preset position
                    // Last 2 cars spawn in same random position
                case 4:
                    enemyCarsVect[0].Y = -550;
                    enemyCarsVect[1].Y = -250;
                    enemyCarsVect[2].Y = randomY;
                    enemyCarsVect[3].Y = randomY;
                    break;
                default:
                    enemyCarsVect[0].Y = -2000;
                    enemyCarsVect[1].Y = -200;
                    enemyCarsVect[2].Y = -250;
                    enemyCarsVect[3].Y = -150;
                    break;
            }
        }

        private void hitTest(int index)
        {
            if (enemyCarsVect[index].X <= (playerPosition.X + carWidth))
                if ((enemyCarsVect[index].X + carWidth) >= playerPosition.X)
                    if (enemyCarsVect[index].Y <= (playerPosition.Y + carHeight))
                        if ((enemyCarsVect[index].Y + carHeight) >= playerPosition.Y)
                        {
                            hitText = "Hit";
                        }
                        else
                        {
                            hitText = "Miss";
                        }
        }

        private void resetGame()
        {
            roadOneVect.Y = 0;
            roadTwoVect.Y = -800;
            roadEndVect.Y = -8000;

            carSpawner();
            enemyPositionCounter = -800;
        }

        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(GameTime gameTime, InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

            KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
            GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];

            // The game pauses either if the user presses the pause button, or if
            // they unplug the active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!
            bool gamePadDisconnected = !gamePadState.IsConnected &&
                                       input.GamePadWasConnected[playerIndex];

            PlayerIndex player;
            if (pauseAction.Evaluate(input, ControllingPlayer, out player) || gamePadDisconnected)
            {
#if WINDOWS_PHONE
                ScreenManager.AddScreen(new PhonePauseScreen(), ControllingPlayer);
#else
                ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
#endif
            }
            else
            {
                //// Otherwise move the player position.
                Vector2 movement = Vector2.Zero;

                //if (keyboardState.IsKeyDown(Keys.Left))
                //    movement.X--;

                //if (keyboardState.IsKeyDown(Keys.Right))
                //    movement.X++;

                //if (keyboardState.IsKeyDown(Keys.Up))
                //    movement.Y--;

                //if (keyboardState.IsKeyDown(Keys.Down))
                //    movement.Y++;

                //Vector2 thumbstick = gamePadState.ThumbSticks.Left;

                //movement.X += thumbstick.X;
                //movement.Y -= thumbstick.Y;

                if (input.TouchState.Count > 0)
                {
                    Vector2 touchPosition = input.TouchState[0].Position;
                    Vector2 distance = new Vector2 (((touchPosition.X - playerPosition.X) - 20), ((touchPosition.Y - playerPosition.Y) - 50));

                    if (playerPosition.X >= 75 && playerPosition.X + carWidth <= 250)
                    {
                        movement = distance / 5;
                    }
                    else if (touchPosition.X >= 95 && touchPosition.X <= 230)
                    {
                        movement = distance / 5;
                    }
                }

                playerPosition += movement;
            }
        }

        public Vector2 getDirection(Vector2 touchPosition)
        {
            Vector2 direction;

            if (touchPosition.X <= 75)
            {
                direction = new Vector2(76, touchPosition.Y) - playerPosition;
            }
            else if (touchPosition.X >= 300)
            {
                direction = new Vector2(299, touchPosition.Y) - playerPosition;
            }
            else
            {
                direction = touchPosition - playerPosition;
            }

            direction.Normalize();

            return direction;
        }

        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // This game has a blue background. Why? Because!
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               Color.CornflowerBlue, 0, 0);

            // Our player and enemy are both actually just text strings.
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            road = content.Load<Texture2D>("road");
            roadEnd = content.Load<Texture2D>("roadEnd");
            enemyCar = content.Load<Texture2D>("enemyCar");
            playerCar = content.Load<Texture2D>("playerCar");

            // Get width and height of car
            carWidth = playerCar.Width;
            carHeight = playerCar.Height;

            spriteBatch.Begin();
            //spriteBatch.Begin();
            //spriteBatch.DrawString(gameFont, "// TODO", playerPosition, Color.Green);

            //spriteBatch.DrawString(gameFont, "Insert Gameplay Here",
            //                       enemyPosition, Color.DarkRed);

            spriteBatch.Draw(road, roadOneVect, Color.White);
            spriteBatch.Draw(road, roadTwoVect, Color.White);
            spriteBatch.Draw(roadEnd, roadEndVect, Color.White);

            for (int i = 0; i < enemyCarNum; i++)
            {
                spriteBatch.Draw(enemyCar, enemyCarsVect[i], Color.White);
            }

            spriteBatch.DrawString(gameFont, hitText, new Vector2(400, 100), Color.White);

            spriteBatch.Draw(playerCar, playerPosition, Color.White);

            spriteBatch.End();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);

                ScreenManager.FadeBackBufferToBlack(alpha);
            }
        }


        #endregion
    }
}
