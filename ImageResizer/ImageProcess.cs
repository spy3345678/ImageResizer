using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Dynamic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ImageResizer
{
    public class ImageProcess
    {
        /// <summary>
        /// 清空目的目錄下的所有檔案與目錄
        /// </summary>
        /// <param name="destPath">目錄路徑</param>
        public void Clean(string destPath)
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }
            else
            {
                var allImageFiles = Directory.GetFiles(destPath, "*", SearchOption.AllDirectories);

                foreach (var item in allImageFiles)
                {
                    File.Delete(item);
                }
            }
        }

        /// <summary>
        /// 進行圖片的縮放作業
        /// </summary>
        /// <param name="sourcePath">圖片來源目錄路徑</param>
        /// <param name="destPath">產生圖片目的目錄路徑</param>
        /// <param name="scale">縮放比例</param>
        public void ResizeImages(string sourcePath, string destPath, double scale)
        {
            var allFiles = FindImages(sourcePath);
            Stopwatch stopwatch = new Stopwatch();
            long[] times = new long[] { 0, 0, 0 };
            foreach (var filePath in allFiles)
            {
                stopwatch.Restart();
                Image imgPhoto = Image.FromFile(filePath);
                string imgName = Path.GetFileNameWithoutExtension(filePath);
                times[0] += stopwatch.ElapsedMilliseconds;

                int sourceWidth = imgPhoto.Width;
                int sourceHeight = imgPhoto.Height;
                int destionatonWidth = (int)(sourceWidth * scale);
                int destionatonHeight = (int)(sourceHeight * scale);
                stopwatch.Restart();
                Bitmap processedImage = processBitmap((Bitmap)imgPhoto,
                    sourceWidth, sourceHeight,
                    destionatonWidth, destionatonHeight);
                times[1] += stopwatch.ElapsedMilliseconds;


                string destFile = Path.Combine(destPath, imgName + ".jpg");
                stopwatch.Restart();
                processedImage.Save(destFile, ImageFormat.Jpeg);
                times[2] += stopwatch.ElapsedMilliseconds;
            }
            Console.WriteLine($"讀檔:{times[0]}ms");
            Console.WriteLine($"運算:{times[1]}ms");
            Console.WriteLine($"寫檔:{times[2]}ms");
        }

        /// <summary>
        /// tasks.WhenAll
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destPath"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public async Task ResizeImagesAsyncI(string sourcePath, string destPath, double scale)
        {
            var allFiles = FindImages(sourcePath);
            List<Task> tasks = new List<Task>();
            foreach (var filePath in allFiles)
            {
                tasks.Add(ResizeImageAsync(filePath, destPath, scale)); 
            }
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// tasks.WaitAll
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destPath"></param>
        /// <param name="scale"></param>
        public void ResizeImagesAsyncII(string sourcePath, string destPath, double scale)
        {
            var allFiles = FindImages(sourcePath);
            List<Task> tasks = new List<Task>();
            foreach (var filePath in allFiles)
            {
                tasks.Add(Task.Run(() => { ResizeImage(filePath, destPath, scale); }));
            }
            Task.WaitAll(tasks.ToArray());
        }

        /// <summary>
        /// Parallel.ForEach
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destPath"></param>
        /// <param name="scale"></param>
        public void ResizeImagesAsyncIII(string sourcePath, string destPath, double scale)
        {
            var allFiles = FindImages(sourcePath);
            Parallel.ForEach(allFiles, filePath => {

                Image imgPhoto = Image.FromFile(filePath);
                string imgName = Path.GetFileNameWithoutExtension(filePath);
  
                int sourceWidth = imgPhoto.Width;
                int sourceHeight = imgPhoto.Height;
                int destionatonWidth = (int)(sourceWidth * scale);
                int destionatonHeight = (int)(sourceHeight * scale);
       
                Bitmap processedImage = processBitmap((Bitmap)imgPhoto,
                    sourceWidth, sourceHeight,
                    destionatonWidth, destionatonHeight);
  
                string destFile = Path.Combine(destPath, imgName + ".jpg");
        
                processedImage.Save(destFile, ImageFormat.Jpeg);
               
            });
        }



        /// <summary>
        /// 任何function都用異步
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="destPath"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public async Task ResizeImageAsync(string filePath,string destPath, double scale)
        {
            await Task.Run(() => {
                Image imgPhoto = Image.FromFile(filePath);
                string imgName = Path.GetFileNameWithoutExtension(filePath);

                int sourceWidth = imgPhoto.Width;
                int sourceHeight = imgPhoto.Height;
                int destionatonWidth = (int)(sourceWidth * scale);
                int destionatonHeight = (int)(sourceHeight * scale);

                Bitmap processedImage = processBitmap((Bitmap)imgPhoto,
                    sourceWidth, sourceHeight,
                    destionatonWidth, destionatonHeight);

                string destFile = Path.Combine(destPath, imgName + ".jpg");
                processedImage.Save(destFile, ImageFormat.Jpeg);
            });
        }
        
        /// <summary>
        /// 直接執行
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="destPath"></param>
        /// <param name="scale"></param>
        public void ResizeImage(string filePath, string destPath, double scale)
        {
            Image imgPhoto = Image.FromFile(filePath);
            string imgName = Path.GetFileNameWithoutExtension(filePath);

            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int destionatonWidth = (int)(sourceWidth * scale);
            int destionatonHeight = (int)(sourceHeight * scale);

            Bitmap processedImage = processBitmap((Bitmap)imgPhoto,
                sourceWidth, sourceHeight,
                destionatonWidth, destionatonHeight);

            string destFile = Path.Combine(destPath, imgName + ".jpg");
        }

        /// <summary>
        /// 找出指定目錄下的圖片
        /// </summary>
        /// <param name="srcPath">圖片來源目錄路徑</param>
        /// <returns></returns>
        public List<string> FindImages(string srcPath)
        {
            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(srcPath, "*.png", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(srcPath, "*.jpg", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(srcPath, "*.jpeg", SearchOption.AllDirectories));
            return files;
        }

        /// <summary>
        /// 針對指定圖片進行縮放作業
        /// </summary>
        /// <param name="img">圖片來源</param>
        /// <param name="srcWidth">原始寬度</param>
        /// <param name="srcHeight">原始高度</param>
        /// <param name="newWidth">新圖片的寬度</param>
        /// <param name="newHeight">新圖片的高度</param>
        /// <returns></returns>
        Bitmap processBitmap(Bitmap img, int srcWidth, int srcHeight, int newWidth, int newHeight)
        {
            Bitmap resizedbitmap = new Bitmap(newWidth, newHeight);
            Graphics g = Graphics.FromImage(resizedbitmap);
            g.InterpolationMode = InterpolationMode.High;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.Clear(Color.Transparent);
            g.DrawImage(img,
                new Rectangle(0, 0, newWidth, newHeight),
                new Rectangle(0, 0, srcWidth, srcHeight),
                GraphicsUnit.Pixel);
            return resizedbitmap;
        }
    }
}
