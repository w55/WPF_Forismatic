using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace MyWpfForismatic
{
    /// <summary>
    /// Interaction logic for frmSettings.xaml
    /// </summary>
    public partial class frmSettings : Window
    {
        MainWindow frmParent = null;

        public bool IsFirstMonitor = true; // radioFirstMonitor
        public bool IsLeftTopCorner = true; // radioLeftTop

        public int CorrectionX = 0; // textCorrectionX
        public int CorrectionY = 0; // textCorrectionY

        // textTransparency
        public int MainWindowTransparency = 80;
        // textVolumeLevel
        // public int VolumeLevel = 90;


        public frmSettings()
        {
            InitializeComponent();
        }

        public frmSettings(MainWindow parent)
        {
            InitializeComponent();
            frmParent = parent;
        }

        private void frmSettings_Loaded(object sender, RoutedEventArgs e)
        {
            radioFirstMonitor.IsChecked = IsFirstMonitor;
            radioSecondMonitor.IsChecked = !IsFirstMonitor;

            radioLeftTop.IsChecked = IsLeftTopCorner;
            radioRightTop.IsChecked = !IsLeftTopCorner;

            textCorrectionX.Text = CorrectionX.ToString();
            textCorrectionY.Text = CorrectionY.ToString();

            textTransparency.Text = MainWindowTransparency.ToString();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            IsFirstMonitor = radioFirstMonitor.IsChecked.Value;
            IsLeftTopCorner = radioLeftTop.IsChecked.Value;

            this.DialogResult = true;
            this.Close();
        }

        private void textCorrectionX_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                int new_val = 0;
                if (int.TryParse(textCorrectionX.Text.Trim(), out new_val))
                {
                    CorrectionX = new_val < 0 ? 0 : new_val > 500 ? 500 : new_val;
                    textCorrectionX.Text = CorrectionX.ToString();
                }
            }
            catch (Exception x)
            {
                System.Windows.MessageBox.Show(x.Message, "textCorrectionX_LostFocus()");
            }
        }

        private void textCorrectionY_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                int new_val = 0;
                if (int.TryParse(textCorrectionY.Text.Trim(), out new_val))
                {
                    CorrectionY = new_val < 0 ? 0 : new_val > 500 ? 500 : new_val;
                    textCorrectionY.Text = CorrectionY.ToString();
                }
            }
            catch (Exception x)
            {
                System.Windows.MessageBox.Show(x.Message, "textCorrectionY_LostFocus()");
            }
        }

        private void textTransparency_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                int new_val = 0;
                if (int.TryParse(textTransparency.Text.Trim(), out new_val))
                {
                    MainWindowTransparency = new_val < 10 ? 10 : new_val > 90 ? 90 : new_val;
                    textTransparency.Text = MainWindowTransparency.ToString();
                }
            }
            catch (Exception x)
            {
                System.Windows.MessageBox.Show(x.Message, "textTransparency_LostFocus()");
            }
        }
    }
}
