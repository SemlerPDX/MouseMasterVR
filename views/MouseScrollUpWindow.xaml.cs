using MouseMasterVR.Properties;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace MouseMasterVR
{
    /// <summary>
    /// Interaction logic for MouseScrollUpWindow.xaml
    /// </summary>
    public partial class MouseScrollUpWindow : Window
    {

        private readonly MainWindow _mainWindow;
        private readonly MouseScrollUpWindowViewModel _viewModelMouseScrollUp;

        public MouseScrollUpWindow(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            _viewModelMouseScrollUp = new MouseScrollUpWindowViewModel(_mainWindow, this);
            this.DataContext = _viewModelMouseScrollUp;

            InitializeComponent();

            this.Loaded += ThisWindow_Loaded;
            this.Closing += ThisWindow_Closing;
        }

        private void ThisWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModelMouseScrollUp.MouseScrollUpWindow_Loaded(sender, e);
        }

        private void ThisWindow_Closing(object sender, CancelEventArgs e)
        {
            _viewModelMouseScrollUp.MouseScrollUpWindow_Closing(sender, e);
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MouseScrollWindow_KeyDown(object sender, KeyEventArgs e)
        {
            Key ScrollUpRebind = e.Key;
            if (!ModifierKey.ModifierKeys.Contains(ScrollUpRebind))
            {
                Settings.Default.MouseUpRebind = ScrollUpRebind.ToString();
            }
        }

    }
}