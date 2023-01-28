using MouseMasterVR.Properties;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using VGLabsFoundation;

namespace MouseMasterVR
{
    class MouseScrollDownWindowViewModel : INotifyPropertyChanged
    {
        #region properties

        // Mouse Scroll Down ImageSource Properties
        private ImageSource _buttonScrollDownSaveImageSource;
        public ImageSource ButtonScrollDownSaveImageSource
        {
            get { return _buttonScrollDownSaveImageSource; }
            set { _buttonScrollDownSaveImageSource = value; OnPropertyChanged(); }
        }

        private ImageSource _buttonScrollDownClearImageSource;
        public ImageSource ButtonScrollDownClearImageSource
        {
            get { return _buttonScrollDownClearImageSource; }
            set { _buttonScrollDownClearImageSource = value; OnPropertyChanged(); }
        }

        #endregion


        public CancellationTokenSource _tokenSourceClick = new CancellationTokenSource();

        private System.Timers.Timer textUpdateTimer = new System.Timers.Timer(MainWindowViewModel.TEXTBLOCK_UPDATE_TIMER);

        private readonly MainWindow _mainWindow;
        private readonly MouseScrollDownWindow _thisWindow;

        public MouseScrollDownWindowViewModel(MainWindow mainWindow, MouseScrollDownWindow thisWindow)
        {
            _mainWindow = mainWindow;
            _thisWindow = thisWindow;

            // Assign the resources image to the binding ImageSource properties
            ButtonScrollDownSaveImageSource = (ImageSource)Application.Current.Resources["panelButton2_Unpressed"];
            ButtonScrollDownClearImageSource = (ImageSource)Application.Current.Resources["panelButton_Unpressed"];
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private void RunOnUIThread(Action action)
        {
            // Dispatch Helper Method
            Application.Current.Dispatcher.Invoke(action);
        }

        private void TextBlock_NameSet(MouseScrollDownWindow window)
        {
            window.buttonReadout_TextBlock.Text = MainWindowViewModel.DEFAULT_PROMPT == Settings.Default.MouseDownRebind ?
                MainWindowViewModel.DEFAULT_PROMPT : Settings.Default.MouseDownRebind.ToUpper().Replace("OEM", "");

            window.buttonReadout_TextBlock.FontSize = _thisWindow.buttonReadout_TextBlock.Text.Length < MainWindowViewModel.DEFAULT_PROMPT.Length ?
                MainWindowViewModel.TEXTBLOCK_TEXT_LARGE : MainWindowViewModel.TEXTBLOCK_TEXT_SMALL;
        }

        public void TextBlockTimer_NameSet(MouseScrollDownWindow window)
        {
            try
            {
                RunOnUIThread(() =>
                {
                    //MainWindow window = (MainWindow)Application.Current.MainWindow;
                    Settings.Default.MainPowerSwitch = false;

                    if (window.buttonReadout_TextBlock != null)
                    {
                        if (String.IsNullOrEmpty(Settings.Default.MouseDownRebind.Trim()))
                            Settings.Default.MouseDownRebind = MainWindowViewModel.DEFAULT_PROMPT;

                        // Load & Resize Text to Button Number TextBlock (only if changed)
                        if (window.buttonReadout_TextBlock.Text != Settings.Default.MouseDownRebind)
                        {
                            TextBlock_NameSet(window);
                        }
                    }
                });
            }
            catch (Exceptions ex)
            {
                ex.LogError("Error at MouseScrollDown Window TextBlock Name Set");
            }
        }


        public void MouseScrollDownWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Set this Window to position & width of parent window
                _thisWindow.Top = _mainWindow.Top;
                _thisWindow.Left = _mainWindow.Left;
                _thisWindow.MaxWidth = _mainWindow.Width;
                _thisWindow.MinWidth = _mainWindow.Width;

                // Set the TextBlock Text of this Rebind to Saved Value
                TextBlock_NameSet(_thisWindow);

                // Create Timer to update TextBlock display
                textUpdateTimer.Elapsed += (s, ea) => TextBlockTimer_NameSet(_thisWindow);
                textUpdateTimer.Start();
            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in MouseScrollDown ViewModel Loaded event");
                Exceptions.ExitApplication(1);
            }
        }

