using System;
using wpf.mvvm.demo.Model;

namespace wpf.mvvm.demo.Design
{
    public class DesignDataService : IDataService
    {
        public void GetData(Action<DataItem, Exception> callback)
        {
            // Use this to create design time data

            var item = new DataItem("Welcome to MVVM Light [design]");
            callback(item, null);
        }

        public void GetHello(Action<DataItem, Exception> callback)
        {
            callback(new DataItem("Hello Kitty [design]"), null);
        }
    }
}