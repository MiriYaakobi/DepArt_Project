using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PL.Converters
{
   public class StatusToColorConverter : IValueConverter
    {
        // המרה מהנתונים (האם פעיל?) לצבע (Brush)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // אנחנו מצפים לקבל בוליאני (true/false)
            if (value is bool isActive)
            {
                if (!isActive) // אם לא פעיל
                {
                    // מחזירים צבע אדמדם/אפרפר בהיר לסימון
                    return Brushes.LavenderBlush;
                }
            }

            // אם פעיל - צבע רגיל (שקוף/לבן)
            return Brushes.Transparent;
        }

        // המרה הפוכה (לא רלוונטי לצבעים, לכן זורקים שגיאה)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
