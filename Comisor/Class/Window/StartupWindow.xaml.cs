
#define Window_Borderless

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;

namespace Comisor.Class
{
	/// <summary>
	/// StartupWindow.xaml 的交互逻辑
	/// </summary>
	public partial class StartupWindow : Window
	{
		public StartupWindow(Comisor.MainWindow mainWindow)
		{
			InitializeComponent();
			
			this.main = mainWindow;
			this.MinWidth = 300;
			this.MinHeight = 300;
			this.Closed += new EventHandler(StartupWindow_Closed);

			InitBookmark();

			if (this.lboxBookmark.Items[0] is string)
				btnOpen.Focus();
			else
				(this.lboxBookmark.Items[0] as FrameworkElement).Focus();

			this.Title = Resource.WidowTitle_Start;
			lbBookmark.Content = Resource.Bookmark_Mark;
			btnOpen.Content = Resource.OpenImage;

			if (!File.Exists(main.imgViewer.UserInfoFileName))
			{
				this.Width = 300;
				this.Height = 300;
				this.SizeToContent = System.Windows.SizeToContent.Manual;
			}
		}

		private void InitBookmark()
		{
			if (main.imgViewer.bookmarks.Count == 0)
			{
				lboxBookmark.Items.Add(Resource.Bookmark_NoBookmark);
				return;
			}

			// 注意 bookmark 只能出现在“=”的右边。
			foreach(ys.Bookmark bk in main.imgViewer.bookmarks)
			{
				MenuItem mit = new MenuItem();
				StackPanel stp = new StackPanel();
				Label lbName = new Label();
				Button btnDelete = new Button();

				btnDelete.Content = main.resource.imgMinuts;
				btnDelete.Margin = new Thickness(2);
				
				lbName.Content = bk.name;

				stp.Orientation = Orientation.Horizontal;
				stp.Children.Add(main.resource.imgBookmark);
				stp.Children.Add(lbName);
				mit.Icon = btnDelete;
				mit.Header = stp;
				mit.ToolTip = bk.filePath + "\n" + bk.date.ToLongDateString();
				lboxBookmark.Items.Add(mit);

				// Events
				ys.Bookmark bookmark = bk; // Cause the bookmark is just a pointer.
				btnDelete.Click += (o, e) =>
				{
					main.imgViewer.bookmarks.Remove(bookmark);
					lboxBookmark.Items.Remove(mit);
				};

				mit.Click += (oo, ee) =>
				{
					if (System.IO.File.Exists(bookmark.filePath))
					{
						main.imgViewer.CheckAndStart(bookmark.filePath, true);
						this.Close();
					}
					else
						if (main.imgViewer.ReportException(Comisor.Resource.Bookmark_FileNotFound, false, MessageBoxButton.OKCancel)
							== MessageBoxResult.OK)
						{
							System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
							openFileDialog.CheckFileExists = true;
							openFileDialog.InitialDirectory = ys.DataProcessor.GetAvailableParentDir(bk.filePath);
							if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
							{
								bk.filePath = openFileDialog.FileName;
								bk.date = DateTime.Now;
								mit.ToolTip = bk.filePath + "\n" + bk.date.ToShortDateString();

								main.imgViewer.CheckAndStart(bk.filePath);
								this.Close();
							}
						}
				};

				mit.PreviewMouseRightButtonUp += (oo, ee) =>
				{
					BookmarkEditor editor = new BookmarkEditor(main, bookmark.name, bookmark.filePath);
					editor.ShowDialog();
					if (editor.isOK)
					{
						bookmark.name = editor.cbName.Text;
						bookmark.filePath = editor.cbPath.Text;
						bookmark.date = DateTime.Now;
						mit.ToolTip = bookmark.filePath + "\n" + bookmark.date.ToShortDateString();
						lbName.Content = editor.cbName.Text;
					}
					ee.Handled = true;
				};
			}
		}

		private void ChooseOpen()
		{
			System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
			openFileDialog.CheckFileExists = true;
			if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				string fullName = openFileDialog.FileName;
				main.imgViewer.CheckAndStart(fullName, true);
				this.Close();
			}
		}

		private void btnOpen_Click(object sender, RoutedEventArgs e)
		{
			ChooseOpen();
		}

		private void StartupWindow_Closed(object sender, EventArgs e)
		{
			if (!main.imgViewer.isInitialized)
				main.imgViewer.CloseWindow();
#if !Window_Borderless
			else
				main.WindowState = WindowState.Normal;
#endif
		}

		private Comisor.MainWindow main;
	}
}
