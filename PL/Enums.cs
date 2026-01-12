using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PL
{
    /// <summary>
    /// Represents a collection of delivery type values, including a default "All" option and all values defined in <see
    /// cref="BO.DeliveryType"/>.
    /// </summary>
    /// <remarks>This collection provides enumeration over all available delivery types, with "All" included
    /// as the first item. It is intended for scenarios such as populating selection lists or filtering options in user
    /// interfaces.</remarks>
    public class DeliveryTypeCollection : IEnumerable
    {
        // Static list to hold the enum values along with the "All" option
        static readonly List<object> s_enums = new List<object>();

        // Static constructor to initialize the collection
        static DeliveryTypeCollection()
        {
            // adding the "All" option
            s_enums.Add("All");
            // adding the rest of the Enum values
            s_enums.AddRange(Enum.GetValues(typeof(BO.DeliveryType)).Cast<object>());
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
    }
}