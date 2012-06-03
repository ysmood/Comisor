
//#define isFX4

using System;
using System.Collections;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Win7;
using Comisor.Properties;
using System.Windows.Media;
using System.Windows.Controls;
using System.Linq;

namespace ys
{
	public partial class ImageViewer
	{
		private void InitNavigator()
		{
			allFileNames = new ArrayList();

			tmrNavImage = new DispatcherTimer();
			tmrNavImage.Tick += new EventHandler(tmr_NavImage);
			tmrNavImage.Interval = TimeSpan.FromMilliseconds(60);

			cavStage.MouseWheel += new MouseWheelEventHandler(NavImageJudge);
		}

		private void DropFileOpen(object sender, DragEventArgs e)
		{
			string fullName = (e.Data.GetData(DataFormats.FileDrop, false) as String[])[0];

			CheckAndStart(fullName);

			main.Activate();
		}

		private void GetCollection(bool isAllDirectories = false)
		{
			string parentFolder = imgInfo.DirectoryName;
			string[] filesInDir;
			if (isAllDirectories)
			{
				allFileNames.Clear();
				try { parentFolder = Directory.GetParent(parentFolder).FullName; }
				catch { /* 磁盘根目录 */ }
				// 这里不能使用 Directory.GetDirectories(parentFolder "*", SearchOption.AllDirectories)，否则子文件夹可能会排到父文件夹前面。
				string[] allDirNames = Directory.GetDirectories(parentFolder);

				allFileNames.AddRange(Directory.GetFiles(parentFolder));

				// 先排序目录再排序每个目录中的文件是高效的算法。
				ys.DataProcessor.SortByName(allDirNames);
				foreach (string dir in allDirNames)
				{
					try
					{
						filesInDir = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
						ys.DataProcessor.SortByName(filesInDir);
						allFileNames.AddRange(filesInDir);
					}
					catch { /* 系统文件夹和一些没有权限访问的 */ }
				}
			}
			else
			{
				filesInDir = Directory.GetFiles(parentFolder);
				ys.DataProcessor.SortByName(filesInDir);
				allFileNames.Clear();
				allFileNames.AddRange(filesInDir);
			}

			currentIndex = allFileNames.IndexOf(imgInfo.FullName);
		}

		private string GetAvailableBesideFileName(int index, int distance)
		{
			index = index + distance;
			if (index >= 0 && index < allFileNames.Count)
			{
				string name = allFileNames[index] as string;
				try
				{
					Stream stream = new FileStream(name, FileMode.Open, FileAccess.Read);
					BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.Default);
					stream.Dispose();
					currentIndex = index;
					if (!taskBar.isNormalState)
						taskBar.SetProgressState(System.Windows.Win7.TbpFlag.Normal);
				}
				catch
				{
					allFileNames[index] = null;
					name = GetAvailableBesideFileName(index, distance);
				}
				return name;
			}
			else
			{
				ArrayList temp = new ArrayList(allFileNames);
				foreach (string s in temp)
				{
					if (s == null) allFileNames.Remove(s);
				}
				currentIndex = allFileNames.IndexOf(imgInfo.FullName);
				if (wheelSwitchspeed == 1)
				{
					taskBar.SetProgressState(System.Windows.Win7.TbpFlag.Paused);
					taskBar.FlashTaskBar(FlashOption.FLASHW_ALL);
					UpdateInfo(Comisor.Resource.State_AtEnd);
				}
				else UpdateInfo();
				return null;
			}
		}

		private void NavImageJudge(object sender, MouseWheelEventArgs e)
		{
			if (e.RightButton == MouseButtonState.Pressed) return;	// Goto the ZoomerStart

			if (e.LeftButton == MouseButtonState.Pressed)			// Navigate frames
			{
				NavFrameJudge(sender, e);
				return;
			}
			lastMouseDelta = e.Delta < 0 ? 1 : -1;
			NavImage(e.Delta < 0 ? 1 : -1);
		}

		private void NavImage(int delta)
		{
			// Half Page Mode;
			if (mitPageMode.isChecked && !isHalfPage)
			{
				int indexTo = Array.IndexOf<CroppedBitmap>(croppedBmpHalf, imgContainer.Source as CroppedBitmap) + delta;
				if (indexTo >= 0 && indexTo < 2)
				{
					imgContainer.Source = croppedBmpHalf[indexTo];

					KeepLastFrameInfo();

					if (mitAutoFit.isChecked)
					{
						AutoFrameSize();
						AutoRenderOption();
					}
					else
						AutoFit();
					if (mitFixedPoint.isChecked)
					{
						AutoPosition();
					}

#if !isFX4
					SnapToPixel();
#endif
					return;
				}
			}

			string name = GetAvailableBesideFileName(currentIndex, delta * wheelSwitchspeed);
			if (name != null)
			{
				imgInfo = new FileInfo(name);

				Mouse.OverrideCursor = main.resource.curHand_Wait;
				if (tmrNavImage.IsEnabled)
				{
					UpdateInfo();
					tmrNavImage.Stop();
					wheelSwitchspeed += 1;
				}

				tmrNavImage.Start();	// 这个指向 OpenImage().
			}
		}

