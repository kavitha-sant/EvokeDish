using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvokeDish.Abstractions
{
    /// <summary>
    /// Provides the path to the datastore. Implemented on each platform, 
    /// because each platform may keep the datastore in a different location on disk.
    /// </summary>
    public interface IDatastoreFolderPathProvider
    {
        /// <summary>
        /// Gets the path to the datastore.
        /// </summary>
        /// <returns>The path to the datastore.</returns>
        string GetPath();
    }
}
