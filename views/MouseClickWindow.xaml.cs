using System.ComponentModel;
using System.Windows;

namespace MouseMasterVR
{
    /// <summary>
    /// Interaction logic for MouseClickWindow.xaml
    /// </summary>
    public partial class MouseClickWindow : Window
    {

        private readonly MainWindow _mainWindow;
        private readonly MouseClickWindowViewModel _viewModelMouseClick;

        public MouseClickWindow(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            _viewModelMouseClick = new MouseClickWindowViewModel(_mainWindow, this);
            this.DataContext = _viewModelMouseClick;

            InitializeComponent();
            _viewModelMouseClick.InitializeMouseClickViewModel(this);

            this.Loaded += ThisWindow_Loaded;
            this.Closing += ThisWindow_Closing;
        }

        private void ThisWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModelMouseClick.MouseClickWindow_Loaded(sender, e);
        }

        private void ThisWindow_Closing(object sender, CancelEventArgs e)
        {
            _viewModelMouseClick.MouseClickWindow_Closing(sender, e);
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
