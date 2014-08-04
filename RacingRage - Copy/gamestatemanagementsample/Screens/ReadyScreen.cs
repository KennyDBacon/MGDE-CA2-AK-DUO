#region File Description
//-----------------------------------------------------------------------------
// PhonePauseScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;

namespace GameStateManagementSample
{
    /// <summary>
    /// A basic pause screen for Windows Phone
    /// </summary>
    class ReadyScreen : PhoneMenuScreen
    {
        public ReadyScreen(string readyCrashText)
            : base(readyCrashText)
        {
            // Create the "Resume" and "Exit" buttons for the screen
            if(readyCrashText.Equals("Crash!"))
            {
                Button startButton = new Button("Continue?");
                startButton.Tapped += startButton_Tapped;
                MenuButtons.Add(startButton);
            }
            else
            {
                Button startButton = new Button("Start!");
                startButton.Tapped += startButton_Tapped;
                MenuButtons.Add(startButton);
            }

            Button exitButton = new Button("Exit");
            exitButton.Tapped += exitButton_Tapped;
            MenuButtons.Add(exitButton);
        }

        void startButton_Tapped(object sender, EventArgs e)
        {
            ScreenManager.enableAd = false;

            ScreenManager.playerReady = true;
            ScreenManager.engineSoundBool = true;
            ExitScreen();
        }

        /// <summary>
        /// The "Exit" button handler uses the LoadingScreen to take the user out to the main menu.
        /// </summary>
        void exitButton_Tapped(object sender, EventArgs e)
        {
            LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(),
                                                           new PhoneMainMenuScreen());
        }

        //protected override void OnCancel()
        //{
        //    ExitScreen();
        //    base.OnCancel();
        //}
    }
}
