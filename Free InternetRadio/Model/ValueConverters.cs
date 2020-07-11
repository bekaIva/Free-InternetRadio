using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace iRadio_Free.Model
{
    public class GenreToString : IValueConverter
    {
        public GenreToString()
        {

        }
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                string g = ((Genre)value).ToString();
                g = g.Replace("DigitStart", "").Replace("_", " ");
                g = char.ToUpper(g[0]).ToString() + new string(g.Skip(1).ToArray());
                return g;
            }
            catch
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
    public class GenresToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                StringBuilder strb = new StringBuilder();
                bool isfirst = false;
                foreach (var genre in (List<Genre>)value)
                {
                    string g = genre.ToString();
                    g = g.Replace("DigitStart", "").Replace("_", " ");
                    g = char.ToUpper(g[0]).ToString() + new string(g.Skip(1).ToArray());
                    if (!isfirst) strb.Append(g);
                    else strb.Append(" ," + g);
                    isfirst = true;
                }
                return strb.ToString();
            }
            catch
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
    public class BoolToPlayPauseIcon : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((bool?)value == true)
            {
                return Symbol.Pause;
            }
            else
            {
                return Symbol.Play;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
    public class NavButtons : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {

            try
            {
                switch (System.Convert.ToInt32((string)parameter))
                {
                    case 0:
                        {
                            var channel = value as Channel;
                            if (channel == null) return false;
                            else
                            {
                                if (channel.PreviousChannel == null) return false;
                                else return true;
                            }
                        }
                    case 1:
                        {
                            var channel = value as Channel;
                            if (channel == null) return false;
                            else
                            {
                                if (channel.NextChannel == null) return false;
                                else return true;
                            }
                        }
                    default:
                        {
                            return false;
                        }
                }
            }
            catch
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
    public class NavPlayToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                var channel = value as Channel;
                if (channel == null) return Visibility.Collapsed;
                else return Visibility.Visible;
            }
            catch
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
    public class IsInFavoritesToSymbol : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                bool? val = value as bool?;
                if (val != null && val != false)
                {
                    return Symbol.Remove;
                }
                else
                {
                    return Symbol.Add;
                }
            }
            catch
            {
                return Symbol.Add;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
    public class boolToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                switch(parameter as string)
                {
                    case "Reverse":
                        {
                            if ((bool)value) return Visibility.Collapsed;
                            else return Visibility.Visible;
                            
                        }
                    case "Forward":
                        {
                            if ((bool)value) return Visibility.Visible;
                            else return Visibility.Collapsed;
                           
                        }
                    case "Opacity":
                        {
                            if ((bool)value) return 0;
                            else return 1;
                        }
                    default:
                        {
                            return null;
                        }
                }
              
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
    public class VolumeToPath:IValueConverter
    {
        Geometry VolumeZero;
        Geometry VolumeMidle;
        Geometry VolumeFull;
        public VolumeToPath()
        {
            try
            {
                VolumeFull = StringToPath("M429 654v-608q0-14-11-25t-25-10-25 10l-186 186h-146q-15 0-25 11t-11 25v214q0 15 11 25t25 11h146l186 186q10 10 25 10t25-10 11-25z m214-304q0-42-24-79t-63-52q-5-3-14-3-14 0-25 10t-10 26q0 12 6 20t17 14 19 12 16 21 6 31-6 32-16 20-19 13-17 13-6 20q0 15 10 26t25 10q9 0 14-3 39-15 63-52t24-79z m143 0q0-85-48-158t-125-105q-7-3-14-3-15 0-26 11t-10 25q0 22 21 33 32 16 43 25 41 30 64 75t23 97-23 97-64 75q-11 9-43 25-21 11-21 33 0 14 10 25t25 11q8 0 15-3 78-33 125-105t48-158z m143 0q0-128-71-236t-189-158q-7-3-14-3-15 0-25 11t-11 25q0 20 22 33 4 2 12 6t13 6q25 14 46 28 68 51 107 127t38 161-38 161-107 127q-21 15-46 28-4 3-13 6t-12 6q-22 13-22 33 0 15 11 25t25 11q7 0 14-3 118-51 189-158t71-236z");
                VolumeMidle = StringToPath("M429 654v-608q0-14-11-25t-25-10-25 10l-186 186h-146q-15 0-25 11t-11 25v214q0 15 11 25t25 11h146l186 186q10 10 25 10t25-10 11-25z m214-304q0-42-24-79t-63-52q-5-3-14-3-14 0-25 10t-10 26q0 12 6 20t17 14 19 12 16 21 6 31-6 32-16 20-19 13-17 13-6 20q0 15 10 26t25 10q9 0 14-3 39-15 63-52t24-79z");
                VolumeZero = StringToPath("M429 654v-608q0-14-11-25t-25-10-25 10l-186 186h-146q-15 0-25 11t-11 25v214q0 15 11 25t25 11h146l186 186q10 10 25 10t25-10 11-25z");
            }
            catch 
            {
                
            }

        }
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double Volume)
            {
                if (Volume == 0d) return VolumeZero;
                if (Volume > 0d && Volume < 1d) return VolumeMidle;
                if (Volume == 1d) return VolumeFull;
                return value;
            }
            else return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
        Geometry StringToPath(string pathData)
        {
            string xamlPath =
                "<Geometry xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>"
                + pathData + "</Geometry>";

            return Windows.UI.Xaml.Markup.XamlReader.Load(xamlPath) as Geometry;
        }
    }
    public class VolumeToPercentage:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
           return Math.Round((((double)value * 100 )/1 )).ToString()+"%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
