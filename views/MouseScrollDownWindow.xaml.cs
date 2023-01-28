using MouseMasterVR.Properties;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace MouseMasterVR
{
    /// <summary>
    /// Interaction logic for MouseScrollDownWindow.xaml
    /// </summary>
    public partial class MouseScrollDownWindow : Window
    {

        private readonly MainWindow _mainWindow;
        private readonly MouseScrollDownWindowViewModel _viewModelMouseScrollDown;

        public MouseScrollDownWindow(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            _viewModelMouseScrollDown = new MouseScrollDownWindowViewModel(_mainWindow, this);
            this.DataContext = _viewModelMouseScrollDown;

            InitializeComponent();

            this.Loaded += ThisWindow_Loaded;
            this.Closing += ThisWindow_Closing;
        }

        private void ThisWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModelMouseScrollDown.MouseScrollDownWindow_Loaded(sender, e);
        }

        private void ThisWindow_Closing(object sender, CancelEventArgs e)
        {
            _viewModelMouseScrollDown.MouseScrollDownWindow_Closing(sender, e);
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MouseScrollWindow_KeyDown(object sender, KeyEventArgs e)
        {
            Key ScrollDownRebind = e.Key;
            if (!ModifierKey.ModifierKeys.Contains(ScrollDownRebind))
            {
                Settings.Default.MouseDownRebind = ScrollDownRebind.ToString();
            }
        }

    }
}