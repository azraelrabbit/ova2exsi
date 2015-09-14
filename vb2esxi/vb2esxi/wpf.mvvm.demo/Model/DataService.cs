using System;

namespace wpf.mvvm.demo.Model
{
    public class DataService : IDataService
    {
        public void GetData(Action<DataItem, Exception> callback)
        {
            // Use this to connect to the actual data service

            var item = new DataItem("Welcome to MVVM Light");
            callback(item, null);
        }

        public void GetHello(Action<DataItem, Exception> callback)
        {
            callback(new DataItem("hello Kitty!"), null);
        }
    }
}