
#define Window_Borderless


using System;
using System.Windows;
using System.Linq;
using System.Windows.Media;
using System.IO;
using System.Windows.Controls;

namespace Comisor
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
#if Window_Borderless
	public partial class MainWindow : ys.WindowBorderless
#else
	public partial class MainWindow : Window
#endif
	{
		public MainWindow()
		{
			InitializeComponent();

			this.ShowInTaskbar = false;
			this.Loaded += Window_Loaded;

#if !Window_Borderless
			this.WindowState = System.Windows.WindowState.Minimized;
#endif
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			resource = new Resource();
			
			imgViewer = new ys.ImageViewer(this);

			// Click file to invoke this app.
			string[] arguments = Environment.GetCommandLineArgs();
			string pathName = null;
			if (arguments.Length > 1)
			{
				// First check, get the file path.
				for (int i = 1; i < arguments.Length; i++)
				{
					if (File.Exists(arguments[i]) || Directory.Exists(arguments[i]))
						pathName = arguments[i];
				}
				// Second check, get the commands.
				for (int i = 1; i < arguments.Length; i++)
				{
					if (arguments[i].StartsWith("-"))
					{
						#region Open associate window.
						if (arguments[i] == Comisor.Resource.Associate_Files_Switch)
						{
							Comisor.Class.Associator fileAssociator = new Comisor.Class.Associator(this);
							fileAssociator.Opacity = 0;
							fileAssociator.Show();
							fileAssociator.WindowStateAnimation(false, 1, true);
							this.Close();
							return;
						}
						#endregion

						#region Collection Explore
						if (arguments[i] == Resource.Collection_Explore_Switch)
						{
							imgViewer.mitCollectionExplore.isChecked = true;
						}
						#endregion

						#region Auto Fit Lock Size
						if (arguments[i].Substring(0, Resource.Auto_Fit_Lock_Switch.Length) == Resource.Auto_Fit_Lock_Switch)
						{
							imgViewer.isAutoFitOn = true;
							imgViewer.indexAutoFitLock = 0;
							imgViewer.rectLastFrame = (Rect)new RectConverter().ConvertFromString(arguments[i].Substring(Resource.Auto_Fit_Lock_Switch.Length));
						}
						#endregion

						#region Background State
						if (arguments[i].Substring(0, Resource.Bookmark_Switch.Length) == Resource.Background_Opacity_Switch)
						{
							if (arguments[i].Length >= Resource.Background_Opacity_Switch.Length)
							{
								string arg = arguments[i].Substring(Resource.Bookmark_Switch.Length);
								if (arg == "0")
									imgViewer.isBgOn = false;
								else
									imgViewer.isBgOn = true;
							}
						}
						#endregion

						#region Auto Levels
						if (arguments[i] == Resource.Auto_Levels_Switch)
						{
							imgViewer.isAutoLevelOn = true;
						}
						#endregion

						#region Open Bookmark.
						if (arguments[i].Substring(0, Resource.Bookmark_Switch.Length) == Resource.Bookmark_Switch)
						{
							// Open at index.
							if (arguments[i].Length > Resource.Bookmark_Switch.Length)
							{
								string arg = arguments[i].Substring(Resource.Bookmark_Switch.Length);
								arg.Replace("\"","");
								try
								{
									pathName = imgViewer.bookmarks.Find((m) => { return m.name == arg; }).filePath;
								}
								catch
								{
									try
									{
										uint index = Convert.ToUInt32(arg) - 1;
										if (index < imgViewer.bookmarks.Count)
											pathName = imgViewer.bookmarks[(int)index].filePath;
										else
										{
											imgViewer.ReportException(Resource.Bookmark_OutOfBound);
										}
									}
									catch
									{
										imgViewer.ReportException(Resource.Bookmark_NotFound);
									}
								}
							}
							// Open to last time.
							else
							{
								if (imgViewer.bookmarks.Count > 0)
									pathName = imgViewer.bookmarks.FindLast((match) => { return match.name == Comisor.Resource.Bookmark_LastTime; }).filePath;
								else
								{
									imgViewer.ReportException(Resource.Bookmark_OutOfBound);
									StartWindow();
								}
							}
						}
						#endregion
					}
				}
			}
			// Open image.
			if(pathName != null)
				imgViewer.CheckAndStart(pathName, true);
			else
				StartWindow();
		}

		private void StartWindow()
		{
			Class.StartupWindow startupWindow = new Class.StartupWindow(this);
			startupWindow.Show();
			startupWindow.WindowState = System.Windows.WindowState.Normal;
		}

		/// Global Member
		public ys.ImageViewer imgViewer;

		public Resource resource;
	}
}
