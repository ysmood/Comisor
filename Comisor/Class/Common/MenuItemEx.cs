using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ys
{
	public class MenuItemEx : MenuItem
	{
		public MenuItemEx(Image icon)
		{
			this.icon1 = icon;
			this.Icon = icon1;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param fullName="icon1">When isChecked is true, display this img.</param>
		/// <param fullName="icon2">When isChecked is false, display this img.</param>
		public MenuItemEx(Image icon1, Image icon2)
		{
			this.icon1 = icon1;
			this.icon2 = icon2;

			isChecked = false;

			this.Click += (o, e) => { isChecked = !isChecked; };
		}

		private Image icon1;
		private Image icon2;
		private bool isCheckedInside;

		public bool isChecked
		{
			set
			{
				isCheckedInside = value;
				if (this.IsEnabled)
				{
					if (isCheckedInside)
						this.Icon = icon1;
					else
						this.Icon = icon2;
				}
			}
			get
			{
				return isCheckedInside;
			}
		}
	}
}
