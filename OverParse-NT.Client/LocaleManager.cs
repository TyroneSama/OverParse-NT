using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace OverParse_NT.Client
{
    public class LocaleManager : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public static LocaleManager Instance { get; private set; }

        static LocaleManager()
        {
            Instance = new LocaleManager();
        }

        private LocaleManager()
        {
            CurrentCulture = CultureInfo.CurrentCulture;
        }

        public string this[string key]
        {
            get => Properties.Translations.ResourceManager.GetString(key, CurrentCulture);
        }

        private CultureInfo _CurrentCulture;

        public CultureInfo CurrentCulture
        {
            get => _CurrentCulture;
            set
            {
                if (_CurrentCulture == value)
                    return;

                _CurrentCulture = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
            }
        }
    }

    public class LocaleExtension : Binding
    {
        public LocaleExtension(string name)
            : base($"[{name}]")
        {
            Mode = BindingMode.OneWay;
            Source = LocaleManager.Instance;
        }
    }
}
