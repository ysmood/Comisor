
//#define isFX4

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.IO;
using System.Windows.Media.Imaging;

namespace ys
{
	public partial class ImageViewer
	{
		private void InitZoom()
		{
			bdrTransformFrame.MouseWheel += ZoomStart;
		}

		private void UpdateSizeInfo()
		{
			szOriginal.Width = (imgContainer.Source as BitmapSource).PixelWidth;
			szOriginal.Height = (imgContainer.Source as BitmapSource).PixelHeight;
			aspectRatio = szOriginal.Width / szOriginal.Height;
		}

		private void ZoomStart(object sender, System.Windows.Input.MouseWheelEventArgs e)
		{
			bdrTransformFrame.MouseMove -= RButtonGesture;

			if (e.RightButton == MouseButtonState.Released) return;

			if (!isZoomOn)
			{
				isZoomOn = true;
				ShowPopup();
			}

			// Accelerate
			if (e.Timestamp - zoomTimestamp < 40)
				zoomVelocity *= 1.8;
			else
				zoomVelocity = (e.Delta > 0 ? 1 : -1)
								* zoomFlotage / 60
								* Math.Log10(bdrTransformFrame.Width);

			CompositionTarget.Rendering -= ZoomEase;
			CompositionTarget.Rendering += ZoomEase;

			zoomTimestamp = e.Timestamp;
		}

		private void ZoomEase(object sender, EventArgs e)
		{
			if (Math.Abs(zoomVelocity) < 0.001 || zoomVelocity <= -1)
			{
				CompositionTarget.Rendering -= ZoomEase;
				AutoRenderOption();
#if !isFX4
				SnapToPixel();
#endif
				return;
			}

			ZoomByRatio(GetSize(true),1 + zoomVelocity);
			// The iteration part to achieve ease movement.
			zoomVelocity *= velocityAttenuater / 200;
		}

		private void ZoomCursorCenterOffset(Size sizeTo)
		{
			Point ptMouse = Mouse.GetPosition(bdrTransformFrame);
			Point pt = bdrTransformFrame.TranslatePoint(
				new Point(
					(1 - sizeTo.Width / GetSize(true).Width) * ptMouse.X,
					(1 - sizeTo.Height / GetSize(true).Height) * ptMouse.Y
				),
				cavStage
			);
			if (Mouse.DirectlyOver == imgContainer)
			{
				TranslateInCanvas(ref bdrTransformFrame, pt, false);
			}
		}

		private void ZoomByRatio(Size szFrom, double ratio)
		{
			Size szTo = new Size(szFrom.Width * ratio, szFrom.Height * ratio);
			ZoomCursorCenterOffset(szTo);
			SetSize(szTo.Width);
			UpdateInfo();
		}

		private void AutoRenderOption()// Once crossed the threshold, zoom by pixel.
		{
			if (RenderOptions.GetBitmapScalingMode(imgContainer) != BitmapScalingMode.NearestNeighbor
				&& (currentRatio > pixelShowThreshold || !mitThreshold.IsChecked)
				)
				RenderOptions.SetBitmapScalingMode(imgContainer, BitmapScalingMode.NearestNeighbor);
			else if(RenderOptions.GetBitmapScalingMode(imgContainer) != scalingMode)
			{
				RenderOptions.SetBitmapScalingMode(imgContainer, scalingMode);
			}
		}

