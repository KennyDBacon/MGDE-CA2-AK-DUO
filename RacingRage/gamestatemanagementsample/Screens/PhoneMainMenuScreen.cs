#region File Description
//-----------------------------------------------------------------------------
// PhoneMainMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using System.IO;
using System.IO.IsolatedStorage;
using AdDuplex.Xna;

namespace GameStateManagementSample
{
    class PhoneMainMenuScreen : PhoneMenuScreen
    {
        public PhoneMainMenuScreen()
            : base("Main Menu")
        {
            // Create a button to start the game
            Button playButton = new Button("Play");
            playButton.Tapped += playButton_Tapped;
            MenuButtons.Add(playButton);

            BooleanButton musicButton = new BooleanButton("Music", true);
            musicButton.Tapped += musicButton_Tapped;
            MenuButtons.Add(musicButton);

            BooleanButton sfxButton = new BooleanButton("Sound", true);
            sfxButton.Tapped += sfxButton_Tapped;
            MenuButtons.Add(sfxButton);

            BooleanButton accelerometerButton = new BooleanButton("Accelerometer", true);
            accelerometerButton.Tapped += accelerometerButton_Tapped;
            MenuButtons.Add(accelerometerButton);

            Button creditsButton = new Button("Credits");
            creditsButton.Tapped += creditsButton_Tapped;
            MenuButtons.Add(creditsButton);
        }

        public override void Activate(bool instancePreserved)
        {
            //IsolatedStorageFile local = IsolatedStorageFile.GetUserStoreForApplication();

            //if (!local.DirectoryExists("DataFolder"))
            //    local.CreateDirectory("DataFolder");

            //using (var isoFileStream =
            //new System.IO.IsolatedStorage.IsolatedStorageFileStream("DataFolder\\DataFile.txt",System.IO.FileMode.OpenOrCreate, local))
            //{
            //    using (var isoFileWriter = new System.IO.StreamWriter(isoFileStream))
            //    {
                    
            //    }
            //}

            checkButton();
            if (ScreenManager.enableMusic == true)
            {
                MediaPlayer.Play(ScreenManager.cheesymusic);
            }

            base.Activate(instancePreserved);
        }

        void checkButton()
        {
            for (int i = 0; i < MenuButtons.Count; i++)
            {
                Button b = MenuButtons[i];

                if(b.Text.Contains("Music"))
                {
                    if (ScreenManager.enableMusic == true)
                    {
                        b.Text = "Music: On";
                    }
                    else
                    {
                        b.Text = "Music: Off";
                    }
                }
                else if (b.Text.Contains("Sound"))
                {
                    if (ScreenManager.enableSoundEffect == true)
                        b.Text = "Sound: On";
                    else
                        b.Text = "Sound: Off";
                }
                else if (b.Text.Contains("Accelerometer"))
                {
                    if (ScreenManager.enableAccelerometer == true)
                        b.Text = "Accelerometer: On";
                    else
                        b.Text = "Accelerometer: Off";
                }
            }
        }

        void playButton_Tapped(object sender, EventArgs e)
        {
            LoadingScreen.Load(ScreenManager, true, PlayerIndex.One, new GameplayScreen());
        }

        void creditsButton_Tapped(object sender, EventArgs e)
        {
            LoadingScreen.Load(ScreenManager, true, PlayerIndex.One, new CreditsScreen());
        }

        void musicButton_Tapped(object sender, EventArgs e)
        {
            BooleanButton button = sender as BooleanButton;
            ScreenManager.enableMusic = !ScreenManager.enableMusic;

            if (ScreenManager.enableMusic == false)
            {
                MediaPlayer.Pause();
            }
            else 
            {
                MediaPlayer.Resume();
            }
            checkButton();
        }

        void sfxButton_Tapped(object sender, EventArgs e)
        {
            BooleanButton button = sender as BooleanButton;
            ScreenManager.enableSoundEffect = !ScreenManager.enableSoundEffect;
            checkButton();
        }

        void accelerometerButton_Tapped(object sender, EventArgs e)
        {
            BooleanButton button = sender as BooleanButton;
            ScreenManager.enableAccelerometer = !ScreenManager.enableAccelerometer;
            checkButton();
        }

        protected override void OnCancel()
        {
            ScreenManager.Game.Exit();
            base.OnCancel();
        }
    }
}
