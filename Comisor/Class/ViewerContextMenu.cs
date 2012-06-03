
#define Window_Borderless

using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using Comisor.Properties;
using System.Windows.Media.Imaging;
using System.Security.Principal;
using System.Diagnostics;
using System.Windows.Threading;
using System.Xml;

namespace ys
{
	public partial class ImageViewer
	{
		private void InitContextMenu()
		{
			#region Init Menu Item
			mitAutoFit = new MenuItemEx(
				main.resource.imgAuto_Fit_On,
				main.resource.imgAuto_Fit_Off);
			mitFixedPoint = new MenuItemEx(
				main.resource.imgFixed_Point_On,
				main.resource.imgFixed_Point_Off);
			mitCollectionExplore = new MenuItemEx(
				main.resource.imgDeep_Explor_On,
				main.resource.imgDeep_Explor_Off);
			mitBookmark = new MenuItemEx(
				main.resource.imgBookmark);
			mitAutoLevels = new MenuItemEx(
				main.resource.imgAutoLevels_On,
				main.resource.imgAutoLevels_Off);
			mitPageMode = new MenuItemEx(
				main.resource.imgViewHalfPage,
				main.resource.imgViewFullPage);
			mitHelp = new MenuItemEx(
				main.resource.imgHelp);
			mitExit = new MenuItemEx(
				main.resource.imgCancel);
			mitSetting = new MenuItem();
			mitAssociateFiles = new MenuItemEx(
				main.resource.imgAssociate);

			mitDropShadow = new MenuItem();
			mitBgOpacity = new MenuItem();
			mitThreshold = new MenuItem();
			mitScalingMode = new MenuItem();
			#endregion

			#region Auto fit desktop
			mitAutoFit.StaysOpenOnClick = true;
			mitAutoFit.isChecked = isAutoFitOn;
			mitAutoFit.ToolTip = Comisor.Resource.Auto_Fit_c;
			mitAutoFit.Header = strAutoFit + "(_F)" + strAutoFitLock;

			mitAutoFit.Click += (o, e) =>
			{
				isAutoFitOn = !isAutoFitOn;
				if (!mitAutoFit.isChecked)
				{
					mitFixedPoint.isChecked = false;
					isFixedPointOn = false;
					mitFixedPoint.Header = Comisor.Resource.Fixed_Point + "(_P)" + strFixedPoint;
				}
				mitAutoFit.Header = strAutoFit + "(_F)" + strAutoFitLock;
			};

			mitAutoFit.MouseWheel += (o, e) =>
			{
				if (mitAutoFit.isChecked)
				{
					// Circle from 0 to 2.
					indexAutoFitLock = (2 + indexAutoFitLock + (e.Delta > 0 ? -1 : 1)) % 2;
					mitAutoFit.Header = strAutoFit + "(_F)" + strAutoFitLock;
				}
			};
			#endregion

			#region Fixed Point
			mitFixedPoint.ToolTip = Comisor.Resource.Fixed_Point_c;
			mitFixedPoint.StaysOpenOnClick = true;
			mitFixedPoint.isChecked = isFixedPointOn;
			mitFixedPoint.Header = Comisor.Resource.Fixed_Point + "(_P)" + strFixedPoint;

			mitFixedPoint.Click += (o, e) =>
			{
				isFixedPointOn = !isFixedPointOn;
				if (mitFixedPoint.isChecked)
				{
					mitAutoFit.isChecked = true;
					isAutoFitOn = true;
					mitAutoFit.Header = strAutoFit + "(_F)" + strAutoFitLock;
				}
				mitFixedPoint.Header = Comisor.Resource.Fixed_Point + "(_P)" + strFixedPoint;
				UpdateRefPoint();
			};

			mitFixedPoint.MouseWheel += (o, e) =>
			{
				// Circle from 0 to 3, to prevent the overflow, additionally plused 4 in front of the indexFixedPoint.
				indexFixedPoint = (4 + indexFixedPoint + (e.Delta > 0 ? -1 : 1)) % 4;
				mitFixedPoint.Header = Comisor.Resource.Fixed_Point + "(_P)" + strFixedPoint;

				UpdateRefPoint();
			};
			#endregion

			#region Page Mode
			mitPageMode.Header = Comisor.Resource.Page_Mode_Full + "(_M)" + strPageModeJudge[indexPageModeJudge];
			mitPageMode.ToolTip = Comisor.Resource.Page_Mode_c;
			mitPageMode.StaysOpenOnClick = true;

			mitPageMode.Click += (o, e) =>
			{
				Size sz = GetSize();
				if (mitPageMode.isChecked)
				{
					mitPageMode.Header = Comisor.Resource.Page_Mode_Half + "(_M)" + strPageModeJudge[indexPageModeJudge];
					DividePage();
					if (isHalfPage) return;
					if ((indexFixedPoint % 2) == 1)
						SetSize(sz.Width * (1 - dbPageModeRatio));
					else
						SetSize(sz.Width * dbPageModeRatio);
					UpdateInfo();
				}
				else
				{
					mitPageMode.Header = Comisor.Resource.Page_Mode_Full + "(_M)" + strPageModeJudge[indexPageModeJudge];

					if (isHalfPage) return;

					imgContainer.Source = bmpOriginal;

					UpdateSizeInfo();
					if ((indexFixedPoint % 2) == 1)
						SetSize(sz.Width / (1 - dbPageModeRatio));
					else
						SetSize(sz.Width / dbPageModeRatio);
					UpdateInfo();
					if (mitAutoLevels.isChecked) AutoLevels();
				}
			};

			mitPageMode.MouseWheel += (o, e) =>
			{
				string currnt =
					mitPageMode.isChecked ?
					Comisor.Resource.Page_Mode_Half + "(_M)" :
					Comisor.Resource.Page_Mode_Full + "(_M)";
				indexPageModeJudge = (2 + indexPageModeJudge + (e.Delta > 0 ? -1 : 1)) % 2;
				mitPageMode.Header = currnt + strPageModeJudge[indexPageModeJudge];

				if (indexPageModeJudge == 0)
					isHalfPage = bmpOriginal.PixelWidth < bmpOriginal.PixelHeight;
				else
					isHalfPage = false;
			};
			#endregion

			#region Collection Explore
			mitCollectionExplore.Header = Comisor.Resource.Collection_Explore + "(_C)";
			mitCollectionExplore.ToolTip = Comisor.Resource.Collection_Explore_c;
			mitCollectionExplore.StaysOpenOnClick = true;
			mitCollectionExplore.isChecked = false;
			mitCollectionExplore.Click += (o, e) =>
			{
				if (bdrInfo.Opacity == 0) ShowInfoBar();

				Mouse.OverrideCursor = Cursors.AppStarting;

				if (mitCollectionExplore.isChecked)
				{
					GetCollection(true);
					UpdateInfo(Comisor.Resource.State_DeepExplore);
				}
				else
				{
					GetCollection(false);
					UpdateInfo();
				}
				System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(new InvokeDelegate(EndBusy), System.Windows.Threading.DispatcherPriority.Background, null);
			};
			#endregion

			#region AutoLevels
			mitAutoLevels.Header = Comisor.Resource.Auto_Levels + "(_L)";
			mitAutoLevels.ToolTip = Comisor.Resource.Auto_Levels_c;
			mitAutoLevels.StaysOpenOnClick = true;
			mitAutoLevels.isChecked = isAutoLevelOn;
			mitAutoLevels.Click += (o, e) =>
			{
				Mouse.OverrideCursor = main.resource.curHand_Wait;
				// Fix the bug of spilt page.
				if (mitPageMode.isChecked && croppedBmpHalf[0] != null)
				{
					if (mitAutoLevels.isChecked)
					{
						int index = Array.IndexOf<CroppedBitmap>(croppedBmpHalf, imgContainer.Source as CroppedBitmap);
						for (int i = 0; i < 2; i++)
						{
							croppedBmpHalf[i] = new CroppedBitmap(ys.ImageProcessor.AutoLevels(
								(BitmapSource)croppedBmpHalf[i],
								autoLevelsThreshold,
								colorWeight[2],
								colorWeight[1],
								colorWeight[0]),
								new Int32Rect(0, 0, (int)szOriginal.Width, (int)szOriginal.Height)
							);
						}
						imgContainer.Source = croppedBmpHalf[index];
					}
					else
					{
						imgContainer.Source = bmpOriginal;
						UpdateSizeInfo();
						DividePage();
					}
				}
				else
					AutoLevels();

				Dispatcher.CurrentDispatcher.Invoke(new InvokeDelegate(EndBusy), System.Windows.Threading.DispatcherPriority.Background, null);
			};
			#endregion

			#region Help
			mitHelp.Header = Comisor.Resource.Help + "(_H)";
			mitHelp.Click += new RoutedEventHandler((o, e) => { ShowHelpBox(); });
			#endregion

			#region Exit
			mitExit.Header = Comisor.Resource.Exit + "(_X)";
			mitExit.Click += new RoutedEventHandler((o, e) => { CloseWindow(); });
			#endregion

			#region Bookmark
			mitBookmark.Header = Comisor.Resource.Bookmark + "(_B)";
			mitBookmark.ToolTip = Comisor.Resource.Bookmark_c;

			#region Init Add button and textbox
			ComboBox cbAdd = new ComboBox();
			MenuItem mitAdd = new MenuItem();
			cbAdd.MinWidth = 120;
			cbAdd.HorizontalAlignment = HorizontalAlignment.Left;
			cbAdd.IsEditable = true;
			btnAddBookmark.Content = main.resource.imgPlus;
			btnAddBookmark.Margin = new Thickness(2);

			mitAdd.StaysOpenOnClick = true;
			mitAdd.Icon = btnAddBookmark;
			mitAdd.Header = cbAdd;
			mitBookmark.Items.Add(mitAdd);
			mitBookmark.Items.Add(new Separator());

			mitBookmark.SubmenuOpened += (o, e) =>
			{
				System.Collections.Generic.List<string> nameOption = new System.Collections.Generic.List<string>();
				nameOption.AddRange(imgInfo.FullName.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries));
				ys.DataProcessor.RemoveSame(ref nameOption);
				nameOption.Reverse();
				cbAdd.ItemsSource = nameOption;
				cbAdd.SelectedIndex = 0;
				cbAdd.Focus();
			};

