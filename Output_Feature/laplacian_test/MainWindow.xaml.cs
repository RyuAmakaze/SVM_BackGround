using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Linq;


namespace laplacian_test
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public String path0 = "D:/grameye/";
        public String path1 = "D:\\grameye\\";
        public String path2 = "D:/grameye/背景判定用/isgoodbackground_kaiseki/";

        public String filename = "OUM801_8月20日16時16分27秒";
        public String pos = "3.5_0_";
        public double start_number = 4084.5;
        public double end_number = 4133;

        public double z_step = 0.5;
        public int x_step = 64;
        public int y_step = 32;

        string[] folder = new string[] { "badbackground_toodark", "badbackground_toolight", "goodbackground" };

        public MainWindow()
        {
            //isback_forSVM();
        }

        private void Button_Click_kaiseki(object sender, RoutedEventArgs e)
        {
            ViewSearchState.AppendText("\n解析開始");
            isback_forSVM();
        }

        //SVM解析用の画像特徴量をクラスラベル，特徴量1,特徴量2の順でcsvに入れるコード．
        public void isback_forSVM()
        {
            
            string path = "D:/grameye/背景判定用/isgoodbackground_kaiseki/";
            //Yamaoka_PCでの画像ファイルを入れる場所のPath,ディレクトリ構造
            //isgoodbackground_kaiseki
            //          +-----goodbackground
            //          +-----badbackground_toolight
            //          +-----badbackground_toodark

            //isgoodbackground_kaiseki以下の各フォルダ名，goodbackground,badbackground_toolight,badbackground_toodarkを取得．
            string[] folders = System.IO.Directory.GetDirectories(path, "*", System.IO.SearchOption.AllDirectories);

            //SVM用の特徴量を格納するcsv
            StreamWriter csv_file = new StreamWriter(path + "/Feature_SVM3.csv", false, Encoding.UTF8);

            foreach (String folder in folders)
            {
                //良い背景はクラス0，濃すぎるは1，薄すぎる背景は-1
                int class_label = 2;
                System.Diagnostics.Debug.WriteLine(folder);
                if (folder == path + "goodbackground")
                {
                    class_label = 0;
                }
                else if(folder == path + "badbackground_toodark")
                {
                    class_label = 1;
                }
                else if (folder == path + "badbackground_toolight")
                {
                    class_label = -1;
                }

                string[] files = System.IO.Directory.GetFiles(folder, "*.png", System.IO.SearchOption.AllDirectories);//各フォルダの全画像にアクセス．
                foreach (String file in files)
                {
                    ViewSearchState.AppendText("\n" + file);

                    double[] Feature_value = for_SVM(file);

                    //Array.Resize(ref Feature_value, Feature_value.Length + 1);
                    //Feature_value[Feature_value.Length - 1] = class_label;

                    //csv_file.WriteLine(Feature_value);
                    csv_file.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}", class_label, Feature_value[1], Feature_value[2], Feature_value[3], Feature_value[4], Feature_value[5], Feature_value[6], Feature_value[7], Feature_value[8], Feature_value[9], Feature_value[10], Feature_value[11], Feature_value[12]);
                }
                
            }
            csv_file.Close();
        }


        //画像へのPathを引数として，画像の特徴量を抽出し，double配列に入れる．
        public double [] for_SVM(string pngFileName)
        {
            double[] Feature_value = { 0 };//最終的に各特徴量を格納する配列．

            Mat frame = Cv2.ImRead(pngFileName);//Pathの画像ファイルを読み込み．
            int x, y;//画像の座標用変数


            ///////////////////////////////////////////////////////////////////////////////
            //////////////////////左半分
            ///////////////////////////////////////////////////////////////////////////////
            double[] array = { 0 };//各色味の値を入れる配列．
            //左半分の画像に関しての特徴量
            for (x = 0; x < frame.Cols / 2; x += 16)
            {
                for (y = 0; y < frame.Rows; y += 16)
                {
                    Vec3b px = frame.At<Vec3b>(y, x);
                    Array.Resize(ref array, array.Length + 1);
                    array[array.Length - 1] = px[1];
                }
            }
            double mean_left = array.Average();//色味の平均．
            double sum = array.Select(a => a * a).Sum();
            double variance_left = sum / array.Length - mean_left * mean_left;//色味の分散計算．
            variance_left = Math.Sqrt(variance_left) / mean_left;//色味の分散計算．
            double PSNR_left = Math.Log10(256 * 256 / variance_left);//ピーク信号対雑音比

            int green_px_left = 0;
            for (x = 0; x < frame.Cols / 2; x += 16)
            {
                for (y = 0; y < frame.Rows; y += 16)
                {
                    Vec3b px = frame.At<Vec3b>(y, x);
                    if (mean_left > px[0])
                    {
                        green_px_left++;
                    }

                }
            }
            double green_per_left = green_px_left * 256 * 100.0 * 2 / frame.Cols / frame.Rows;

            ///////////////////////////////////////////////////////////////////////////////
            //////////////////////右半分
            ///////////////////////////////////////////////////////////////////////////////
            double[] array2 = { 0 };//各色味の値を入れる配列．

            //左半分の画像に関しての特徴量
            for (x = 0; x < frame.Cols / 2; x += 16)
            {
                for (y = 0; y < frame.Rows; y += 16)
                {
                    Vec3b px = frame.At<Vec3b>(y, x);
                    Array.Resize(ref array2, array2.Length + 1);
                    array2[array2.Length - 1] = px[1];
                }
            }
            double mean_right = array2.Average();//色味の平均．

            double sum2 = array2.Select(a => a * a).Sum();
            double variance_right = sum2 / array2.Length - mean_right * mean_right;
            variance_right = Math.Sqrt(variance_right) / mean_right;//色味の分散計算．
            double PSNR_right = Math.Log10(256 * 256 / variance_right);//ピーク信号対雑音比

            int green_px_right = 0;
            for (x = 0; x < frame.Cols / 2; x += 16)
            {
                for (y = 0; y < frame.Rows; y += 16)
                {
                    Vec3b px = frame.At<Vec3b>(y, x);
                    if (mean_right > px[0])
                    {
                        green_px_right++;
                    }

                }
            }
            double green_per_right = green_px_right * 256 * 100.0 * 2 / frame.Cols / frame.Rows;

            ///////////////////////////////////////////////////////////////////////////////
            //////////////////////全体
            ///////////////////////////////////////////////////////////////////////////////
            double[] array0 = { 0 };//各色味の値を入れる配列．

            //左半分の画像に関しての特徴量
            for (x = 0; x < frame.Cols; x += 16)
            {
                for (y = 0; y < frame.Rows; y += 16)
                {
                    Vec3b px = frame.At<Vec3b>(y, x);
                    Array.Resize(ref array0, array0.Length + 1);
                    array0[array0.Length - 1] = px[1];
                }
            }
            double mean_all = array0.Average();//色味の平均．
            double sum0 = array0.Select(a => a * a).Sum();
            double variance_all = sum0 / array0.Length - mean_all * mean_all;
            variance_all = Math.Sqrt(variance_all) / mean_all;//色味の分散計算．
            double PSNR_all = Math.Log10(256 * 256 / variance_all);//ピーク信号対雑音比

            int green_px_all = 0;
            for (x = 0; x < frame.Cols / 2; x += 16)
            {
                for (y = 0; y < frame.Rows; y += 16)
                {
                    Vec3b px = frame.At<Vec3b>(y, x);
                    if (mean_all > px[0])
                    {
                        green_px_all++;
                    }

                }
            }
            double green_per_all = green_px_all * 256 * 100.0 * 2 / frame.Cols / frame.Rows;
            ///////////////////////////////////////////////////////////////////////////////
            //////////////////////代入
            ///////////////////////////////////////////////////////////////////////////////
            ///

            ///画像全体に関する特徴量，色味平均，色味分散，色味PSNR,色味平均以上のピクセル割合
            ///

            //左半分/////////////////////////////////////////
            Array.Resize(ref Feature_value, Feature_value.Length + 1);
            Feature_value[Feature_value.Length - 1] = mean_left;

            Array.Resize(ref Feature_value, Feature_value.Length + 1);
            Feature_value[Feature_value.Length - 1] = variance_left;

            Array.Resize(ref Feature_value, Feature_value.Length + 1);
            Feature_value[Feature_value.Length - 1] = PSNR_left;

            Array.Resize(ref Feature_value, Feature_value.Length + 1);
            Feature_value[Feature_value.Length - 1] = green_per_left;


            //右半分/////////////////////////////
            Array.Resize(ref Feature_value, Feature_value.Length + 1);
            Feature_value[Feature_value.Length - 1] = mean_right;

            Array.Resize(ref Feature_value, Feature_value.Length + 1);
            Feature_value[Feature_value.Length - 1] = variance_right;

            Array.Resize(ref Feature_value, Feature_value.Length + 1);
            Feature_value[Feature_value.Length - 1] = PSNR_right;

            Array.Resize(ref Feature_value, Feature_value.Length + 1);
            Feature_value[Feature_value.Length - 1] = green_per_right;


            //全体/////////////////////////////////////////
            Array.Resize(ref Feature_value, Feature_value.Length + 1);
            Feature_value[Feature_value.Length - 1] = mean_all;

            Array.Resize(ref Feature_value, Feature_value.Length + 1);
            Feature_value[Feature_value.Length - 1] = variance_all;

            Array.Resize(ref Feature_value, Feature_value.Length + 1);
            Feature_value[Feature_value.Length - 1] = PSNR_all;

            Array.Resize(ref Feature_value, Feature_value.Length + 1);
            Feature_value[Feature_value.Length - 1] = green_per_all;

            frame.Dispose();
            
            return Feature_value;
        }

        ////画像へのPathを引数として，画像全体のラプラシアンバリアンスを返す．．
        public static double LaplacianVariance(string pngFileName)
        {
            double result;

            Mat img = Cv2.ImRead(pngFileName);
            result = Contrast(img);
            img.Dispose();

            return result;
        }

        //部分ピント合わせのために用いる．
        ////画像へのPathを引数として，画像を格子状に区切ったのち，複数の格子ごとにラプラシアンバリアンスを計算し，最大値となる値を返す．
        public double LaplacianMaxVariance(string pngFileName)
        {
            Mat img = Cv2.ImRead(pngFileName);

            int width = img.Cols;
            int hight = img.Rows;

            double grid_valiance_max = 0;
            for (int x = 1; x < width - x_step; x += x_step)
            {

                for (int y = 1; y < hight - y_step; y += y_step)
                {
                    var grid_img = img.Clone(new OpenCvSharp.Rect(x, y, x_step, y_step));
                    double grid_valiance = Contrast0(grid_img);
                    if (grid_valiance_max < grid_valiance)
                    {
                        grid_valiance_max = grid_valiance;
                    }
                    grid_img.Dispose();
                }

            }
            img.Dispose();
            return grid_valiance_max;
        }

        //LaplacianVariance()に用いる
        static double Contrast(Mat frame)
        {
            int resize_parameter = 1;
            Cv2.Resize(frame, frame, new OpenCvSharp.Size(frame.Cols / resize_parameter, frame.Rows / resize_parameter), 0, 0, InterpolationFlags.Cubic);
            using (var laplacian = new Mat())
            {
                Cv2.CvtColor(frame, frame, ColorConversionCodes.BGRA2GRAY);
                Cv2.Laplacian(frame, laplacian, MatType.CV_64FC1);
                Cv2.MeanStdDev(laplacian, out var mean, out var stddev);
                laplacian.Dispose();
                return stddev.Val0 * stddev.Val0;
            }
        }

        //LaplacianMaxVariance()に用いる
        static double Contrast0(Mat frame)
        {
            using (var laplacian = new Mat())
            {
                Cv2.CvtColor(frame, laplacian, ColorConversionCodes.BGRA2GRAY);
                Cv2.MeanStdDev(laplacian, out var mean, out var stddev);
                laplacian.Dispose();
                return stddev.Val0 * stddev.Val0;
            }
        }


    }


}
