using System.IO;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Comisor
{
	public class Resource
	{
		public Resource()
		{
			assembly = GetType().Assembly;

			curHand_Drag = GetCursor("Comisor.Resources.Cursor.Hand_Drag.cur");
			curHand_Over = GetCursor("Comisor.Resources.Cursor.Hand_Over.cur");
			curHand_Wait = GetCursor("Comisor.Resources.Cursor.Hand_Wait.ani");
			curSmall = GetCursor("Comisor.Resources.Cursor.curSmall.ani");
		}

		private Image GetImage(string name)
		{
			BitmapImage bmp = new BitmapImage();
			bmp.BeginInit();
			bmp.StreamSource = assembly.GetManifestResourceStream(name);
			bmp.EndInit();
			Image img = new Image();
			img.Width = bmp.PixelWidth;
			img.Height = bmp.PixelHeight;
			img.Source = bmp;
			return img;
		}

		private BitmapImage GetBmp(string name)
		{
			BitmapImage bmp = new BitmapImage();
			bmp.BeginInit();
			bmp.StreamSource = assembly.GetManifestResourceStream(name);
			bmp.EndInit();
			return bmp;
		}

		private Cursor GetCursor(string name)
		{
			return new Cursor(assembly.GetManifestResourceStream(name));
		}

		private Assembly assembly;

		#region Cursor
		public Cursor curHand_Drag;
		public Cursor curHand_Over;
		public Cursor curHand_Wait;
		public Cursor curSmall;
		#endregion

		//不要使用奇数尺寸的图片！这是wpf的bug！
		#region Image 
		public Image imgAssociate		{ get { return GetImage("Comisor.Resources.Icon.Associate.png"); } }
		public Image imgAuto_Fit_Off	{ get { return GetImage("Comisor.Resources.Icon.Auto_Fit_Off.png"); } }
		public Image imgAuto_Fit_On		{ get { return GetImage("Comisor.Resources.Icon.Auto_Fit_On.png"); } }
		public Image imgBookmark		{ get { return GetImage("Comisor.Resources.Icon.Bookmark.png"); } }
		public Image imgAutoLevels_On	{ get { return GetImage("Comisor.Resources.Icon.Auto_Levels_On.png"); } }
		public Image imgAutoLevels_Off	{ get { return GetImage("Comisor.Resources.Icon.Auto_Levels_Off.png"); } }
		public Image imgViewFullPage	{ get { return GetImage("Comisor.Resources.Icon.View_Full_Page.png"); } }
		public Image imgViewHalfPage	{ get { return GetImage("Comisor.Resources.Icon.View_Half_Page.png"); } }
		public Image imgCancel			{ get { return GetImage("Comisor.Resources.Icon.Cancel.png"); } }
		public Image imgDeep_Explor_Off	{ get { return GetImage("Comisor.Resources.Icon.Deep_Explor_Off.png"); } }
		public Image imgDeep_Explor_On	{ get { return GetImage("Comisor.Resources.Icon.Deep_Explor_On.png"); } }
		public Image imgHelp			{ get { return GetImage("Comisor.Resources.Icon.Help.png"); } }
		public Image imgMinuts			{ get { return GetImage("Comisor.Resources.Icon.Minuts.png"); } }
		public Image imgOK				{ get { return GetImage("Comisor.Resources.Icon.OK.png"); } }
		public Image imgFixed_Point_Off	{ get { return GetImage("Comisor.Resources.Icon.Pin_Off.png"); } }
		public Image imgFixed_Point_On	{ get {	return GetImage("Comisor.Resources.Icon.Pin_On.png"); } }
		public Image imgPlus			{ get { return GetImage("Comisor.Resources.Icon.Plus.png"); } }
		public Image imgSetting			{ get { return GetImage("Comisor.Resources.Icon.Setting.png"); } }
		#endregion

		#region Bitmap
		public BitmapSource imgZeroDemension { get { return GetBmp("Comisor.Resources.Image.ZeroDemension.PNG"); } }
		#endregion

		#region String
		public static string CurrentLanguage = "中文";

		public static string Update = "检查更新";
		public static string UpdatePagePath = "http://www.ysmood.org/project/comisor";
		public static string Update_None = "没有发现更新的版本";
		public static string Update_Current = "当前版本：";
		public static string Update_Latest = "最新版本：";
		public static string Update_Exist = "有新的版本，点击确定打开下载页面";

		public static string Associate_Files = "文件关联";
		public static string Associate_Files_Reverse = "反选";
		public static string Associate_Files_c = "设置文件关联";
		public static string Associate_Files_Switch = "-a";
		public static string Attenuate = "缓动系数";
		public static string Attenuate_c = "缓动速度的迭代衰减值";

		public static string Auto_Fit = "适应画布";
		public static string Auto_Fit_c = "开启/关闭 切换图片时自动缩放并居中";
		public static string Auto_Fit_LastRatio = "上次比例";
		public static string Auto_Fit_Lock = "锁定图片";
		public static string Auto_Fit_Lock_On = "锁定开启";
		public static string Auto_Fit_Lock0 = " [ 尺寸 ]";
		public static string Auto_Fit_Lock1 = " [ 缩放 ]";
		public static string Auto_Fit_RectLastFrame = "上次位置";
		public static string Auto_Fit_szAutoFrame = "尺寸锁定";
		public static string Auto_Fit_Lock_Switch = "-f";

		public static string Auto_Levels = "自动色阶";
		public static string Auto_Levels_ColorWeight = "通道权值";
		public static string Auto_Levels_Threshold = "色阶阈值";
		public static string Auto_Levels_c = "适用于低质量黑白漫画扫图的批量观看";
		public static string Auto_Levels_ColorWeight_Illegal = "各色彩通道的权值之和不能大于 1，此项将自动恢复出场设置。";
		public static string Auto_Levels_Switch = "-l";

		public static string Bookmark = "书签设置";
		public static string Bookmark_c = "点击“+”按钮添加书签\n用滚轮切换自动生成的候选名";
		public static string Bookmark_Date = "日期";
		public static string Bookmark_FilePath = "路径";
		public static string Bookmark_LastTime = "上次浏览";
		public static string Bookmark_Mark = "书签";
		public static string Bookmark_Name = "书签名";
		public static string Bookmark_Editor = "编辑书签";
		public static string Bookmark_CreatShortcut = "创建书签桌面快捷方式";
		public static string Bookmark_OutOfBound = "超出书签索引边界";
		public static string Bookmark_NotFound = "未能找到对应书签";
		public static string Bookmark_NoBookmark = "您还未设置书签";
		public static string Bookmark_FileNotFound = "文件被删除，或者被移动到别的位置。\n\n点击确定重新定位文件地址。";
		public static string Bookmark_Switch = "-b";

		public static string Color_Bg = "背景颜色";
		public static string Color_Border = "外框颜色";
		public static string Color_Font = "字体颜色";
		public static string Color_Info_Bg = "信息栏色";

		public static string Exit = "退出程序";

		public static string Collection_Explore = "群集浏览";
		public static string Collection_Explore_c = "浏览父级文件夹的所有子文件夹";
		public static string Collection_Explore_Switch = "-c";

		public static string Copy = "复制";
		public static string Copied = "成功复制到剪切板";


		public static string Exception_NoMatchCodec = "没有配备此文件的解码器，您可以将出现问题的文件用邮件发送给我，协助软件的改善：y.s.outside@gmail.com\n\n注：此信息已经拷贝到剪切板";
		public static string Exception_NoMatchFile = "没有可用来解析的文件。";
		public static string Exception_NoLastFile = "这是此集合中的最后一张图片了！你可以先关闭本程序，再手动删除此文件。";
		public static string Exception_IllegalInitValue = "有非法初始参数,本次启动将会使部分设定恢复成出场设置。";
		public static string Exception_CannotSaveSetting = "无法正确保存设置，请确保有您足够的权限读写此位置的文件。";

		public static string Fixed_Point = "固定边角";
		public static string Fixed_Point_c = "用滚轮设置，阅读日式漫画时参考点一般设为[右上]\n中式和美式漫画一般为[左上]";
		public static string Fixed_Point_index = "边角参考";
		public static string Fixed_Point_pt = "边距数值";
		public static string Fixed_Point0 = " [ 左上 ]";
		public static string Fixed_Point1 = " [ 右上 ]";
		public static string Fixed_Point2 = " [ 左下 ]";
		public static string Fixed_Point3 = " [ 右下 ]";
		public static string Fixed_Point4 = " 关";

		public static string Page_Mode_Full = "全页浏览";
		public static string Page_Mode_Half = "半页浏览";
		public static string Page_Mode_c = "是否将图片从中间分割成两张图片观看";
		public static string Page_Mode_Left = "-左";
		public static string Page_Mode_Right = "-右";
		public static string Page_Mode_Ratio = "分割比例";
		public static string Page_Mode_Ratio_c = "分割页面的分割线对应页面位置的比例";
		public static string Page_Mode_Force = " [ 强制 ]";
		public static string Page_Mode_Intelligent = " [ 智能 ]";

		public static string Float_Value = "浮力系数";
		public static string Float_Value_c = "若PC性能不佳，可以将其值减小";

		public static string Help = "程序说明";

		public static string Pixel_Threshold = "缩放阈值";
		public static string Pixel_Threshold_c = "关闭非破坏性缩放渲染的阈值";

		public static string ScalingMode_Unspecified = "默认";
		public static string ScalingMode_LowQuality = "低质量";
		public static string ScalingMode_HighQuality = "高质量";
		public static string ScalingMode_NearestNeighbor = "最近邻域";

		public static string OpenImage = "打开图片";
		public static string SaveImage = "保存图片";
		public static string SaveFilter = ""
										+ "PNG|*.png|"
										+ "PNG 交错|*.png|"
										+ "JPG 品质50%|*.jpg|"
										+ "JPG 品质85%|*.jpg|"
										+ "JPG 品质100%|*.jpg|"
										+ "GIF|*.gif|"
										+ "TIFF 仅当前图层|*.tif|"
										+ "TIFF 所有图层（未叠加变换）|*.tif|"
										+ "BMP|*.bmp|"
										+ "WDP|*.wdp";

		public static string Setting = "参数设置";
		public static string Setting_c = "点击左端按钮打开设置文件所在的文件夹";

		public static string Background_Opacity = "背景透明";
		public static string Background_Opacity_c = "设置背景的透明度";
		public static string Background_Opacity_On = "背景开启";
		public static string Background_Opacity_Switch = "-g";
		public static string Shadow_Radius = "阴影半径";
		public static string Shadow_Radius_c = "适时关闭其边缘阴影，以便边缘细节的观察";

		public static string WidowTitle_Start = "开始";

		public static string State_AtEnd = "末";
		public static string State_DeepExplore = "群";

		public static string Info_c = "点击鼠标右键,容复制内容到剪切板";
		public static string Info_Title = "图片信息栏：";
		public static string Info_FileIndex = "位置 : ";
		public static string Info_LayerIndex = "图层 : ";
		public static string Info_FileType = "类型 : ";
		public static string Info_ZoomRatio = "缩放 : ";
		public static string Info_FileSize = "大小 : ";
		public static string Info_Dimension = "尺寸 : ";
		public static string Info_FilePath = "路径 : ";
		public static string Info_FileName = "名称 : ";
		public static string Info_Times = "倍";
		public static string Gesture_OriginalSize = "原始尺寸";
		public static string Gesture_FitStage = "适应画布";
		#endregion

		#region Text Stream
		public Stream sReadme { get { return assembly.GetManifestResourceStream("Comisor.Resources.Readme.rtf"); } }
		#endregion

		#region 程序集特性访问器
		public string AssemblyTitle
		{
			get
			{
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
				if (attributes.Length > 0)
				{
					AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
					if (titleAttribute.Title != "")
					{
						return titleAttribute.Title;
					}
				}
				return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
			}
		}

		public string AssemblyVersion
		{
			get
			{
				return Assembly.GetExecutingAssembly().GetName().Version.ToString();
			}
		}

		public string AssemblyDescription
		{
			get
			{
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
				if (attributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyDescriptionAttribute)attributes[0]).Description;
			}
		}

		public string AssemblyProduct
		{
			get
			{
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
				if (attributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyProductAttribute)attributes[0]).Product;
			}
		}

		public string AssemblyCopyright
		{
			get
			{
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
				if (attributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
			}
		}

		public string AssemblyCompany
		{
			get
			{
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
				if (attributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyCompanyAttribute)attributes[0]).Company;
			}
		}
		#endregion
	}
}
