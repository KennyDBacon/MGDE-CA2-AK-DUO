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

            BooleanButton vibrationButton = new BooleanButton("Vibration", true);
            vibrationButton.Tapped += vibrationButton_Tapped;
            MenuButtons.Add(vibrationButton);
        }

        public override void Activate(bool instancePreserved)
        {
            checkButton();
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
                        b.Text = "Music: On";
                    else
                        b.Text = "Music: Off";
                }
                else if (b.Text.Contains("Sound"))
                {
                    if (ScreenManager.enableSoundEffect == true)
                        b.Text = "Sound: On";
                    else
                        b.Text = "Sound: Off";
                }
                else if (b.Text.Contains("Vibration"))
                {
                    if (ScreenManager.enableVibration == true)
                        b.Text = "Vibration: On";
                    else
                        b.Text = "Vibration: Off";
                }
            }
        }

        void playButton_Tapped(object sender, EventArgs e)
        {
            LoadingScreen.Load(ScreenManager, true, PlayerIndex.One, new GameplayScreen());
        }

        void musicButton_Tapped(object sender, EventArgs e)
        {
            BooleanButton button = sender as BooleanButton;
            ScreenManager.enableMusic = !ScreenManager.enableMusic;
            checkButton();
        }

        void sfxButton_Tapped(object sender, EventArgs e)
        {
            BooleanButton button = sender as BooleanButton;
            ScreenManager.enableSoundEffect = !ScreenManager.enableSoundEffect;
            checkButton();
        }

        void vibrationButton_Tapped(object sender, EventArgs e)
        {
            BooleanButton button = sender as BooleanButton;
            ScreenManager.enableVibration = !ScreenManager.enableVibration;
            checkButton();
        }

        protected override void OnCancel()
        {
            ScreenManager.Game.Exit();
            base.OnCancel();
        }
    }
}
