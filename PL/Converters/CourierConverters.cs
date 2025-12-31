using System.Globalization;
using System.Windows.Data;
using System;

namespace PL.Converters
{
    // ממיר 1: מחליט מה יהיה כתוב על הכפתור והכותרת (Add או Update)
    public class IdToContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // אם ה-ID הוא 0, סימן שאנחנו ביצירת שליח חדש
            if (value is int id && id == 0)
                return "Add";

            // אחרת, זה עדכון
            return "Update";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // ממיר 2: מחליט האם תיבת ה-ID פתוחה לכתיבה (רק בהוספה מותר לשנות ID)
    public class IdToIsReadOnlyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // אם זה 0 (הוספה), התיבה לא לקריאה בלבד (false) -> אפשר לכתוב
            if (value is int id && id == 0)
                return false;

            // אחרת (עדכון), התיבה לקריאה בלבד (true) -> אי אפשר לשנות
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}