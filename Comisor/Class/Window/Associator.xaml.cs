

#define Debug

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace Comisor.Class
{
	/// <summary>
	/// Associator.xaml 的交互逻辑
	/// </summary>
	public partial class Associator : ys.WindowBorderless
	{
		public Associator(MainWindow mainWindow)
		{
			InitializeComponent();

			this.main = mainWindow;
			lbTitle.Content = Resource.Associate_Files;

			btnOK.Content = mainWindow.resource.imgOK;
			btnCancel.Content = mainWindow.resource.imgCancel;

			btnReverse.Content = Resource.Associate_Files_Reverse;
			btnReverse.Click += new RoutedEventHandler(btnReverse_Click);
			btnReverse.Cursor = Cursors.Arrow;

			bdrMain.Cursor = main.resource.curHand_Over;
		}

		private void btnReverse_Click(object sender, RoutedEventArgs e)
		{
			foreach (CheckBox cb in list.Children)
			{
				cb.IsChecked = !cb.IsChecked;
			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			bdrMain.MouseLeftButtonDown += new MouseButtonEventHandler(DragStart);
			bdrMain.MouseLeftButtonUp += new MouseButtonEventHandler(DragStop);

			this.Activate();
		}

		private void DragStart(object sender, MouseButtonEventArgs e)
		{
			bdrMain.CaptureMouse();

			bdrMain.Cursor = main.resource.curHand_Drag;
			ptOffset = e.GetPosition(this);
			bdrMain.MouseMove += new MouseEventHandler(DragOn);
		}

		private void DragOn(object sender, MouseEventArgs e)
		{
			Point ptCurrent = e.GetPosition(null);
			ptCurrent.Offset(-ptOffset.X, -ptOffset.Y);
			this.Left = this.Left + ptCurrent.X;
			this.Top = this.Top + ptCurrent.Y;
		}

		private void DragStop(object sender, MouseEventArgs e)
		{
			bdrMain.Cursor = main.resource.curHand_Over;

			bdrMain.ReleaseMouseCapture();
			bdrMain.MouseMove -= DragOn;
		}

		private void btnOK_Click(object sender, RoutedEventArgs e)
		{
			string AppPath = GetType().Assembly.Location;

			foreach (CheckBox cb in list.Children)
			{
				if (cb.IsChecked.Value)
				{
					string[] exs = (cb.Tag as string).Split(',');
					foreach (string ex in exs)
					{
						SetAssociation(
							ex,
							main.resource.AssemblyProduct + ex,
							AppDomain.CurrentDomain.BaseDirectory + @"File Icon\" + exs[0].Replace(".", "").ToUpper() + ".ico",
							AppPath
						);
					}
				}
				else
				{
					string[] exs = (cb.Tag as string).Split(',');
					foreach (string ex in exs)
					{
						RemoveAssociation(
							ex,
							main.resource.AssemblyProduct + ex
						);
					}
				}
			}

			SHChangeNotify(0x8000000, 0, IntPtr.Zero, IntPtr.Zero);

			WindowStateAnimation(true);
		}

		public static void SetAssociation(string Extension, string RootKeyName, string icon, string AppPath)
		{
			RegistryKey rootKey;
			if (Registry.ClassesRoot.OpenSubKey(RootKeyName) != null)
				Registry.ClassesRoot.DeleteSubKeyTree(RootKeyName);
			rootKey = Registry.ClassesRoot.CreateSubKey(RootKeyName);
			if (File.Exists(icon))
				rootKey.CreateSubKey("DefaultIcon").SetValue("", icon);
			rootKey.CreateSubKey("Shell").CreateSubKey("open").CreateSubKey("command").SetValue("", "\"" + AppPath + "\"" + " \"%1\"");
			rootKey.Close();

			RegistryKey userKey;
			string userKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" + Extension;
			userKey = Registry.CurrentUser.CreateSubKey(userKeyPath);
			object backupProgid = userKey.GetValue("Progid");
			if (backupProgid == null) backupProgid = "";
			userKey.SetValue("Backup", backupProgid);
			userKey.SetValue("Progid", RootKeyName);
			object choiceBackup = null;
			if (Registry.CurrentUser.OpenSubKey(userKeyPath + @"\UserChoice") != null)
			{
				choiceBackup = Registry.CurrentUser.OpenSubKey(userKeyPath + @"\UserChoice").GetValue("Progid");
				Registry.CurrentUser.DeleteSubKey(userKeyPath + @"\UserChoice");
			}
			using (RegistryKey choiceKey = userKey.CreateSubKey("UserChoice"))
			{
				choiceKey.SetValue("Progid", RootKeyName);
				if (choiceBackup == null) choiceBackup = "";
				choiceKey.SetValue("Backup", choiceBackup);
			}
			userKey.Close();
		}

		public static void RemoveAssociation(string Extension, string RootKeyName)
		{
			if (Registry.ClassesRoot.OpenSubKey(RootKeyName) != null)
				Registry.ClassesRoot.DeleteSubKeyTree(RootKeyName);

			RegistryKey userKey;
			string userKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" + Extension;
			userKey = Registry.CurrentUser.OpenSubKey(userKeyPath,true);
			if (userKey != null)
			{
				try
				{
					userKey.SetValue("Progid", userKey.GetValue("Backup"));
					userKey.DeleteValue("Backup");
					object choiceBackup = null;
					if (Registry.CurrentUser.OpenSubKey(userKeyPath + @"\UserChoice") != null)
					{
						choiceBackup = Registry.CurrentUser.OpenSubKey(userKeyPath + @"\UserChoice").GetValue("Backup");
						Registry.CurrentUser.DeleteSubKey(userKeyPath + @"\UserChoice");
					}
					userKey.CreateSubKey("UserChoice").SetValue("Progid", choiceBackup);
					userKey.Close();
				}
				catch { /* Not exist */ }
			}
		}

		[DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);


		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			WindowStateAnimation(true);
		}

		private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Escape:
					WindowStateAnimation(true);
					break;
				case Key.Enter:
					btnOK_Click(sender, e);
					break;
			}
		}

		private MainWindow main;
		private Point ptOffset;
	}
}
