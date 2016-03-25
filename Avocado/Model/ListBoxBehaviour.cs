using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Avocado.Model {
    /// <summary>
    /// Used to force ListBox to scroll down when element is added.
    /// </summary>
    public class ListBoxBehaviour {
        private static readonly Dictionary<ListBox, Capture> _associations = new Dictionary<ListBox, Capture>();

        public static readonly DependencyProperty ScrollOnNewItemProperty =
            DependencyProperty.RegisterAttached(
                "ScrollOnNewItem",
                typeof(bool),
                typeof(ListBoxBehaviour),
                new UIPropertyMetadata(false, OnScrollNewItemChanged));

        public static bool GetScrollOnNewItem(DependencyObject obj) {
            return (bool)obj.GetValue(ScrollOnNewItemProperty);
        }

        public static void SetScrollOnNewItem(DependencyObject obj, bool value) {
            obj.SetValue(ScrollOnNewItemProperty, value);
        }

        public static void OnScrollNewItemChanged(DependencyObject dObj, DependencyPropertyChangedEventArgs e) {
            ListBox listBox = dObj as ListBox;
            if (listBox == null) return;

            bool oldValue = (bool)e.OldValue,
                newValue = (bool)e.NewValue;

            if (newValue == oldValue) return;

            if (newValue) {
                listBox.Loaded += ListBox_Loaded;
                listBox.Unloaded += ListBox_Unloaded;

                PropertyDescriptor itemsSourcePropertyDescriptor = TypeDescriptor.GetProperties(listBox)["ItemsSource"];
                itemsSourcePropertyDescriptor.AddValueChanged(listBox, ListBox_ItemsSourceChanged);
            } else {
                listBox.Loaded -= ListBox_Loaded;
                listBox.Unloaded -= ListBox_Unloaded;

                if (_associations.ContainsKey(listBox))
                    _associations[listBox].Dispose();
                _associations[listBox] = new Capture(listBox);

                PropertyDescriptor itemsSourcePropertyDescriptor = TypeDescriptor.GetProperties(listBox)["ItemsSource"];
                itemsSourcePropertyDescriptor.RemoveValueChanged(listBox, ListBox_ItemsSourceChanged);
            }
        }

        public static void ListBox_ItemsSourceChanged(object sender, EventArgs e) {
            ListBox listBox = (ListBox)sender;

            if (_associations.ContainsKey(listBox))
                _associations[listBox].Dispose();

            _associations[listBox] = new Capture(listBox);
        }

        private static void ListBox_Unloaded(object sender, RoutedEventArgs e) {
            ListBox listBox = (ListBox)sender;
            INotifyCollectionChanged incc = listBox.Items;
            if (incc == null) return;

            listBox.Loaded -= ListBox_Unloaded;
            _associations[listBox] = new Capture(listBox);
        }

        private static void ListBox_Loaded(object sender, RoutedEventArgs e) {
            ListBox listBox = (ListBox)sender;
            INotifyCollectionChanged incc = listBox.Items;
            if (incc == null) return;

            listBox.Loaded -= ListBox_Loaded;
            _associations[listBox] = new Capture(listBox);
        }
    }

    internal class Capture : IDisposable {
        private readonly INotifyCollectionChanged incc;
        private readonly ListBox listBox;

        public Capture(ListBox listBox) {
            this.listBox = listBox;

            incc = listBox.ItemsSource as INotifyCollectionChanged;
            if (incc != null)
                incc.CollectionChanged += incc_CollectionChanged;
        }

        public void Dispose() {
            if (incc == null) return;

            incc.CollectionChanged -= incc_CollectionChanged;
        }

        private void incc_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (e.Action == NotifyCollectionChangedAction.Add) listBox.ScrollIntoView(e.NewItems[0]);
        }
    }
}