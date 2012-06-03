using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using IWshRuntimeLibrary;

namespace Comisor.Class
{
	/// <summary>
	/// BookmarkEditor.xaml 的交互逻辑
	/// </summary>
	public partial class BookmarkEditor : Window
	{
		public BookmarkEditor(Comisor.MainWindow mainWindow, string strName, string strPath, string strCurrentPath = "")
		{
			InitializeComponent();

			this.main = mainWindow;
			this.Title = Comisor.Resource.Bookmark_Editor;
			lbName.Content = Comisor.Resource.Bookmark_Name + ":";
			lbPath.Content = Comisor.Resource.Bookmark_FilePath + ":";

			List<string> nameOption = new List<string>();
			nameOption.AddRange(strPath.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries));
			nameOption.Reverse();
			nameOption.Insert(0, strName);
			ys.DataProcessor.RemoveSame(ref nameOption);
			cbName.ItemsSource = nameOption;
			cbName.SelectedIndex = 0;

			List<string> pathOption = new List<string>();
			pathOption.Add(strPath);
			if (strCurrentPath != "") pathOption.Add(strCurrentPath);
			ys.DataProcessor.RemoveSame(ref pathOption);
			cbPath.Focus();
			cbPath.ItemsSource = pathOption;
			cbPath.SelectedIndex = 0;

			ckbShortcut.Content = Comisor.Resource.Bookmark_CreatShortcut;

			btnOK.Content = mainWindow.resource.imgOK;
			btnCancel.Content = mainWindow.resource.imgCancel;

			this.PreviewKeyDown += new KeyEventHandler(InputBox_PreviewKeyDown);
		}

		void InputBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Escape:
					btnCancel.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, btnCancel));
					e.Handled = true;
					break;
				case Key.Enter:
					btnOK.RaiseEvent(new RoutedEventArgs(Button.ClickEvent,btnOK));
					e.Handled = true;
					break;
			}
		}

		private void btnOK_Click(object sender, RoutedEventArgs e)
		{
			isOK = true;

			if ((bool)ckbShortcut.IsChecked)
			{
				WshShell shell = new WshShell();
				IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(
					Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\\" +
					cbName.Text + ".lnk"
				);
				shortcut.TargetPath = GetType().Assembly.Location;
				shortcut.Arguments = " -b\"" + cbName.Text + "\"";
　　			shortcut.Save();
			}

			this.Close();
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private Comisor.MainWindow main;

		public bool isOK = false;
	}
}
