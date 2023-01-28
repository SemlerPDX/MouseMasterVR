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
    class MouseRightClickWindowViewModel : INotifyPropertyChanged
    {
        #region properties

        // Mouse Right Click ImageSource Properties
        private ImageSource _buttonRightClickSaveImageSource;
        public ImageSource ButtonRightClickSaveImageSource
        {
            get { return _buttonRightClickSaveImageSource; }
            set { _buttonRightClickSaveImageSource = value; OnPropertyChanged(); }
        }

        private ImageSource _buttonRightClickClearImageSource;
        public ImageSource ButtonRightClickClearImageSource
        {
            get { return _buttonRightClickClearImageSource; }
            set { _buttonRightClickClearImageSource = value; OnPropertyChanged(); }
        }

        #endregion


        public CancellationTokenSource _tokenSourceClick = new CancellationTokenSource();

        private System.Timers.Timer textUpdateTimer = new System.Timers.Timer(MainWindowViewModel.TEXTBLOCK_UPDATE_TIMER);

        private readonly MainWindow _mainWindow;
        private readonly MouseRightClickWindow _thisWindow;

        public MouseRightClickWindowViewModel(MainWindow mainWindow, MouseRightClickWindow thisWindow)
        {
            _mainWindow = mainWindow;
            _thisWindow = thisWindow;

            // Assign the resource images to the binding ImageSource properties
            ButtonRightClickSaveImageSource = (ImageSource)Application.Current.Resources["panelButton_Unpressed"];
            ButtonRightClickClearImageSource = (ImageSource)Application.Current.Resources["panelButton2_Unpressed"];
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void InitializeMouseRightClickViewModel(object sender)
        {
            try
            {
                //THIS WORKS - BUT ONLY WHEN APP IN FOCUS (NEEDS WORK)
                //----------------------------------------------------------------------------
                // Inform Joystick Polling that RightClick Rebind is being set
                Settings.Default.GettingRightRebindKey = true;
                //----------------------------------------------------------------------------
            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in MouseRightClick Window ViewModel Initialize event");
                Exceptions.ExitApplication(1);
            }
        }

        private void RunOnUIThread(Action action)
        {
            // Dispatch Helper Method
            Application.Current.Dispatcher.Invoke(action);
        }

        private void TextBlock_NameSet(MouseRightClickWindow window)
        {
            window.buttonReadout_TextBlock.Text = MainWindowViewModel.DEFAULT_PROMPT_BUTTON == Settings.Default.MouseRightClickRebind ?
                MainWindowViewModel.DEFAULT_PROMPT_BUTTON : Settings.Default.MouseRightClickRebind.ToUpper().Replace("OEM", "");

            window.buttonReadout_TextBlock.FontSize = window.buttonReadout_TextBlock.Text.Length < MainWindowViewModel.DEFAULT_PROMPT_BUTTON.Length ?
                MainWindowViewModel.TEXTBLOCK_TEXT_LARGE : MainWindowViewModel.TEXTBLOCK_TEXT_SMALL;
        }

        private void TextBlockTimer_NameSet(MouseRightClickWindow window)
        {
            try
            {
                RunOnUIThread(() =>
                {
                    if (window.buttonReadout_TextBlock != null)
                    {
                        if (String.IsNullOrEmpty(Settings.Default.MouseRightClickRebind.Trim()))
                            Settings.Default.MouseRightClickRebind = MainWindowViewModel.DEFAULT_PROMPT_BUTTON;

                        // Load & Resize Text to Button Number TextBlock (only if changed)
                        if (window.buttonReadout_TextBlock.Text != Settings.Default.MouseRightClickRebind)
                        {
                            TextBlock_NameSet(window);
                        }
                    }
                });
            }
            catch (Exceptions ex)
            {
                ex.LogError("Error at MouseRightClick Window TextBlock Name Set");
            }
        }

        public void MouseRightClickWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Set this Window to position & width of parent window
                _thisWindow.Top = _mainWindow.Top;
                _thisWindow.Left = _mainWindow.Left;
                _thisWindow.MaxWidth = _mainWindow.Width;
                _thisWindow.MinWidth = _mainWindow.Width;

                // Joystick Polling bool engages functional & faster inner polling loop
                if (!Settings.Default.JoystickIsPolling)
                    Settings.Default.JoystickIsPolling = true;

                // Set the TextBlock Text of this Rebind to Saved Value
                TextBlock_NameSet(_thisWindow);

                // Create Timer to update TextBlock display
                textUpdateTimer.Elapsed += (s, ea) => TextBlockTimer_NameSet(_thisWindow);
                textUpdateTimer.Start();
            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in MouseRightClick ViewModel Loaded event");
                Exceptions.ExitApplication(1);
            }
        }

        public void MouseRightClickWindow_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                // Cleanup and Dispose of resources
                textUpdateTimer.Stop();
                textUpdateTimer.Dispose();

                // Clear 'Getting' flag for Right Rebind Key & call save
                Settings.Default.GettingRightRebindKey = false;
                Settings.Default.Save();
            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in MouseRightClick Window ViewModel on closing cleanup and save");
            }
        }


        #region controls

        // Right Mouse Click Rebind SAVE Button
        private ICommand _saveRightClickRebind;
        public ICommand SaveRightClickRebind
        {
            get
            {
                if (_saveRightClickRebind == null)
                {
                    _saveRightClickRebind = new RelayCommand<object, RoutedEventArgs>(Button_SaveRightClickRebind);
                }
                return _saveRightClickRebind;
            }
        }
        public async void Button_SaveRightClickRebind(object sender, RoutedEventArgs e)
        {
            try
            {
                // Set the button's content to the pressed image
                ButtonRightClickSaveImageSource = (ImageSource)Application.Current.Resources["panelButton_Pressed"];
                TranslateTransform transform = new TranslateTransform();
                _thisWindow.saveRightClick_TextBlock.RenderTransform = transform;
                transform.Y = 1;
                transform.X = -1;

                // Wait a moment...
                await Task.Delay(TimeSpan.FromSeconds(0.25));

                // Set the button's content to the Unpressed image
                ButtonRightClickSaveImageSource = (ImageSource)Application.Current.Resources["panelButton_Unpressed"];
                transform = new TranslateTransform();
                _thisWindow.saveRightClick_TextBlock.RenderTransform = transform;
                transform.Y = 0;
                transform.X = 0;

                // Wait a smidge...
                await Task.Delay(TimeSpan.FromSeconds(0.10));

            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in MouseRightClick Window ViewModel on save left click button");
            }
            finally
            {
                _thisWindow.Close();
            }
        }

        // Right Mouse Click Rebind CLEAR Button
        private ICommand _clearRightClickRebind;
        public ICommand ClearRightClickRebind
        {
            get
            {
                if (_clearRightClickRebind == null)
                {
                    _clearRightClickRebind = new RelayCommand<object, RoutedEventArgs>(Button_ClearRightClickRebind);
                }
                return _clearRightClickRebind;
            }
        }
        public async void Button_ClearRightClickRebind(object sender, RoutedEventArgs e)
        {
            try
            {
                // Set the scratched button's content to the pressed image
                ButtonRightClickClearImageSource = (ImageSource)Application.Current.Resources["panelButton2_Pressed"];
                TranslateTransform transform = new TranslateTransform();
                _thisWindow.clearRightClick_TextBlock.RenderTransform = transform;
                transform.Y = 1;
                transform.X = -1;

                // Wait a moment...
                await Task.Delay(TimeSpan.FromSeconds(0.25));

                // Set the scratched button's content to the Unpressed image
                ButtonRightClickClearImageSource = (ImageSource)Application.Current.Resources["panelButton2_Unpressed"];
                transform = new TranslateTransform();
                _thisWindow.clearRightClick_TextBlock.RenderTransform = transform;
                transform.Y = 0;
                transform.X = 0;

                // Wait a smidge...
                await Task.Delay(TimeSpan.FromSeconds(0.10));

                // Reset 
                Settings.Default.MouseRightRebindKey = MainWindowViewModel.DEFAULT_UNBOUND_KEY;
                Settings.Default.MouseRightClickRebind = MainWindowViewModel.DEFAULT_PROMPT_BUTTON;

            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in MouseRightClick Window ViewModel on clear left click button");
            }
            finally
            {
                _thisWindow.Close();
            }
        }

        #endregion


    }
}
