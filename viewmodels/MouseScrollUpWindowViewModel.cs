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
    class MouseScrollUpWindowViewModel : INotifyPropertyChanged
    {
        #region properties

        // Mouse Scroll Up ImageSource Properties
        private ImageSource _buttonScrollUpSaveImageSource;
        public ImageSource ButtonScrollUpSaveImageSource
        {
            get { return _buttonScrollUpSaveImageSource; }
            set { _buttonScrollUpSaveImageSource = value; OnPropertyChanged(); }
        }

        private ImageSource _buttonScrollUpClearImageSource;
        public ImageSource ButtonScrollUpClearImageSource
        {
            get { return _buttonScrollUpClearImageSource; }
            set { _buttonScrollUpClearImageSource = value; OnPropertyChanged(); }
        }

        #endregion


        public CancellationTokenSource _tokenSourceClick = new CancellationTokenSource();

        private System.Timers.Timer textUpdateTimer = new System.Timers.Timer(MainWindowViewModel.TEXTBLOCK_UPDATE_TIMER);

        private readonly MainWindow _mainWindow;
        private readonly MouseScrollUpWindow _thisWindow;

        public MouseScrollUpWindowViewModel(MainWindow mainWindow, MouseScrollUpWindow thisWindow)
        {
            _mainWindow = mainWindow;
            _thisWindow = thisWindow;

            // Assign the resource images to the binding ImageSource properties
            ButtonScrollUpSaveImageSource = (ImageSource)Application.Current.Resources["panelButton_Unpressed"];
            ButtonScrollUpClearImageSource = (ImageSource)Application.Current.Resources["panelButton2_Unpressed"];
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

        private void TextBlock_NameSet(MouseScrollUpWindow window)
        {
            window.buttonReadout_TextBlock.Text = MainWindowViewModel.DEFAULT_PROMPT == Settings.Default.MouseUpRebind ?
                MainWindowViewModel.DEFAULT_PROMPT : Settings.Default.MouseUpRebind.ToUpper().Replace("OEM", "");

            window.buttonReadout_TextBlock.FontSize = window.buttonReadout_TextBlock.Text.Length < MainWindowViewModel.DEFAULT_PROMPT.Length ?
                MainWindowViewModel.TEXTBLOCK_TEXT_LARGE : MainWindowViewModel.TEXTBLOCK_TEXT_SMALL;
        }

        private void TextBlockTimer_NameSet(MouseScrollUpWindow window)
        {
            try
            {
                RunOnUIThread(() =>
                {
                    if (window.buttonReadout_TextBlock != null)
                    {
                        if (String.IsNullOrEmpty(Settings.Default.MouseUpRebind.Trim()))
                            Settings.Default.MouseUpRebind = MainWindowViewModel.DEFAULT_PROMPT;

                        // Load & Resize Text to Button Number TextBlock (only if changed)
                        if (window.buttonReadout_TextBlock.Text != Settings.Default.MouseUpRebind)
                        {
                            TextBlock_NameSet(window);
                        }
                    }
                });
            }
            catch (Exceptions ex)
            {
                ex.LogError("Error at MouseScrollUp Window TextBlock Name Set");
            }
        }


        public void MouseScrollUpWindow_Loaded(object sender, RoutedEventArgs e)
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
                ex.LogError("Error in MouseScrollUp ViewModel Loaded event");
                Exceptions.ExitApplication(1);
            }
        }

        public void MouseScrollUpWindow_Closing(object sender, CancelEventArgs e)
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
                ex.LogError("Error in MouseScrollUp Window ViewModel on closing cleanup and save");
            }
        }


        #region controls

        // Left Mouse Click Rebind SAVE Button
        private ICommand _saveMouseUpRebind;
        public ICommand SaveMouseUpRebind
        {
            get
            {
                if (_saveMouseUpRebind == null)
                {
                    _saveMouseUpRebind = new RelayCommand<object, RoutedEventArgs>(Button_SaveMouseUpRebind);
                }
                return _saveMouseUpRebind;
            }
        }
        public async void Button_SaveMouseUpRebind(object sender, RoutedEventArgs e)
        {
            try
            {
                // Set the button's content to the pressed image
                ButtonScrollUpSaveImageSource = (ImageSource)Application.Current.Resources["panelButton_Pressed"];
                TranslateTransform transform = new TranslateTransform();
                _thisWindow.saveMouseUp_TextBlock.RenderTransform = transform;
                transform.Y = 1;
                transform.X = -1;

                // Wait a moment...
                await Task.Delay(TimeSpan.FromSeconds(0.25));

                // Set the button's content to the Unpressed image
                ButtonScrollUpSaveImageSource = (ImageSource)Application.Current.Resources["panelButton_Unpressed"];
                transform = new TranslateTransform();
                _thisWindow.saveMouseUp_TextBlock.RenderTransform = transform;
                transform.Y = 0;
                transform.X = 0;

                // Wait a smidge...
                await Task.Delay(TimeSpan.FromSeconds(0.10));

            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in MouseScrollUp Window ViewModel on save scroll up rebind key");
            }
            finally
            {
                _thisWindow.Close();
            }
        }

        // Left Mouse Click Rebind CLEAR Button
        private ICommand _clearMouseUpRebind;
        public ICommand ClearMouseUpRebind
        {
            get
            {
                if (_clearMouseUpRebind == null)
                {
                    _clearMouseUpRebind = new RelayCommand<object, RoutedEventArgs>(Button_ClearMouseUpRebind);
                }
                return _clearMouseUpRebind;
            }
        }
        public async void Button_ClearMouseUpRebind(object sender, RoutedEventArgs e)
        {
            try
            {
                // Set the scratched button's content to the pressed image
                ButtonScrollUpClearImageSource = (ImageSource)Application.Current.Resources["panelButton2_Pressed"];
                TranslateTransform transform = new TranslateTransform();
                _thisWindow.clearMouseUp_TextBlock.RenderTransform = transform;
                transform.Y = 1;
                transform.X = -1;

                // Wait a moment...
                await Task.Delay(TimeSpan.FromSeconds(0.25));

                // Set the scratched button's content to the Unpressed image
                ButtonScrollUpClearImageSource = (ImageSource)Application.Current.Resources["panelButton2_Unpressed"];
                transform = new TranslateTransform();
                _thisWindow.clearMouseUp_TextBlock.RenderTransform = transform;
                transform.Y = 0;
                transform.X = 0;

                // Wait a smidge...
                await Task.Delay(TimeSpan.FromSeconds(0.10));

                // Reset 
                Settings.Default.MouseUpRebind = MainWindowViewModel.DEFAULT_PROMPT;

            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in MouseScrollUp Window ViewModel on clear scroll up rebind key");
            }
            finally
            {
                _thisWindow.Close();
            }
        }

        #endregion


    }
}
