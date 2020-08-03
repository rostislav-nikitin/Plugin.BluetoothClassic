namespace DigitExample.ViewModel
{
    using System.ComponentModel;

    public class DigitViewModel : INotifyPropertyChanged
    {
        public const byte DigitDefault = 0;
        private byte _digit = DigitDefault;

        public event PropertyChangedEventHandler PropertyChanged;

        public double DigitAsDouble
        {
            get
            {
                return Digit;
            }
            set
            {
                Digit = (byte)value;
            }
        }

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

    }
}
