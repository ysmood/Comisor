using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

namespace System.Windows.Win7
{
    /// <summary>
    /// Win7任务栏进度条
    /// </summary>
	class TaskBar
	{
		public TaskBar()
		{
			isWin7 = 
				(Environment.OSVersion.Version.Major > 6) || 
				(Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 1);
			if (isWin7) TaskbarList = (ITaskbarList)new TaskbarList();
		}

		/// <summary>
		/// TaskBar的状态设置
		/// </summary>
		/// <param fullName="state">枚举 System.Windows.Win7.TbpFlag</param>
		public void SetProgressState(TbpFlag state)
		{
			if (isWin7)
			{
				if (state == TbpFlag.Normal)
					isNormalState = true;
				else
					isNormalState = false;
				TaskbarList.SetProgressState(Process.GetCurrentProcess().MainWindowHandle, state);
			}
		}

		/// <summary>
		/// TaskBar的进度条值
		/// </summary>
		/// <param fullName="Completed">已完成的数量</param>
		/// <param fullName="Total">总数量</param>
		public void ChangeProcessValue(ulong Completed, ulong Total)
		{
			if (isWin7) TaskbarList.SetProgressValue(Process.GetCurrentProcess().MainWindowHandle, Completed, Total);
		}

		public void FlashTaskBar(FlashOption flashOption)
		{
			FLASHWINFO fi = new FLASHWINFO();
			fi.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(fi);
			fi.hwnd = Process.GetCurrentProcess().MainWindowHandle;
			fi.dwFlags = flashOption;
			fi.uCount = 1;
			fi.dwTimeout = 0;
			FlashWindowEx(ref fi);
		}
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

		private bool isWin7;
		private static ITaskbarList TaskbarList;
		public bool isNormalState = true;
	}

	#region Interface

	[ComImport(), Guid("EA1AFB91-9E28-4B86-90E9-9E9F8A5EEFAF"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ITaskbarList
    {
        #region ITaskbarList
        void HrInit();
        void AddTab(IntPtr hWnd);
        void DeleteTab(IntPtr hWnd);
        void ActivateTab(IntPtr hWnd);
        void SetActiveAlt(IntPtr hWnd);
        #endregion
        #region ITaskbarList2
        void MarkFullscreenWindow(IntPtr hWnd, bool fFullscreen);
        #endregion
        #region ITaskbarList3
        /// <summary>
        /// 设置任务栏显示进度
        /// </summary>
        /// <param fullName="hWnd">任务栏对应的窗口句柄</param>
        /// <param fullName="Completed">进度的当前值</param>
        /// <param fullName="Total">总的进度值</param>
        void SetProgressValue(IntPtr hWnd, ulong Completed, ulong Total);
        /// <summary>
        /// 设置任务栏状态
        /// </summary>
        /// <param fullName="hWnd">任务栏对应的窗口句柄</param>
        /// <param fullName="Flags">状态指示，具体见TbpFlag定义</param>
        void SetProgressState(IntPtr hWnd, TbpFlag Flags);
        /// <summary>
        /// 
        /// </summary>
        /// <param fullName="hWndTab"></param>
        /// <param fullName="hWndMDI"></param>
        void RegisterTab(IntPtr hWndTab, IntPtr hWndMDI);
        /// <summary>
        /// 
        /// </summary>
        /// <param fullName="hWndTab"></param>
        void UnregisterTab(IntPtr hWndTab);
        void SetTabOrder(IntPtr hWndTab, IntPtr hwndInsertBefore);
        void SetTabActive(IntPtr hWndTab, IntPtr hWndMDI, uint dwReserved);
        void ThumbBarAddButtons(IntPtr hWnd, uint cButtons, [MarshalAs(UnmanagedType.LPArray)] ThumbButton[] pButtons);
        void ThumbBarUpdateButtons(IntPtr hWnd, uint cButtons, [MarshalAs(UnmanagedType.LPArray)]ThumbButton[] pButtons);
        void ThumbBarSetImageList(IntPtr hWnd, IntPtr himl);
        void SetOverlayIcon(IntPtr hWnd, IntPtr hIcon, [MarshalAs(UnmanagedType.LPWStr)]string pszDescription);
        void SetThumbnailTooltip(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)]string pszTip);
        void SetThumbnailClip(IntPtr hWnd, ref tagRECT prcClip);

        #endregion
	}

    [GuidAttribute("56FDF344-FD6D-11d0-958A-006097C9A090")]
    [ClassInterfaceAttribute(ClassInterfaceType.None)]
    [ComImportAttribute()]
    internal class TaskbarList { }

    [Flags]
    public enum TbpFlag : uint
    {
		NoProgress = 0x00000000, //不显示进度
		Indeterminate = 0x00000001, //进度循环显示
		Normal = 0x00000002, //进度条显示绿色
		Error = 0x00000004, //进度条红色显示
		Paused = 0x00000008 //进度条黄色显示
    }

    /// <summary>
    /// 指名在ThumbButton结构中哪个成员包含有有效信息
    /// </summary>
    [Flags]
    public enum ThumbButtonMask : uint
    {
        Bitmap = 0x01, //ThumbButton.iBitmap包含有效信息
        Icon = 0x02, //ThumbButton.hIcon包含有效信息
        ToolTip = 0x04, //ThumbButton.szTip包含有效信息
        Flags = 0x08 //ThumbButton.dwFlags包含有效信息
    }
    [Flags]
    public enum ThumbButtonFlags : uint
    {
        Enabled = 0x00, //按钮是可用的
        Disabled = 0x01, //按钮是不可用的
        DisMissonClick = 0x02, //当按钮被点击，任务栏按钮的弹出立刻关闭
        NoBackground = 0x04, //不标示按钮边框，只显示按钮图像
        Hidden = 0x08, //隐藏按钮
        NonInterActive = 0x10 //该按钮启用，但没有互动，没有按下按钮的状态绘制。此值用于按钮所在的通知是在使用实例。
    }
	[Flags]
	public enum FlashOption : uint
	{
		//Stop flashing. The system restores the window to its original state. 
		FLASHW_STOP = 0,
		//Flash the window caption. 
		FLASHW_CAPTION = 1,
		//Flash the taskbar button. 
		FLASHW_TRAY = 2,
		//Flash both the window caption and taskbar button. 
		//This is equivalent to setting the FLASHW_CAPTION | FLASHW_TRAY flags. 
		FLASHW_ALL = 3,
		//Flash continuously, until the FLASHW_STOP flag is set. 
		FLASHW_TIMER = 4,
		//Flash continuously until the window comes to the foreground. 
		FLASHW_TIMERNOFG = 12,
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct FLASHWINFO
	{
		public UInt32 cbSize;
		public IntPtr hwnd;
		public FlashOption dwFlags;
		public UInt32 uCount;
		public UInt32 dwTimeout;

	}
    [StructLayout(LayoutKind.Sequential)]
    public struct tagRECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
    public struct ThumbButton
    {
        public ThumbButtonMask dwMask;
        public uint iID;
        public uint iBitmap;
        IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szTip;
        public ThumbButtonFlags dwFlags;
    }

	#endregion
}
