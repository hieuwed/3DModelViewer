using System;
using System.Globalization;
using System.Windows.Data;
using _3DModelViewer.Models;

namespace _3DModelViewer.Converters
{
    /// <summary>
    /// Converter để hiển thị kích thước file trong XAML
    /// </summary>
    public class FileSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Model3DFile model)
            {
                return model.GetFileSizeString();
            }

            if (value is long fileSize)
            {
                return FormatFileSize(fileSize);
            }

            return "0 B";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("ConvertBack is not supported for FileSizeConverter");
        }

        private string FormatFileSize(long fileSize)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = fileSize;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }
}