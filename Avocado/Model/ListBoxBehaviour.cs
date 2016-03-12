using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Avocado.Model {
	public class ListBoxBehaviour {
		private static readonly Dictionary<ListBox, Capture> Associations = new Dictionary<ListBox, Capture>();

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

				if (Associations.ContainsKey(listBox))
					Associations[listBox].Dispose();
				Associations[listBox] = new Capture(listBox);

				PropertyDescriptor itemsSourcePropertyDescriptor = TypeDescriptor.GetProperties(listBox)["ItemsSource"];
				itemsSourcePropertyDescriptor.RemoveValueChanged(listBox, ListBox_ItemsSourceChanged);
			}
		}

		public static void ListBox_ItemsSourceChanged(object sender, EventArgs e) {
			ListBox listBox = (ListBox)sender;

			if (Associations.ContainsKey(listBox))
				Associations[listBox].Dispose();

			Associations[listBox] = new Capture(listBox);
		}

		private static void ListBox_Unloaded(object sender, RoutedEventArgs e) {
			ListBox listBox = (ListBox)sender;
			INotifyCollectionChanged incc = listBox.Items;
			if (incc == null) return;

			listBox.Loaded -= ListBox_Unloaded;
			Associations[listBox] = new Capture(listBox);
		}

		private static void ListBox_Loaded(object sender, RoutedEventArgs e) {
			ListBox listBox = (ListBox)sender;
			INotifyCollectionChanged incc = listBox.Items;
			if (incc == null) return;

			listBox.Loaded -= ListBox_Loaded;
			Associations[listBox] = new Capture(listBox);
		}
	}

	internal class Capture : IDisposable {
		private readonly INotifyCollectionChanged _incc;
		private readonly ListBox _listBox;

		public Capture(ListBox listBox) {
			_listBox = listBox;

			_incc = listBox.ItemsSource as INotifyCollectionChanged;
			if (_incc != null)
				_incc.CollectionChanged += incc_CollectionChanged;
		}

		public void Dispose() {
			if (_incc == null) return;

			_incc.CollectionChanged -= incc_CollectionChanged;
		}

		private void incc_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			if (e.Action == NotifyCollectionChangedAction.Add) _listBox.ScrollIntoView(e.NewItems[0]);
		}
	}
}