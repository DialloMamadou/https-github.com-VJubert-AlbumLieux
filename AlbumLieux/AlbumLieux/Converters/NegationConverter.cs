﻿using System;
using System.Globalization;
using Xamarin.Forms;

namespace AlbumLieux.Converters
{
	public class NegationConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool b)
			{
				return !b;
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