			cbAdd.PreviewKeyDown += (o, e) =>
			{
				if (e.Key == Key.Enter)
				{
					btnAddBookmark.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, btnAddBookmark));
				}
			};

			btnAddBookmark.Click += (o, e) =>
			{
				if (e.OriginalSource is string) cbAdd.Text = e.OriginalSource as string;
				Bookmark bk = new Bookmark(cbAdd.Text, imgInfo.FullName);
				bookmarks.Insert(0, bk);

				Button btnDelete = new Button();
				Label lbName = new Label();
				MenuItem mit = new MenuItem();

				btnDelete.Content = main.resource.imgMinuts;
				btnDelete.Margin = new Thickness(2);
				btnDelete.Click += (oo, ee) =>
				{
					bookmarks.Remove(bk);
					mitBookmark.Items.Remove(mit);
				};
				lbName.Content = cbAdd.Text;
				mit.Icon = btnDelete;
				mit.Header = lbName;
				mit.ToolTip = bk.filePath + "\n" + bk.date.ToShortDateString();

				mitBookmark.Items.Insert(2, mit);

				mit.Click += (oo, ee) =>
				{
					if (File.Exists(bk.filePath))
						CheckAndStart(bk.filePath);
					else
					{
						if (ReportException(Comisor.Resource.Bookmark_FileNotFound, false, MessageBoxButton.OKCancel)
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
								CheckAndStart(bk.filePath);
							}
						}
					}
				};

				mit.PreviewMouseRightButtonUp += (oo, ee) =>
				{
					Comisor.Class.BookmarkEditor editor = new Comisor.Class.BookmarkEditor(main, bk.name, bk.filePath, imgInfo.FullName);
					editor.ShowDialog();
					if (editor.isOK)
					{
						bk.name = editor.cbName.Text;
						bk.filePath = editor.cbPath.Text;
						bk.date = DateTime.Now;
						mit.ToolTip = bk.filePath + "\n" + bk.date.ToShortDateString();
						lbName.Content = editor.cbName.Text;
					}
					ee.Handled = true;
				};
			};
			#endregion

			#region Init bookmark list
			foreach (Bookmark bookmark in bookmarks)
			{
				Label lbName = new Label();
				Button btnDelete = new Button();
				MenuItem mit = new MenuItem();
				// bookmark 此时只能出现在“=”右边。它只是指针，最后将停留在bookmarks数组的最后一项。
				ys.Bookmark bk = bookmark;

				lbName.Content = bk.name;
				btnDelete.Content = main.resource.imgMinuts;
				btnDelete.Margin = new Thickness(2);

				btnDelete.Click += (o, e) =>
				{
					bookmarks.Remove(bk);
					mitBookmark.Items.Remove(mit);
				};
				mit.Icon = btnDelete;
				mit.Header = lbName;
				mit.ToolTip = bk.filePath + "\n" + bk.date.ToLongDateString();
				mitBookmark.Items.Add(mit);
				mit.Click += (o, e) =>
				{
					if (File.Exists(bk.filePath))
						CheckAndStart(bk.filePath);
					else
					{
						if (ReportException(Comisor.Resource.Bookmark_FileNotFound, false, MessageBoxButton.OKCancel)
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
							}
							CheckAndStart(bk.filePath);
						}
					}
				};

				mit.PreviewMouseRightButtonUp += (oo, ee) =>
				{
					Comisor.Class.BookmarkEditor editor = new Comisor.Class.BookmarkEditor(main, bk.name, bk.filePath, imgInfo.FullName);
					editor.ShowDialog();
					if (editor.isOK)
					{
						bk.name = editor.cbName.Text;
						bk.filePath = editor.cbPath.Text;
						bk.date = DateTime.Now;
						mit.ToolTip = bk.filePath + "\n" + bk.date.ToShortDateString();
						lbName.Content = editor.cbName.Text;
					}
					ee.Handled = true;
				};
			}
			#endregion
			#endregion

			#region Setting
			#region Associate Files
			Label lbAssociateFiles = new Label();
			lbAssociateFiles.Content = Comisor.Resource.Associate_Files + "(_A)";
			lbAssociateFiles.ToolTip = Comisor.Resource.Associate_Files_c;
			mitAssociateFiles.Header = lbAssociateFiles;
			mitAssociateFiles.Click += (o, e) =>
			{
				WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
				bool hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);

				if (!hasAdministrativeRight)
				{
					ProcessStartInfo processInfo = new ProcessStartInfo();
					processInfo.Verb = "runas";
					processInfo.FileName = GetType().Assembly.Location;
					processInfo.Arguments = Comisor.Resource.Associate_Files_Switch;
					try
					{
						Process.Start(processInfo);
					}
					catch
					{
						//Do nothing. Probably the user canceled the UAC window
					}
				}
				else
				{
					Comisor.Class.Associator fileAssociator = new Comisor.Class.Associator(main);
					fileAssociator.Opacity = 0;
					fileAssociator.Show();
					fileAssociator.WindowStateAnimation(false, 1, true);
				}
			};
			#endregion

			#region Background Opacity
			Label lbBgOpacity = new Label();
			StackPanel stpBgOpacity = new StackPanel();
			if (!File.Exists(UserInfoFileName))
			{
				sldBgOpacity.Minimum = 0;
				sldBgOpacity.Maximum = 100;
				sldBgOpacity.Value = 50;
			}
			lbBgOpacity.Content = Comisor.Resource.Background_Opacity + "(_B)：" + sldBgOpacity.Value.ToString("0");
			sldBgOpacity.ToolTip = Comisor.Resource.Background_Opacity_c;
			sldBgOpacity.TickFrequency = (sldBgOpacity.Maximum - sldBgOpacity.Minimum) / 4;
			sldBgOpacity.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.TopLeft;
