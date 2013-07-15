using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MangaEpsilon
{
    //based on code by Dominik Schmidt
    //http://www.dominikschmidt.net/2010/12/netc-get-first-visible-item-of-a-listbox/
    // and later: http://www.dominikschmidt.net/2011/01/netc-first-visible-item-listbox-attached-behavior/
    //also based on code from here: http://stackoverflow.com/questions/830272/calculating-number-of-visible-items-in-listbox

    public static class ListBoxVisibleItemsHelper
    {
        // keeping track of list boxes not to add handler multiple times
        private static List<ListBox> _listBoxes = new List<ListBox>();

        public static readonly DependencyProperty VisibleItemsProperty =
            DependencyProperty.RegisterAttached("VisibleItems", typeof(object), typeof(ListBoxVisibleItemsHelper),
               new FrameworkPropertyMetadata() { PropertyChangedCallback = OnVisibleItemsChanged, BindsTwoWayByDefault = true });

        public static object GetVisibleItems(DependencyObject d)
        {
            if (d == null) throw new ArgumentNullException();
            return d.GetValue(VisibleItemsProperty);
        }

        public static void SetVisibleItems(DependencyObject d, object value)
        {
            if (d == null) throw new ArgumentNullException();
            d.SetValue(VisibleItemsProperty, value);
        }

        public static object GetVisibleItemsExt(this ListBox listBox)
        {
            return listBox.Items.Count > 0 ?
                GetVisibleItemsFromListbox(listBox, listBox) : null;
        }

        private static bool IsItemVisible(FrameworkElement element, FrameworkElement container)
        {
            if (!element.IsVisible)
                return false;

            Rect bounds =
                element.TransformToAncestor(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
            var rect = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);
            return rect.Contains(bounds.TopLeft) || rect.Contains(bounds.BottomRight);
        }

        private static List<object> GetVisibleItemsFromListbox(ListBox listBox, FrameworkElement parentToTestVisibility)
        {
            var items = new List<object>();

            foreach (var item in listBox.Items)
            {

                var element = (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromItem(item);

                if (element == null) continue;

                if (IsItemVisible(element, parentToTestVisibility))
                {
                    items.Add(item);
                }
                else if (items.Any())
                {
                    break;
                }

            }

            return items;
        }

        private static void OnVisibleItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ListBox listBox = d as ListBox;

            if (d == null)
                throw new InvalidOperationException("The FirstVisibleItem attached property can only be applied to ListBox controls.");

            // add scroll changed handler only if not yet added
            if (!_listBoxes.Contains(listBox))
            {
                _listBoxes.Add(listBox);
                listBox.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(ScrollChanged));
            }

            //if (e.OldValue != e.NewValue)
            //SetVisibleItems(listBox, e.NewValue);
        }

        private static void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ListBox listBox = (ListBox)sender;
            if (listBox.Items.Count > 0 && listBox.GetPanel() != null)
                SetVisibleItems(listBox, listBox.GetVisibleItemsExt());
        }
        /// <summary>
        /// Gets horizontal or vertical offset (depending on panel orientation).
        /// </summary>
        /// <param name="listBox">The ListBox</param>
        /// <returns>The offset or 0 if no VirtualizingStackPanel was found</returns>
        private static int GetPanelOffset(this ListBox listBox)
        {
            VirtualizingStackPanel panel = listBox.GetPanel();
            if (panel != null)
                return (int)((panel.Orientation == Orientation.Horizontal) ? panel.HorizontalOffset : panel.VerticalOffset);
            else
                return 0;
        }

        /// <summary>
        /// Retrieves the ListBox's items panel as VirtualizingStackPanel.
        /// </summary>
        /// <param name="listBox">The ListBox</param>
        /// <returns>The item panel or null if no VirtualizingStackPanel was found</returns>
        public static VirtualizingStackPanel GetPanel(this ListBox listBox)
        {
            VirtualizingStackPanel panel = VisualHelpers.TryFindChild<VirtualizingStackPanel>(listBox);
            if (panel == null)
                Debug.WriteLine("No VirtualizingStackPanel found for ListBox.");
            return panel;
        }

    }
    public static class VisualHelpers
    {
        /// <summary>
        /// Finds a parent of a given item on the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="child">A direct or indirect child of the
        /// queried item.</param>
        /// <returns>The first parent item that matches the submitted
        /// type parameter. If not matching item can be found, a null
        /// reference is being returned.</returns>
        public static T TryFindParent<T>(this DependencyObject child)
            where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = GetParentObject(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                //use recursion to proceed with next level
                return TryFindParent<T>(parentObject);
            }
        }

        /// <summary>
        /// Finds a child of the given item on the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="parent">A parent of the queried item.</param>
        /// <returns>The first child item that matches the submitted
        /// type parameter. If not matching item can be found, a null
        /// reference is being returned.</returns>
        public static T TryFindChild<T>(this DependencyObject parent)
            where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is T)
                {
                    return (T)child;
                }
                else
                {
                    child = TryFindChild<T>(child);
                    if (child != null)
                    {
                        return (T)child;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// This method is an alternative to WPF's
        /// <see cref="VisualTreeHelper.GetParent"/> method, which also
        /// supports content elements. Keep in mind that for content element,
        /// this method falls back to the logical tree of the element!
        /// </summary>
        /// <param name="child">The item to be processed.</param>
        /// <returns>The submitted item's parent, if available. Otherwise
        /// null.</returns>
        public static DependencyObject GetParentObject(this DependencyObject child)
        {
            if (child == null) return null;

            //handle content elements separately
            ContentElement contentElement = child as ContentElement;
            if (contentElement != null)
            {
                DependencyObject parent = ContentOperations.GetParent(contentElement);
                if (parent != null) return parent;

                FrameworkContentElement fce = contentElement as FrameworkContentElement;
                return fce != null ? fce.Parent : null;
            }

            //also try searching for parent in framework elements (such as DockPanel, etc)
            FrameworkElement frameworkElement = child as FrameworkElement;
            if (frameworkElement != null)
            {
                DependencyObject parent = frameworkElement.Parent;
                if (parent != null) return parent;
            }

            //if it's not a ContentElement/FrameworkElement, rely on VisualTreeHelper
            return VisualTreeHelper.GetParent(child);
        }
    }

}
