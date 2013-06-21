using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MangaEpsilon.AttachedProperties
{
    //http://www.hardcodet.net/2009/05/trigger-wpf-animations-through-attached-events
    public static class LoadedOnceHelper
    {
        public static readonly DependencyProperty HasLoadedBeforeProperty = DependencyProperty.RegisterAttached(
            "HasLoadedBefore", typeof(bool), typeof(UIElement), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));


        public static void SetHasLoadedBefore(UIElement element, object value)
        {
            if (value == null || !(value is bool)) throw new ArgumentNullException("value");
            element.SetValue(HasLoadedBeforeProperty, value);

        }

        public static object GetHasLoadedBefore(UIElement element)
        {
            return element.GetValue(HasLoadedBeforeProperty);
        }

        public static readonly DependencyProperty HandleLoadedBeforeProperty = DependencyProperty.RegisterAttached(
            "HandleLoadedBefore", typeof(bool), typeof(UIElement), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));


        public static void SetHandleLoadedBefore(UIElement element, object value)
        {
            bool val;
            if (value == null || !bool.TryParse(value.ToString(), out val)) throw new ArgumentNullException("value");
            element.SetValue(HandleLoadedBeforeProperty, val);

            if ((bool)val)
                ((FrameworkElement)element).Loaded += LoadedOnceHelper_Loaded;
            else
                ((FrameworkElement)element).Loaded -= LoadedOnceHelper_Loaded;
        }

        static void LoadedOnceHelper_Loaded(object sender, RoutedEventArgs e)
        {
            DependencyObject target = (DependencyObject)e.Source;
            if ((bool)target.GetValue(HasLoadedBeforeProperty) == false)
            {
                RaiseFirstLoaded(target);
            }
        }

        public static object GetHandleLoadedBefore(UIElement element)
        {
            return element.GetValue(HandleLoadedBeforeProperty);
        }


        public static readonly RoutedEvent FirstLoadedEvent =
            EventManager.RegisterRoutedEvent("FirstLoaded",
                                             RoutingStrategy.Bubble,
                                             typeof(RoutedEventHandler),
                                             typeof(LoadedOnceHelper));

        internal static RoutedEventArgs RaiseFirstLoaded(DependencyObject target)
        {
            if (target == null) return null;

            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = FirstLoadedEvent;

            if (target is UIElement && (bool)target.GetValue(HasLoadedBeforeProperty) == false)
            {
                (target as UIElement).RaiseEvent(args);
                target.SetValue(HasLoadedBeforeProperty, true);
            }
            else if (target is ContentElement && (bool)target.GetValue(HasLoadedBeforeProperty) == false)
            {
                (target as ContentElement).RaiseEvent(args);
                target.SetValue(HasLoadedBeforeProperty, true);
            }

            return args;
        }

    }
}
