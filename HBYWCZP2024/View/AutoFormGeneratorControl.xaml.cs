using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace HBYWCZP2024.View
{
    /// <summary>
    /// AutoFormGeneratorControl.xaml 的交互逻辑
    /// </summary>
    public partial class AutoFormGeneratorControl : UserControl, INotifyPropertyChanged
    {
        #region Properties

        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        private List<ClassField> _fields;
        public List<ClassField> Fields
        {
            get { return _fields; }
            set
            {
                _fields = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Constructors

        public AutoFormGeneratorControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        #endregion

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public class ClassField
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }
}
