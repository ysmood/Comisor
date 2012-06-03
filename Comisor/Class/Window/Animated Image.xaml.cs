using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System;
using System.Windows.Media;
using System.Windows.Interop;

namespace Comisor.Class
{
	/// <summary>
	/// Animated_Image.xaml 的交互逻辑
	/// </summary>
	public partial class Animated_Image : Window
	{
		public Animated_Image()
		{
			InitializeComponent();
		}

		private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Escape:
					this.Close();
					break;
				case Key.Enter:
					this.Close();
					break;
			}
		}
	}
}
