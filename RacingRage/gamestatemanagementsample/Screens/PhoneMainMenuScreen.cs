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
            : base("Racing Rage")
        {
            // Create a button to start the game
            Button playButton = new Button("Play");
            playButton.Tapped += playButton_Tapped;
            MenuButtons.Add(playButton);
            playButton.Size = new Vector2(300, 100);

            BooleanButton musicButton = new BooleanButton("Music", true);
            musicButton.Tapped += musicButton_Tapped;
            MenuButtons.Add(musicButton);

            BooleanButton sfxButton = new BooleanButton("Sound", true);
            sfxButton.Tapped += sfxButton_Tapped;
            MenuButtons.Add(sfxButton);

            Button controlButton = new Button("Control: Gyro");
            controlButton.Tapped += controlButton_Tapped;
            MenuButtons.Add(controlButton);

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
                else if (b.Text.Contains("Control"))
                {
                    if (ScreenManager.enableAccelerometer == true)
                        b.Text = "Control: Gyro";
                    else
                        b.Text = "Control: Touch";
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
                MediaPlayer.Stop();
            }
            else 
            {
                MediaPlayer.Play(ScreenManager.cheesymusic);
            }
            checkButton();
        }

        void sfxButton_Tapped(object sender, EventArgs e)
        {
            BooleanButton button = sender as BooleanButton;
            ScreenManager.enableSoundEffect = !ScreenManager.enableSoundEffect;
            checkButton();
        }

        void controlButton_Tapped(object sender, EventArgs e)
        {
            Button button = sender as Button;

            if (ScreenManager.enableAccelerometer == false)
            {
                ScreenManager.enableAccelerometer = !ScreenManager.enableAccelerometer;
                button.Text = "Control: Gyro";
            }
            else
            {
                ScreenManager.enableAccelerometer = !ScreenManager.enableAccelerometer;
                button.Text = "Control: Touch";
            }
        }

        protected override void OnCancel()
        {
            ScreenManager.Game.Exit();
            base.OnCancel();
        }
    }
}
