//MouseMasterVR mouse centering & rebind utility app
// by SemlerPDX Jan2023
// Copyright © 2023 CC BY-SA-NC
// VETERANS-GAMING.COM
//
using MahApps.Metro.Controls;
using MouseMasterVR.Properties;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VGLabsFoundation;

namespace MouseMasterVR
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        #region param

        public const string DEFAULT_PROCESS = "Falcon BMS";
        public const string DEFAULT_PROMPT = "Press Any Key";
        public const string DEFAULT_PROMPT_BUTTON = "Press Any Button";
        public const int DEFAULT_UNBOUND_KEY = 2323;

        public const int BUTTON_TEXT_SMALL = 10;
        public const int BUTTON_TEXT_LARGE = 12;

        public const int TEXTBLOCK_TEXT_SMALL = 10;
        public const int TEXTBLOCK_TEXT_LARGE = 12;
        public const int TEXTBLOCK_UPDATE_TIMER = 50; //in ms

        public const int TEXTBOX_TEXT_SMALL = 7;
        public const int TEXTBOX_TEXT_LARGE = 10;

        private const double EXPANDED_ROW_HEIGHT = 50;

        private static int stateDialPos = 1;

        #endregion

        #region properties

        // Main Window Size Properties ============
        private double _mainWindowTop;
        public double MainWindowTop
        {
            get { return _mainWindowTop; }
            set { _mainWindowTop = value; OnPropertyChanged(); }
        }

        private double _mainWindowLeft;
        public double MainWindowLeft
        {
            get { return _mainWindowLeft; }
            set { _mainWindowLeft = value; OnPropertyChanged(); }
        }

        private double _mainWindowWidth;
        public double MainWindowWidth
        {
            get { return _mainWindowWidth; }
            set { _mainWindowWidth = value; OnPropertyChanged(); }
        }


        // Main Window ImageSource Properties ============
        private ImageSource _appBackgroundImageSource;
        public ImageSource AppBackgroundImageSource
        {
            get { return _appBackgroundImageSource; }
            set { _appBackgroundImageSource = value; OnPropertyChanged(); }
        }

        private ImageSource _buttonDialImageSource;
        public ImageSource ButtonDialImageSource
        {
            get { return _buttonDialImageSource; }
            set { _buttonDialImageSource = value; OnPropertyChanged(); }
        }

        private ImageSource _toggleButtonImageSource;
        public ImageSource ToggleButtonImageSource
        {
            get { return _toggleButtonImageSource; }
            set { _toggleButtonImageSource = value; OnPropertyChanged(); }
        }

        private ImageSource _optionsButtonImageSource;
        public ImageSource OptionsButtonImageSource
        {
            get { return _optionsButtonImageSource; }
            set { _optionsButtonImageSource = value; OnPropertyChanged(); }
        }

        private ImageSource _buttonLeftClickMenuImageSource;
        public ImageSource ButtonLeftClickMenuImageSource
        {
            get { return _buttonLeftClickMenuImageSource; }
            set { _buttonLeftClickMenuImageSource = value; OnPropertyChanged(); }
        }
        private ImageSource _buttonRightClickMenuImageSource;
        public ImageSource ButtonRightClickMenuImageSource
        {
            get { return _buttonRightClickMenuImageSource; }
            set { _buttonRightClickMenuImageSource = value; OnPropertyChanged(); }
        }
        private ImageSource _buttonScrollUpMenuImageSource;
        public ImageSource ButtonScrollUpMenuImageSource
        {
            get { return _buttonScrollUpMenuImageSource; }
            set { _buttonScrollUpMenuImageSource = value; OnPropertyChanged(); }
        }
        private ImageSource _buttonScrollDownMenuImageSource;
        public ImageSource ButtonScrollDownMenuImageSource
        {
            get { return _buttonScrollDownMenuImageSource; }
            set { _buttonScrollDownMenuImageSource = value; OnPropertyChanged(); }
        }


        // Main Window ToggleButton IsChecked Properties ============
        private bool _isMouseCenteringEnabled;
        public bool IsMouseCenteringEnabled
        {
            get { return _isMouseCenteringEnabled; }
            set { _isMouseCenteringEnabled = value; OnPropertyChanged(); }
        }

        private bool _isCollapsedOptionsMenu;
        public bool IsCollapsedOptionsMenu
        {
            get { return _isCollapsedOptionsMenu; }
            set { _isCollapsedOptionsMenu = value; OnPropertyChanged(); }
        }


        // Main Window Process Target TextBox Property ============
        private string _textBoxTextProcess = Settings.Default.GameTargetName;
        public string TextBoxTextProcess
        {
            get { return _textBoxTextProcess; }
            set
            {
                _textBoxTextProcess = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TextBoxFontSizeProcess));
            }
        }

        private int _textBoxFontSizeProcess;
        public int TextBoxFontSizeProcess
        {
            get
            {
                return _textBoxTextProcess.Length > 14 ?
                  TEXTBOX_TEXT_SMALL : TEXTBOX_TEXT_LARGE;
            }
            set
            {
                _textBoxFontSizeProcess = value;
                OnPropertyChanged();
            }
        }

        #endregion


        private CancellationTokenSource _tokenSourceCentering = new CancellationTokenSource();
        public CancellationTokenSource _tokenSourcePolling = new CancellationTokenSource();
        private CancellationTokenSource _tokenSourceScrollUp = new CancellationTokenSource();
        private CancellationTokenSource _tokenSourceScrollDown = new CancellationTokenSource();

        private MouseBinding mouseBind;

        private readonly MainWindow _mainWindow;

        public MainWindowViewModel(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;

            // Assign the resource images to the binding ImageSource properties
            AppBackgroundImageSource = (ImageSource)Application.Current.Resources["backgroundSlate"];
            ToggleButtonImageSource = (ImageSource)Application.Current.Resources["toggleOFF"];
            OptionsButtonImageSource = (ImageSource)Application.Current.Resources["slideButton"];
            ButtonDialImageSource = (ImageSource)Application.Current.Resources["selectKnob2"];

            ButtonLeftClickMenuImageSource = (ImageSource)Application.Current.Resources["panelButton_Unpressed"];
            ButtonRightClickMenuImageSource = (ImageSource)Application.Current.Resources["panelButton_Unpressed"];
            ButtonScrollDownMenuImageSource = (ImageSource)Application.Current.Resources["panelButton2_Unpressed"];
            ButtonScrollUpMenuImageSource = (ImageSource)Application.Current.Resources["panelButton_Unpressed"];

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

        private void MouseCentering_Initialize()
        {
            try
            {
                MouseCentering centering = new MouseCentering();
                centering.Initialize(_tokenSourceCentering.Token);
            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in MainWindow ViewModel on mouse centering initialize");
            }
        }


        public void InitializeViewModel(object sender)
        {
            try
            {
                stateDialPos = Settings.Default.UpdateModeDial;

                // Initialize Mouse and Keyboard Binding Thread  TextBoxTextProcess
                mouseBind = new MouseBinding();
                mouseBind.Subscribe();

                // Begin Mouse Centering Model
                MouseCentering_Initialize();

                // This will remain until ready for release, placeholder setting initialization
                Settings.Default.JoystickIsPolling = false;

            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in MainWindow ViewModel Initialization");
                Exceptions.ExitApplication(1);
            }
        }

        public void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Center App on Primary Screen on First Use
                if (Settings.Default.FirstTimeUse)
                {
                    Settings.Default.WindowPosTop = (SystemParameters.PrimaryScreenHeight - _mainWindow.ActualHeight) / 2;
                    Settings.Default.WindowPosLeft = (SystemParameters.PrimaryScreenWidth - _mainWindow.ActualWidth) / 2;

                    // Display First Time Use Notice MessageBox
                    AppUpdates.FirstTimeUseNotice();
                }

                // Load Saved Settings
                _mainWindow.Top = Settings.Default.WindowPosTop;
                _mainWindow.Left = Settings.Default.WindowPosLeft;

                // Set the image of App Updates Mode Dial to point at Saved Value
                DialButton_ImageSet(Settings.Default.UpdateModeDial);
            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in MainWindow ViewModel Loaded event");
                Exceptions.ExitApplication(1);
            }
        }

        private void SaveSettings()
        {
            try
            {
                // Save the window position between sessions
                Settings.Default.WindowPosTop = _mainWindow.Top;
                Settings.Default.WindowPosLeft = _mainWindow.Left;
            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in MainWindow ViewModel unable to save WindowPos");
            }
            finally
            {
                Settings.Default.Save();
            }
        }

        public void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                // Cancel & Dispose of all tokens
                _tokenSourceCentering.Cancel();
                _tokenSourcePolling.Cancel();
                _tokenSourceScrollUp.Cancel();
                _tokenSourceScrollDown.Cancel();
                _tokenSourceCentering.Dispose();
                _tokenSourcePolling.Dispose();
                _tokenSourceScrollUp.Dispose();
                _tokenSourceScrollDown.Dispose();

                // Unbind mouse scroll wheel monitor
                if (mouseBind != null)
                    mouseBind.Unsubscribe();

                SaveSettings();
            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in MainWindow ViewModel on closing cleanup and save");
            }
        }


        #region controls

        // Toggle Mouse Centering MAIN ToggleButton
        private ICommand _toggleMouseCentering;
        public ICommand ToggleMouseCentering
        {
            get
            {
                if (_toggleMouseCentering == null)
                {
                    _toggleMouseCentering = new RelayCommand<object, RoutedEventArgs>(ToggleButton_MouseCentering);
                }
                return _toggleMouseCentering;
            }
        }
        private void ToggleButton_MouseCentering(object sender, RoutedEventArgs e)
        {
            try
            {
                // Set the image via ternary
                ToggleButtonImageSource = (ImageSource)Application.Current.Resources[IsMouseCenteringEnabled ? "toggleON" : "toggleOFF"];
                Settings.Default.MainPowerSwitch = IsMouseCenteringEnabled;
            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in MainWindow ViewModel on mouse centering toggle button");
            }
        }

        // App User Options EXPAND Button
        private ICommand _expandUserOptions;
        public ICommand ExpandUserOptions
        {
            get
            {
                if (_expandUserOptions == null)
                {
                    _expandUserOptions = new RelayCommand<object, RoutedEventArgs>(Button_ExpandUserOptions);
                }
                return _expandUserOptions;
            }
        }
        private void Button_ExpandUserOptions(object sender, RoutedEventArgs e)
        {
            try
            {
                // Set Animation
                GridLengthAnimation animation = new GridLengthAnimation
                {
                    From = IsCollapsedOptionsMenu ? new GridLength(0) : new GridLength(EXPANDED_ROW_HEIGHT),
                    To = IsCollapsedOptionsMenu ? new GridLength(EXPANDED_ROW_HEIGHT) : new GridLength(0),
                    Duration = TimeSpan.FromMilliseconds(125)
                };

                // Set the Height property of all hidden rows
                _mainWindow.FifthRow.BeginAnimation(RowDefinition.HeightProperty, animation);
                _mainWindow.SixthRow.BeginAnimation(RowDefinition.HeightProperty, animation);
#if DEBUG
                // Mouse Left/Right Click Rebind Buttons (Disabled for RELEASE, needs work - see comments re. SharpDX Input)
                _mainWindow.SeventhRow.BeginAnimation(RowDefinition.HeightProperty, animation);
#endif
                _mainWindow.EighthRow.BeginAnimation(RowDefinition.HeightProperty, animation);
                _mainWindow.NinthRow.BeginAnimation(RowDefinition.HeightProperty, animation);

                // Toggle the rotation of the Button between 0 and 180 degrees depending on the collapse state of the panel
                OptionsButtonImageSource = (ImageSource)Application.Current.Resources[IsCollapsedOptionsMenu ? "slideButton_Open" : "slideButton"];
            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in MainWindow ViewModel on expand options panel button");
            }
        }


        private void DialButton_ImageSet(int dialPos)
        {
            try
            {
                int[] knobs = { 1, 2, 3, 2 };
                ButtonDialImageSource = (ImageSource)Application.Current.Resources["selectKnob" + knobs[dialPos % 4]];
            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in MainWindow ViewModel on set dial button image source");
            }
        }


        // App Updates Mode DIAL Button
        private ICommand _setUpdatesModeDial;
        public ICommand SetUpdatesModeDial
        {
            get
            {
                if (_setUpdatesModeDial == null)
                {
                    _setUpdatesModeDial = new RelayCommand<object, RoutedEventArgs>(Button_SetUpdatesModeDial);
                }
                return _setUpdatesModeDial;
            }
        }
        private void Button_SetUpdatesModeDial(object sender, RoutedEventArgs e)
        {
            try
            {
                // Increment Dial Pos State within 0-3 range
                stateDialPos = (stateDialPos + 1) % 4;
                Settings.Default.UpdateModeDial = stateDialPos;
                DialButton_ImageSet(stateDialPos);
            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in MainWindow ViewModel on set dial button updates mode");
            }
        }


        // Left Mouse Click Rebind MENU Button
        private ICommand _openMouseClickWindow;
        public ICommand OpenMouseClickWindow
        {
            get
            {
                if (_openMouseClickWindow == null)
                {
                    _openMouseClickWindow = new RelayCommand<object, RoutedEventArgs>(Button_OpenMouseClickWindow);
                }
                return _openMouseClickWindow;
            }
        }
        private async void Button_OpenMouseClickWindow(object sender, RoutedEventArgs e)
        {
            try
            {
                // Set the button's content to the pressed image
                ButtonLeftClickMenuImageSource = (ImageSource)Application.Current.Resources["panelButton_Pressed"];
                TranslateTransform transform = new TranslateTransform();
                _mainWindow.openLeftClickWindow_TextBlock.RenderTransform = transform;
                transform.Y = 1;
                transform.X = -1;

                // Wait a moment...
                await Task.Delay(TimeSpan.FromSeconds(0.25));

                // Set the button's content to the Unpressed image
                ButtonLeftClickMenuImageSource = (ImageSource)Application.Current.Resources["panelButton_Unpressed"];
                transform = new TranslateTransform();
                _mainWindow.openLeftClickWindow_TextBlock.RenderTransform = transform;
                transform.Y = 0;
                transform.X = 0;

                // Wait a smidge...
                await Task.Delay(TimeSpan.FromSeconds(0.10));

                //Open Mouse Left Click Rebind Window
                MouseClickWindow mouseClickWindow = new MouseClickWindow(_mainWindow);
                mouseClickWindow.ShowDialog();
            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in MainWindow ViewModel on open left click window");
            }
        }


        // Right Mouse Click Rebind MENU Button
        private ICommand _openMouseRightClickWindow;
        public ICommand OpenMouseRightClickWindow
        {
            get
            {
                if (_openMouseRightClickWindow == null)
                {
                    _openMouseRightClickWindow = new RelayCommand<object, RoutedEventArgs>(Button_OpenMouseRightClickWindow);
                }
                return _openMouseRightClickWindow;
            }
        }
        private async void Button_OpenMouseRightClickWindow(object sender, RoutedEventArgs e)
        {
            try
            {
                // Set the button's content to the pressed image
                ButtonRightClickMenuImageSource = (ImageSource)Application.Current.Resources["panelButton_Pressed"];
                TranslateTransform transform = new TranslateTransform();
                _mainWindow.openRightClickWindow_TextBlock.RenderTransform = transform;
                transform.Y = 1;
                transform.X = -1;

                // Wait a moment...
                await Task.Delay(TimeSpan.FromSeconds(0.25));

                // Set the button's content to the Unpressed image
                ButtonRightClickMenuImageSource = (ImageSource)Application.Current.Resources["panelButton_Unpressed"];
                transform = new TranslateTransform();
                _mainWindow.openRightClickWindow_TextBlock.RenderTransform = transform;
                transform.Y = 0;
                transform.X = 0;

                // Wait a smidge...
                await Task.Delay(TimeSpan.FromSeconds(0.10));

                //Open Mouse Right Click Rebind Window
                MouseRightClickWindow mouseRightClickWindow = new MouseRightClickWindow(_mainWindow);
                mouseRightClickWindow.ShowDialog();
            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in MainWindow ViewModel on open right click window");
            }
        }


        // Mouse Scroll Up Rebind MENU Button
        private ICommand _openScrollUpWindow;
        public ICommand OpenScrollUpWindow
        {
            get
            {
                if (_openScrollUpWindow == null)
                {
                    _openScrollUpWindow = new RelayCommand<object, RoutedEventArgs>(Button_OpenScrollUpWindow);
                }
                return _openScrollUpWindow;
            }
        }
        private async void Button_OpenScrollUpWindow(object sender, RoutedEventArgs e)
        {
            try
            {
                // Set the scratched button's content to the pressed image
                ButtonScrollUpMenuImageSource = (ImageSource)Application.Current.Resources["panelButton_Pressed"];
                TranslateTransform transform = new TranslateTransform();
                _mainWindow.openScrollUpWindow_TextBlock.RenderTransform = transform;
                transform.Y = 1;
                transform.X = -1;

                // Wait a moment...
                await Task.Delay(TimeSpan.FromSeconds(0.25));

                // Set the scratched button's content to the Unpressed image
                ButtonScrollUpMenuImageSource = (ImageSource)Application.Current.Resources["panelButton_Unpressed"];
                transform = new TranslateTransform();
                _mainWindow.openScrollUpWindow_TextBlock.RenderTransform = transform;
                transform.Y = 0;
                transform.X = 0;

                // Wait a smidge...
                await Task.Delay(TimeSpan.FromSeconds(0.10));

                //Open Mouse Scroll Down Rebind Window
                MouseScrollUpWindow mouseScrollUpWindow = new MouseScrollUpWindow(_mainWindow);
                mouseScrollUpWindow.ShowDialog();
            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in MainWindow ViewModel on open scroll up window");
            }
        }


        // Mouse Scroll Down Rebind MENU Button
        private ICommand _openScrollDownWindow;
        public ICommand OpenScrollDownWindow
        {
            get
            {
                if (_openScrollDownWindow == null)
                {
                    _openScrollDownWindow = new RelayCommand<object, RoutedEventArgs>(Button_OpenScrollDownWindow);
                }
                return _openScrollDownWindow;
            }
        }
        private async void Button_OpenScrollDownWindow(object sender, RoutedEventArgs e)
        {
            try
            {
                // Set the button's content to the pressed image
                ButtonScrollDownMenuImageSource = (ImageSource)Application.Current.Resources["panelButton2_Pressed"];
                TranslateTransform transform = new TranslateTransform();
                _mainWindow.openScrollDownWindow_TextBlock.RenderTransform = transform;
                transform.Y = 1;
                transform.X = -1;

                // Wait a moment...
                await Task.Delay(TimeSpan.FromSeconds(0.25));

                // Set the button's content to the Unpressed image
                ButtonScrollDownMenuImageSource = (ImageSource)Application.Current.Resources["panelButton2_Unpressed"];
                transform = new TranslateTransform();
                _mainWindow.openScrollDownWindow_TextBlock.RenderTransform = transform;
                transform.Y = 0;
                transform.X = 0;

                // Wait a smidge...
                await Task.Delay(TimeSpan.FromSeconds(0.10));

                //Open Mouse Scroll Down Rebind Window
                MouseScrollDownWindow mouseScrollDownWindow = new MouseScrollDownWindow(_mainWindow);
                mouseScrollDownWindow.ShowDialog();
            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in MainWindow ViewModel on open scroll down window");
            }
        }


        // Mouse Centering Process Target TextBox
        private ICommand _resizeTextProcessTargetTextbox;
        public ICommand ResizeTextProcessTargetTextbox
        {
            get
            {
                if (_resizeTextProcessTargetTextbox == null)
                {
                    _resizeTextProcessTargetTextbox = new RelayCommand<object, RoutedEventArgs>(TextBox_ResizeProcessTarget);
                }
                return _resizeTextProcessTargetTextbox;
            }
        }
        public void TextBox_ResizeProcessTarget(object sender, RoutedEventArgs e)
        {
            try
            {
                RunOnUIThread(() =>
                {
                    // Update the Binding so it can update the FontSize Binding
                    TextBoxTextProcess = _mainWindow.setProcessTarget_TextBox.Text;
                });
            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in MainWindow ViewModel on textbox resize process target");
            }
        }


        private ICommand _resetTextProcessTargetTextbox;
        public ICommand ResetTextProcessTargetTextbox
        {
            get
            {
                if (_resetTextProcessTargetTextbox == null)
                {
                    _resetTextProcessTargetTextbox = new RelayCommand<object, RoutedEventArgs>(TextBox_ResetTextProcessTarget);
                }
                return _resetTextProcessTargetTextbox;
            }
        }
        public void TextBox_ResetTextProcessTarget(object sender, RoutedEventArgs e)
        {
            try
            {
                RunOnUIThread(() =>
                {
                    // Revert to saved value
                    TextBoxTextProcess = Settings.Default.GameTargetName;
                });
            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in MainWindow ViewModel on textbox reset process target");
            }
        }


        private ICommand _setProcessTarget_Hotkeys;
        public ICommand SetProcessTarget_Hotkeys
        {
            get
            {
                if (_setProcessTarget_Hotkeys == null)
                {
                    _setProcessTarget_Hotkeys = new RelayCommand<object, KeyEventArgs>(TextBox_SetProcessTarget_Hotkeys);
                }
                return _setProcessTarget_Hotkeys;
            }
        }
        private void TextBox_SetProcessTarget_Hotkeys(object sender, KeyEventArgs e)
        {
            try
            {
                RunOnUIThread(() =>
                {
                    // Enter & Escape keys will exit TextBox entry field
                    if (e.Key == Key.Enter || e.Key == Key.Return || e.Key == Key.Escape)
                    {
                        // Enter key will save text to settings
                        if (e.Key == Key.Enter || e.Key == Key.Return)
                        {
                            // Default the TextBox Process Target Name if empty
                            if (string.IsNullOrEmpty(TextBoxTextProcess.Trim()))
                            {
                                TextBoxTextProcess = DEFAULT_PROCESS;
                            }

                            Settings.Default.GameTargetName = TextBoxTextProcess;
                        }

                        e.Handled = true;
                        Keyboard.ClearFocus();
                    }
                });
            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in MainWindow ViewModel on textbox process target hotkeys");
            }
        }


        private ICommand _mainWindow_Hotkeys;
        public ICommand MainWindow_Hotkeys
        {
            get
            {
                if (_mainWindow_Hotkeys == null)
                {
                    _mainWindow_Hotkeys = new RelayCommand<object, KeyEventArgs>(PreviewKeyDown_MainWindow_Hotkeys);
                }
                return _mainWindow_Hotkeys;
            }
        }
        private void PreviewKeyDown_MainWindow_Hotkeys(object sender, KeyEventArgs e)
        {
            try
            {
                // Only Handle Hotkey(s) if not in TextBox
                if (!(FocusManager.GetFocusedElement(_mainWindow) is TextBox))
                {
                    // Control + Arrow Key (any direction) will center App to primary monitor
                    if (Keyboard.Modifiers == ModifierKeys.Control ||
                        Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down)
                        {
                            // Move window to the center of the main monitor
                            _mainWindow.Top = (SystemParameters.PrimaryScreenHeight - _mainWindow.ActualHeight) / 2;
                            _mainWindow.Left = (SystemParameters.PrimaryScreenWidth - _mainWindow.ActualWidth) / 2;
                        }
                    }
                    // Alt + F4 will gracefully shutdown application
                    else if ((Keyboard.Modifiers == ModifierKeys.Alt ||
                        Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)) && e.SystemKey == Key.F4)
                    {
                        // Close will trigger OnClosing in ViewModel
                        e.Handled = true;
                        _mainWindow.Close();
                    }
                    e.Handled = true;
                }
            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in MainWindow ViewModel on main hotkeys");
            }
        }



        // Toggle Mouse Centering MAIN ToggleButton
        private ICommand _openRepositoryButton;
        public ICommand OpenRepositoryButton
        {
            get
            {
                if (_openRepositoryButton == null)
                {
                    _openRepositoryButton = new RelayCommand<object, RoutedEventArgs>(Button_OpenRepositoryLink);
                }
                return _openRepositoryButton;
            }
        }
        private void Button_OpenRepositoryLink(object sender, RoutedEventArgs e)
        {
            try
            {
                string myRepositoriesLink = "https://github.com/SemlerPDX?tab=repositories";
                Applications.LaunchApp(myRepositoriesLink);
            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in MainWindow ViewModel on open repositories link button");
            }
        }

        #endregion


    }
}
