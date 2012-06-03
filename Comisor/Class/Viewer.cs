
//#define Debug
//#define isFX4
#define Window_Borderless

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Xml;

namespace ys
{
	public partial class ImageViewer
	{
		public ImageViewer(Comisor.MainWindow mainWindow)
		{
			this.main = mainWindow;

			LoadUserInfo();
#if Window_Borderless
			FullScreenSwitch();
#endif
			InitMainWindow();
			InitCanvasStage();
			InitBorderTransformFrame();
			InitImageContainer();
			InitInfoBar();
			InitDrag();
			InitZoom();
			InitNavigator();
			InitKeyControlLogic();
		}

		public void CheckAndStart(string fullName, bool isFirstImage = false)
		{
			#region Check file or directoty existence, and get the first file.
			if (File.Exists(fullName))
			{
				try
				{
					Stream stream = new FileStream(fullName, FileMode.Open, FileAccess.Read);	// If don't set the "FileAccess.Read", it can't read the files in Read Only dir.
					BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.Default);
					stream.Dispose();
					imgInfo = new FileInfo(fullName);
				}
				catch
				{
					string info = fullName + "\n\n" + Comisor.Resource.Exception_NoMatchCodec;
					Clipboard.SetText(info);
					ReportException(info, isFirstImage);
				}
			}
			else if (Directory.Exists(fullName))
			{
				string[] files = Directory.GetFiles(fullName);
				ys.DataProcessor.SortByName(files);
				foreach (string file in files)
				{
					try
					{
						Stream stream = new FileStream(fullName, FileMode.Open, FileAccess.Read);
						BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.Default);
						stream.Dispose();
						imgInfo = new FileInfo(fullName);
						goto Found;
					}
					catch { /* File not available.  */ }
				}

				// 这里不能使用 Directory.GetDirectories(fullName "*", SearchOption.AllDirectories)，否则子文件夹可能会排到父文件夹前面。
				string[] dirs = Directory.GetDirectories(fullName);
				ys.DataProcessor.SortByName(dirs);
				foreach (string dir in dirs)
				{
					files = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
					ys.DataProcessor.SortByName(files);
					foreach (string file in files)
					{
						try
						{
							Stream stream = new FileStream(file, FileMode.Open, FileAccess.Read);
							BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.Default);
							imgInfo = new FileInfo(file);
							goto Found;
						}
						catch { /* File not available.  */ }
					}
				}
			}
		Found:
			if (imgInfo == null)
				ReportException(Comisor.Resource.Exception_NoMatchFile, true);
			#endregion

			if (!isInitialized)
			{
				InitContextMenu(); // It should be init after the Start Window closed.
				main.ShowInTaskbar = true;
				isInitialized = true;
			}
			
			this.isFirstImage = isFirstImage;

			if (!isFirstImage)
			{
				mitCollectionExplore.isChecked = false;
				mitPageMode.isChecked = false;
				mitAutoLevels.isChecked = false;
			}
			GetCollection(mitCollectionExplore.isChecked);

			NavImage(0);
#if Window_Borderless
			if (isFirstImage) main.WindowStateAnimation(false, 2, isFirstImage);
#else
			if (main.WindowState == WindowState.Minimized) main.WindowState = WindowState.Normal;
