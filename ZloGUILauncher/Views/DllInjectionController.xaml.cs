using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using Zlo.Extras;

namespace ZloGUILauncher.Views
{
    /// <summary>
    /// Interaction logic for DllInjectionController.xaml
    /// </summary>
    public partial class DllInjectionController : UserControl
    {
        public DllInjectionController()
        {
            InitializeComponent();
        }


        public ZloGame SelectedGame
        {
            get { return (ZloGame)GetValue(SelectedGameProperty); }
            set { SetValue(SelectedGameProperty , value); }
        }

        // Using a DependencyProperty as the backing store for SelectedGame.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedGameProperty =
            DependencyProperty.Register("SelectedGame" , typeof(ZloGame) , typeof(DllInjectionController) , new PropertyMetadata(default(ZloGame) , SelectedGameChanged));

        private List<string> QuickDlls
        {
            get
            {
                return App.Client?.GetDllsList(SelectedGame);
            }
        }


        private static void SelectedGameChanged(DependencyObject d , DependencyPropertyChangedEventArgs e)
        {
            if (App.Client == null || !App.Client.IsConnectedToZCLient)
            {
                return;
            }
            var owner = d as DllInjectionController;
            owner.ViewerListBox.ItemsSource = App.Client.GetDllsList((ZloGame)e.NewValue);
        }
        OpenFileDialog of;
        private void AddInjectedDllButton_Click(object sender , RoutedEventArgs e)
        {
            if (of == null)
            {
                of = new OpenFileDialog();
            }
            of.Reset();
            of.Title = "Please select the dll you want to inject";
            of.ValidateNames = true;
            of.DefaultExt = ".dll";
            of.ShowReadOnly = false;
            of.AddExtension = true;
            of.CheckFileExists = true;
            of.CheckPathExists = true;
            of.Filter = "*.dll|*.dll";
            of.FilterIndex = 0;
            of.Multiselect = true;
            var res = of.ShowDialog();
            if (res.HasValue && res.Value)
            {
                var list = QuickDlls;
                if (list != null)
                {
                    foreach (var item in of.FileNames)
                    {
                        if (!list.Contains(item))
                        {
                            list.Add(item);
                        }
                    }
                    ViewerListBox.Items.Refresh();
                    App.Client.SaveSettings();
                }
            }
        }

        private void RemoveInjectedDllsButton_Click(object sender , RoutedEventArgs e)
        {
            var selected_items = ViewerListBox.SelectedItems.Cast<string>();
            var dz = QuickDlls;
            if (dz != null)
            {
                foreach (var item in selected_items)
                {
                    dz.Remove(item);
                }
                ViewerListBox.Items.Refresh();
                App.Client.SaveSettings();
            }
        }
    }
}
