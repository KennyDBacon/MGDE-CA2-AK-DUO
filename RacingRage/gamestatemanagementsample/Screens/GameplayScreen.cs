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
using Microsoft.Xna.Framework.Audio;
using Microsoft.Devices;
using Microsoft.Xna.Framework.Media;
using AdDuplex.Xna;
using SOMAWP7;
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
        SpriteFont smallerGameFont;

        Vector2 playerPosition = new Vector2(200, 400);
        Vector2 enemyPosition = new Vector2(100, 100);

        float pauseAlpha;

        InputAction pauseAction;

        Vector2 distance;

        // Sprites
        Texture2D road;
        Texture2D roadEnd;
        Texture2D playerCar;
        Texture2D enemyCar;
        Texture2D sideUI;
        Texture2D fuelCan;

        // Sprites positions
        Vector2 roadOneVect = new Vector2(0, 0);
        Vector2 roadTwoVect = new Vector2(0, -800);
        Vector2 roadEndVect = new Vector2(0, -80000);

        Vector2 UICarVect = new Vector2(416, 570);

        Vector2 fuelVect = new Vector2(0, 0);

        Vector2[] enemyCarsVect = new Vector2[]{new Vector2(80,0),
                                                new Vector2(138,0),
                                                new Vector2(198,0),
                                                new Vector2(254,0)};

        Rectangle fuelRect = new Rectangle(0, 0, 0, 0);
        Rectangle playerRect = new Rectangle(0, 0, 0, 0);
        Rectangle[] enemyRect = new Rectangle[4];

        SoundEffect engineLoop;
        SoundEffectInstance EngineInstance;
        SoundEffect carCrash;
        SoundEffectInstance carCrashInstance;
        SoundEffect WinMusic;
        SoundEffectInstance WinMusicInstance;
        SoundEffect PickupSound;
        SoundEffectInstance PickupInstance;

        VibrateController vibration = VibrateController.Default;

        // Special operation
        int enemyPositionCounter = -800;
        int enemyCarNum;
        int carWidth;
        int carHeight;

        float fuelTransparency = 0.0f;
        int fuelPerTurn = 5;
        bool fuelExist = false;

        float timer = 1.0f;
        float UITimer = 0.2f;
        float gameTimer = 60.0f;
        int fuelInLane = 0;
        int fuelCounter = 100;
        int gearNumEngine = 1;
        string fuel;

        string readyCrashText = "Ready?";
        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            // Spawn car at the start of the game
            enemyCarNum = enemyCarsVect.Length;
            //carSpawner();

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
            if (ScreenManager.enableMusic == true)
            {
                MediaPlayer.Stop();
            }

            if (!instancePreserved)
            {
                if (content == null)
                    content = new ContentManager(ScreenManager.Game.Services, "Content");

                // Load fonts
                gameFont = content.Load<SpriteFont>("gamefont");
                smallerGameFont = content.Load<SpriteFont>("smallerGameFont");
                //

                // Load textures
                road = content.Load<Texture2D>("road");
                roadEnd = content.Load<Texture2D>("roadEnd");
                sideUI = content.Load<Texture2D>("ui");
                enemyCar = content.Load<Texture2D>("enemyCar");
                playerCar = content.Load<Texture2D>("playerCar");
                fuelCan = content.Load<Texture2D>("fuelCan");
                //

                // Get width and height of car
                carWidth = playerCar.Width;
                carHeight = playerCar.Height;
                //carSpawner();

                fuelRect = new Rectangle(0, 0, 0, 0);

                playerRect = new Rectangle(Convert.ToInt32(playerPosition.X + 4), Convert.ToInt32(playerPosition.Y + 4), carWidth - 8, carHeight - 8);
                
                for (int i = 0; i < enemyCarsVect.Length; i++)
                {
                    enemyRect[i] = new Rectangle(Convert.ToInt32(enemyCarsVect[i].X + 4), 0, carWidth - 8, carHeight - 8);
                }

                // Load all sound effects
                engineLoop = content.Load<SoundEffect>("engineLoop");
                EngineInstance = engineLoop.CreateInstance();
                EngineInstance.Volume = 0.5f;
                EngineInstance.IsLooped = true;

                carCrash = content.Load<SoundEffect>("Car Crash");
                carCrashInstance = carCrash.CreateInstance();
                carCrashInstance.Volume = 0.5f;
                carCrashInstance.IsLooped = false;

                WinMusic = content.Load<SoundEffect>("WinMusic");
                WinMusicInstance = WinMusic.CreateInstance();

                PickupSound = content.Load<SoundEffect>("Pickup-Sound");
                PickupInstance = PickupSound.CreateInstance();

                // Reset the elapsed time when this screen is activated
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

            ScreenManager.getAd.Update(gameTime);

            // Gradually fade in or out depending on whether we are covered by the pause screen.
            if (coveredByOtherScreen)
                pauseAlpha = Math.Min(pauseAlpha + 1f / 32, 1);
            else
                pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);

            if (IsActive)
            {
                // Prompt player to start
                if (ScreenManager.playerReady == false)
                {
                    ScreenManager.enableAd = true;

                    ScreenManager.AddScreen(new ReadyScreen(readyCrashText), PlayerIndex.One);
                    readyCrashText = "Ready?";
                    resetGame();
                }

                // Reset game every new game
                if (ScreenManager.resetGame == true)
                {
                    ScreenManager.resetGame = false;
                    resetGame();
                }

                if (ScreenManager.playerReady == true)
                {
                    //
                    // Accelerometer
                    //
                    if (ScreenManager.enableAccelerometer == true)
                    {
                        //poll the acceleration value
                        Vector3 acceleration = Accelerometer.GetState().Acceleration;

                        if (playerPosition.X >= 72 && playerPosition.X <= 300)
                        {
                            // Sudden change of direction
                            if (distance.X < 0 && acceleration.X > 0)
                            {
                                distance.X = 0;
                            }
                            else if (distance.X > 0 && acceleration.X < 0)
                            {
                                distance.X = 0;
                            }

                            distance.X = acceleration.X * 25.0f;
                            distance.Y = 0;

                            playerPosition += distance;
                            playerRect.X += Convert.ToInt32(distance.X);

                            if (playerPosition.X <= 72)
                            {
                                playerPosition.X = 72;
                                distance.X = 0;
                            }
                            else if (playerPosition.X + carWidth >= 300)
                            {
                                playerPosition.X = 300 - carWidth;
                                distance.X = 0;
                            }
                        }
                    }
                    //
                    //

                    //
                    // Engine Vroom Vroom
                    //
                    if (ScreenManager.enableSoundEffect == true)
                    {
                        if (ScreenManager.engineSoundBool == true)
                        {
                            if (EngineInstance.State != SoundState.Playing)
                            {
                                EngineInstance.Play();
                            }

                        }
                        else
                        {
                            EngineInstance.Pause();
                            EngineInstance.Pitch = 0;
                        }

                        switch (gearNumEngine)
                        {
                            case 1: if (EngineInstance.Pitch < 0.8)
                                {
                                    EngineInstance.Pitch += 0.01f;
                                }
                                else
                                {
                                    EngineInstance.Pitch = -0.1f;
                                    gearNumEngine++;
                                }
                                break;
                            case 2: if (EngineInstance.Pitch < 0.7)
                                {
                                    EngineInstance.Pitch += 0.005f;
                                }
                                else
                                {
                                    EngineInstance.Pitch = -0.2f;
                                    gearNumEngine++;
                                }
                                break;
                            case 3: if (EngineInstance.Pitch < 0.6)
                                {
                                    EngineInstance.Pitch += 0.001f;
                                }
                                break;
                        }
                    }
                    timer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (timer <= 0)
                    {
                        timer = 1.0f;
                        fuelCounter -= 2;

                        if (fuelCounter <= 0)
                        {
                            EngineInstance.Pause();
                            ScreenManager.engineSoundBool = false;
                            ScreenManager.AddScreen(new GameOverScreen(), PlayerIndex.One);
                        }
                    }
                    //
                    //

                    //Win Statement
                    //If goes through successfully, the game will end
                    UITimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (UITimer <= 0)
                    {
                        UITimer = 0.1f;
                        UICarVect.Y -= 1;
                        if (UICarVect.Y == 115)
                        {
                            roadEndVect.Y = -800;
                            clearCars();
                        }
                    }

                    if (roadEndVect.Y == 300)
                    {
                        ScreenManager.playerReady = false;
                        EngineInstance.Pause();
                        WinMusicInstance.Play();
                        ScreenManager.AddScreen(new WinScreen(), PlayerIndex.One);
                    }
                    if (fuelCounter <= 0)
                    {
                        ScreenManager.AddScreen(new GameOverScreen(), PlayerIndex.One);
                    }

                    gameTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                    //
                    // Road Movement
                    //
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
                        // For testing
                        roadOneVect.Y = 0;
                        roadTwoVect.Y = -800;
                        roadEndVect.Y = -80000;
                    }
                    //
                    //

                    // Fuel movement
                    fuelVect.Y += 20;

                    // Car movement
                    // Car collision detection
                    for (int i = 0; i < enemyCarNum; i++)
                    {
                        enemyCarsVect[i].Y += 20;
                        enemyRect[i].Y += 20;
                    }

                    // Respawn car when they leave bottom of the screen
                    enemyPositionCounter += 20;

                    if (enemyPositionCounter >= 1600)
                    {
                        fuelPerTurn--;

                        if (fuelPerTurn < 1)
                        {
                            fuelPerTurn = 5;
                            fuelTransparency = 1.0f;
                            fuelExist = true;
                        }
                        else
                        {
                            fuelTransparency = 0.0f;
                            fuelExist = false;
                        }

                        carSpawner();
                        enemyPositionCounter = -800;
                    }

                    for (int i = 0; i < enemyCarNum; i++)
                    {
                        hitTest(i);

                        if (fuelExist == true)
                        {
                            fuelPickUp();
                        }
                    }
                }
            }
        }

        private void carSpawner()
        {
            Random rand = new Random();
            int luckyNum = rand.Next(0, 6);
            int randomY = rand.Next(-800, 0);

            // Anything over -1600 won't be spawned, currently at -3200
            // First car is on the left, last car is on the right, going from 0, 1, 2, 3
            switch (luckyNum)
            {
                    // First 3 cars spawn in preset position
                    // Last car spawn randomly
                case 0:
                    for (int i = 0; i < 3; i++)
                    {
                        enemyCarsVect[i].Y = 0;
                        enemyCarsVect[i].Y = -350 * ( i + 1 );
                    }
                    enemyCarsVect[3].Y = randomY;

                    fuelInLane = 0;
                    setFuelVector(fuelInLane);
                    break;
                    // First car spawn randomly
                    // Last 3 cars spawn in preset position
                case 1:
                    enemyCarsVect[0].Y = randomY;

                    for (int i = 1; i < 4; i++)
                    {
                        enemyCarsVect[i].Y = 0;
                        enemyCarsVect[i].Y = -350 * i;
                    }

                    fuelInLane = 1;
                    setFuelVector(fuelInLane);
                    break;
                    // First and last car spawn in same random position
                    // Middle 2 cars spawn in preset position
                case 2:
                    enemyCarsVect[0].Y = randomY;
                    enemyCarsVect[1].Y = -1000;
                    enemyCarsVect[2].Y = -400;
                    enemyCarsVect[3].Y = randomY;

                    fuelInLane = 3;
                    setFuelVector(fuelInLane);
                    break;
                    // First 2 cars spawn in same random position
                    // Last 2 cars spawn in preset position
                case 3:
                    enemyCarsVect[0].Y = randomY;
                    enemyCarsVect[1].Y = randomY;
                    enemyCarsVect[2].Y = -450;
                    enemyCarsVect[3].Y = -850;

                    fuelInLane = 1;
                    setFuelVector(fuelInLane);
                    break;
                    // First 2 cars spawn in preset position
                    // Last 2 cars spawn in same random position
                case 4:
                    enemyCarsVect[0].Y = -850;
                    enemyCarsVect[1].Y = -450;
                    enemyCarsVect[2].Y = randomY;
                    enemyCarsVect[3].Y = randomY;

                    fuelInLane = 2;
                    setFuelVector(fuelInLane);
                    break;
                default:
                    enemyCarsVect[0].Y = -3200;
                    enemyCarsVect[1].Y = -450;
                    enemyCarsVect[2].Y = -380;
                    enemyCarsVect[3].Y = -400;

                    fuelInLane = 2;
                    setFuelVector(fuelInLane);
                    break;
            }

            for (int i = 0; i < enemyCarsVect.Length; i++)
            {
                enemyRect[i] = new Rectangle(Convert.ToInt32(enemyCarsVect[i].X + 4), Convert.ToInt32(enemyCarsVect[i].Y + 4), carWidth - 8, carHeight - 8);
            }
        }

        private void clearCars() 
        {
            enemyCarsVect[0].Y = -3200;
            enemyCarsVect[1].Y = -3200;
            enemyCarsVect[2].Y = -3200;
            enemyCarsVect[3].Y = -3200;
            fuelVect.Y = 2000;
        }

        private void setFuelVector(int index)
        {
            fuelVect.X = enemyCarsVect[index].X;
            fuelRect.X = Convert.ToInt32(enemyCarsVect[index].X);

            if (enemyCarsVect[index].Y - 200 >= -800)
            {
                fuelVect.Y = enemyCarsVect[index].Y + 200;
                fuelRect.Y = Convert.ToInt32(enemyCarsVect[index].Y + 200);
            }
            else
            {
                fuelVect.Y = enemyCarsVect[index].Y - 200;
                fuelRect.Y = Convert.ToInt32(enemyCarsVect[index].Y - 200);
            }
        }

        private void fuelPickUp()
        {
            if (playerRect.Intersects(fuelRect))
            {
                PickupInstance.Play();
                if (fuelCounter + 25 < 100)
                {
                    fuelCounter += 25;
                }
                else
                {
                    fuelCounter = 100;
                }

                fuelExist = false;
                fuelTransparency = 0.0f;
            }

            //if (fuelVect.X <= (playerPosition.X + carWidth))
            //    if ((fuelVect.X + carWidth) >= playerPosition.X)
            //        if (fuelVect.Y <= (playerPosition.Y + carHeight))
            //            if ((fuelVect.Y + carHeight) >= playerPosition.Y)
            //            {
            //                PickupInstance.Play();
            //                if (fuelCounter + 30 < 100)
            //                {
            //                    fuelCounter += 30;
            //                }
            //                else
            //                {
            //                    fuelCounter = 100;
            //                }

            //                fuelExist = false;
            //                fuelTransparency = 0.0f;
            //            }
        }

        // Detect collion of player car with enemy cars
        private void hitTest(int index)
        {
            if (playerRect.Intersects(enemyRect[index]))
            {
                vibration.Start(TimeSpan.FromMilliseconds(500));
                resetGame(3000);
            }
            //if (enemyCarsVect[index].X <= (playerPosition.X + carWidth))
            //    if ((enemyCarsVect[index].X + carWidth) >= playerPosition.X)
            //        if (enemyCarsVect[index].Y <= (playerPosition.Y + carHeight))
            //            if ((enemyCarsVect[index].Y + carHeight) >= playerPosition.Y)
            //            {
            //                vibration.Start(TimeSpan.FromMilliseconds(500));
            //                resetGame(3000);
            //            }
        }

        private void resetGame()
        {
            ScreenManager.playerReady = false;

            roadOneVect.Y = 0;
            roadTwoVect.Y = -800;
            roadEndVect.Y = -80000;

            readyCrashText = "Ready?";

            carSpawner();
            enemyPositionCounter = -800;
        }

        //Used on every new round
        private void resetGame(int timePause)
        {
            ScreenManager.playerReady = false;
            EngineInstance.Pause();
            EngineInstance.Pitch = 0;
            gearNumEngine = 1;
            ScreenManager.engineSoundBool = false;
            roadOneVect.Y = 0;
            roadTwoVect.Y = -800;
            roadEndVect.Y = -80000;

            carSpawner();
            enemyPositionCounter = -800;

            readyCrashText = "Crash!";

            if (ScreenManager.enableSoundEffect)
            {
                carCrashInstance.Play();
            }
            Thread.Sleep(timePause);

            fuelCounter -= 20;

            if (fuelCounter < 0)
            {
                fuelCounter = 0;
                ScreenManager.AddScreen(new GameOverScreen(), PlayerIndex.One);
            }
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
                EngineInstance.Pause();
                ScreenManager.AddScreen(new PhonePauseScreen(), ControllingPlayer);
#else
                ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
#endif
            }
            else
            {
                // If Accelerometer is off, touch input will be used
                if (ScreenManager.enableAccelerometer == false)
                {
                    //// Otherwise move the player position.
                    Vector2 movement = Vector2.Zero;

                    if (input.TouchState.Count > 0)
                    {
                        Vector2 touchPosition = input.TouchState[0].Position;
                        //distance = new Vector2(((touchPosition.X - playerPosition.X) - 20), ((touchPosition.Y - playerPosition.Y) - 50));
                        distance = new Vector2(((touchPosition.X - playerPosition.X) - 20), 0);

                        movement = distance / 8;
                    }

                    playerPosition += movement;
                    playerRect.X += Convert.ToInt32(movement.X);

                    // Prevent player from moving off the road
                    if (playerPosition.X <= 72)
                    {
                        playerPosition.X = 72;
                        distance.X = 0;
                    }
                    else if (playerPosition.X + carWidth >= 300)
                    {
                        playerPosition.X = 300 - carWidth;
                        distance.X = 0;
                    }
                    //
                }
            }
        }

        // Obtain the distance for player car to travel
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

            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;

            fuel = fuelCounter + "%";

            spriteBatch.Begin();

            spriteBatch.Draw(road, roadOneVect, Color.White);
            spriteBatch.Draw(road, roadTwoVect, Color.White);
            spriteBatch.Draw(roadEnd, roadEndVect, Color.White);

            for (int i = 0; i < enemyCarNum; i++)
            {
                spriteBatch.Draw(enemyCar, enemyCarsVect[i], Color.White);

                if (fuelInLane == i)
                {
                    spriteBatch.Draw(fuelCan, fuelRect, Color.White * fuelTransparency);
                }
            }

            spriteBatch.Draw(sideUI, new Vector2(390, 0), Color.White);
            spriteBatch.DrawString(smallerGameFont, fuel, new Vector2(475 - smallerGameFont.MeasureString(fuel).X, 680), Color.Black);

            spriteBatch.Draw(playerCar, playerPosition, Color.White);
            spriteBatch.Draw(playerCar, UICarVect, Color.White);

            //spriteBatch.Draw(playerCar, new Vector2(playerPosition.X + 4, playerPosition.Y + 4), playerRect, Color.White);

            spriteBatch.End();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);

                ScreenManager.FadeBackBufferToBlack(alpha);
            }

            if (ScreenManager.enableAd == true)
            {
                ScreenManager.getAd.Draw(spriteBatch, new Vector2(0, viewport.Height - 75));
            }
        }

        #endregion
    }
}
