using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;

namespace ys
{
	class DataProcessor
	{
		DataProcessor()
		{
		}
		public static void RemoveSame(ref List<string> list)
		{
			for (int i = 0; i < list.Count; i++)
				for (int j = i + 1; j < list.Count; j++)
					if (list[i] == list[j])
					{
						list.RemoveAt(j);
						i = 0;
						break;
					}
		}
		public static string GetAvailableParentDir(string path)
		{
			string upOnoLevel = path.Remove(path.LastIndexOf('\\'));
			if (Directory.Exists(upOnoLevel))
				return upOnoLevel;
			else
				return GetAvailableParentDir(upOnoLevel);
		}

		public static void SortByName(string[] names)
		{
			Array.Sort(names, StringLogicalComparer.Default);
		}
	}
}
