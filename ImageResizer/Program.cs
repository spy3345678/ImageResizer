using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageResizer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string sourcePath = Path.Combine(Environment.CurrentDirectory, "images");
            string destinationPath = Path.Combine(Environment.CurrentDirectory, "output"); ;
            string asyncDestinationPath = Path.Combine(Environment.CurrentDirectory, "outputasync"); ;

            ImageProcess imageProcess = new ImageProcess();
            imageProcess.Clean(destinationPath);
            imageProcess.Clean(asyncDestinationPath);

            Stopwatch sw = new Stopwatch();


            sw.Start();
            imageProcess.ResizeImages(sourcePath, destinationPath, 2.0);
            sw.Stop();
            double syncgms = sw.ElapsedMilliseconds;
            Console.WriteLine($"同步花費時間: {sw.ElapsedMilliseconds} ms");


            sw.Restart();
            await imageProcess.ResizeImagesAsync(sourcePath, asyncDestinationPath, 2.0);
            sw.Stop();
            double asyncms = sw.ElapsedMilliseconds;
            Console.WriteLine($"非同步花費時間: {sw.ElapsedMilliseconds} ms");

            Console.WriteLine($"效能提升百分比{Math.Round((1 - (asyncms / syncgms))*100)  }%");

            Console.ReadKey();
        }
    }
}
