﻿// Copyright (c) 2013 Blue Onion Software - All rights reserved

using System;
using System.Windows;
using tweetz5.Model;

namespace tweetz5.Controls
{
    public partial class Authenticate
    {
        private Twitter.OAuthTokens Tokens { get; set; }

        public Authenticate()
        {
            InitializeComponent();
        }

        private void GetPin_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Tokens = Twitter.GetRequestToken();
                var url = "https://api.twitter.com/oauth/authenticate?oauth_token=" + Tokens.OAuthToken;
                Commands.OpenLinkCommand.Command.Execute(url, this);
            }
            catch (Exception ex)
            {
                Commands.AlertCommand.Command.Execute(ex.Message, this);
            }
        }

        private void SignIn_OnClick(object sender, RoutedEventArgs e)
        {
            var tokens = Twitter.GetAccessToken(Tokens.OAuthToken, Tokens.OAuthSecret, Pin.Text);
            Pin.Text = string.Empty;
            Properties.Settings.Default.AccessToken = tokens.OAuthToken;
            Properties.Settings.Default.AccessTokenSecret = tokens.OAuthSecret;
            Properties.Settings.Default.UserId = tokens.UserId;
            Properties.Settings.Default.ScreenName = tokens.ScreenName;
            Properties.Settings.Default.Save();
            Commands.SignInCommand.Command.Execute(null, this);
        }
    }
}