#endif
		}

		public MessageBoxResult ReportException(string s = "", bool shutdown = false, MessageBoxButton mb = MessageBoxButton.OK)
		{
			MessageBoxResult re = MessageBox.Show(s, "Comisor", mb, MessageBoxImage.Warning);
			if (shutdown) System.Diagnostics.Process.GetCurrentProcess().Kill();
			return re;
		}

		public void CloseWindow()
		{
#if Window_Borderless
			SaveUserInfo();
			main.WindowStateAnimation(true);
#else
			main.Close();
#endif
		}

		public bool isInitialized = false;

	// ******************************** Common **************************************

		private void InitMainWindow()
		{
			imgContainer = new Image();
			bdrTransformFrame = new Border();
			cavStage = new Canvas();

			bdrTransformFrame.Child = imgContainer;
			cavStage.Children.Add(bdrTransformFrame);
			main.Content = cavStage;
			main.FontFamily = new FontFamily("Microsoft YaHei");
			main.SnapsToDevicePixels = true;
			main.AllowDrop = true;
			main.Drop += DropFileOpen;
#if !Window_Borderless
			main.Closing += (o, e) => 
			{
				SaveUserInfo();
			};
#endif
		}

		private void InitCanvasStage()
		{
			cavStage.ClipToBounds = true;
			cavStage.Background = scbBg;
			if(isBgOn)
				cavStage.Background.Opacity = scbBg.Opacity / 100;
			else
				cavStage.Background.Opacity = 0;
#if isFX4
			cavStage.UseLayoutRounding = true;
#endif
#if Window_Borderless
			cavStage.MouseDown += (o, e) =>
			{
				if (isBgOn)
					ShowHideBg(!(Mouse.DirectlyOver == cavStage));
			};
#else
			bool isBgShowed = false;
			cavStage.MouseDown += (o, e) =>
			{
				if (!isBgShowed)
				{
					isBgShowed = true;
					ShowHideBg();
				}
			};
			#region Mouse Control Logic
			cavStage.MouseRightButtonDown += (o, e) =>
			{
				ptRButtonDonw = e.GetPosition(cavStage);
				cavStage.MouseMove += RButtonGesture;
			};
			cavStage.MouseRightButtonUp += (o, e) =>
			{
				cavStage.MouseMove -= RButtonGesture;
				HidePopup();

				cavStage.Cursor = main.resource.curHand_Over;
				if (e.LeftButton == MouseButtonState.Pressed)
				{
					if (bdrInfo.Opacity == 0) ShowInfoBar();
					else HideInfoBar();
					e.Handled = true;
				}
				else if (isZoomOn)
				{
					isZoomOn = false;
					e.Handled = true;
				}
				else if (e.GetPosition(cavStage).Y - ptRButtonDonw.Y > 5)
				{
					AutoFit();
					e.Handled = true;
				}
				else if (ptRButtonDonw.Y - e.GetPosition(cavStage).Y > 5)
				{
					ZoomToOriginal();
					e.Handled = true;
				}
			};
			#endregion
#endif
		}

		private void InitBorderTransformFrame()
		{
			bdrTransformFrame.Background = new SolidColorBrush(colorImageBg);
			bdrTransformFrame.BorderBrush = new SolidColorBrush(colorBorder);

			#region Outer Border
			const double borderThickness = 1, padding = 3;
			bdrTransformFrame.CornerRadius = new CornerRadius(3);		// These settings change the final size of szOringal.
			bdrTransformFrame.BorderThickness = new Thickness(borderThickness);
			bdrTransformFrame.Padding = new Thickness(padding);
			szOffset = new Size(2 * (borderThickness + padding), 2 * (borderThickness + padding));

			DoubleAnimation dbaToZero = new DoubleAnimation(0, durationCommon);
			DoubleAnimation dbaToOne = new DoubleAnimation(1, durationCommon);

			bdrTransformFrame.MouseEnter += new MouseEventHandler(
				(o, e) =>
				{
					if (mitAutoFit.isChecked && indexAutoFitLock == 0) return;
					bdrTransformFrame.BorderBrush.BeginAnimation(Brush.OpacityProperty, dbaToZero);
					bdrInfo.BorderBrush.BeginAnimation(Brush.OpacityProperty, dbaToOne);
				}
			);
			bdrTransformFrame.MouseLeave += new MouseEventHandler(
				(o, e) =>
				{
					bdrTransformFrame.BorderBrush.BeginAnimation(Brush.OpacityProperty, dbaToOne);
					bdrInfo.BorderBrush.BeginAnimation(Brush.OpacityProperty, dbaToZero);
				}
			);
			#endregion

#if Window_Borderless
			#region Mouse Control Logic
			bdrTransformFrame.MouseRightButtonDown += (o, e) =>
			{
				ptRButtonDonw = e.GetPosition(cavStage);
				bdrTransformFrame.MouseMove += RButtonGesture;
			};
			bdrTransformFrame.MouseRightButtonUp += (o, e) =>
			{
				bdrTransformFrame.MouseMove -= RButtonGesture;
				HidePopup();

				bdrTransformFrame.Cursor = main.resource.curHand_Over;
				if (e.LeftButton == MouseButtonState.Pressed)
				{
					if (bdrInfo.Opacity == 0) ShowInfoBar();
					else HideInfoBar();
					e.Handled = true;
				}
				else if (isZoomOn)
				{
					isZoomOn = false;
					e.Handled = true;
				}
				else if (e.GetPosition(cavStage).Y - ptRButtonDonw.Y > 5)
				{
					AutoFit();
					e.Handled = true;
				}
				else if (ptRButtonDonw.Y - e.GetPosition(cavStage).Y > 5)
				{
					ZoomToOriginal();
					e.Handled = true;
				}
			};
			#endregion
#endif
		}

		private void InitImageContainer()
		{
			RenderOptions.SetCachingHint(imgContainer, CachingHint.Cache);
			shadowEffect.Color = Colors.Black;
			shadowEffect.Opacity = 0.75;
			shadowEffect.Direction = 270;
			shadowEffect.BlurRadius = dropShadowRadius;
			shadowEffect.ShadowDepth = 0;
			imgContainer.Effect = shadowEffect;
			imgContainer.Stretch = Stretch.Uniform;

			imgContainer.SourceUpdated += new EventHandler<System.Windows.Data.DataTransferEventArgs>((o, e) => { MessageBox.Show(""); });
		}

		private void InitKeyControlLogic()
		{
			#region Mouse Double Click
			main.MouseDoubleClick += (o, e) =>
			{
				if (Mouse.DirectlyOver == bdrTransformFrame &&
					e.RightButton == MouseButtonState.Released)
				{
					System.Diagnostics.Process.Start("explorer.exe", "/select," + imgInfo.FullName);
					CloseWindow();
				}
				else if (e.RightButton == MouseButtonState.Pressed &&
					e.LeftButton == MouseButtonState.Pressed)
				{
					main.PreviewMouseRightButtonUp += (oo, ee) =>
					{
						ee.Handled = true;
						CloseWindow();
					};
				}
			};
			#endregion

			#region Mouse Down
			main.MouseDown += (o, e) =>
				{
					if (frameCount > 1)
					{
						if (e.XButton1 == MouseButtonState.Pressed)
							NavFrame(1);
						if (e.XButton2 == MouseButtonState.Pressed)
							NavFrame(-1);
					}
					else
					{
						if (e.XButton1 == MouseButtonState.Pressed)
							NavImage(1);
						if (e.XButton2 == MouseButtonState.Pressed)
							NavImage(-1);
					}
					if (e.MiddleButton == MouseButtonState.Pressed)
					{
						CloseWindow();
					}
				};
			#endregion

			#region Key Down
			main.PreviewKeyDown += (o, e) =>
				{
					switch (e.Key)
					{
						// Exit
						case Key.Escape:
							CloseWindow();
							break;
						// Fit
						case Key.Multiply:
							AutoFit();
							break;
						case Key.F:
							AutoFit();
							break;
						// Add Bookmark
						case Key.B:
							Comisor.Class.BookmarkEditor editor = new Comisor.Class.BookmarkEditor(main, imgInfo.Name, imgInfo.FullName);
							editor.ShowDialog();
							if(editor.isOK)
								btnAddBookmark.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, editor.cbName.Text));
							break;
						// Mouse mode
						case Key.M:
							SwitchMouseMode();
							break;
						// Original
						case Key.O:
							ZoomToOriginal();
							break;
						case Key.Divide:
							ZoomToOriginal();
							break;
						// Zoom in
						case Key.Add:
							ZoomByRatio(szOriginal, currentRatio * 1.05);
							break;
						case Key.OemPlus:
							ZoomByRatio(szOriginal, currentRatio * 1.05);
							break;
						// Zoom out
						case Key.Subtract:
							ZoomByRatio(szOriginal, currentRatio * 0.95);
							break;
						case Key.OemMinus:
							ZoomByRatio(szOriginal, currentRatio * 0.95);
							break;
						// Next img
						case Key.Space:
							NavImage(1);
							break;
						case Key.Right:
							NavImage(1);
							break;
						// Next frame
						case Key.Down:
							NavFrame(1);
							break;
						// Pre img
						case Key.Left:
							NavImage(-1);
							break;
						// Pre frame
						case Key.Up:
							NavFrame(-1);
							break;
						// Auto Levels
						case Key.L:
							mitAutoLevels.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
							e.Handled = true;
							break;
						// Rotate 90 degree
						case Key.R:
							Rotate(90);
							break;
						// Flip H
						case Key.H:
							Flip(-1, 1);
							break;
						// Flip V
						case Key.V:
							Flip(1, -1);
							break;
						// Reset transform
						case Key.Z:
							imgContainer.Source = bmpOriginal;
							UpdateSizeInfo();
							AutoFit();
							break;
						// Animated Image
						case Key.P:
							if (imgInfo.Extension.ToUpper() == ".GIF" &&
								frameCount > 1)
								ShowGifAnimation();
								break;
						// Copy img
						case Key.C:
							Clipboard.SetImage(ys.ImageProcessor.LayWhiteBg(imgContainer.Source as BitmapSource));
							break;
						// Save img
						case Key.S:
							SaveCurrentAs();
							main.Focus();
							break;
						// Delete img
						case Key.Delete:
							DeleteCurrent();
							break;
						// Information bar
						case Key.F2:
							if (bdrInfo.Opacity == 0) ShowInfoBar();
							else HideInfoBar();
							break;
						// HelpBox
						case Key.F1:
							ShowHelpBox();
							break;
						// HelpBox
						case Key.F5:
							NavImage(0);
							break;
						// Top most
						case Key.T:
							if (!main.Topmost)
								main.Topmost = true;
							else
								main.Topmost = false;
							break;
						// Maxi
						case Key.W:
#if Window_Borderless
							main.WindowStateAnimation();
#else
							if(main.WindowState == WindowState.Normal)
								main.WindowState = WindowState.Minimized;
							else
								main.WindowState = WindowState.Normal;
#endif
							break;
						// Full screen
						case Key.F11:
							FullScreenSwitch();
							break;
					}
