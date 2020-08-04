namespace DigitExample.ViewModel
{
    using System;
    using System.ComponentModel;

    public class DigitViewModel : INotifyPropertyChanged
    {
        public const byte DigitDefault = 0;
        private byte _digit = DigitDefault;
        
        public DigitViewModel()
        {
            SetRecived();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        internal bool Reciving { get; set; }

        public byte Digit
        {
            get
            {
                return _digit;
            }
            set
            {
                if (_digit != value)
                {
                    _digit = value;
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Digit"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DigitAsDouble"));
            }
        }

        internal void SetReciving()
        {
            Reciving = true;
        }

        internal void SetRecived()
        {
            Reciving = false;
        }
    }
}
