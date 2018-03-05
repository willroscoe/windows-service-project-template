using $ext_projectname$.Core;
using $ext_projectname$.Core.Models;
using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace $safeprojectname$
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Hello World Method button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void HelloWorldMethod_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // setup logging output from API
                var progressHandler = new Progress<LogModel>(value =>
                {
                    UpdateLog(value.LogType, value.Message);
                });

                var progress = progressHandler as IProgress<LogModel>;

                // run the test xml method
                await new TaskProcess(progress).RunHelloWorld(CheckBox_HelloWorldMethodTimeElapsed.IsChecked ?? false);

            }
            catch (Exception ex)
            {
                UpdateLog(LogType.Error, "Exception! " + ex.Message);
            }
        }

        /// <summary>
        /// Output log entries into rich text box
        /// </summary>
        /// <param name="type"></param>
        /// <param name="msg"></param>
        private void UpdateLog(LogType type, string msg = "")
        {
            var outputTxt = string.Format("{0} - {1}", DateTime.Now.ToTimeStamp(), msg);

            Paragraph paragraph = new Paragraph(new Run(outputTxt));
            paragraph.Margin = new Thickness(0); // remove spacing between paragraphs

            switch (type)
            {
                case LogType.BeginEnd:
                    paragraph.Foreground = Brushes.Gray;
                    break;
                case LogType.Minor:
                    paragraph.Foreground = Brushes.Gray;
                    break;
                case LogType.Error:
                    paragraph.Foreground = Brushes.Red;
                    break;
                case LogType.Highlight:
                    paragraph.FontWeight = FontWeights.Bold;
                    break;
                default:
                    paragraph.Foreground = Brushes.Black;
                    break;
            }
            LogOutput.Document.Blocks.Add(paragraph);
            LogOutput.ScrollToEnd();
        }

        /// <summary>
        /// Clears the log ouput
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            LogOutput.Document.Blocks.Clear();
        }
    }
}