#if Window_Borderless
			main.firstStartComplete = (o, e) => { ShowHideBg(isBgOn); };
#endif
			sldBgOpacity.ValueChanged += (o, e) =>
			{
				if(isBgOn) scbBg.Opacity = sldBgOpacity.Value / 100;
				lbBgOpacity.Content = Comisor.Resource.Background_Opacity + "(_B)：" + sldBgOpacity.Value.ToString("0");
			};

			mitBgOpacity.IsCheckable = true;
			mitBgOpacity.IsChecked = isBgOn;
			mitBgOpacity.Click += (o, e) =>
			{
				isBgOn = !isBgOn;
				ShowHideBg(isBgOn);
			};

			stpBgOpacity.Children.Add(lbBgOpacity);
			stpBgOpacity.Children.Add(sldBgOpacity);
			mitBgOpacity.Header = stpBgOpacity;
			#endregion

			#region Drop Shadow
			Label lbDropShadow = new Label();
			Slider sldDropShadow = new Slider();
			StackPanel stpDropShadow = new StackPanel();
			lbDropShadow.Content = Comisor.Resource.Shadow_Radius + "(_S)：" + dropShadowRadius.ToString("00");
			sldDropShadow.ToolTip = Comisor.Resource.Shadow_Radius_c;
			sldDropShadow.Minimum = 0;
			sldDropShadow.Maximum = 30;
			sldDropShadow.TickFrequency = (sldDropShadow.Maximum - sldDropShadow.Minimum) / 4;
			sldDropShadow.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.TopLeft;
			sldDropShadow.Value = dropShadowRadius;
			sldDropShadow.ValueChanged += (o, e) =>
			{
				dropShadowRadius = sldDropShadow.Value;
				shadowEffect.BlurRadius = dropShadowRadius;
				lbDropShadow.Content = Comisor.Resource.Shadow_Radius + "(_S)：" + dropShadowRadius.ToString("00");
			};

			mitDropShadow.IsCheckable = true;
			mitDropShadow.IsChecked = true;
			mitDropShadow.Click += (o, e) =>
			{
				// 这是WPF的一个bug。
				if (mitDropShadow.IsChecked)
				{
					imgContainer.Effect = shadowEffect;
				}
				else
				{
					imgContainer.Effect = null;
				}
			};

			stpDropShadow.Children.Add(lbDropShadow);
			stpDropShadow.Children.Add(sldDropShadow);
			mitDropShadow.Header = stpDropShadow;
			#endregion

			#region Pixel Threshold
			Label lbThreshold = new Label();
			ComboBox cbScalingMode = new ComboBox();
			Slider sldThreshold = new Slider();
			StackPanel stpThreshold = new StackPanel();

			lbThreshold.Content = Comisor.Resource.Pixel_Threshold + "(_P)：" + pixelShowThreshold.ToString("00");

			cbScalingMode.MinWidth = 100;
			cbScalingMode.Margin = new Thickness(3);
			cbScalingMode.ItemsSource = new string[]
			{
				Comisor.Resource.ScalingMode_Unspecified,
				Comisor.Resource.ScalingMode_LowQuality,
				Comisor.Resource.ScalingMode_HighQuality,
				Comisor.Resource.ScalingMode_NearestNeighbor,
			};
			cbScalingMode.SelectedIndex = 0;
			cbScalingMode.SelectionChanged += (o, e) =>
			{
				scalingMode = (BitmapScalingMode)cbScalingMode.SelectedIndex;
				AutoRenderOption();
			};

			sldThreshold.ToolTip = Comisor.Resource.Pixel_Threshold_c;
			sldThreshold.Minimum = 0;
			sldThreshold.Maximum = 36;
			sldThreshold.TickFrequency = (sldThreshold.Maximum - sldThreshold.Minimum) / 4;
			sldThreshold.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.TopLeft;
			sldThreshold.Value = pixelShowThreshold;
			sldThreshold.ValueChanged += new RoutedPropertyChangedEventHandler<double>(
				(o, e) =>
				{
					pixelShowThreshold = sldThreshold.Value;
					lbThreshold.Content = Comisor.Resource.Pixel_Threshold + "(_P)：" + pixelShowThreshold.ToString("00");
					AutoRenderOption();
				}
			);

			mitThreshold.IsCheckable = true;
			mitThreshold.IsChecked = true;
			mitThreshold.Click += (o, e) =>
			{
				stpThreshold.IsEnabled = mitThreshold.IsChecked;
				AutoRenderOption();
			};

			stpThreshold.Children.Add(lbThreshold);
			stpThreshold.Children.Add(cbScalingMode);
			stpThreshold.Children.Add(sldThreshold);
			mitThreshold.Header = stpThreshold;
			#endregion

			#region Page Mode Ratio
			Label lbPageModeRatio = new Label();
			Slider sldPageModeRatio = new Slider();
			StackPanel stpPageModeRatio = new StackPanel();

			lbPageModeRatio.Content = Comisor.Resource.Page_Mode_Ratio + "：" + dbPageModeRatio.ToString("p0");
			sldPageModeRatio.ToolTip = Comisor.Resource.Page_Mode_Ratio_c;
			sldPageModeRatio.Minimum = 0;
			sldPageModeRatio.Maximum = 1;
			sldPageModeRatio.LargeChange = 0.01;
			sldPageModeRatio.TickFrequency = (sldPageModeRatio.Maximum - sldPageModeRatio.Minimum) / 4;
			sldPageModeRatio.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.TopLeft;
			sldPageModeRatio.Value = dbPageModeRatio;
			stpPageModeRatio.Children.Add(lbPageModeRatio);
			stpPageModeRatio.Children.Add(sldPageModeRatio);
			sldPageModeRatio.ValueChanged += new RoutedPropertyChangedEventHandler<double>(
				(o, e) =>
				{
					dbPageModeRatio = sldPageModeRatio.Value;
					lbPageModeRatio.Content = Comisor.Resource.Page_Mode_Ratio + "：" + dbPageModeRatio.ToString("p0");
				}
			);
			#endregion

			#region Float Value
			Label lbFlotageDrag = new Label();
			Slider sldFlotageDrag = new Slider();
			StackPanel stpFlotageDrag = new StackPanel();

			lbFlotageDrag.Content = Comisor.Resource.Float_Value + "：" + dragFlotage.ToString("00");
			sldFlotageDrag.ToolTip = Comisor.Resource.Float_Value_c;
			sldFlotageDrag.Minimum = 0;
			sldFlotageDrag.Maximum = 70;
			sldFlotageDrag.TickFrequency = (sldFlotageDrag.Maximum - sldFlotageDrag.Minimum) / 4;
			sldFlotageDrag.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.TopLeft;
			sldFlotageDrag.Value = dragFlotage;
			stpFlotageDrag.Children.Add(lbFlotageDrag);
			stpFlotageDrag.Children.Add(sldFlotageDrag);
			sldFlotageDrag.ValueChanged += new RoutedPropertyChangedEventHandler<double>(
				(o, e) =>
				{
					dragFlotage = sldFlotageDrag.Value;
					lbFlotageDrag.Content = Comisor.Resource.Float_Value + "：" + dragFlotage.ToString("00");
				}
			);
			#endregion

			#region Attenuater
			Label lbAttenuater = new Label();
			Slider sldAttenuater = new Slider();
			StackPanel stpAttenuater = new StackPanel();

			lbAttenuater.Content = Comisor.Resource.Attenuate + "：" + velocityAttenuater.ToString("00");
			sldAttenuater.ToolTip = Comisor.Resource.Attenuate_c;
			sldAttenuater.Minimum = 110;
			sldAttenuater.Maximum = 190;
			sldAttenuater.TickFrequency = (sldAttenuater.Maximum - sldAttenuater.Minimum) / 4;
			sldAttenuater.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.TopLeft;
			sldAttenuater.Value = velocityAttenuater;
			stpAttenuater.Children.Add(lbAttenuater);
			stpAttenuater.Children.Add(sldAttenuater);
			sldAttenuater.ValueChanged += new RoutedPropertyChangedEventHandler<double>(
				(o, e) =>
				{
					velocityAttenuater = sldAttenuater.Value;
					lbAttenuater.Content = Comisor.Resource.Attenuate + "：" + velocityAttenuater.ToString("00");
				}
			);
			#endregion

			#region Main
			Border bdrSetting = new Border();
			bdrSetting.CornerRadius = new CornerRadius(2);
			bdrSetting.Padding = new Thickness(2);
			bdrSetting.Child = main.resource.imgSetting;
			bdrSetting.Focusable = false;
			bdrSetting.BorderThickness = new Thickness(1);
			bdrSetting.MouseEnter += (o, e) =>
			{
				bdrSetting.BorderBrush = Brushes.Gray;
				bdrSetting.Background = Brushes.White;
			};
			bdrSetting.MouseLeave += (o, e) =>
			{
				bdrSetting.BorderBrush = Brushes.Transparent;
				bdrSetting.Background = Brushes.Transparent;
			};
			bdrSetting.PreviewMouseDown += (o, e) =>
			{
				System.Diagnostics.Process.Start("explorer.exe", "/select," + UserInfoFileName);
			};

			mitSetting.Icon = bdrSetting;
			mitSetting.Header = Comisor.Resource.Setting + "(_S)";
			mitSetting.ToolTip = Comisor.Resource.Setting_c;
			mitSetting.Items.Add(mitAssociateFiles);
			mitSetting.Items.Add(new Separator());
			mitSetting.Items.Add(mitBgOpacity);
			mitSetting.Items.Add(mitDropShadow);
			mitSetting.Items.Add(mitThreshold);
			mitSetting.Items.Add(mitScalingMode);
			mitSetting.Items.Add(stpPageModeRatio);
			mitSetting.Items.Add(stpFlotageDrag);
			mitSetting.Items.Add(stpAttenuater);

			#endregion

			#endregion

			// Init the context menu.
			contextMenu = new ContextMenu();

			contextMenu.Items.Add(mitAutoFit);
			contextMenu.Items.Add(mitFixedPoint);
			contextMenu.Items.Add(mitPageMode);
			contextMenu.Items.Add(mitCollectionExplore);
			contextMenu.Items.Add(mitAutoLevels);
			contextMenu.Items.Add(mitHelp);
			contextMenu.Items.Add(mitExit);
			contextMenu.Items.Add(new Separator());
			contextMenu.Items.Add(mitBookmark);
			contextMenu.Items.Add(mitSetting);

			contextMenu.FontFamily = new FontFamily("Microsoft YaHei");
			foreach (Control mit in contextMenu.Items)
				if (mit is MenuItem) mit.Height = 24;
#if Window_Borderless
			bdrTransformFrame.ContextMenu = contextMenu;
#else
			cavStage.ContextMenu = contextMenu;
#endif
		}

		private MenuItemEx mitAutoFit;
		private MenuItemEx mitFixedPoint;
		private MenuItemEx mitPageMode;
		private MenuItemEx mitBookmark;
		private MenuItemEx mitAutoLevels;
		private MenuItemEx mitHelp;
		private MenuItemEx mitExit;
		private MenuItem mitSetting;
		private MenuItemEx mitAssociateFiles;
		
		private MenuItem mitBgOpacity;
		private MenuItem mitDropShadow;
		private MenuItem mitThreshold;
		private MenuItem mitScalingMode;

		private ContextMenu contextMenu;

		private Slider sldBgOpacity = new Slider();

		private Button btnAddBookmark = new Button();

		public MenuItemEx mitCollectionExplore;
	}
}
