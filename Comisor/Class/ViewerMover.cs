
//#define isFX4


#define Window_Borderless

using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace ys
{
	public partial class ImageViewer
	{
		private void InitDrag()
		{
			// Cursor initialization.
			// Tip: 数据流的使用.

			tmrMouseRecorder = new Timer(new TimerCallback(RecordMouse), null, 0, 10);
			// Don't use the QueryContinueDrag, it can't meet the need.(Even in .NET4)
#if Window_Borderless
			bdrTransformFrame.Cursor = main.resource.curHand_Over;
			bdrTransformFrame.MouseLeftButtonDown += new MouseButtonEventHandler(DragStart);
			bdrTransformFrame.MouseLeftButtonUp += new MouseButtonEventHandler(DragStop);
#else
			cavStage.Cursor = main.resource.curHand_Over;
			cavStage.MouseLeftButtonDown += new MouseButtonEventHandler(DragStart);
			cavStage.MouseLeftButtonUp += new MouseButtonEventHandler(DragStop);
#endif
		}

		private void DragStart(object sender, MouseEventArgs e)
		{
			CompositionTarget.Rendering -= DragEase;
			CompositionTarget.Rendering -= ZoomEase;

			bdrTransformFrame.Cursor = main.resource.curHand_Drag;

			bdrTransformFrame.CaptureMouse();
			bdrTransformFrame.MouseMove += DragOn;

			ptDragOffset = e.GetPosition(bdrTransformFrame);
			ptPrevious = e.GetPosition(cavStage);
			ptPrevious.Offset(-ptDragOffset.X, -ptDragOffset.Y);
			ptStartDrag = ptPrevious;
		}

		private void RecordMouse(object o)
		{
			// Value of the win32 api point should be integer.
			nptStart = nptEnd;
			GetCursorPos(out nptEnd);
		}

		private void DragOn(object sender, MouseEventArgs e)
		{
			ptCurrent = Mouse.GetPosition(cavStage);
			ptCurrent.Offset(-ptDragOffset.X, -ptDragOffset.Y);
			
			// 当为半页浏览模式时防抖动
			if (mitPageMode.isChecked == true &&
				Math.Abs(ptCurrent.X - ptStartDrag.X) < nPreventDither)
				TranslateInCanvas(ref bdrTransformFrame, new Point(ptStartDrag.X, ptCurrent.Y), false);
			else
				TranslateInCanvas(ref bdrTransformFrame, ptCurrent, true);
			
			ptPrevious = ptCurrent;
		}

		private void DragStop(object sender, MouseEventArgs e)
		{
			bdrTransformFrame.MouseMove -= DragOn;
			bdrTransformFrame.ReleaseMouseCapture();
			bdrTransformFrame.Cursor = main.resource.curHand_Over;

			// Get the velocity of the image.
			// 当为半页浏览模式时防抖动
			if (mitPageMode.isChecked == true &&
				Math.Abs(ptCurrent.X - ptStartDrag.X) < nPreventDither)
				dragVelocity.X = 0;
			else
				dragVelocity.X = (nptEnd.X - nptStart.X) * dragFlotage / 1000 * Math.Sqrt(bdrTransformFrame.Width);
			dragVelocity.Y = (nptEnd.Y - nptStart.Y) * dragFlotage / 1000 * Math.Sqrt(bdrTransformFrame.Height);

			CompositionTarget.Rendering += new EventHandler(DragEase);		// Launch ease movement.
		}

		private void DragEase(object sender, EventArgs e)
		{
			if (Math.Abs(dragVelocity.X) < 0.01 && Math.Abs(dragVelocity.Y) < 0.01)
			{
				CompositionTarget.Rendering -= DragEase;
				AutoRenderOption();
#if !isFX4
				SnapToPixel();
#endif
				return;
			}
			Point pt = bdrTransformFrame.TranslatePoint(new Point(dragVelocity.X, dragVelocity.Y), cavStage);
			TranslateInCanvas(ref bdrTransformFrame, pt, true);

			// The iteration part to achieve ease movement.
			dragVelocity.X *= velocityAttenuater / 200;
			dragVelocity.Y *= velocityAttenuater / 200;
		}

		private void SetSightEdge(ref Margin rgnSightEdge)
		{
			const double ratio = 0.05;
			rgnSightEdge.Left = cavStage.ActualWidth * ratio - bdrTransformFrame.Width;
			rgnSightEdge.Right = cavStage.ActualWidth * (1 - ratio);
			rgnSightEdge.Top = cavStage.ActualHeight * ratio - bdrTransformFrame.Height;
			rgnSightEdge.Bottom = cavStage.ActualHeight * (1 - ratio);
		}

		private void TranslateInCanvas(ref Border target, Point ptCurrent, bool isCheckSight)// Translate and limit the position of the image.
		{
			// Make its edge always can be seen.
			// Bug：IF 语句将限定图片框架的移动，防止最大化窗口时产生的死锁。
			bool isXset = true;
			if (isCheckSight)
			{
				SetSightEdge(ref rgnSightEdge);

				if ((ptCurrent.X - ptPrevious.X < 0 && ptCurrent.X > rgnSightEdge.Left) ||
					(ptCurrent.X - ptPrevious.X > 0 && ptCurrent.X < rgnSightEdge.Right))
				{
					Canvas.SetLeft(target, ptCurrent.X);
				}
				else isXset = false;
				if ((ptCurrent.Y - ptPrevious.Y < 0 && ptCurrent.Y > rgnSightEdge.Top) ||
					(ptCurrent.Y - ptPrevious.Y > 0 && ptCurrent.Y < rgnSightEdge.Bottom))
				{
					Canvas.SetTop(target, ptCurrent.Y);
				}
				else
				{
					if (!isXset) ptDragOffset = Mouse.GetPosition(bdrTransformFrame);
				}
			}
			else
			{
				Canvas.SetLeft(target, ptCurrent.X);
				Canvas.SetTop(target, ptCurrent.Y);
			}
		}
#if !isFX4
		public void SnapToPixel() // Snap to the pixel, make it clear to see. Used in .Net3.5
		{
			Canvas.SetLeft(bdrTransformFrame, Math.Round(Canvas.GetLeft(bdrTransformFrame), 0));
			Canvas.SetTop(bdrTransformFrame, Math.Round(Canvas.GetTop(bdrTransformFrame), 0));
		}
#endif
		private void AutoPosition()
		{
			switch (indexFixedPoint)
			{
				// 不能同时使用SetLeft和SetRight，否则会拉伸图片，也不能使用ActualWidth之类的，否则会得到非即时数据。
				case 0:
					Canvas.SetLeft(bdrTransformFrame, ptFixedPoint.X);
					Canvas.SetTop(bdrTransformFrame, ptFixedPoint.Y);
					break;
				case 1:
					Canvas.SetLeft(bdrTransformFrame, ptFixedPoint.X - bdrTransformFrame.Width);
					Canvas.SetTop(bdrTransformFrame, ptFixedPoint.Y);
					break;
				case 2:
					Canvas.SetLeft(bdrTransformFrame, ptFixedPoint.X);
					Canvas.SetTop(bdrTransformFrame, ptFixedPoint.Y - bdrTransformFrame.Height);
					break;
				case 3:
					Canvas.SetLeft(bdrTransformFrame, ptFixedPoint.X - bdrTransformFrame.Width);
					Canvas.SetTop(bdrTransformFrame, ptFixedPoint.Y - bdrTransformFrame.Height);
					break;
			}
		}

		private void UpdateRefPoint()
		{
			Point ptFixed;
			switch (indexFixedPoint)
			{
				case 0:
					ptFixed = new Point(0, 0);
					break;
				case 1:
					ptFixed = new Point(bdrTransformFrame.ActualWidth, 0);
					break;
				case 2:
					ptFixed = new Point(0, bdrTransformFrame.ActualHeight);
					break;
				case 3:
					ptFixed = new Point(bdrTransformFrame.ActualWidth, bdrTransformFrame.ActualHeight);
					break;
				default:
					ptFixed = new Point(0, 0);
					break;
			}
			ptFixedPoint = bdrTransformFrame.TranslatePoint(ptFixed, cavStage);
		}

		[DllImport("user32.dll")]
		private static extern bool GetCursorPos(out nPoint p);

		/// Global Members
		private Point ptStartDrag;
		private Point ptPrevious;
		private Point ptCurrent;
		private Point ptDragOffset;
		private int nPreventDither = 80;

		private Timer tmrMouseRecorder;
		private nPoint nptStart, nptEnd;

		private Vector dragVelocity;
	}

	#region Integer value, the WPF Point contains double float value.
	public struct nPoint
	{
		public int X;
		public int Y;
	}
	#endregion
}
