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
using System.Windows.Controls;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Phone.Tasks;
using System.IO.IsolatedStorage;
using System.IO;
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
        SpriteBatch spriteBatch;

        Vector2 playerPosition = new Vector2(200, 400);
        Vector2 enemyPosition = new Vector2(100, 100);

        float pauseAlpha;

        InputAction pauseAction;

        Vector2 distance;

        // Sprites
        Texture2D road;
        Texture2D roadEnd;
        Texture2D playerCarNotDestroy;
        Texture2D playerCar;
        Texture2D playerCarDestroy;
        Texture2D enemyCar;
        Texture2D sideUI;
        Texture2D fuelCan;
        Texture2D trollFaceTex;

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
        Vector2 TrollFacePos = new Vector2(-100, -100);
        

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

        float roadLength;
        float endingAnimationDeccelaration = 20;

        float fuelTransparency = 0.0f;
        int fuelPerTurn = 3;
        bool fuelExist = false;

        float timer = 1.0f;
        float UITimer = 0.2f;
        float gameTimer = 60.0f;
        int fuelInLane = 0;
        int fuelCounter = 100;
        int gearNumEngine = 1;
        string fuel;

        string readyCrashText = "Ready?";

        // Smaato Ad
        Texture2D textureSomaAd;
        Vector2 somaAdPosition = new Vector2(0, 720);
        Vector2 somaAdSize = new Vector2(480, 80);
        string currentAdImageFileName = "";
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
            roadLength = Math.Abs(480 / (roadEndVect.Y / 50));
            //carSpawner();

            pauseAction = new InputAction(
                new Buttons[] { Buttons.Start, Buttons.Back },
                new Keys[] { Keys.Escape },
                true);

            TouchPanel.EnabledGestures = GestureType.Tap;
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

                spriteBatch = new SpriteBatch(ScreenManager.GraphicsDevice);

                textureSomaAd = content.Load<Texture2D>("sampleAd");

                // Load fonts
                gameFont = content.Load<SpriteFont>("gamefont");
                smallerGameFont = content.Load<SpriteFont>("smallerGameFont");
                //

                // Load textures
                road = content.Load<Texture2D>("road");
                roadEnd = content.Load<Texture2D>("roadEnd");
                sideUI = content.Load<Texture2D>("ui");
                enemyCar = content.Load<Texture2D>("enemyCar");
                playerCarNotDestroy = content.Load<Texture2D>("playerCar");
                playerCar = content.Load<Texture2D>("playerCar");
                playerCarDestroy = content.Load<Texture2D>("playerCarDestroy");
                fuelCan = content.Load<Texture2D>("fuelCan");
                trollFaceTex = content.Load<Texture2D>("Troll_Face");

                // Get width and height of car
                carWidth = playerCar.Width;
                carHeight = playerCar.Height;
                //carSpawner();

                fuelRect = new Rectangle(0, 0, fuelCan.Width, fuelCan.Height);

                
                
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

                resetGame();
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
            ScreenManager.getSoma.Dispose();
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

            #region SomaAd
            // if the ad panel was tapped, show the click thru ad
            if (ScreenManager.enableAd == true)
            {
                while (TouchPanel.IsGestureAvailable)
                {
                    GestureSample gestureSample = TouchPanel.ReadGesture();

                    if (gestureSample.GestureType == GestureType.Tap)
                    {
                        Vector2 touchPosition = gestureSample.Position;

                        if (touchPosition.X >= 0 &&
                            touchPosition.X < somaAdSize.X &&
                            touchPosition.Y >= somaAdPosition.Y &&
                            touchPosition.Y < (somaAdPosition.Y + somaAdSize.Y))
                        {
                            WebBrowserTask webBrowserTask = new WebBrowserTask();
                            webBrowserTask.Uri = new Uri(ScreenManager.getSoma.Uri);
                            webBrowserTask.Show();
                        }
                    }
                }
            }

            // if there is a new ad, get it from Isolated Storage and  show it
            if (ScreenManager.getSoma.Status == "success" && ScreenManager.getSoma.AdImageFileName != null && ScreenManager.getSoma.ImageOK)
            {
                try
                {
                    if (currentAdImageFileName != ScreenManager.getSoma.AdImageFileName)
                    {
                        currentAdImageFileName = ScreenManager.getSoma.AdImageFileName;
                        IsolatedStorageFile myIsoStore = IsolatedStorageFile.GetUserStoreForApplication();
                        IsolatedStorageFileStream myAd = new IsolatedStorageFileStream(ScreenManager.getSoma.AdImageFileName, FileMode.Open, myIsoStore);
                        textureSomaAd = Texture2D.FromStream(ScreenManager.GraphicsDevice, myAd);

                        myAd.Close();
                        myAd.Dispose();
                        myIsoStore.Dispose();
                    }
                }
                catch (IsolatedStorageException ise)
                {
                    string message = ise.Message;
                }
            }
            #endregion

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
                    fuelExist = true;
                    resetGame();
                }

                if (ScreenManager.playerReady == true)
                {

                    TrollFacePos = new Vector2(-100, -100);
                    #region Accelerometer
                    if (ScreenManager.enableAccelerometer == true && ScreenManager.enableAnimation == false)
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
                    #endregion

                    #region Engine Sound
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
                    #endregion
                    playerCar = playerCarNotDestroy;
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
                    //UITimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                    //if (UITimer != 0)
                    //{
                    //    UITimer = 0.1f;
                    //    UICarVect.Y -= 1;
                    //    if (UICarVect.Y <= 115)
                    //    {
                    //        roadEndVect.Y = -800;
                    //        clearCars();
                    //    }
                    //}

                    //UICarVect.Y -= 1;
                    //if (UICarVect.Y <= 100)
                    //{
                    //    clearCars();
                    //}

                    //if (roadEndVect.Y == 300)
                    //{
                    //    // Win Animation
                    //    ScreenManager.enableAnimation = true;
                    //    if (playerPosition.Y >= -100)
                    //    {
                    //        playerPosition.Y += 5;
                    //    }
                    //    else
                    //    {
                    //        ScreenManager.playerReady = false;
                    //        EngineInstance.Pause();
                    //        WinMusicInstance.Play();
                    //        ScreenManager.AddScreen(new WinScreen(), PlayerIndex.One);
                    //    }
                    //}
                    if (fuelCounter <= 0)
                    {
                        ScreenManager.AddScreen(new GameOverScreen(), PlayerIndex.One);
                    }

                    gameTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                    //
                    // Road Movement
                    //
                    if (ScreenManager.enableAnimation == false)
                    {
                        roadOneVect.Y += 50;
                        roadTwoVect.Y += 50;
                        roadEndVect.Y += 50;

                        UICarVect.Y -= roadLength;

                        // Fuel movement
                        fuelVect.Y += 20;
                        fuelRect.Y += 20;

                        // Enemy car movement
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
                                fuelPerTurn = 3;
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

                    // Looping of two road sprite
                    if (roadOneVect.Y >= 800)
                        roadOneVect.Y = -800;
                    else if (roadTwoVect.Y >= 800)
                        roadTwoVect.Y = -800;

                    if (roadEndVect.Y == -1600)
                    {
                        clearCars();
                    }

                    // When ending reached, put ending sprite in between two road sprite
                    if (roadEndVect.Y == -800)
                    {
                        roadOneVect.Y = 0;
                        roadTwoVect.Y = -1600;
                        clearCars();
                    }

                    // Actual end
                    if (roadEndVect.Y == 150)
                    {
                        if (ScreenManager.enableAnimation == false)
                        {
                            EngineInstance.Pause();
                            WinMusicInstance.Play();
                        }

                        ScreenManager.enableAnimation = true;

                        if (endingAnimationDeccelaration > 0)
                        {
                            playerPosition.Y -= endingAnimationDeccelaration;
                            endingAnimationDeccelaration--;
                        }
                        else
                        {
                            ScreenManager.playerReady = false;
                            ScreenManager.AddScreen(new WinScreen(), PlayerIndex.One);
                        }
                    }
                    //
                    //
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
            fuelVect.Y = -3200;
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
                playerCar = playerCarDestroy;
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
            playerPosition = new Vector2(200, 400);
            Point position = new Point(200, 400);
            playerRect = new Rectangle(Convert.ToInt32(playerPosition.X + 4), Convert.ToInt32(playerPosition.Y + 4), carWidth - 8, carHeight - 8);
            playerRect.Location = position;
            //UICarVect.Y = 570;
            roadOneVect.Y = 0;
            roadTwoVect.Y = -800;
            //roadEndVect.Y = -80000;

            if (fuelExist == true)
            {
                fuelExist = false;
                fuelPerTurn = 3;
            }
            else
            {
                fuelPerTurn--;
            }

            readyCrashText = "Ready?";

            carSpawner();
            enemyPositionCounter = -800;
            endingAnimationDeccelaration = 20;
            
        }

        //Used on every new round
        private void resetGame(int timePause)
        {
            ScreenManager.playerReady = false;

            EngineInstance.Pause();
            EngineInstance.Pitch = 0;
            gearNumEngine = 1;
            ScreenManager.engineSoundBool = false;

            if (ScreenManager.enableSoundEffect)
            {
                carCrashInstance.Play();
            }
            TrollFacePos = new Vector2(10, 10);
            playerCar = playerCarDestroy;
            
            Thread.Sleep(timePause);
            
            fuelCounter -= 20;

            if (fuelCounter < 0)
            {
                fuelCounter = 0;
                ScreenManager.AddScreen(new GameOverScreen(), PlayerIndex.One);
                
            }
        }
        
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
                if (ScreenManager.enableAccelerometer == false && ScreenManager.enableAnimation == false)
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

        public override void Draw(GameTime gameTime)
        {
            // This game has a blue background. Why? Because!
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               Color.CornflowerBlue, 0, 0);

            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;

            fuel = fuelCounter + "%";

            spriteBatch.Begin();

            spriteBatch.Draw(road, roadOneVect, Color.White);
            spriteBatch.Draw(road, roadTwoVect, Color.White);
            spriteBatch.Draw(roadEnd, roadEndVect, Color.White);

            for (int i = 0; i < enemyCarNum; i++)
            {
                spriteBatch.Draw(enemyCar, enemyCarsVect[i], Color.White);
            }
            
            spriteBatch.Draw(fuelCan, fuelRect, Color.White * fuelTransparency);

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
                spriteBatch.Begin();
                spriteBatch.Draw(trollFaceTex, TrollFacePos, null, Color.White, 0.0f, new Vector2(0, 0), 0.1f, SpriteEffects.None, 0.0f);
                spriteBatch.Draw(textureSomaAd, new Rectangle(0, 720, 480, 80), Color.White);
                spriteBatch.End();
            }
        }

        #endregion
    }
}
