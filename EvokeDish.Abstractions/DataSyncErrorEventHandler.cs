using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvokeDish.Abstractions
{
    /// <summary>
    /// A generically-typed delegate for handling data sync errors.
    /// </summary>
    public delegate void DataSyncErrorEventHandler<T>(object sender, DataSyncErrorEventArgs<T> e);
}
