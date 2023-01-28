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
    class MouseClickWindowViewModel : INotifyPropertyChanged
    {
        #region properties

        // Mouse Left Click ImageSource Properties
        private ImageSource _buttonLeftClickSaveImageSource;
        public ImageSource ButtonLeftClickSaveImageSource
        {
            get { return _buttonLeftClickSaveImageSource; }
            set { _buttonLeftClickSaveImageSource = value; OnPropertyChanged(); }
        }

        private ImageSource _buttonLeftClickClearImageSource;
        public ImageSource ButtonLeftClickClearImageSource
        {
            get { return _buttonLeftClickClearImageSource; }
            set { _buttonLeftClickClearImageSource = value; OnPropertyChanged(); }
        }

        #endregion


        public CancellationTokenSource _tokenSourceClick = new CancellationTokenSource();

        private System.Timers.Timer textUpdateTimer = new System.Timers.Timer(MainWindowViewModel.TEXTBLOCK_UPDATE_TIMER);

        private readonly MainWindow _mainWindow;
        private readonly MouseClickWindow _thisWindow;

        public MouseClickWindowViewModel(MainWindow mainWindow, MouseClickWindow thisWindow)
        {
            _mainWindow = mainWindow;
            _thisWindow = thisWindow;

            // Assign the resource images to the binding ImageSource properties
            ButtonLeftClickSaveImageSource = (ImageSource)Application.Current.Resources["panelButton2_Unpressed"];
            ButtonLeftClickClearImageSource = (ImageSource)Application.Current.Resources["panelButton_Unpressed"];
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void InitializeMouseClickViewModel(object sender)
        {
            try
            {
                //THIS WORKS - BUT ONLY WHEN APP IN FOCUS (NEEDS WORK)
                //----------------------------------------------------------------------------
                // Inform Joystick Polling that LeftClick Rebind is being set (wip placeholder)
                Settings.Default.GettingLeftRebindKey = true;
                //----------------------------------------------------------------------------
            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in MouseClick Window ViewModel Initialize event");
                Exceptions.ExitApplication(1);
            }
        }

        private void RunOnUIThread(Action action)
        {
            // Dispatch Helper Method
            Application.Current.Dispatcher.Invoke(action);
        }

        private void TextBlock_NameSet(MouseClickWindow window)
        {
            window.buttonReadout_TextBlock.Text = MainWindowViewModel.DEFAULT_PROMPT_BUTTON == Settings.Default.MouseLeftClickRebind ?
                MainWindowViewModel.DEFAULT_PROMPT_BUTTON : Settings.Default.MouseLeftClickRebind.ToUpperInvariant().Replace("OEM", "");

            window.buttonReadout_TextBlock.FontSize = window.buttonReadout_TextBlock.Text.Length < MainWindowViewModel.DEFAULT_PROMPT_BUTTON.Length ?
                MainWindowViewModel.TEXTBLOCK_TEXT_LARGE : MainWindowViewModel.TEXTBLOCK_TEXT_SMALL;
        }

        private void TextBlockTimer_NameSet(MouseClickWindow window)
        {
            try
            {
                RunOnUIThread(() =>
                {
                    if (window.buttonReadout_TextBlock != null)
                    {
                        //TextBlock textBlock = buttonTextblock;
                        if (String.IsNullOrEmpty(Settings.Default.MouseLeftClickRebind.Trim()))
                            Settings.Default.MouseLeftClickRebind = MainWindowViewModel.DEFAULT_PROMPT_BUTTON;

                        // Load & Resize Text to Button Number TextBlock (only if changed)
                        if (window.buttonReadout_TextBlock.Text != Settings.Default.MouseLeftClickRebind)
                        {
                            TextBlock_NameSet(window);
                        }
                    }
                });
            }
            catch (Exceptions ex)
            {
                ex.LogError("Error at MouseClick Window TextBlock Name Set");
            }
        }

        public void MouseClickWindow_Loaded(object sender, RoutedEventArgs e)
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
                ex.LogError("Error in MouseClick Window ViewModel Loaded event");
                Exceptions.ExitApplication(1);
            }
        }

        public void MouseClickWindow_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                // Cleanup and Dispose of resources
                textUpdateTimer.Stop();
                textUpdateTimer.Dispose();

                // Clear 'Getting' flag for Left Rebind Key & call save
                Settings.Default.GettingLeftRebindKey = false;
                Settings.Default.Save();
            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in MouseClick Window ViewModel on closing cleanup and save");
            }
        }


        #region controls

        // Left Mouse Click Rebind SAVE Button
        private ICommand _saveLeftClickRebind;
        public ICommand SaveLeftClickRebind
        {
            get
            {
                if (_saveLeftClickRebind == null)
                {
                    _saveLeftClickRebind = new RelayCommand<object, RoutedEventArgs>(Button_SaveLeftClickRebind);
                }
                return _saveLeftClickRebind;
            }
        }
        public async void Button_SaveLeftClickRebind(object sender, RoutedEventArgs e)
        {
            try
            {
                // Set the scratched button's content to the pressed image
                ButtonLeftClickSaveImageSource = (ImageSource)Application.Current.Resources["panelButton2_Pressed"];
                TranslateTransform transform = new TranslateTransform();
                _thisWindow.saveLeftClick_TextBlock.RenderTransform = transform;
                transform.Y = 1;
                transform.X = -1;

                // Wait a moment...
                await Task.Delay(TimeSpan.FromSeconds(0.25));

                // Set the scratched button's content to the Unpressed image
                ButtonLeftClickSaveImageSource = (ImageSource)Application.Current.Resources["panelButton2_Unpressed"];
                transform = new TranslateTransform();
                _thisWindow.saveLeftClick_TextBlock.RenderTransform = transform;
                transform.Y = 0;
                transform.X = 0;

                // Wait a smidge...
                await Task.Delay(TimeSpan.FromSeconds(0.10));

            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in MouseClick Window ViewModel on save left click button");
            }
            finally
            {
                _thisWindow.Close();
            }
        }

        // Left Mouse Click Rebind CLEAR Button
        private ICommand _clearLeftClickRebind;
        public ICommand ClearLeftClickRebind
        {
            get
            {
                if (_clearLeftClickRebind == null)
                {
                    _clearLeftClickRebind = new RelayCommand<object, RoutedEventArgs>(Button_ClearLeftClickRebind);
                }
                return _clearLeftClickRebind;
            }
        }
        public async void Button_ClearLeftClickRebind(object sender, RoutedEventArgs e)
        {
            try
            {
                // Set the button's content to the pressed image
                ButtonLeftClickClearImageSource = (ImageSource)Application.Current.Resources["panelButton_Pressed"];
                TranslateTransform transform = new TranslateTransform();
                _thisWindow.clearLeftClick_TextBlock.RenderTransform = transform;
                transform.Y = 1;
                transform.X = -1;

                // Wait a moment...
                await Task.Delay(TimeSpan.FromSeconds(0.25));

                // Set the button's content to the Unpressed image
                ButtonLeftClickClearImageSource = (ImageSource)Application.Current.Resources["panelButton_Unpressed"];
                transform = new TranslateTransform();
                _thisWindow.clearLeftClick_TextBlock.RenderTransform = transform;
                transform.Y = 0;
                transform.X = 0;

                // Wait a smidge...
                await Task.Delay(TimeSpan.FromSeconds(0.10));

                // Reset 
                Settings.Default.MouseLeftRebindKey = MainWindowViewModel.DEFAULT_UNBOUND_KEY;
                Settings.Default.MouseLeftClickRebind = MainWindowViewModel.DEFAULT_PROMPT_BUTTON;

            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in MouseClick Window ViewModel on clear left click button");
            }
            finally
            {
                _thisWindow.Close();
            }
        }

        #endregion


    }
}
