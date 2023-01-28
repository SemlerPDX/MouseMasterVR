//MouseMasterVR mouse centering & rebind utility app
// by SemlerPDX Jan2023
// Copyright © 2023 CC BY-SA-NC
// VETERANS-GAMING.COM
//
using MouseMasterVR.Properties;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MouseMasterVR
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly MainWindowViewModel _viewModel;

        public MainWindow()
        {
            // Pre-initialization Update System
            AppUpdates.UpgradeSettings();
            if (AppUpdates.CanUpdateCheck())
                this.Close();

            _viewModel = new MainWindowViewModel(this);
            this.DataContext = _viewModel;

            InitializeComponent();
            _viewModel.InitializeViewModel(this);

            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.Window_Loaded(sender, e);
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            _viewModel.Window_Closing(sender, e);
        }


        private void MainWindow_Drag(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Minimize_Button_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        public void TextBox_SetTextProcessTarget(object sender, KeyEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            textbox.Text = Settings.Default.GameTargetName;
            TextBox_ResizeProcessTarget(sender, null);
        }
        public void TextBox_ResizeProcessTarget(object sender, RoutedEventArgs e)
        {
            // Event Handler for TextBox to resize text to fit
            TextBox textbox = sender as TextBox;
            textbox.FontSize = textbox.Text.Length > 14 ?
                MainWindowViewModel.TEXTBOX_TEXT_SMALL : MainWindowViewModel.TEXTBOX_TEXT_LARGE;
        }

        public void TextBox_LostFocusRelay(object sender, RoutedEventArgs e)
        {
            TextBox_SetTextProcessTarget(sender, null);
        }

        private void TextBoxTarget_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Escape)
            {
                Keyboard.ClearFocus();

                // Get the TextBox Process Target Name entered by users (default Falcon BMS)
                TextBox textBox = sender as TextBox;

                // Save the TextBox Process Target Name
                if (textBox.Text == "")
                    textBox.Text = MainWindowViewModel.DEFAULT_PROCESS;

                Settings.Default.GameTargetName = textBox.Text;
                TextBox_SetTextProcessTarget(sender, null);

            }
        }
    }
}
