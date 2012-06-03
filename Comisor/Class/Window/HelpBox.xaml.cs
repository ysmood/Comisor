
#define Debug
//#define isFX4
#define Window_Borderless

using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Windows;
using System.Windows.Input;
using System.Threading;


namespace Comisor.Class
{
	/// <summary>
	/// Window1.xaml 的交互逻辑
	/// </summary>
	public partial class HelpBox : ys.WindowBorderless
	{
		public HelpBox(MainWindow mainWindow)
		{
			InitializeComponent();

			this.main = mainWindow;
			this.title.Content = Comisor.Resource.Help;

			btnUpdate.Content = Comisor.Resource.Update;
			btnOK.Content = mainWindow.resource.imgCancel;

			lbInfo.Content =
				mainWindow.resource.AssemblyProduct + 
				"     Version: " + mainWindow.resource.AssemblyVersion + "      " + 
				mainWindow.resource.AssemblyCopyright;

			txtReadme.Selection.Load(main.resource.sReadme, DataFormats.Rtf);
			txtReadme.Document.LineHeight = 28;

			bdrMain.Cursor = main.resource.curHand_Over;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			bdrMain.MouseLeftButtonDown += new MouseButtonEventHandler(DragStart);
			bdrMain.MouseLeftButtonUp += new MouseButtonEventHandler(DragStop);
		}

		void DragStart(object sender, MouseButtonEventArgs e)
		{
			bdrMain.CaptureMouse();

			bdrMain.Cursor = main.resource.curHand_Drag;
			ptOffset = e.GetPosition(this);
			bdrMain.MouseMove += new MouseEventHandler(DragOn);
		}

		void DragOn(object sender, MouseEventArgs e)
		{
			Point ptCurrent = e.GetPosition(null);
			ptCurrent.Offset(-ptOffset.X, -ptOffset.Y);
			this.Left = this.Left + ptCurrent.X;
			this.Top = this.Top + ptCurrent.Y;
		}

		void DragStop(object sender, MouseEventArgs e)
		{
			bdrMain.Cursor = main.resource.curHand_Over;

			bdrMain.ReleaseMouseCapture();
			bdrMain.MouseMove -= DragOn;
		}

		private void Update(object sender, RoutedEventArgs e)
		{
			Thread th = new Thread(
				new ParameterizedThreadStart((o)=>
					{
						WebClient client = new WebClient();

						Stream data = client.OpenRead("http://www.ysmood.org/project/download.php?r=comisor");
						DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(ComisorFileInfo));
						ComisorFileInfo fileInfo = json.ReadObject(data) as ComisorFileInfo;
						if (fileInfo.version.CompareTo(main.resource.AssemblyVersion) > 0)
						{
							if (MessageBox.Show(
								Comisor.Resource.Update_Current + main.resource.AssemblyVersion + "\n\n" +
								Comisor.Resource.Update_Latest + fileInfo.version + "\n\n" +
								Comisor.Resource.Update_Exist,
								Comisor.Resource.Update,
								MessageBoxButton.OKCancel) == MessageBoxResult.OK)
							{
								System.Diagnostics.Process.Start(Comisor.Resource.UpdatePagePath);
							}
						}
						else
							MessageBox.Show(Comisor.Resource.Update_None);
					}
				)
			);
			th.Start();
			WindowStateAnimation(true);
		}

		private void btnOK_Click(object sender, RoutedEventArgs e)
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
					WindowStateAnimation(true);
					break;
				case Key.F1:
					WindowStateAnimation(true);
					break;
			}
		}

		private MainWindow main;
		private Point ptOffset;
	}

	public class ComisorFileInfo
	{
		public string name;
		public string size;
		public string version;
	}
}
