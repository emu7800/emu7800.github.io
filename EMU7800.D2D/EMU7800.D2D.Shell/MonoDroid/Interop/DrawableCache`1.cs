using System;

namespace EMU7800.D2D.Interop
{
    public sealed class DrawableCache<T> : IDisposable where T : Drawable
    {
        #region Fields

        readonly T[] _cache;
        readonly int _mask;
        int _index;

        #endregion

        public T Get(RectF rect)
        {
            var key = Drawable.ToKey(rect.Right - rect.Left, rect.Bottom - rect.Top);
            return Get(key);
        }

        public T Get(uint key)
        {
            T item = null;

            for (var i = 0; i < _cache.Length; i++)
            {
                item = _cache[i];
                if (item == null)
                    break;
                if (item.Key == key)
                    break;
            }

            return item;
        }

        public void Put(T item)
        {
            using (_cache[_index & _mask]) {}
            _cache[_index++ & _mask] = item;
        }

        #region IDisposable Members

        public void Dispose()
        {
            for (var i = 0; i < _cache.Length; i++)
            {
                using (_cache[i]) {}
            }
        }

        #endregion

        #region Constructors

        public DrawableCache(int size)
        {
            _cache = new T[1 << size];
            _mask = _cache.Length - 1;
        }

        #endregion
    }
}