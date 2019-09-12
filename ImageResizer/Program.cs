using System;
using System.Collections.Generic;
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
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Restart();
            Console.WriteLine($"================= Test Start =================");
            await SyncTest();
            Console.WriteLine($"Test End 總耗時:{stopwatch.ElapsedMilliseconds}ms\r\n");

            Console.ReadKey();
        }

        static async Task SyncTest() {
            string sourcePath = Path.Combine(Environment.CurrentDirectory, "images");
            string destinationPath = Path.Combine(Environment.CurrentDirectory, "output"); ;
            string asyncDestinationPath = Path.Combine(Environment.CurrentDirectory, "outputasync"); ;
            string asyncDestinationPath2 = Path.Combine(Environment.CurrentDirectory, "outputasync2"); ;
            string asyncDestinationPath3 = Path.Combine(Environment.CurrentDirectory, "outputasync3"); ;

            ImageProcess imageProcess = new ImageProcess();
            imageProcess.Clean(destinationPath);
            imageProcess.Clean(asyncDestinationPath);
            imageProcess.Clean(asyncDestinationPath2);
            imageProcess.Clean(asyncDestinationPath3);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            imageProcess.ResizeImages(sourcePath, destinationPath, 2.0);
            sw.Stop();
            double syncgms = sw.ElapsedMilliseconds;
            Console.WriteLine($"同步花費時間: {sw.ElapsedMilliseconds} ms\r\n");

            // =====================================================================
            sw.Restart();
            await imageProcess.ResizeImagesAsyncI(sourcePath, asyncDestinationPath, 2.0);
            sw.Stop();
            double asyncms = sw.ElapsedMilliseconds;
            Console.WriteLine($"非同步(tasks.whenall+最後異步方法)花費時間: {sw.ElapsedMilliseconds} ms");
            Console.WriteLine($"效能提升百分比{Math.Round((1 - (asyncms / syncgms)) * 100)  }%\r\n");
            // =====================================================================
            sw.Restart();
            imageProcess.ResizeImagesAsyncII(sourcePath, asyncDestinationPath2, 2.0);
            sw.Stop();
            double asyncmsII = sw.ElapsedMilliseconds;
            Console.WriteLine($"非同步(tasks.WaitAll+最後同步方法)花費時間: {sw.ElapsedMilliseconds} ms");
            Console.WriteLine($"效能提升百分比{Math.Round((1 - (asyncmsII / syncgms)) * 100)  }% (這個永遠比較快一點點)\r\n");
            // =====================================================================
            sw.Restart();
            imageProcess.ResizeImagesAsyncIII(sourcePath, asyncDestinationPath3, 2.0);
            sw.Stop();
            double asyncmsIII = sw.ElapsedMilliseconds;
            Console.WriteLine($"非同步(Parallel.ForEach)花費時間: {sw.ElapsedMilliseconds} ms");
            Console.WriteLine($"效能提升百分比{Math.Round((1 - (asyncmsIII / syncgms)) * 100)  }%\r\n");
            // =====================================================================

        }
    }
}
