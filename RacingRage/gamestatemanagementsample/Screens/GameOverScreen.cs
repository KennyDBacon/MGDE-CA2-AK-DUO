#region File Description
//-----------------------------------------------------------------------------
// PhonePauseScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;

namespace GameStateManagementSample
{
    /// <summary>
    /// A basic pause screen for Windows Phone
    /// </summary>
    class GameOverScreen : PhoneMenuScreen
    {
        public GameOverScreen()
            : base("Game Over!")
        {
            // Create the "Resume" and "Exit" buttons for the screen

            Button retryButton = new Button("Retry");
            retryButton.Tapped += retryButton_Tapped;
            MenuButtons.Add(retryButton);

            Button exitButton = new Button("Exit");
            exitButton.Tapped += exitButton_Tapped;
            MenuButtons.Add(exitButton);
        }

        /// <summary>
        /// The "Resume" button handler just calls the OnCancel method so that 
        /// pressing the "Resume" button is the same as pressing the hardware back button.
        /// </summary>
        void retryButton_Tapped(object sender, EventArgs e)
        {
            ScreenManager.engineSoundBool = true;
            //LoadingScreen.Load(ScreenManager, true, PlayerIndex.One, new GameplayScreen());
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

        protected override void OnCancel()
        {
            ExitScreen();
            base.OnCancel();
        }
    }
}
