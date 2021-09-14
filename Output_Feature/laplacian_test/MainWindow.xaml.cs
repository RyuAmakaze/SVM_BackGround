using OpenCvSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

using System.Windows.Forms;


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

        public MainWindow()
        {
            //isback_forSVM();
            //judge_bySVM();
        }

        private void Button_Click_kaiseki(object sender, RoutedEventArgs e)
        {
            ViewSearchState.AppendText("\n解析開始");
            //judge_bySVM();
            //sisback_forSVM();
        }

        public bool judge_bySVM(double[] feature)
        {
            string SVM_cof = "SVM/SVM12_coefficient.csv";
            string SVM_thresh = "SVM/SVM12_intercept.csv";

            StreamReader sr = new StreamReader(@SVM_cof);
            string[] feature_cof = sr.ReadLine().Split(',');

            StreamReader sr2 = new StreamReader(@SVM_thresh);
            string[] thresh = sr2.ReadLine().Split(',');

            double calculate = 0;

            if (feature_cof.Length != feature.Length)
            {
                MessageBox.Show("SVMの係数の数と、入力された特徴量の数が異なります。特徴量の数をチェック！");
            }

            for(int i = 0; i < feature.Length; i++)
            {
                calculate = calculate + feature[i]*double.Parse(feature_cof[i]);
            }

            if(Math.Abs(calculate) <= Math.Abs(double.Parse(thresh[0])))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //SVM解析用の画像特徴量をクラスラベル，特徴量1,特徴量2の順でcsvに入れるコード．
        public void isback_forSVM()
        {

            string path = "D:/gramEye/isgoodbackground_kaiseki/";
            //Yamaoka_PCでの画像ファイルを入れる場所のPath,ディレクトリ構造
            //isgoodbackground_kaiseki
            //          +-----goodbackground
            //          +-----badbackground_toolight
            //          +-----badbackground_toodark

            //isgoodbackground_kaiseki以下の各フォルダ名，goodbackground,badbackground_toolight,badbackground_toodarkを取得．
            string[] folders = System.IO.Directory.GetDirectories(path, "*", System.IO.SearchOption.AllDirectories);

            //SVM用の特徴量を格納するcsv
            StreamWriter csv_file = new StreamWriter(path + "/Feature_SVM7.csv", false, Encoding.UTF8);

            foreach (String folder in folders)
            {
                //良い背景はクラス0，濃すぎるは1，薄すぎる背景は-1, もしくはダメな背景は1
                int class_label = 2;
                Debug.WriteLine(folder);
                if (folder == path + "goodbackground")
                {
                    class_label = 0;
                }
                else if (folder == path + "badbackground_toodark")
                {
                    class_label = 1;
                }
                else if (folder == path + "badbackground_toolight")
                {
                    class_label = 1;
                }

                string[] files = System.IO.Directory.GetFiles(folder, "*.png", System.IO.SearchOption.AllDirectories);//各フォルダの全画像にアクセス．
                Debug.WriteLine(folder.Replace(path,"") + "は" +  files.Length + "つのファイルで構成");
                foreach (String file in files)
                {
                    ViewSearchState.AppendText("\n" + file);
                    Debug.WriteLine(file);

                    double[] Feature_value = for_SVM(file);

                    //Array.Resize(ref Feature_value, Feature_value.Length + 1);
                    //Feature_value[Feature_value.Length - 1] = class_label;

                    //csv_file.WriteLine(Feature_value);
                    
                    //後日書き直し．
                    //36種類ver
                    //csv_file.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33},{34},{35},{36}", class_label, Feature_value[1], Feature_value[2], Feature_value[3], Feature_value[4], Feature_value[5], Feature_value[6], Feature_value[7], Feature_value[8], Feature_value[9], Feature_value[10], Feature_value[11], Feature_value[12], Feature_value[13], Feature_value[14], Feature_value[15], Feature_value[16], Feature_value[17], Feature_value[18], Feature_value[19], Feature_value[20], Feature_value[21], Feature_value[22], Feature_value[23], Feature_value[24], Feature_value[25], Feature_value[26], Feature_value[27], Feature_value[28], Feature_value[29], Feature_value[30], Feature_value[31], Feature_value[32], Feature_value[33], Feature_value[34], Feature_value[35], Feature_value[36]);
                    
                    //12種ver
                    csv_file.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}", class_label, Feature_value[1], Feature_value[2], Feature_value[3], Feature_value[4], Feature_value[5], Feature_value[6], Feature_value[7], Feature_value[8], Feature_value[9], Feature_value[10], Feature_value[11], Feature_value[12]);                
                }


            }
            csv_file.Close();
            Debug.WriteLine("解析終了");
        }


        //画像へのPathを引数として，画像の特徴量を抽出し，double配列に入れる．
        public double[] for_SVM(string pngFileName)
        {
            double[] Feature_value = { 0 };//最終的に各特徴量を格納する配列．

            Mat frame = Cv2.ImRead(pngFileName);//Pathの画像ファイルを読み込み．
            int x, y;//画像の座標用変数
            int area = 3;//解析する領域数
            int color = 1;//解析する色の数

            ///////////////////////////////////////////////////////////////////////////////
            //////////////////////左半分，右半分，全体の順番でデータを取る．
            ///////////////////////////////////////////////////////////////////////////////
            for(int j = 0; j < area; j++) {
                int x_limit = 0; 
                int x_start = frame.Cols / 2;

                if (j == 0)//左半分
                {
                    x_start = 0;
                    x_limit = frame.Cols/2;
                }
                if (j == 1)//右半分
                {
                    x_start = frame.Cols / 2;
                    x_limit = frame.Cols;   
                }
                if (j == 2)//全体
                {
                    x_start = 0;
                    x_limit = frame.Cols;
                }

                for (int i = 0; i < color; i++)//三色，GBRの順番でデータを取る．
                {
                    double[] array = { 0 };//各色味の値を入れる配列．
                    for (x = x_start; x < x_limit; x += 16)
                    {
                        for (y = 0; y < frame.Rows; y += 16)
                        {
                            Vec3b px = frame.At<Vec3b>(y, x);
                            Array.Resize(ref array, array.Length + 1);
                            array[array.Length - 1] = px[i];
                        }
                    }
                    double mean = array.Average();//色味の平均．
                    double sum = array.Select(a => a * a).Sum();
                    double variance = sum / array.Length - mean * mean;//色味の分散計算．
                    variance = Math.Sqrt(variance) / mean;//色味の分散計算．
                    double PSNR = Math.Log10(256 * 256 / variance);//ピーク信号対雑音比

                    int color_px = 0;
                    for (x = x_start; x < x_limit; x += 16)
                    {
                        for (y = 0; y < frame.Rows; y += 16)
                        {
                            Vec3b px = frame.At<Vec3b>(y, x);
                            if (mean > px[i])
                            {
                                color_px++;
                            }

                        }
                    }
                    double color_per = color_px * 256 * 100.0 * 2 / frame.Cols / frame.Rows;

                    //戻る配列に対して値の代入,3領域×3色×4特徴量＝36種/////////////////////////////////////////
                    Array.Resize(ref Feature_value, Feature_value.Length + 1);
                    Feature_value[Feature_value.Length - 1] = mean;

                    Array.Resize(ref Feature_value, Feature_value.Length + 1);
                    Feature_value[Feature_value.Length - 1] = variance;

                    Array.Resize(ref Feature_value, Feature_value.Length + 1);
                    Feature_value[Feature_value.Length - 1] = PSNR;

                    Array.Resize(ref Feature_value, Feature_value.Length + 1);
                    Feature_value[Feature_value.Length - 1] = color_per;

                }
            }
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
