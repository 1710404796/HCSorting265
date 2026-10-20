using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HCSortingCZP.View
{
    public partial class Busying : UserControl
    {




        public string BusyText
        {
            get { return (string)GetValue(BusyTextProperty); }
            set { SetValue(BusyTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for _busyText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BusyTextProperty =
            DependencyProperty.Register("BusyText", typeof(string), typeof(Busying), new PropertyMetadata(string.Empty, OnBusyTextChange));



        public int Progess
        {
            get { return (int)GetValue(ProgessProperty); }
            set { SetValue(ProgessProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Progess.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProgessProperty =
            DependencyProperty.Register("Progess", typeof(int), typeof(Busying), new PropertyMetadata(0, OnProgessChange));
        private static void OnBusyTextChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (Busying)d;
            control.BusyText1.Text = (string)e.NewValue;
        }
        private static void OnProgessChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (Busying)d;
            control.PP.Value = (int)e.NewValue;
        }
        public Busying()
        {
            InitializeComponent();
        }
    }
}