        public void MouseScrollDownWindow_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                // Cleanup and Dispose of resources
                textUpdateTimer.Stop();
                textUpdateTimer.Dispose();

                // Save Scroll Up Rebind Key
                Settings.Default.Save();
            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in MouseScrollDown Window ViewModel on closing cleanup and save");
            }
        }


        #region controls

        // Left Mouse Click Rebind SAVE Button
        private ICommand _saveMouseDownRebind;
        public ICommand SaveMouseDownRebind
        {
            get
            {
                if (_saveMouseDownRebind == null)
                {
                    _saveMouseDownRebind = new RelayCommand<object, RoutedEventArgs>(Button_SaveMouseDownRebind);
                }
                return _saveMouseDownRebind;
            }
        }
        public async void Button_SaveMouseDownRebind(object sender, RoutedEventArgs e)
        {
            try
            {
                // Set the scratched button's content to the pressed image
                ButtonScrollDownSaveImageSource = (ImageSource)Application.Current.Resources["panelButton2_Pressed"];
                TranslateTransform transform = new TranslateTransform();
                _thisWindow.saveMouseDown_TextBlock.RenderTransform = transform;
                transform.Y = 1;
                transform.X = -1;

                // Wait a moment...
                await Task.Delay(TimeSpan.FromSeconds(0.25));

                // Set the scratched button's content to the Unpressed image
                ButtonScrollDownSaveImageSource = (ImageSource)Application.Current.Resources["panelButton2_Unpressed"];
                transform = new TranslateTransform();
                _thisWindow.saveMouseDown_TextBlock.RenderTransform = transform;
                transform.Y = 0;
                transform.X = 0;

                // Wait a smidge...
                await Task.Delay(TimeSpan.FromSeconds(0.10));

            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in MouseScrollDown Window ViewModel on save scroll down rebind key");
            }
            finally
            {
                _thisWindow.Close();
            }
        }

        // Left Mouse Click Rebind CLEAR Button
        private ICommand _clearMouseDownRebind;
        public ICommand ClearMouseDownRebind
        {
            get
            {
                if (_clearMouseDownRebind == null)
                {
                    _clearMouseDownRebind = new RelayCommand<object, RoutedEventArgs>(Button_ClearMouseDownRebind);
                }
                return _clearMouseDownRebind;
            }
        }
        public async void Button_ClearMouseDownRebind(object sender, RoutedEventArgs e)
        {
            try
            {
                // Set the button's content to the pressed image
                ButtonScrollDownClearImageSource = (ImageSource)Application.Current.Resources["panelButton_Pressed"];
                TranslateTransform transform = new TranslateTransform();
                _thisWindow.clearMouseDown_TextBlock.RenderTransform = transform;
                transform.Y = 1;
                transform.X = -1;

                // Wait a moment...
                await Task.Delay(TimeSpan.FromSeconds(0.25));

                // Set the button's content to the Unpressed image
                ButtonScrollDownClearImageSource = (ImageSource)Application.Current.Resources["panelButton_Unpressed"];
                transform = new TranslateTransform();
                _thisWindow.clearMouseDown_TextBlock.RenderTransform = transform;
                transform.Y = 0;
                transform.X = 0;

                // Wait a smidge...
                await Task.Delay(TimeSpan.FromSeconds(0.10));

                // Reset 
                Settings.Default.MouseDownRebind = MainWindowViewModel.DEFAULT_PROMPT;

            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in MouseScrollDown Window ViewModel on clear scroll down rebind key");
            }
            finally
            {
                _thisWindow.Close();
            }
        }

        #endregion


    }
}
