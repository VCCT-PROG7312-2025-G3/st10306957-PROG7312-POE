using System;

namespace PROG7312_POE
{
    public class IssueCollection
    {
        private IssueReport[] items;
        private int count;
        private const int DefaultCapacity = 4;

        public int Count => count;

        public IssueCollection()
        {
            items = new IssueReport[DefaultCapacity];
            count = 0;
        }

        public void Add(IssueReport item)
        {
            if (count == items.Length)
            {
                EnsureCapacity(count + 1);
            }
            items[count++] = item;
        }

        public IssueReport this[int index]
        {
            get
            {
                if (index < 0 || index >= count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return items[index];
            }
            set
            {
                if (index < 0 || index >= count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                items[index] = value;
            }
        }

        private void EnsureCapacity(int minCapacity)
        {
            int newCapacity = items.Length == 0 ? DefaultCapacity : items.Length * 2;
            if (newCapacity < minCapacity) newCapacity = minCapacity;
            
            IssueReport[] newItems = new IssueReport[newCapacity];
            Array.Copy(items, newItems, count);
            items = newItems;
        }

        public void Clear()
        {
            Array.Clear(items, 0, count);
            count = 0;
        }

        public bool Contains(IssueReport item)
        {
            for (int i = 0; i < count; i++)
            {
                if (Equals(items[i], item))
                    return true;
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= count)
                throw new ArgumentOutOfRangeException(nameof(index));

            count--;
            if (index < count)
            {
                Array.Copy(items, index + 1, items, index, count - index);
            }
            items[count] = null;
        }
    }
}
