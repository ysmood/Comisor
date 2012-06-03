using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ys
{
	public class Bookmark
	{
		public Bookmark(string name, string filePath)
		{
			InitCommon(name, filePath);
			date = DateTime.Now;
		}

		public Bookmark(string name, string filePath, DateTime date)
		{
			InitCommon(name, filePath);
			this.date = date;
		}

		private void InitCommon(string name, string filePath)
		{
			this.name = name;
			this.filePath = filePath;
		}

		public string name;
		public string filePath;
		public DateTime date;
	}
}
