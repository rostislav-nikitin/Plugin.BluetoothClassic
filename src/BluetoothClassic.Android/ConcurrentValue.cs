namespace BluetoothClassic.Droid
{
    using System.Threading;

    internal class ConcurrentValue<T>
    {
        private const int TimoutInfinity = -1;
        public ConcurrentValue(T initialValue = default)
        {
            Value = initialValue;
        }

        private ReaderWriterLock _rwLock = new ReaderWriterLock();

        private T _entity;

        public T Value
        {
            get
            {
                _rwLock.AcquireReaderLock(TimoutInfinity);
                try
                {
                    return _entity;
                }
                finally
                {
                    _rwLock.ReleaseReaderLock();
                }
            }
            set
            {
                _rwLock.AcquireWriterLock(TimoutInfinity);
                try
                {
                    _entity = value;
                }
                finally
                {
                    _rwLock.ReleaseWriterLock();
                }
            }
        }
    }
}