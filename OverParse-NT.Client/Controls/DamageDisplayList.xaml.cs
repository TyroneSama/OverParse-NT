using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OverParse_NT.Client.Controls
{
    /// <summary>
    /// Interaction logic for DamageDisplayList.xaml
    /// </summary>
    public partial class DamageDisplayList : UserControl
    {
        public class DamageDisplayData : INotifyPropertyChanged
        {
            private string _Name;
            private long _Damage;
            private double _DamageRatio;
            private double _DamageRatioNormal;
            private double _DamageRatioZanverse;
            private string _MaxHitName;
            private long _MaxHitDamage;

            public string Name
            {
                get { return _Name; }
                set
                {
                    _Name = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                }
            }
            public long Damage
            {
                get { return _Damage; }
                set
                {
                    _Damage = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Damage)));
                }
            }
            public double DamageRatio
            {
                get { return _DamageRatio; }
                set
                {
                    _DamageRatio = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DamageRatio)));
                }
            }
            public double DamageRatioNormal
            {
                get { return _DamageRatioNormal; }
                set
                {
                    _DamageRatioNormal = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DamageRatioNormal)));
                }
            }
            public double DamageRatioZanverse
            {
                get { return _DamageRatioZanverse; }
                set
                {
                    _DamageRatioZanverse = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DamageRatioZanverse)));
                }
            }
            public string MaxHitName
            {
                get { return _MaxHitName; }
                set
                {
                    _MaxHitName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MaxHitName)));
                }
            }
            public long MaxHitDamage
            {
                get { return _MaxHitDamage; }
                set
                {
                    _MaxHitDamage = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MaxHitDamage)));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        private class DamageDisplayDataContext
        {
            public ObservableCollection<DamageDisplayData> Items { get; set; }
        }

        public ObservableCollection<DamageDisplayData> Items { get; } = new ObservableCollection<DamageDisplayData>();

        public DamageDisplayList()
        {
            InitializeComponent();
            _ItemsControl.DataContext = new DamageDisplayDataContext { Items = Items };
        }
    }
}
