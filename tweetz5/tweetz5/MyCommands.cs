﻿using System.Windows.Input;

namespace tweetz5
{
    internal static class MyCommands
    {
        public static readonly RoutedCommand SignOutCommand = new RoutedUICommand();
        public static readonly RoutedCommand SettingsCommand = new RoutedUICommand();
        public static readonly RoutedCommand UpdateLayoutCommand = new RoutedUICommand();
        public static readonly RoutedCommand OpenComposeCommand = new RoutedUICommand();
        public static readonly RoutedCommand ShortcutHelpCommand = new RoutedUICommand();
    }
}