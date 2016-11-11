using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ZloGUILauncher
{
    public static class Helpers
    {
        public static T Find<T>(this ObservableCollection<T> Source , Predicate<T> predicate)
        {
            if (Source == null || predicate == null)
            {
                return default(T);
            }

            int count = Source.Count;
            for (int i = 0; i < count; i++)
            {
                var elem = Source[i];
                try
                {
                    if (predicate(elem))
                    {
                        return elem;
                    }
                }
                catch { }
            }
            return default(T);
        }
        public static void Remove<T>(this ObservableCollection<T> Source , Predicate<T> predicate)
        {
            if (Source == null || predicate == null)
            {
                return;
            }
            var element = Source.Find(predicate);
            if (element == null)
            {
                return;
            }
            else
            {
                Source.Remove(element);
            }
        }
    }
    public class ScrollViewerCorrector
    {
        public static bool GetFixScrolling(DependencyObject obj)
        {
            return (bool)obj.GetValue(FixScrollingProperty);
        }

        public static void SetFixScrolling(DependencyObject obj , bool value)
        {
            obj.SetValue(FixScrollingProperty , value);
        }

        public static readonly DependencyProperty FixScrollingProperty =
            DependencyProperty.RegisterAttached("FixScrolling" , typeof(bool) , typeof(ScrollViewerCorrector) , new FrameworkPropertyMetadata(false , ScrollViewerCorrector.OnFixScrollingPropertyChanged));

        public static void OnFixScrollingPropertyChanged(object sender , DependencyPropertyChangedEventArgs e)
        {
            ScrollViewer viewer = sender as ScrollViewer;
            if (viewer == null)
                throw new ArgumentException("The dependency property can only be attached to a ScrollViewer" , "sender");

            if ((bool)e.NewValue == true)
                viewer.PreviewMouseWheel += HandlePreviewMouseWheel;
            else if ((bool)e.NewValue == false)
                viewer.PreviewMouseWheel -= HandlePreviewMouseWheel;
        }
        private static List<MouseWheelEventArgs> _reentrantList = new List<MouseWheelEventArgs>();
        private static void HandlePreviewMouseWheel(object sender , MouseWheelEventArgs e)
        {
            var scrollControl = sender as ScrollViewer;
            if (!e.Handled && sender != null && !_reentrantList.Contains(e))
            {
                var previewEventArg = new MouseWheelEventArgs(e.MouseDevice , e.Timestamp , e.Delta)
                {
                    RoutedEvent = UIElement.PreviewMouseWheelEvent ,
                    Source = sender
                };
                var originalSource = e.OriginalSource as UIElement;
                _reentrantList.Add(previewEventArg);
                originalSource.RaiseEvent(previewEventArg);
                _reentrantList.Remove(previewEventArg);
                // at this point if no one else handled the event in our children, we do our job


                if (!previewEventArg.Handled && ((e.Delta > 0 && scrollControl.VerticalOffset == 0)
                    || (e.Delta <= 0 && scrollControl.VerticalOffset >= scrollControl.ExtentHeight - scrollControl.ViewportHeight)))
                {
                    e.Handled = true;
                    var eventArg = new MouseWheelEventArgs(e.MouseDevice , e.Timestamp , e.Delta);
                    eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                    eventArg.Source = sender;
                    var parent = (UIElement)((FrameworkElement)sender).Parent;
                    parent.RaiseEvent(eventArg);
                }
            }
        }
    }
}
