using System.Collections;
using BO;

namespace PL.Order;

public class OrderStatusCollection : IEnumerable
{
    public IEnumerator GetEnumerator()
    {
        yield return "All";
        foreach (var status in System.Enum.GetValues(typeof(OrderStatus)))
        {
            yield return status;
        }
    }
}