		private void AutoFrameSize()
		{
			switch (indexAutoFitLock)
			{
				case 0:	// Lock Size
					if (szOriginal.Width > rectLastFrame.Width || szOriginal.Height > rectLastFrame.Height)
					{
						if (aspectRatio > rectLastFrame.Width / rectLastFrame.Height)
						{
							imgContainer.Width = rectLastFrame.Width;
							imgContainer.Height = rectLastFrame.Width / aspectRatio;
						}
						else
						{
							imgContainer.Height = rectLastFrame.Height;
							imgContainer.Width = rectLastFrame.Height * aspectRatio;
						}
					}
					else // 当小于边框时，不进行拉伸。
					{
						imgContainer.Width = szOriginal.Width;
						imgContainer.Height = szOriginal.Height;
					}
					currentRatio = imgContainer.Width / szOriginal.Width;
					break;
				case 1: // Lock Ratio
					SetSize(szOriginal.Width * currentRatio);
					// Center by last image
					Canvas.SetLeft(bdrTransformFrame, Canvas.GetLeft(bdrTransformFrame) + (rectLastFrame.Width - GetSize().Width) / 2);
					Canvas.SetTop(bdrTransformFrame, Canvas.GetTop(bdrTransformFrame) + (rectLastFrame.Height - GetSize().Height) / 2);
#if !isFX4
					SnapToPixel();
#endif
					break;
			}
			AutoRenderOption();
			UpdateInfo();
		}

		private void AutoFit()
		{
			if (szOriginal.Width > cavStage.ActualWidth || szOriginal.Height > cavStage.ActualHeight)
			{
				// 这个需要按一定顺序赋值。
				if (aspectRatio > cavStage.ActualWidth / cavStage.ActualHeight)
				{
					SetSize(cavStage.ActualWidth - szOffset.Width);
				}
				else
				{
					SetSize(0, cavStage.ActualHeight - szOffset.Height);
				}
			}
			else
			{
				SetSize(szOriginal.Width);
			}
			Canvas.SetLeft(bdrTransformFrame, (cavStage.ActualWidth - bdrTransformFrame.Width) / 2);
			Canvas.SetTop(bdrTransformFrame, (cavStage.ActualHeight - bdrTransformFrame.Height) / 2);
#if !isFX4
			SnapToPixel();
#endif
			AutoRenderOption();
			UpdateInfo();
		}

		private void ZoomToOriginal()
		{
			ZoomByRatio(szOriginal, 1);
			AutoRenderOption();
#if !isFX4
			SnapToPixel();
#endif
		}

		private void SetSize(double width = 0, double height = 0)
		{
			#region 这部分使外框贴合imgContainer
			if (!double.IsNaN(imgContainer.Width))
			{
				// This is very important for the dependency property to auto size in the border frame.
				imgContainer.ClearValue(Image.WidthProperty);
				imgContainer.ClearValue(Image.HeightProperty);

				Point ptOffset = new Point(
					(rectLastFrame.Width - imgContainer.ActualWidth) / 2,
					(rectLastFrame.Height - imgContainer.ActualHeight) / 2
				);

				Canvas.SetLeft(bdrTransformFrame, Canvas.GetLeft(bdrTransformFrame) + ptOffset.X);
				Canvas.SetTop(bdrTransformFrame, Canvas.GetTop(bdrTransformFrame) + ptOffset.Y);

				SetSize(imgContainer.ActualWidth, imgContainer.ActualHeight);
			}
			#endregion

			if (height == 0)
			{
				bdrTransformFrame.Width = width + szOffset.Width;
				bdrTransformFrame.Height = width / aspectRatio + szOffset.Height;
			}
			else
			{
				bdrTransformFrame.Height = height + szOffset.Height;
				bdrTransformFrame.Width = height * aspectRatio + szOffset.Width;
			}
			currentRatio = (bdrTransformFrame.Width - szOffset.Width) / szOriginal.Width;
		}

		private Size GetSize(bool isCheckImage = false)
		{
			Size sz;
			if (isCheckImage && !double.IsNaN(imgContainer.Width))
				sz = new Size(imgContainer.Width, imgContainer.Height);
			else
				sz = new Size(bdrTransformFrame.Width - szOffset.Width, bdrTransformFrame.Height - szOffset.Height);
			return sz;
		}

		/// Global Members
		private bool isZoomOn = false;
		private Size szOriginal;
		private Size szOffset;
		private double aspectRatio;
		private double zoomVelocity;
		private Margin rgnSightEdge;		// Avoid the image been drag out of the frame.
		private int zoomTimestamp = 0;
		private BitmapScalingMode scalingMode = BitmapScalingMode.Fant;
	}

	public struct Margin
	{
		public double Left;
		public double Right;
		public double Top;
		public double Bottom;
	}
}
