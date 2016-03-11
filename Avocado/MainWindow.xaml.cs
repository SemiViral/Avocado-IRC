using System;
using System.Windows;

namespace Avocado {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();
		}

		protected override void OnClosed(EventArgs e) {
			base.OnClosed(e);

			Application.Current.Shutdown();
		}
	}
}
