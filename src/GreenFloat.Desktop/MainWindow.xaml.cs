using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace GreenFloat.Desktop
{
    /// <summary>
    /// Interaction logic for _MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region PublicVariables
        public int WindowHeight
        {
            get { return _windowHeight; }
            set
            {
                if (value != _windowHeight)
                {
                    _windowHeight = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("WindowHeight"));
                }
            }
        }
        public int WindowWidth
        {
            get { return _windowWidth; }
            set
            {
                if (value != _windowWidth)
                {
                    _windowWidth = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("WindowWidth"));
                }
            }
        }

        public ICommand ExitCommand
        {
            get { return _exitCommand; }
            set
            {
                if( value != _exitCommand)
                {
                    _exitCommand = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("ExitCommand"));
                }
            }
        }

        public ICommand OpenCommand
        {
            get { return _openCommand; }
            set
            {
                if (value != _openCommand)
                {
                    _openCommand = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("OpenCommand"));
                }
            }
        }

        public ICommand SaveAsCommand
        {
            get { return _saveAsCommand; }
            set
            {
                if (value != _saveAsCommand)
                {
                    _saveAsCommand = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("SaveAsCommand"));
                }
            }
        }

        public ICommand ResetZoomCommand
        {
            get { return _resetZoomCommand; }
            set
            {
                if (value != _resetZoomCommand)
                {
                    _resetZoomCommand = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("ResetZoomCommand"));
                }
            }
        }

        public ICommand CopyCommand
        {
            get { return _copyCommand; }
            set
            {
                if (value != _copyCommand)
                {
                    _copyCommand = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("CopyCommand"));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region PrivateVariables
        private string _currentImageSource;
        private int _windowHeight;
        private int _windowWidth;
        private ICommand _exitCommand;
        private ICommand _openCommand;
        private ICommand _saveAsCommand;
        private ICommand _resetZoomCommand;
        private ICommand _copyCommand;

        private User32Helper user32Helper;
        #endregion

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            WindowHeight = 350;
            WindowWidth = 525;

            ExitCommand = new Command((object parameters) => { return true; }, (object parameters) => { Application.Current.Shutdown(); });
            OpenCommand = new Command((object parameters) => { return true; }, (object parameters) => { this.LaunchOpenDialog(); });
            ResetZoomCommand = new Command((object parameters) => { return !string.IsNullOrEmpty(_currentImageSource); }, (object parameters) => { ImgZoomBorder.Reset(); });
            SaveAsCommand = new Command((object parameters) => { return !string.IsNullOrEmpty(_currentImageSource); }, (object parameters) => { this.LaunchSaveAsDialog(); });
            CopyCommand = new Command((object parameters) => { return !string.IsNullOrEmpty(_currentImageSource); }, (object parameters) => { this.CopyToClipboard(); });

            this.SourceInitialized += Window_SourceInitialized;
        }
        #endregion

        #region PrivateMethods
        private void SetImage(string imageUri, bool resizeWindow)
        {

            if (imageUri != "" && imageUri != null)
            {
                ImgZoomBorder.Reset();
                mainImage.RenderTransform = new MatrixTransform();

                BitmapImage newImage = new BitmapImage();
                newImage.BeginInit();
                newImage.UriSource = new Uri(imageUri, UriKind.Absolute);
                newImage.EndInit();
                mainImage.Source = newImage;

                this._currentImageSource = imageUri;
            }

            if (resizeWindow)
            {
                var heightOffset = GetHeightOffset();
                var widthOffset = GetWidthOffset();

                if (mainImage.Source.Height <= System.Windows.SystemParameters.WorkArea.Height)
                        this.WindowHeight = (int)mainImage.Source.Height + (int)heightOffset;
                    else
                        this.WindowHeight = (int)System.Windows.SystemParameters.WorkArea.Height;

                if (mainImage.Source.Width <= System.Windows.SystemParameters.WorkArea.Width)
                    this.WindowWidth = (int)mainImage.Source.Width + (int)widthOffset;
                else
                    this.WindowWidth = (int)System.Windows.SystemParameters.WorkArea.Width;

            }

            var fileName = System.IO.Path.GetFileName(_currentImageSource);
            this.Title = $"{fileName} - GreenFloat";

            this.AutoSize();
        }
        

       
        public void LaunchOpenDialog()
        {
            var ofd = new OpenFileDialog();
            try
            {
                

                ofd.Filter = "Image Files (*.bmp, *.jpg, *.png)|*.bmp;*.jpg;*.jpeg;*.png";
                ofd.Multiselect = false;

                var result = ofd.ShowDialog();



                if (result == true)
                {
                    Dispatcher.Invoke(new Action<string, bool>(SetImage), ofd.FileName, true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening file '{ofd.FileName}': {ex.Message}", "Open Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        public void LaunchSaveAsDialog()
        {
            var sfd = new SaveFileDialog();
            try
            {
                if (string.IsNullOrWhiteSpace(_currentImageSource))
                {
                    MessageBox.Show("Cannot proceed with Save As, no image has been loaded.", "No Image", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    return;
                }

                
                sfd.CreatePrompt = false;
                sfd.OverwritePrompt = true;
                sfd.ValidateNames = true;
                sfd.FileName = System.IO.Path.GetFileName(_currentImageSource);
                sfd.AddExtension = true;

                var ext = System.IO.Path.GetExtension(_currentImageSource);

                sfd.DefaultExt = ext;
                sfd.Filter = $"Image Files ({ext})|{ext}";

                var result = sfd.ShowDialog(this);
                if (result.Value)
                {
                    if (!_currentImageSource.Equals(sfd.FileName, StringComparison.CurrentCultureIgnoreCase))
                        File.Copy(_currentImageSource, sfd.FileName, true);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Error saving file '{sfd.FileName}': {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CopyToClipboard()
        {
            if (string.IsNullOrWhiteSpace(_currentImageSource))
            {
                MessageBox.Show("Cannot proceed with Copy, no image has been loaded.", "No Image", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }

            Clipboard.SetImage((BitmapSource)mainImage.Source);
        }
        
        private void Resize(double x, double y)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => _MainWindow.Width = x));
            Application.Current.Dispatcher.BeginInvoke(new Action(() => _MainWindow.Height = y));
        }

        private void AutoSize()
        {
            ;
            if ((this.WindowState == System.Windows.WindowState.Maximized || this.WindowState == System.Windows.WindowState.Minimized))
                return;

            double aspectRatio = mainImage.Source.Width / mainImage.Source.Height;
            var heightOffset = GetHeightOffset();
            var widthOffset = GetWidthOffset();

            double windowHeight = _MainWindow.ActualHeight;
            double newWinWidth = ((windowHeight - heightOffset) * aspectRatio) + widthOffset;
            Resize(newWinWidth, windowHeight);
            this.WindowWidth = (int)newWinWidth;
        }

        private double GetHeightOffset()
        {
            return this.MainBorder.BorderThickness.Top + this.MainBorder.BorderThickness.Bottom + this.MainBorder.Margin.Top + this.MainBorder.Margin.Bottom;
        }

        private double GetWidthOffset()
        {
            return this.MainBorder.BorderThickness.Left + this.MainBorder.BorderThickness.Right + this.MainBorder.Margin.Left + this.MainBorder.Margin.Right;
        }

        #endregion

        #region EventHandlers
        private void Window_SourceInitialized(object sender, EventArgs ea)
        {
            HwndSource hwndSource = (HwndSource)HwndSource.FromVisual((Window)sender);
            var aspectRatio = this.Width / this.Height;


            user32Helper = new User32Helper(this);
            user32Helper.SetMaximizeMenuItem(true);
            user32Helper.SetMinimizeMenuItem(true);
            user32Helper.SetRestoreMenuItem(true);
            user32Helper.SetResizeMenuItem(false);

            user32Helper.LockAspectRatio(aspectRatio);

            var mouseLocation = User32Helper.GetMousePosition();

            var transform = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
            var mouse = transform.Transform(mouseLocation);
            
            this.Left = mouse.X - (this.ActualWidth / 2);
            this.Top = mouse.Y - (this.ActualHeight / 2);
        }

        private void _MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // prevent error when clicking outside of window area to close a dialog box
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void mainImage_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {

                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files.Length > 0)
                {

                    Dispatcher.Invoke(new Action<string, bool>(SetImage), files[0], true);
                }
            }

        }

        private void _MainWindow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.WindowState != System.Windows.WindowState.Maximized)
                this.WindowState = System.Windows.WindowState.Maximized;
            else
                this.WindowState = System.Windows.WindowState.Normal;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }

        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;

        }

        private void _MainWindow_StateChanged(object sender, EventArgs e)
        {
            mainImage.Focus();
        }
        
        private void _MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this._currentImageSource = "";

            if (Application.Current.Properties["CmdLineArgs"].ToString() != "NoArgs")
            {
                this.SetImage(Application.Current.Properties["CmdLineArgs"].ToString(), true);
            }
        }

        private void _MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (mainImage.Source != null)
                AutoSize();
        }

        private void SystemMenuButton_Click(object sender, RoutedEventArgs e)
        {
            user32Helper.ShowContextMenu(PointToScreen(new Point(25, 25)));
        }

        private void SystemMenuButton_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
        #endregion


    }
}
