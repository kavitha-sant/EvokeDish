using System;
using EvokeDish.Abstractions;

namespace EvokeDish.Common.Droid
{
	public class DatastoreFolderPathProvider : IDatastoreFolderPathProvider
	{
		public string GetPath()
		{
			return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
		}
	}
}

