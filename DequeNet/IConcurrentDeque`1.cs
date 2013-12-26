using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DequeNet
{
    public interface IConcurrentDeque<T>
    {
        bool IsEmpty { get; }

        void PushRight(T item);
        void PushLeft(T item);

        bool TryPopRight(out T item);
        bool TryPopLeft(out T item);
    }
}
