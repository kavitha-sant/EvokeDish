using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvokeDish.Abstractions
{
    public interface IDataSyncConflictMessagePresenter
    {
        void PresentConflictMessage();
    }
}
