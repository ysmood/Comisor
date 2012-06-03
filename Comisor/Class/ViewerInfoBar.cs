using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ys
{
	public partial class ImageViewer
	{
		private void InitInfoBar()
		{
			#region Main Infor Bar
			bdrInfo.ToolTip = Comisor.Resource.Info_c;
			bdrInfo.CornerRadius = new CornerRadius(3);
			bdrInfo.BorderThickness = new Thickness(1);
			bdrInfo.BorderBrush = new SolidColorBrush(colorBorder);
			bdrInfo.Opacity = 0;
			bdrInfo.Visibility = Visibility.Hidden;

			Border bdrContent = new Border();
			StackPanel stpContent = new StackPanel();
			TextBlock txtTitle = new TextBlock();
			Border bdrTitle = new Border();
			bdrContent.Background = new SolidColorBrush(colorInfoBg);
			bdrContent.CornerRadius = new CornerRadius(2);
			bdrContent.Margin = new Thickness(2);

			txtTitle.Text = Comisor.Resource.Info_Title;
			txtTitle.Foreground = new SolidColorBrush(colorFont);
			txtTitle.FontSize = 12;
			bdrTitle.Padding = new Thickness(5, 5, 5, 3);
			bdrTitle.BorderThickness = new Thickness(0, 0, 0, 1);
			bdrTitle.BorderBrush = new LinearGradientBrush(Color.FromArgb(100, 255, 255, 255), Color.FromArgb(0, 255, 255, 255), 0);
			bdrTitle.Child = txtTitle;
			bdrTitle.Cursor = main.resource.curHand_Over;

			txtInfo.IsReadOnly = true;
			txtInfo.BorderThickness = new Thickness(0);
			txtInfo.Padding = new Thickness(3);
			txtInfo.Background = Brushes.Transparent;
			txtInfo.Foreground = new SolidColorBrush(colorFont);
			txtInfo.FontSize = 12;

			stpContent.Children.Add(bdrTitle);
			stpContent.Children.Add(txtInfo);
			bdrContent.Child = stpContent;
			bdrInfo.Child = bdrContent;

			MouseEventHandler InfoDragOn = (o, e) =>
			{
				Point pt = Mouse.GetPosition(cavStage);
				pt.Offset(-ptDragOffset.X, -ptDragOffset.Y);
				TranslateInCanvas(ref bdrInfo, pt, false);
			};

			bdrTitle.PreviewMouseLeftButtonDown += (o, e) =>
			{
				bdrTitle.CaptureMouse();
				bdrTitle.Cursor = main.resource.curHand_Drag;

				ptDragOffset = Mouse.GetPosition(bdrInfo);
				bdrTitle.MouseMove += InfoDragOn;
				e.Handled = true;
			};

			bdrTitle.PreviewMouseLeftButtonUp += (o, e) =>
			{
				bdrTitle.ReleaseMouseCapture();
				bdrTitle.Cursor = main.resource.curHand_Over;
				bdrTitle.MouseMove -= InfoDragOn;
				e.Handled = true;
			};

			bdrInfo.PreviewMouseRightButtonUp += (o, e) =>
			{
				string info = txtInfo.SelectedText.Replace("\t", "\r\n");
				if (info == "") info = txtInfo.Text;
				Clipboard.SetText(info);
				txtPopup.Text = Comisor.Resource.Copied;
				ShowPopup();

				DispatcherTimer tmr = new DispatcherTimer();
				tmr.Interval = TimeSpan.FromMilliseconds(1000);
				EventHandler tmrHidePopup = (oo, ee) =>
				{
					tmr.Stop();
					HidePopup();
				};
				tmr.Tick += tmrHidePopup;
				tmr.Start();
				
				e.Handled = true;
			};

			bdrInfo.PreviewMouseRightButtonDown += (o, e) => { e.Handled = true; };	// Fix the right button up Zoom trigger.

			cavStage.Children.Add(bdrInfo);
			Canvas.SetLeft(bdrInfo, 5);
			Canvas.SetTop(bdrInfo, 5);
			#endregion

			#region Ratio Info Bar
			txtPopup = new TextBlock();
			txtPopup.FontFamily = new FontFamily("Arial");
			txtPopup.FontWeight = FontWeights.Bold;
			txtPopup.FontSize = 14;
			txtPopup.TextAlignment = TextAlignment.Center;
			txtPopup.Foreground = new SolidColorBrush(colorFont);

			txtOriginalSize = new TextBlock();
			txtOriginalSize.FontFamily = new FontFamily("Arial");
			txtOriginalSize.FontSize = 10;
			txtOriginalSize.TextAlignment = TextAlignment.Center;
			txtOriginalSize.Foreground = new SolidColorBrush(Colors.White);

			bdrPopup = new Border();
			bdrPopup.Background = new SolidColorBrush(colorInfoBg);
			bdrPopup.CornerRadius = new CornerRadius(3);
			bdrPopup.Padding = new Thickness(5, 2, 5, 2);
			bdrPopup.Opacity = 0;
			bdrPopup.Visibility = Visibility.Collapsed;

			stpZoomInfoBar = new StackPanel();
			stpZoomInfoBar.Children.Add(txtPopup);
			stpZoomInfoBar.Children.Add(txtOriginalSize);

			bdrPopup.Child = stpZoomInfoBar;
			cavStage.Children.Add(bdrPopup);
			#endregion
		}

		private void ShowInfoBar()
		{
			bdrInfo.Visibility = Visibility.Visible;

			DoubleAnimation dbaOpacity = new DoubleAnimation(1, durationCommon + durationCommon);

			ScaleTransform scaleTransform = new ScaleTransform();
			bdrInfo.RenderTransform = scaleTransform;

			LinearDoubleKeyFrame dbk01 = new LinearDoubleKeyFrame(1 + 6 / bdrInfo.ActualWidth, TimeSpan.FromMilliseconds(120));
			LinearDoubleKeyFrame dbk02 = new LinearDoubleKeyFrame(1, TimeSpan.FromMilliseconds(200));

			DoubleAnimationUsingKeyFrames dbak = new DoubleAnimationUsingKeyFrames();
			dbak.KeyFrames.Add(dbk01);
			dbak.KeyFrames.Add(dbk02);
			dbak.AccelerationRatio = 0;

			bdrInfo.BeginAnimation(Border.OpacityProperty, dbaOpacity);
			scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, dbak);
			scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, dbak);
		}

		private void HideInfoBar()
		{
			DoubleAnimation dbaOpacity = new DoubleAnimation(0, durationCommon);
			dbaOpacity.Completed += (o, e) =>
			{
				bdrInfo.Visibility = Visibility.Collapsed;
			};

			DoubleAnimation dbaScale = new DoubleAnimation(0.95, durationCommon);
			dbaScale.DecelerationRatio = 1;
			ScaleTransform scaleTransform = new ScaleTransform();
			bdrInfo.RenderTransform = scaleTransform;

			bdrInfo.BeginAnimation(Border.OpacityProperty, dbaOpacity);
			scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, dbaScale);
			scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, dbaScale);
		}

		private void UpdateInfo(string state = null)
		{
			#region Index
			string indexInfo = (currentIndex + 1).ToString() + "/" + allFileNames.Count;
			#endregion

			#region Frame Index
			string pageModeView = "";
			if (mitPageMode.isChecked)
			{
				if (Array.IndexOf<CroppedBitmap>(croppedBmpHalf, imgContainer.Source as CroppedBitmap) == indexFixedPoint % 2)
					pageModeView = Comisor.Resource.Page_Mode_Left;
				else
					pageModeView = Comisor.Resource.Page_Mode_Right;
			}
			string indexFrame = (currentFrame + 1).ToString() + pageModeView + "/" + frameCount;
			#endregion

			#region Size
			string sizeInfo;
			if (imgInfo.Length < 1E3)
				sizeInfo = imgInfo.Length.ToString("N1") + " B";
			else if (imgInfo.Length < 1E6)
				sizeInfo = ((double)imgInfo.Length / 1024).ToString("N1") + " KB";
			else
				sizeInfo = ((double)imgInfo.Length / (1024 * 1024)).ToString("N1") + " MB";
			#endregion

			#region Zoom Ratio
			string zoomRatioInfo;
			double zoomRatio = currentRatio;
			if (zoomRatio < 10)
				zoomRatioInfo = zoomRatio.ToString("0%");
			else
				zoomRatioInfo = zoomRatio.ToString("0" + Comisor.Resource.Info_Times);
			#endregion

			#region Code Info
			string fileExtensions = "";
			int bitsPerPixel = 0;
			double bmpDpiX = 0;
			if (bmpDecoder != null)
			{
				fileExtensions = bmpDecoder.CodecInfo.FileExtensions;
				bitsPerPixel = bmpOriginal.Format.BitsPerPixel;
				bmpDpiX = bmpOriginal.DpiX;
			}
			#endregion

			#region Original Size and Ratio
			txtPopup.Text = zoomRatioInfo;
			txtOriginalSize.Text = szOriginal.Width + "×" + szOriginal.Height;
			#endregion

			#region Dimension
			string dimension =
				txtOriginalSize.Text + "×" +
				bitsPerPixel +
				" ( " + bmpDpiX.ToString("0") + "ppi )";
			#endregion

			txtInfo.Text = ""
				+ Comisor.Resource.Info_FileIndex + indexInfo + " " + state + "\t"
				+ Comisor.Resource.Info_LayerIndex + indexFrame + "\t"
				+ Comisor.Resource.Info_FileType + fileExtensions + "\t\n"

				+ Comisor.Resource.Info_ZoomRatio + zoomRatioInfo + "\t"
				+ Comisor.Resource.Info_FileSize + sizeInfo + "\t"
				+ Comisor.Resource.Info_Dimension + dimension + "\t\n"

				+ Comisor.Resource.Info_FilePath + imgInfo.DirectoryName + "\t\n"
				+ Comisor.Resource.Info_FileName + imgInfo.Name;

			// Windows任务栏信息
			taskBar.ChangeProcessValue((ulong)currentIndex + 1, (ulong)allFileNames.Count);
			main.Title = "[" + indexInfo + "," + indexFrame + "][" + zoomRatioInfo + "][" + sizeInfo + "][" + dimension + "] " + imgInfo.FullName;
		}

		private void ShowPopup()
		{
			if (bdrPopup.Visibility == Visibility.Visible)
				return;
			else
				bdrPopup.Visibility = Visibility.Visible;

			Point ptMouse = Mouse.GetPosition(cavStage);
			Canvas.SetLeft(bdrPopup, ptMouse.X + 15);
			Canvas.SetTop(bdrPopup, ptMouse.Y + 15);
			cavStage.MouseMove += Popup_FollowMouse;

			DoubleAnimation dbaOpacity, dbaScale;
			dbaOpacity = new DoubleAnimation(1, durationCommon);
			dbaScale = new DoubleAnimation(0.8, 1, durationCommon);

			dbaScale.AccelerationRatio = 1;

			ScaleTransform scaleTransform = new ScaleTransform();
			bdrPopup.RenderTransform = scaleTransform;

			bdrPopup.BeginAnimation(Border.OpacityProperty, dbaOpacity);
			scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, dbaScale);
			scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, dbaScale);
		}

		private void HidePopup()
		{
			if (bdrPopup.Visibility != Visibility.Visible) return;

			DoubleAnimation dbaScale = new DoubleAnimation(1, 0.8, durationCommon + durationCommon);
			dbaScale.AccelerationRatio = 1;

			DoubleAnimation dbaOpacity = new DoubleAnimation(0, durationCommon + durationCommon);
			// 防止它捕捉到鼠标
			dbaOpacity.Completed += (o, e) =>
			{
				cavStage.MouseMove -= Popup_FollowMouse;
				bdrPopup.Visibility = Visibility.Collapsed;
			};

			bdrPopup.BeginAnimation(Border.OpacityProperty, dbaOpacity);

			ScaleTransform scaleTransform = new ScaleTransform();
			bdrPopup.RenderTransform = scaleTransform;
			scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, dbaScale);
			scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, dbaScale);
		}

		private void Popup_FollowMouse(object sender, MouseEventArgs e)
		{
			Point ptMouse = Mouse.GetPosition(cavStage);
			Canvas.SetLeft(bdrPopup, ptMouse.X + 15);
			Canvas.SetTop(bdrPopup, ptMouse.Y + 15);
		}

		private TextBox txtInfo = new TextBox();
		private Border bdrInfo = new Border();

		private TextBlock txtPopup;
		private TextBlock txtOriginalSize;
		private StackPanel stpZoomInfoBar;
		private Border bdrPopup;
	}
}
