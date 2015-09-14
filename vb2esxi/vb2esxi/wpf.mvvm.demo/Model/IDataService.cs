using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wpf.mvvm.demo.Model
{
    public interface IDataService
    {
        void GetData(Action<DataItem, Exception> callback);
        void GetHello(Action<DataItem, Exception> callback);
    }
}
