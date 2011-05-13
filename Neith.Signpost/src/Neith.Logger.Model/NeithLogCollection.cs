using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Neith.Logger.Model
{
    public class NeithLogCollection : ObservableCollection<NeithLogVM>
    {
        public NeithLogCollection() : base() { }
        public NeithLogCollection(IEnumerable<NeithLogVM> collection) : base(collection) { }
        public NeithLogCollection(IList<NeithLogVM> collection) : base(collection) { }

        public NeithLogCollection(IEnumerable<NeithLog> collection) : base(collection.Select(a => new NeithLogVM(a))) { }

    }
}
