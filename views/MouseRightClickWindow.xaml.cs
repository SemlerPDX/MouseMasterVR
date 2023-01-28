using System.ComponentModel;
using System.Windows;

namespace MouseMasterVR
{
    /// <summary>
    /// Interaction logic for MouseRightClickWindow.xaml
    /// </summary>
    public partial class MouseRightClickWindow : Window
    {

        private readonly MainWindow _mainWindow;
        private readonly MouseRightClickWindowViewModel _viewModelMouseRightClick;

        public MouseRightClickWindow(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            _viewModelMouseRightClick = new MouseRightClickWindowViewModel(_mainWindow, this);
            this.DataContext = _viewModelMouseRightClick;

            InitializeComponent();
            _viewModelMouseRightClick.InitializeMouseRightClickViewModel(this);

            this.Loaded += ThisWindow_Loaded;
            this.Closing += ThisWindow_Closing;
        }

        private void ThisWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModelMouseRightClick.MouseRightClickWindow_Loaded(sender, e);
        }

        private void ThisWindow_Closing(object sender, CancelEventArgs e)
        {
            _viewModelMouseRightClick.MouseRightClickWindow_Closing(sender, e);
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
