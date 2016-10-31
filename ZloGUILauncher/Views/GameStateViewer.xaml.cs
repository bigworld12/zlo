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
using System.Windows.Shapes;
using Zlo.Extras;

namespace ZloGUILauncher.Views
{
    /// <summary>
    /// Interaction logic for GameStateViewer.xaml
    /// </summary>
    public partial class GameStateViewer : Window
    {
        public GameStateViewer()
        {
            InitializeComponent();
        }

        public void StateReceived(ZloGame game , string type , string message)
        {
            var t = DateTime.Now;
            Run DateText = new Run($"{t.ToShortTimeString()} : ");
            DateText.Foreground = new SolidColorBrush(Colors.White);

            Run GameText = new Run($"[{game}] ");
            GameText.Foreground = new SolidColorBrush(Colors.LightGreen);

            Run TypeText = new Run($"[{type}] ");
            TypeText.Foreground = new SolidColorBrush(Color.FromRgb(77 , 188 , 233));

            Run MessageText = new Run($"{message}");
            MessageText.Foreground = new SolidColorBrush(Colors.White);

            Paragraph NewParagraph = new Paragraph();
            NewParagraph.Inlines.Add(DateText);
            NewParagraph.Inlines.Add(GameText);
            NewParagraph.Inlines.Add(TypeText);
            NewParagraph.Inlines.Add(MessageText);

            StateTextBox.Document.Blocks.Add(NewParagraph);
        }

        private void ClearButton_Click(object sender , RoutedEventArgs e)
        {
            StateTextBox.Document.Blocks?.Clear();
        }

        private void Window_Closing(object sender , System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
