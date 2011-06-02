using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using ReactiveUI;

namespace Neith.Signpost
{
    public class EorzeaClockViewModel : ReactiveValidatedObject
    {
        public EorzeaClockModel Model { get; protected set; }


        public EorzeaClockViewModel()
            : this(new EorzeaClockModel(DateTimeOffset.Now))
        {
        }

        public EorzeaClockViewModel(EorzeaClockModel model)
        {
            Model = model;
        }


    }
}