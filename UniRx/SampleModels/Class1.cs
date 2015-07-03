using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UniRx;

namespace SampleModels
{
    public class Class1
    {
        public async Task RunAsync(IObserver<string> observer)
        {
            var urls = new[]
            {
                "http://yahoo.co.jp",
                "http://google.co.jp",
                "http://bing.co.jp",
                "http://awsedrftgyhujikol.jp/",
            };

            foreach (var url in urls)
            {
                await Observable.Timer(TimeSpan.FromSeconds(1));

                try
                {
                    var res = await GetAsStringAsync(url);
                    observer.OnNext(res);
                }
                catch(Exception ex)
                {
                    observer.OnError(ex);
                }
            }
        }

        public async Task<string> GetAsStringAsync(string url)
        {
            var req = WebRequest.Create(url);
            var res = await req.GetResponseAsObservable();

            using (var sr = new StreamReader(res.GetResponseStream()))
                return sr.ReadToEnd();
        }
    }
}