		private void tmr_NavImage(object sender, EventArgs e)	// Achieve the fast switch.
		{
			tmrNavImage.Stop();
			wheelSwitchspeed = 1;
			currentFrame = 0;
			OpenImage();
			Dispatcher.CurrentDispatcher.Invoke(new InvokeDelegate(EndBusy), System.Windows.Threading.DispatcherPriority.Background, null);
		}

		private void NavFrameJudge(object sender, MouseWheelEventArgs e)
		{
			NavFrame(e.Delta < 0 ? 1 : -1);
			e.Handled = true;
		}

		private void NavFrame(int delta)
		{
			if (currentFrame + delta >= 0 &&
				currentFrame + delta < frameCount)
			{
				currentFrame += delta;

				OpenImage();
			}
		}

		private void CheckAndUpdateSource()
		{
			Stream bmpStream = new MemoryStream(File.ReadAllBytes(imgInfo.FullName), false);
			bmpDecoder = BitmapDecoder.Create(bmpStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None);

			frameCount = bmpDecoder.Frames.Count;
			if (frameCount == 0)
				imgContainer.Source = main.resource.imgZeroDemension;
			// 某些图片的某些图层尺寸为0
			else if (bmpDecoder.Frames[currentFrame].PixelWidth == 0 ||
				bmpDecoder.Frames[currentFrame].PixelHeight == 0)
				imgContainer.Source = main.resource.imgZeroDemension;
			else if (frameCount == 1)
			{
				currentFrame = 0;
				imgContainer.Source = bmpDecoder.Frames[currentFrame];
			}
			else
				imgContainer.Source = bmpDecoder.Frames[currentFrame];

			bmpOriginal = imgContainer.Source as BitmapSource;
			if (indexPageModeJudge == 0)
				isHalfPage = bmpOriginal.PixelWidth < bmpOriginal.PixelHeight;	// Only divide wide page.
			else
				isHalfPage = false;

			UpdateSizeInfo();

			AutoLevels();

			UpdateInfo();
		}

		private void OpenImage()
		{
			if (isFirstImage)
			{
				isFirstImage = false;
				if (mitAutoFit.isChecked)	// Confirm the bdr has been initialized.
				{
					bdrTransformFrame.Width = rectLastFrame.Width + szOffset.Width;
					bdrTransformFrame.Height = rectLastFrame.Height + szOffset.Height;
					Canvas.SetLeft(bdrTransformFrame, rectLastFrame.X - szOffset.Width);
					Canvas.SetTop(bdrTransformFrame, rectLastFrame.Y - szOffset.Height);
				}
				else
				{
					rectLastFrame.Size = cavStage.RenderSize;
				}
			}
			else
				KeepLastFrameInfo();

			CheckAndUpdateSource();

			if (mitPageMode.isChecked)
				DividePage();

			if (mitAutoFit.isChecked)
			{
				AutoFrameSize();
				AutoRenderOption();
			}
			else
				AutoFit();
			if (mitFixedPoint.isChecked)
			{
				AutoPosition();
			}

#if !isFX4
			SnapToPixel();
#endif
		}

		private void EndBusy()
		{
			Mouse.OverrideCursor = null;
		}

		private void KeepLastFrameInfo()
		{
			if (isInitialized)
			{

				// 注意位置修正
				rectLastFrame = new Rect(
					bdrTransformFrame.TranslatePoint(new Point(szOffset.Width, szOffset.Height), cavStage),
					GetSize()
				);
			}
		}

		private delegate void InvokeDelegate();

		private DispatcherTimer tmrNavImage;
		private int wheelSwitchspeed = 1;

		private BitmapDecoder bmpDecoder;
		private BitmapSource bmpOriginal;

		private CroppedBitmap[] croppedBmpHalf = new CroppedBitmap[2];
		private double dbPageModeRatio = 0.5;
		private bool isHalfPage = false;
		private int lastMouseDelta = 0;

		private TaskBar taskBar = new TaskBar();
		private ArrayList allFileNames;
		private FileInfo imgInfo;
		private int currentIndex;
		private int currentFrame = 0;
		private int frameCount = 1;
		private bool isFirstImage = true;
	}
}