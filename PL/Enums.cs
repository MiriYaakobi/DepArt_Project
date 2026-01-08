using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PL
{
    // מחלקה שמספקת את רשימת סוגי המשלוח + אופציית "הכל" עבור ה-XAML
    public class DeliveryTypeCollection : IEnumerable
    {
        static readonly List<object> s_enums = new List<object>();

        static DeliveryTypeCollection()
        {
            // הוספת "הכל" כברירת מחדל
            s_enums.Add("All");
            // הוספת שאר ערכי ה-Enum
            s_enums.AddRange(Enum.GetValues(typeof(BO.DeliveryType)).Cast<object>());
        }

        public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
    }
}