#if !isFX4
					SnapToPixel();
#endif
				};
			#endregion

			#region Key Up
			main.PreviewKeyUp += (o, e) =>
				{
					switch (e.Key)
					{
						// zoom in
						case Key.Add:
							AutoRenderOption();
							break;
						case Key.OemPlus:
							AutoRenderOption();
							break;
						// zoom out
						case Key.Subtract:
							AutoRenderOption();
							break;
						case Key.OemMinus:
							AutoRenderOption();
							break;
					}
				};
			#endregion
		}

		private void RButtonGesture(object o, MouseEventArgs e)
		{
			if (isZoomOn) return;
			if (e.GetPosition(cavStage).Y - ptRButtonDonw.Y > 5)
			{
				if (txtPopup.Text == Comisor.Resource.Gesture_FitStage) return;

				txtPopup.Text = Comisor.Resource.Gesture_FitStage;
				ShowPopup();
			}
			else if (ptRButtonDonw.Y - e.GetPosition(cavStage).Y > 5)
			{
				if (txtPopup.Text == Comisor.Resource.Gesture_OriginalSize) return;

				txtPopup.Text = Comisor.Resource.Gesture_OriginalSize;
				ShowPopup();
			}
		}

		private void ShowHideBg(bool show = true)
		{
			DoubleAnimation dba = new DoubleAnimation();
			dba.Duration = durationCommon + durationCommon;
			dba.FillBehavior = FillBehavior.Stop;
			dba.Completed += (o, e) => { scbBg.Opacity = (double)dba.To; };
			if (show)
				dba.To = sldBgOpacity.Value / 100;
			else
				dba.To = 0;
			scbBg.BeginAnimation(Brush.OpacityProperty, dba);
		}

		private void LoadUserInfo()
		{
			if(File.Exists(UserInfoFileName))
			{
				try
				{
					XmlReader xr = XmlReader.Create(UserInfoFileName);
					while (xr.Read())
					{
						// First to load bookmark which is we most don't want to lose when an error occurs.
						#region Bookmark
						if (xr.Name == Comisor.Resource.Bookmark)
						{
							XmlReader sub = xr.ReadSubtree();
							while (sub.Read())
							{
								if (sub.Name == Comisor.Resource.Bookmark_Mark)
								{
									int i = 0;
									string[] bookmarkValues = new string[3];
									while (xr.MoveToNextAttribute())
									{
										string n = xr.Name;
										string v = xr.GetAttribute(i++);

										if (n == Comisor.Resource.Bookmark_Name)
											bookmarkValues[0] = v;
										else if (n == Comisor.Resource.Bookmark_FilePath)
											bookmarkValues[1] = v;
										else if (n == Comisor.Resource.Bookmark_Date)
											bookmarkValues[2] = v;
									}
									bookmarks.Add(new Bookmark(
										bookmarkValues[0],
										bookmarkValues[1],
										Convert.ToDateTime(bookmarkValues[2])
										));
								}
							}
							xr.Skip();
						}
						#endregion

						#region Setting
						if (xr.Name == Comisor.Resource.Setting)
						{
							int i = 0;
							while (xr.MoveToNextAttribute())
							{
								string n = xr.Name;
								string v = xr.GetAttribute(i++);

								if (n == Comisor.Resource.Color_Bg)
									scbBg.Color = (Color)ColorConverter.ConvertFromString(v);

								else if (n == Comisor.Resource.Color_Border)
									colorBorder = (Color)ColorConverter.ConvertFromString(v);

								else if (n == Comisor.Resource.Color_Font)
									colorFont = (Color)ColorConverter.ConvertFromString(v);

								else if (n == Comisor.Resource.Color_Info_Bg)
									colorInfoBg = (Color)ColorConverter.ConvertFromString(v);

								else if (n == Comisor.Resource.Auto_Levels_Threshold)
									autoLevelsThreshold = Convert.ToInt32(v);

								else if (n == Comisor.Resource.Auto_Levels_ColorWeight)
								{
									string[] s = v.Split(new char[] { ',' }, 3, StringSplitOptions.RemoveEmptyEntries);
									double[] dbTemp = new double[3];

									for (int j = 0; j < s.Length; j++)
										dbTemp[j] = Math.Abs(Convert.ToDouble(s[j]));
									if (dbTemp[0] + dbTemp[1] + dbTemp[2] <= 1)
										colorWeight = dbTemp;
									else
										ReportException(Comisor.Resource.Auto_Levels_ColorWeight_Illegal);
								}

								else if (n == Comisor.Resource.Auto_Fit_Lock_On)
									isAutoFitOn = Convert.ToBoolean(v);

								else if (n == Comisor.Resource.Auto_Fit_Lock)
									indexAutoFitLock = Math.Abs(Array.IndexOf(_strAutoFitLock, v) % 2);

								else if (n == Comisor.Resource.Auto_Fit_LastRatio)
									currentRatio = Convert.ToDouble(v);

								else if (n == Comisor.Resource.Auto_Fit_RectLastFrame)
									rectLastFrame = (Rect)(new RectConverter()).ConvertFromString(v);

								else if (n == Comisor.Resource.Fixed_Point)
									isFixedPointOn = Convert.ToBoolean(v);

								else if (n == Comisor.Resource.Fixed_Point_index)
									indexFixedPoint = Math.Abs(Array.IndexOf(_strFixedPoint, v) % 4);

								else if (n == Comisor.Resource.Fixed_Point_pt)
									ptFixedPoint = (Point)(new PointConverter()).ConvertFromString(v);

								else if (n == Comisor.Resource.Background_Opacity_On)
									isBgOn = Convert.ToBoolean(v);

								else if (n == Comisor.Resource.Background_Opacity)
								{
									sldBgOpacity.Minimum = 0;
									sldBgOpacity.Maximum = 100;
									sldBgOpacity.Value = Math.Abs(Convert.ToDouble(v) % 100);
								}

								else if (n == Comisor.Resource.Shadow_Radius)
									dropShadowRadius = Math.Abs(Convert.ToDouble(v));

								else if (n == Comisor.Resource.Float_Value)
									dragFlotage = Math.Abs(Convert.ToDouble(v));

								else if (n == Comisor.Resource.Attenuate)
									velocityAttenuater = Math.Abs(Convert.ToDouble(v) % 190);

								else if (n == Comisor.Resource.Pixel_Threshold)
									pixelShowThreshold = Math.Abs(Convert.ToDouble(v));
							}
							xr.Skip();
						}
						#endregion
#if !Debug
						#region Language
						if (xr.Name == "Language")
						{
							int i = 0;
							while (xr.MoveToNextAttribute())
							{
								string n = xr.Name;
								string v = xr.GetAttribute(i++);

								System.Reflection.FieldInfo[] fieldInfoes = main.resource.GetType().GetFields();
								foreach (System.Reflection.FieldInfo fieldInfo in fieldInfoes)
								{
									if (fieldInfo.Name == n)
										fieldInfo.SetValue(main.resource, v);
								}
							}
							xr.Skip();
						}
						#endregion
#endif
					}
					xr.Close();
				}
				catch
				{
					ReportException(Comisor.Resource.Exception_IllegalInitValue, false);
				}
			}
		}

		private void SaveUserInfo()
		{
			KeepLastFrameInfo();

			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.IndentChars = ("    ");
			settings.NewLineOnAttributes = true;

			XmlWriter xw = XmlWriter.Create(UserInfoFileName, settings);

			xw.WriteStartDocument(true);
			xw.WriteStartElement(main.resource.AssemblyProduct);
			xw.WriteAttributeString("Info",
				main.resource.AssemblyProduct + " " +
				main.resource.AssemblyVersion + "  " +
				main.resource.AssemblyCopyright
			);
			xw.WriteAttributeString("AppPath", GetType().Assembly.Location);

			#region Setting
			xw.WriteStartElement(Comisor.Resource.Setting);
			xw.WriteAttributeString(Comisor.Resource.Color_Bg, scbBg.Color.ToString());
			xw.WriteAttributeString(Comisor.Resource.Color_Border, colorBorder.ToString());
			xw.WriteAttributeString(Comisor.Resource.Color_Info_Bg, colorInfoBg.ToString());
			xw.WriteAttributeString(Comisor.Resource.Color_Font, colorFont.ToString());
			xw.WriteAttributeString(Comisor.Resource.Auto_Levels_Threshold, autoLevelsThreshold.ToString());
			xw.WriteAttributeString(Comisor.Resource.Auto_Levels_ColorWeight, colorWeight[0].ToString() + "," + colorWeight[1].ToString() + "," + colorWeight[2].ToString());
			xw.WriteAttributeString(Comisor.Resource.Auto_Fit_Lock_On, isAutoFitOn.ToString());
			xw.WriteAttributeString(Comisor.Resource.Auto_Fit_Lock, _strAutoFitLock[indexAutoFitLock]);
			xw.WriteAttributeString(Comisor.Resource.Auto_Fit_LastRatio, currentRatio.ToString("F2"));
			xw.WriteAttributeString(Comisor.Resource.Auto_Fit_RectLastFrame, rectLastFrame.X.ToString("0") + "," + rectLastFrame.Y.ToString("0") + "," + rectLastFrame.Width.ToString("0") + "," + rectLastFrame.Height.ToString("0"));
			xw.WriteAttributeString(Comisor.Resource.Fixed_Point, isFixedPointOn.ToString());
			xw.WriteAttributeString(Comisor.Resource.Fixed_Point_index, _strFixedPoint[indexFixedPoint]);
			xw.WriteAttributeString(Comisor.Resource.Fixed_Point_pt, ptFixedPoint.X.ToString("0") + "," + ptFixedPoint.Y.ToString("0"));
			xw.WriteAttributeString(Comisor.Resource.Background_Opacity_On, isBgOn.ToString());
			xw.WriteAttributeString(Comisor.Resource.Background_Opacity, sldBgOpacity.Value.ToString("0"));
			xw.WriteAttributeString(Comisor.Resource.Shadow_Radius, dropShadowRadius.ToString("0"));
			xw.WriteAttributeString(Comisor.Resource.Float_Value, dragFlotage.ToString("0"));
			xw.WriteAttributeString(Comisor.Resource.Attenuate, velocityAttenuater.ToString("0"));
			xw.WriteAttributeString(Comisor.Resource.Pixel_Threshold, pixelShowThreshold.ToString("0"));
			xw.WriteEndElement();
			#endregion

			#region Bookmark
			xw.WriteStartElement(Comisor.Resource.Bookmark);

			// Update the last bk by removing it first.
			if (isInitialized)
			{
				bookmarks.Remove(bookmarks.FindLast((match) => { return match.name == Comisor.Resource.Bookmark_LastTime; }));
				bookmarks.Add(new Bookmark(Comisor.Resource.Bookmark_LastTime, imgInfo.FullName));
			}

			for (int index = 0; index < bookmarks.Count; index++)
			{
				xw.WriteStartElement(Comisor.Resource.Bookmark_Mark);
				xw.WriteAttributeString(Comisor.Resource.Bookmark_Name, bookmarks[index].name);
				xw.WriteAttributeString(Comisor.Resource.Bookmark_FilePath, bookmarks[index].filePath);
				xw.WriteAttributeString(Comisor.Resource.Bookmark_Date, bookmarks[index].date.ToShortDateString());
				xw.WriteEndElement();
			}

			xw.WriteEndElement();
			#endregion
			
			#region Language
			xw.WriteStartElement("Language");

			System.Reflection.FieldInfo[] fieldInfoes = main.resource.GetType().GetFields();
			foreach (System.Reflection.FieldInfo fieldInfo in fieldInfoes)
			{
				if (fieldInfo.IsStatic && fieldInfo.FieldType == typeof(String))
					xw.WriteAttributeString(fieldInfo.Name, (string)fieldInfo.GetValue(main.resource));
			}

			xw.WriteEndElement();
			#endregion

			xw.Close();

			if (!File.Exists(UserInfoFileName) || new FileInfo(UserInfoFileName).Length == 0)
				ReportException(Comisor.Resource.Exception_CannotSaveSetting);
		}

		private void ShowHelpBox()
		{
			Comisor.Class.HelpBox helpBox = new Comisor.Class.HelpBox(main);
			helpBox.Opacity = 0;
			helpBox.Show();
			helpBox.WindowStateAnimation(false, 1, true);
		}

		private void AutoLevels()
		{
			if (mitAutoLevels.isChecked)
			{
				imgContainer.Source = ys.ImageProcessor.AutoLevels(
					(BitmapSource)imgContainer.Source,
					autoLevelsThreshold,
					colorWeight[2],
					colorWeight[1],
					colorWeight[0]);
			}
			else
			{
				imgContainer.Source = bmpOriginal;
			}
		}

		private void DividePage()
		{
			if (isHalfPage) return;
			// This part also judged the reading order.
			croppedBmpHalf[indexFixedPoint % 2] = new CroppedBitmap(
				(BitmapSource)imgContainer.Source,
				new Int32Rect(0, 0, (int)(szOriginal.Width * dbPageModeRatio), (int)szOriginal.Height)
			);
			croppedBmpHalf[(indexFixedPoint + 1) % 2] = new CroppedBitmap(
				(BitmapSource)imgContainer.Source,
				new Int32Rect((int)(szOriginal.Width * dbPageModeRatio), 0, (int)(szOriginal.Width * (1 - dbPageModeRatio)), (int)szOriginal.Height)
			);
			if (lastMouseDelta == -1)
				imgContainer.Source = croppedBmpHalf[1];
			else
				imgContainer.Source = croppedBmpHalf[0];

			UpdateSizeInfo();
		}

		private void ShowGifAnimation()
		{
			Comisor.Class.Animated_Image animatedImage = new Comisor.Class.Animated_Image();
			animatedImage.Width =
				bdrTransformFrame.Width - 10 +
				2 * System.Windows.Forms.SystemInformation.FrameBorderSize.Width;
			animatedImage.Height =
				bdrTransformFrame.Height - 10 +
				System.Windows.Forms.SystemInformation.ToolWindowCaptionHeight +
				2 * System.Windows.Forms.SystemInformation.FrameBorderSize.Width;
			animatedImage.webBrowser.NavigateToString(
				"<html>" +
				// Fuck this declaration, it wastes me hours to solve the code problem.
				"<head><meta content=\"text/html; charset=utf-8\" http-equiv=\"content-type\" /></head>" +
				"<body style=\"margin:0;\" scroll=\"no\">" +
				"<image src=\"" + imgInfo.FullName + "\"/>" +
				"</body>" +
				"</html>"
			);
			animatedImage.Show();
		}

		private void Rotate(double rotateDegree)
		{
			Size sz = GetSize();
			TransformedBitmap tbmpRotate = new TransformedBitmap((BitmapSource)imgContainer.Source, new RotateTransform(rotateDegree));
			imgContainer.Source = tbmpRotate;

			double temp = szOriginal.Width;
			UpdateSizeInfo();
			SetSize(sz.Height);
			UpdateInfo();
		}

		private void Flip(double x,double y)
		{
			TransformedBitmap tbmpFilp = new TransformedBitmap((BitmapSource)imgContainer.Source, new ScaleTransform(x, y));
			imgContainer.Source = tbmpFilp;
		}

		private void SaveCurrentAs()
		{
			System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
			saveFileDialog.Title = Comisor.Resource.SaveImage;
			saveFileDialog.OverwritePrompt = true;
			saveFileDialog.Filter = Comisor.Resource.SaveFilter;
			saveFileDialog.AddExtension = true;
			saveFileDialog.ValidateNames = true;
			saveFileDialog.InitialDirectory = ys.DataProcessor.GetAvailableParentDir(imgInfo.FullName);
			saveFileDialog.FileName = imgInfo.Name.Remove(imgInfo.Name.LastIndexOf('.'));
			if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				BitmapEncoder encoder;
				BitmapSource bmp = imgContainer.Source as BitmapSource;
				string name = saveFileDialog.FileName.ToLower();
				switch (name.Remove(0, name.LastIndexOf('.')))
				{
					case ".png":
						PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
						if (saveFileDialog.FilterIndex == 2)
							pngEncoder.Interlace = PngInterlaceOption.On;
						encoder = pngEncoder;
						break;
					case ".jpg":
						JpegBitmapEncoder jpgEncoder = new JpegBitmapEncoder();
						switch (saveFileDialog.FilterIndex)
						{
							case 3:
								jpgEncoder.QualityLevel = 50;
								break;
							case 4:
								jpgEncoder.QualityLevel = 85;
								break;
							case 5:
								jpgEncoder.QualityLevel = 100;
								break;
						}
						encoder = jpgEncoder;
						bmp = ys.ImageProcessor.LayWhiteBg(imgContainer.Source as BitmapSource);
						break;
					case ".gif":
						GifBitmapEncoder gifEncoder = new GifBitmapEncoder();
						encoder = gifEncoder;
						bmp = ys.ImageProcessor.LayWhiteBg(imgContainer.Source as BitmapSource);
						break;
					case ".bmp":
						BmpBitmapEncoder bmpEncoder = new BmpBitmapEncoder();
						encoder = bmpEncoder;
						bmp = ys.ImageProcessor.LayWhiteBg(imgContainer.Source as BitmapSource);
						break;
					case ".tif":
						TiffBitmapEncoder tifEncoder = new TiffBitmapEncoder();
						encoder = tifEncoder;
						if (saveFileDialog.FilterIndex == 8)
						{
							encoder.Frames = bmpDecoder.Frames;
							FileStream multilayerFS = new FileStream(saveFileDialog.FileName, FileMode.Create);
							encoder.Save(multilayerFS);
							multilayerFS.Dispose();
							return;
						}
						break;
					case ".wdp":
						WmpBitmapEncoder wdpEncoder = new WmpBitmapEncoder();
						encoder = wdpEncoder;
						break;
					default:
						return;
				}
				encoder.Frames.Add(BitmapFrame.Create(bmp));
				FileStream fs = new FileStream(saveFileDialog.FileName, FileMode.Create);
				encoder.Save(fs);
				fs.Dispose();
			}
		}

		private void DeleteCurrent()
		{
			string preImage = imgInfo.FullName;
			NavImage(1);
			if (preImage == imgInfo.FullName)
			{
				if (allFileNames.Count == 1)
				{
					ReportException(Comisor.Resource.Exception_NoLastFile, false);
					return;
				}
				else
					NavImage(-1);
			}

			Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(
				preImage,
				Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
				Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin,
				Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing
			);
			GetCollection(mitCollectionExplore.isChecked);
		}

		private void SwitchMouseMode()
		{
			if (Mouse.OverrideCursor == null)
				Mouse.OverrideCursor = main.resource.curSmall;
			else
				Mouse.OverrideCursor = null;
		}

		private void FullScreenSwitch()
		{
#if Window_Borderless
			System.Drawing.Rectangle rectDeskArea = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
			if (main.Width != rectDeskArea.Width || main.Height != rectDeskArea.Height)
			{
				main.Left = rectDeskArea.X;
				main.Top = rectDeskArea.Y;
				main.Width = rectDeskArea.Width;
				main.Height = rectDeskArea.Height;
			}
			else
			{
				main.Left = 0;
				main.Top = 0;
				main.Width = SystemParameters.PrimaryScreenWidth;
				main.Height = SystemParameters.PrimaryScreenHeight;
			}
#else
			if (main.WindowState == WindowState.Normal)
			{
				main.WindowState = WindowState.Normal;
				main.WindowStyle = WindowStyle.None;
				main.ResizeMode = ResizeMode.NoResize;

				main.MaxWidth = SystemParameters.PrimaryScreenWidth;
				main.MaxHeight = SystemParameters.PrimaryScreenHeight;
				main.WindowState = WindowState.Maximized;
			}
			else
			{
				main.WindowStyle = WindowStyle.SingleBorderWindow;
				main.ResizeMode = ResizeMode.CanResize;
				main.WindowState = WindowState.Normal;
				
			}
#endif
		}

		[DllImport("kernel32")]
		private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

		[DllImport("kernel32")]
		private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
		
		#region Global Members
		private Comisor.MainWindow main;
		private Canvas cavStage;
		private Border bdrTransformFrame;
		private Image imgContainer;

		private DropShadowEffect shadowEffect = new DropShadowEffect();

		private Duration durationCommon = new Duration(TimeSpan.FromMilliseconds(150));

		private Point ptRButtonDonw;

		private SolidColorBrush scbBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A000"));
		private Color colorBorder = (Color)ColorConverter.ConvertFromString("#A000");
		private Color colorFont = (Color)ColorConverter.ConvertFromString("#FFFF");
		private Color colorInfoBg = (Color)ColorConverter.ConvertFromString("#A000");
		private Color colorImageBg = (Color)ColorConverter.ConvertFromString("#01000000");
		private Vector vFlip = new Vector(1, 1);
		public Rect rectLastFrame;
		private double currentRatio = 1;

		public bool isAutoFitOn = false;
		public int indexAutoFitLock = 0;
		private string[] _strAutoFitLock = {
											   Comisor.Resource.Auto_Fit_Lock0,
											   Comisor.Resource.Auto_Fit_Lock1
										   };
		private string strAutoFitLock
		{
			get
			{
				if (isAutoFitOn)
					return _strAutoFitLock[indexAutoFitLock];
				else
					return "";
			}
		}
		private string strAutoFit
		{
			get
			{
				if (!isAutoFitOn)
					return Comisor.Resource.Auto_Fit;
				else
					return Comisor.Resource.Auto_Fit_Lock;
			}
		}

		private bool isFixedPointOn = false;
		private Point ptFixedPoint = new Point(0, 0);
		private int indexFixedPoint = 0;
		private string[] _strFixedPoint = {
											  Comisor.Resource.Fixed_Point0,
											  Comisor.Resource.Fixed_Point1,
											  Comisor.Resource.Fixed_Point2,
											  Comisor.Resource.Fixed_Point3
										  };
		private string strFixedPoint
		{
			get
			{
				if (isFixedPointOn)
					return _strFixedPoint[indexFixedPoint];
				else
					return _strFixedPoint[indexFixedPoint] + Comisor.Resource.Fixed_Point4;
			}
		}

		private int indexPageModeJudge = 0;
		private string[] strPageModeJudge ={
											   Comisor.Resource.Page_Mode_Intelligent,
											   Comisor.Resource.Page_Mode_Force
										   };

		private int autoLevelsThreshold = 100;
		private double[] colorWeight = { 0.299, 0.587, 0.114 };

		public bool isBgOn = true;
		private double dropShadowRadius = 15;
		private double dragFlotage = 35;
		private double zoomFlotage = 1;
		private double velocityAttenuater = 150;	// Both Drag and Zoom. Must less than 2.
		private double pixelShowThreshold = 18;		// 非破坏性缩放渲染的阈值。

		public string UserInfoFileName = AppDomain.CurrentDomain.BaseDirectory + Comisor.Resource.Setting + ".xml";
		public bool isAutoLevelOn = false;
		public List<Bookmark> bookmarks = new List<Bookmark>();
		#endregion
	}

}
