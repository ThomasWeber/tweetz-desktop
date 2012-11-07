﻿// Copyright (c) 2012 Blue Onion Software - All rights reserved

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace tweetz5.Model
{
    [ValueConversion(typeof(string), typeof(Brush))]
    public class TweetTypeValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((string)value).Contains("m") 
                ? new SolidColorBrush(Color.FromRgb(33, 55, 33)) 
                : DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}