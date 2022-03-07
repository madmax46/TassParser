using NLog;
using NLog.Targets;
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

namespace TassParserController
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Logger logger = LogManager.GetCurrentClassLogger();

        private Model model;
        public List<string> loggerStringsView = new List<string>();
        private System.Windows.Forms.NotifyIcon notifyIcon;

        ConsoleContent dc = new ConsoleContent();
        public MainWindow()
        {
            InitializeComponent();

            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Click += NotifyIcon_Click;
            notifyIcon.BalloonTipText = "The app has been minimised. Click the tray icon to show.";
            notifyIcon.BalloonTipTitle = "The App";
            notifyIcon.Text = "The App";
            notifyIcon.Icon = new System.Drawing.Icon("appIcon.ico");



            DataContext = dc;
            Loaded += MainWindow_Loaded;
            InitNlog();
            model = new Model();
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            ShowInTaskbar = true;
            notifyIcon.Visible = false;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //InputBlock.KeyDown += InputBlock_KeyDown;
            //InputBlock.Focus();
        }

        void InputBlock_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //dc.ConsoleInput = InputBlock.Text;
                //dc.RunCommand();
                //InputBlock.Focus();
                //Scroller.ScrollToBottom();
            }
        }


        private void InitNlog()
        {
            var config = new NLog.Config.LoggingConfiguration();
            MethodCallTarget target = new MethodCallTarget("TargetTextBox", GetLog);
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, target);
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "log.txt" };
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logfile);
            LogManager.Configuration = config;
        }

        public void GetLog(LogEventInfo logEvent, object[] parms)
        {
            string msg = $"{logEvent.TimeStamp} | {logEvent.Level} | {logEvent.LoggerName} | {logEvent.FormattedMessage} {logEvent.Exception}";
            loggerStringsView.Add(msg);

            try
            {
                Dispatcher.Invoke(() =>
                {
                    //dc.ConsoleInput = msg;
                    //dc.RunCommand();
                    if (msg.Length > 1000)
                    {
                        dc.RunCommand(msg.Substring(0, 1000));
                    }
                    else
                    {
                        dc.RunCommand(msg);
                    }

                    Scroller.ScrollToBottom();
                    //UILogger.Text = string.Join(Environment.NewLine, loggerStringsView);
                    //CheckOnLogLength();
                    //UILogger.CaretIndex = UILogger.Text.Length;
                    //UILogger.ScrollToEnd();
                });
            }
            catch (Exception ex)
            {

            }
            ////UILogger.Focus();
            ////UILogger.CaretIndex = UILogger.Text.Length;
        }


        private void CheckOnLogLength()
        {
            if (loggerStringsView.Count > 1000)
            {
                while (loggerStringsView.Count > 1000)
                {
                    loggerStringsView.RemoveAt(0);
                }
            }
        }
        private void UIStartCrawler_Click(object sender, RoutedEventArgs e)
        {
            model.StartCrawler();
        }

        private void UIStartParseNews_Click(object sender, RoutedEventArgs e)
        {
            model.StartParsingNews();
        }

        private void UIStopCrawler_Click(object sender, RoutedEventArgs e)
        {
            model.StopCrawler();
        }

        private void UIStopParseNews_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_StateChanged(object sender, EventArgs e)
        {

            switch (this.WindowState)
            {
                case WindowState.Maximized:
                    break;
                case WindowState.Minimized:
                    this.Hide();
                    notifyIcon.Visible = true;
                    ShowInTaskbar = false;
                    break;
                case WindowState.Normal:

                    break;
            }


        }

    }
}
