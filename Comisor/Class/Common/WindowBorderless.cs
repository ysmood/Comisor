using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ys
{
	public class WindowBorderless : Window
	{
		public WindowBorderless()
		{
			this.AllowsTransparency = true;
			this.WindowStyle = System.Windows.WindowStyle.None;
			this.Background = Brushes.Transparent;
		}

		/// <summary>
		/// isFirstStart Should set to be true when first start the window, or Max and Min window animation won't be triggered.
		/// </summary>
		/// <param name="isShutDown">Shutdown this window after state animation finished.</param>
		/// <param name="time">Unit is 200ms</param>
		/// <param name="isFirstStart">Must be true if it's first time to trigger the animation.</param>
		public virtual void WindowStateAnimation(bool isShutDown = false, int time = 1, bool isFirstStart = false)
		{
			DoubleAnimation dbaWindowState = new DoubleAnimation();
			dbaWindowState.Duration = new Duration(TimeSpan.FromMilliseconds(180 * time));
			if (isFirstStart)
			{
				this.Opacity = 0;
				dbaWindowState.Completed += (o, e) => { if (isFirstStart) { isFirstStart = false; source.AddHook(WndProc); } };
				if (firstStartComplete != null) dbaWindowState.Completed += firstStartComplete;
			}

			if (isShutDown)
			{
				if(this.Opacity == 1)
					dbaWindowState.Completed += (o, e) => { this.Close(); };
				else
				{
					this.Close();	// 当处于隐藏状态时关闭窗口，防止闪烁。
					return;
				}
			}


			if (this.Opacity == 1)
			{
				dbaWindowState.From = 1;
				dbaWindowState.To = 1.2;
				dbaWindowState.Completed += (o, e) => 
				{
					this.WindowState = System.Windows.WindowState.Minimized;
				};
			}
			else
			{
				this.WindowState = System.Windows.WindowState.Normal;
				dbaWindowState.From = 1.2;
				dbaWindowState.To = 1;
			}

			ScaleTransform scaleTransform = new ScaleTransform();
			this.RenderTransform = scaleTransform;
			scaleTransform.CenterX = this.ActualWidth / 2;
			scaleTransform.CenterY = this.ActualHeight / 2;

			scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, dbaWindowState);
			scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, dbaWindowState);

			if (this.Opacity == 1)
				dbaWindowState.To = 0;
			else
				dbaWindowState.From = 0;
			this.BeginAnimation(UIElement.OpacityProperty, dbaWindowState);
		}

		#region 重写窗口的最大化、最小化、关闭动画
		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);
			source = PresentationSource.FromVisual(this) as HwndSource;
		}

		const int WM_SYSCOMMAND = 0x112;
		const int SC_MINIMIZE = 0xF020;
		const int SC_RESTORE = 0xF120;
		const int SC_MAXIMIZE = 0xF030;
		const int SC_ClOSE = 0xF060;
		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == WM_SYSCOMMAND)
			{
				switch ((int)wParam)
				{
					case SC_MINIMIZE:
						WindowStateAnimation();
						handled = true;
						break;
					case SC_RESTORE:
						WindowStateAnimation();
						handled = true;
						break;
					case SC_MAXIMIZE:
						this.Left = 0;
						this.Top = 0;
						this.Width = SystemParameters.PrimaryScreenWidth;
						this.Height = SystemParameters.PrimaryScreenHeight;
						handled = true;
						break;
					case SC_ClOSE:
						WindowStateAnimation(true);
						handled = true;
						break;
				}
			}
			return IntPtr.Zero;
		}
		#endregion

		private HwndSource source;

		public EventHandler firstStartComplete;
	}
}
