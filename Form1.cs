using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace kojinseisaku
{
    public partial class Form1 : Form
    {

        public string user_name;  // ログインしたユーザー名

        Form2 form2;              // form2の情報

        // 接続文字列
        private string cnstr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\ckadai\C#\kojinseisaku\Database1.mdf;Integrated Security=True;Connect Timeout=30";

        private DateTime today = DateTime.Now.Date; // 現在の日付（時刻部分は無視）
        private const double THICK_BORDER_WIDTH = 2; // 太い枠線の幅（ピクセル）

        // Windows APIをインポート
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        private const int WM_SETREDRAW = 0x000B;


        public Form1(Form f2, string name)
        {
            InitializeComponent();
            form2 = (Form2)f2;  // form2の情報(グローバル変数)
            user_name = name;   // ユーザー名
        }

        // 
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                return cp;
            }
        }

        // 年の値を増やす処理
        private void button3_Click(object sender, EventArgs e)
        {
            int cnt = 1;  // 年を増やす変数
            int num;      // 表示されている年の数値化用変数

            // 表示されている年が数字であるか判定する
            if (int.TryParse(label13.Text, out num) == true)
            {
                label13.Text = (int.Parse(label13.Text) + cnt).ToString();  // 年を+1増分する
                Calendar_clear();  // 関数Calendar_clearの呼び出し
            }
            else
            {
                MessageBox.Show("正数ではありません。入力しなおして下さい");
                label13.Text = DateTime.Now.Year.ToString();  // 初期値(2020)に戻す
                Calendar_clear();                             // 関数Calendar_clearの呼び出し
            }
        }

        // 年の値wを減らす処理
        private void button2_Click(object sender, EventArgs e)
        {
            int cnt = -1;  // 年を減らす変数

            int num;      // 表示されている年の数値化用変数

            if (int.TryParse(label13.Text, out num) == true)
            {
                label13.Text = (int.Parse(label13.Text) + cnt).ToString();  // 年を-1減分する
                Calendar_clear();                                             // 関数Calendar_clearの呼び出し
            }
            else
            {
                MessageBox.Show("正数ではありません。入力しなおして下さい");
                label13.Text = DateTime.Now.Year.ToString();
                Calendar_clear();
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        // メイン画面
        private void Form1_Load(object sender, EventArgs e)
        {

            label13.Text = DateTime.Now.Year.ToString();  // 現在の年をラベルに代入

            // 1. 描画を一時停止
            SendMessage(this.Handle, WM_SETREDRAW, 0, 0); // 0はFALSE (描画停止)

            try
            {

                // コントロール処理
                this.SuspendLayout();
                Calendar_clear(); // 関数Calendar_clearの呼び出し
                this.ResumeLayout(true);
                this.PerformLayout();
            }
            finally
            {
                // 3. 描画を再開し、即座に再描画（強制描画）
                SendMessage(this.Handle, WM_SETREDRAW, 1, 0); //　(描画再開)
                this.Invalidate();
            }
        }


        // カレンダーの初期化
        public void Calendar_clear()
        {
            int i;  // ループカウンタ(月)
            Dictionary<DateTime, (string Name, string Color)> holidays = GetHoliday();

            // カレンダーを作成する処理(1～12月)
            for (i = 1; i <= 12; i++)
            {
                Create_calendar(i, holidays);  // 関数Create_calendarの呼び出し
            }

        }

        // Tableのデータを取得する関数(辞書)
        private Dictionary<DateTime, (string Name, string Color)> GetHoliday()
        {
            SqlConnection cn = new SqlConnection();
            // keyValueの構成を定義
            var keyValue = new Dictionary<DateTime, (string Name, string Color)>();
            DateTime date;　　// 日付(データ)
            DateTime sDate;   // 年始
            DateTime eDate;   // 年末
            string color;     // 色(データ)
            string name;      // ユーザー名(データ)

            try
            {
                sDate = new DateTime(int.Parse(label13.Text), 1, 1);  // 年始
                eDate = new DateTime(int.Parse(label13.Text), 12, 31);  // 年末

                cn.ConnectionString = cnstr;

                cn.Open();
                SqlCommand cmd = cn.CreateCommand();
                // Tableよりその年の年始から年末までのデータを検索
                cmd.CommandText = "SELECT Date,Name, Color FROM [dbo].[Table] WHERE Date >= @sDate AND Date <= @eDate AND Name = @name";
                cmd.Parameters.Add(new SqlParameter("@sDate", sDate));   // パラメータ取得
                cmd.Parameters.Add(new SqlParameter("@eDate", eDate));
                cmd.Parameters.Add(new SqlParameter("@name", user_name));

                SqlDataReader rd = cmd.ExecuteReader();
                // コマンドで受け取ったデータを読み取り
                while (rd.Read())
                {
                    date = DateTime.Parse(rd["Date"].ToString());  // 日
                    color = rd["Color"].ToString();                // 色
                    name = rd["Name"].ToString();                  // 名前

                    // 同じdateが重複しないように
                    if (!keyValue.ContainsKey(date))
                    {
                        keyValue.Add(date, (name, color));         // 辞書追加
                    }

                }
                rd.Close();      // 接続終了
                cmd.Dispose();   // リソース開放
            }
            catch (Exception en)
            {
                MessageBox.Show("接続エラー" + en.Message);
            }
            finally
            {
                cn.Close();
            }

            return (keyValue);  // 辞書を返す(SELECT文で検索したデータたち)


        }

        // MemoTableのデータを取得する関数(辞書)
        private Dictionary<DateTime, (string Name, string Memo, string Title)> GetMemo()
        {

            SqlConnection cn = new SqlConnection();
            // keyMemoの構成を定義
            var keyMemo = new Dictionary<DateTime, (string Name, string Memo, string Title)>();
            DateTime date;
            DateTime sDate;
            DateTime eDate;
            string name;
            string memo;
            string title;

            try
            {
                sDate = new DateTime(int.Parse(label13.Text), 1, 1);  // 年始
                eDate = new DateTime(int.Parse(label13.Text), 12, 31);  // 年末

                cn.ConnectionString = cnstr;

                cn.Open();
                SqlCommand cmd = cn.CreateCommand();
                // MemoTableよりその年の年始から年末までのデータを検索
                cmd.CommandText = "SELECT Date,Name, Memo,Title FROM [dbo].[MemoTable] WHERE Date >= @sDate AND Date <= @eDate AND Name = @name";
                cmd.Parameters.Add(new SqlParameter("@sDate", sDate));
                cmd.Parameters.Add(new SqlParameter("@eDate", eDate));
                cmd.Parameters.Add(new SqlParameter("@name", user_name));

                SqlDataReader rd = cmd.ExecuteReader();
                // コマンドで受け取ったデータを読み取り
                while (rd.Read())
                {
                    date = DateTime.Parse(rd["Date"].ToString()).Date;  // テーブルデータ
                    name = rd["Name"].ToString();
                    memo = rd["Memo"].ToString();
                    title = rd["Title"].ToString();

                    // 同じ日が重複しないように
                    if (!keyMemo.ContainsKey(date))
                    {
                        keyMemo.Add(date, (name, memo, title));
                    }

                }
                rd.Close();

                cmd.Dispose();
            }
            catch (Exception en)
            {
                MessageBox.Show("接続エラー" + en.Message);
            }
            finally
            {
                cn.Close();
            }

            return (keyMemo);  // 辞書型keyMemoを返す


        }


        // カレンダーの作成
        public void Create_calendar(int month, Dictionary<DateTime, (string Name, string Color)> holiday)
        {
            string[,] calendar = new string[6, 7];  // 6行7列の2次元配列calendar
            DataGridView data = null;  //   
            int i;  // ループカウンタ
            int j;  // ループカウンタ

            // このフォーム.cs内で指定した名前のコントロールを探す
            Control[] cs = this.Controls.Find("dataGridView" + string.Format("{0:}", month), true);
            // 見つかった場合
            if (cs.Length > 0)
            {
                data = (DataGridView)cs[0];

                data.CellPainting -= Cell_Paint; // 二重登録防止
                data.CellPainting += Cell_Paint;
            }
            // 見つからなかった場合
            else
            {
                return;
            }

            // カレンダーの作成
            data.Rows.Clear();

            // 対象月の月初め
            DateTime firstDate = new DateTime(int.Parse(label13.Text), month, 1);

            int date_top = (int)firstDate.DayOfWeek;  // 月初めの曜日を表すセルの列番号

            DateTime startdate = firstDate.AddDays(-1 * date_top);  // その月の1日からdate_top分引いた日付
            DateTime enddate = firstDate.AddMonths(1).AddDays(-1);  // 月の終わり((その月 + 一か月) - 1)

            int addDay = 0;                // セル内に表示する日
            DateTime weekday = startdate;  // 開始場所
            for (j = 0; j < calendar.GetLength(0); j++)
            {
                data.Rows.Add();  // 行追加

                for (i = 0; i < calendar.GetLength(1); i++)
                {
                    weekday = startdate.AddDays(addDay);  // startdateからaddDayを加算したDatetimeを返す
                    addDay++;  // 日を増分

                    // 対象月以外の場合(ここでその月の1日目までのデータをnullにする)
                    if (weekday.Month != month)
                    {
                        continue;  // スキップ
                    }

                    data[i, j].Value = weekday.Day;  // 日付をセルに代入

                    // セルに色付けする処理
                    if (holiday.ContainsKey(weekday.Date) && holiday[weekday.Date].Name == user_name)
                    {
                        string back_color = holiday[weekday.Date].Color;  // 辞書から受け取った日付に対応する色を取得

                        DateTime color_date = weekday.Date;

                        // 色が入っていない(例外)
                        if (back_color == null)
                        {
                            continue;
                        }
                        else if (back_color == "Red")  // 赤色の場合
                        {
                            if (color_date >= DateTime.Now.Date)
                            {
                                data[i, j].Style.BackColor = Color.Red;
                            }
                            else
                            {
                                data[i, j].Style.BackColor = System.Drawing.Color.FromArgb(255, 156, 151);
                            }
                                
                        }
                        else if (back_color == "RoyalBlue")  // 青色の場合
                        {
                            if (color_date >= DateTime.Now.Date)
                            {
                                data[i, j].Style.BackColor = Color.RoyalBlue;
                            }
                            else
                            {
                                data[i, j].Style.BackColor = System.Drawing.Color.FromArgb(173, 224, 238);
                            }
                           
                        }
                        else if (back_color == "Green")  //緑の場合
                        {
                            if (color_date >= DateTime.Now.Date)
                            {
                                data[i, j].Style.BackColor = Color.Green;
                            }
                            else
                            {
                                data[i, j].Style.BackColor = System.Drawing.Color.FromArgb(206, 226, 193);
                            }
                        }
                    }

                    // enddate(最終日に到達したら)
                    if (weekday.CompareTo(enddate) == 0)
                    {
                        break;  
                    }
                }

                if (weekday.CompareTo(enddate) == 0)
                {
                    break;
                }
            }

            data.ClearSelection();   // datagridviewの選択解除

        }



        // プログラム終了処理
        private void button1_Click(object sender, EventArgs e)
        {
            // 確認ダイアログを表示
            DialogResult result = MessageBox.Show(
                "プログラムを終了しますか？", // メッセージ本文
                "※確認※",                           // ダイアログのタイトル
                MessageBoxButtons.OKCancel,      // OKとキャンセルのボタンを表示
                MessageBoxIcon.Question           // 疑問符のアイコンを表示
            );

            // ユーザーの選択を判定
            if (result == DialogResult.OK)
            {
                this.Close();   // formを閉じる
                form2.Close();
            }

        }

        // カレンダーの日(セル)クリック時の画面遷移
        private void DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                // 選択はクリアしておくと安全
                ((DataGridView)sender).ClearSelection();
                return;
            }

            int x; // x座標
            int y; // y座標
            int year = int.Parse(label13.Text);  // 現在選択している年
            string memo_text = "";
            string title_text = "";
            string color = "";
            Dictionary<DateTime, (string Name, string Memo, string Title)> memo = GetMemo();
            Dictionary<DateTime, (string Name, string Color)> color_dic = GetHoliday();

            if (!int.TryParse(label13.Text, out year) || year < 1 || year > 9999)
            {
                // 例: "202"など無効な値が残っている場合
                MessageBox.Show("年が正しく入力されていません。カレンダーを更新してください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ((DataGridView)sender).ClearSelection();
                return;
            }

            x = e.ColumnIndex;  // クリックされたセルの行番号
            y = e.RowIndex;     // クリックされたセルの列番号 

            DataGridView data = (DataGridView)sender;  // クリックされたセルのDataGridViewの情報をdataに代入

            // セルに値が入っていない(null)の時
            if (data[x, y].Value == null || !int.TryParse(data[x, y].Value.ToString(), out int day_value_clicked))
            {
                data.ClearSelection(); // セルの選択を解除  
                return;
            }

            int targetmonth = 0;  // 初期化

            // 現在参照中のdatagridviewの月を取得
            if (data.Name.StartsWith("dataGridView") && int.TryParse(data.Name.Substring(12), out int parsedmonth))
            {
                targetmonth = parsedmonth;
            }

            // 現在選択中の年月日
            DateTime current_date = new DateTime(year, targetmonth, day_value_clicked);

            // メモ情報を格納
            if (memo.ContainsKey(current_date) && memo[current_date].Name == user_name)
            {
                memo_text = memo[current_date].Memo;
            }
            // タイトル情報を格納
            if (memo.ContainsKey(current_date) && memo[current_date].Name == user_name)
            {
                title_text = memo[current_date].Title;
            }
            if (color_dic.ContainsKey(current_date) && color_dic[current_date].Name == user_name)
            {
                color = color_dic[current_date].Color;
            }


            data.ClearSelection(); // セルの選択を解除
                                   // イベント発生元（sender）をDataGridViewコントロールとして取得
            DataGridView sourceGrid = sender as DataGridView;

            string gridName = sourceGrid.Name;


            // 1. 親フォームの背景（画像）の上に、黒の半透明なOverlayFormを表示
            using (var overlay = new Overlayform()) // OverlayFormのOpacityを0.5などにする
            {
                // 親フォーム全体を覆うように位置とサイズを設定
                overlay.Size = this.Size;
                overlay.Location = this.Location;

                overlay.Show(this); // 親フォームをオーナーとして表示

                // 2. 子フォーム（遷移先のフォーム）をモーダル表示
                Form3 form3 = new Form3(this, data, x, y, label13.Text, user_name, memo_text, title_text,color);

                form3.TopMost = true;  // form3を最前面に呼び出す
                form3.ShowDialog();    // モーダル表示
                Calendar_clear();      // カレンダー作成(form3を閉じたときに色付け更新する)
            }
        }



        // 現在日時のセルに色付け(黄色い枠で囲む処理)
        private void Cell_Paint(object sender, DataGridViewCellPaintingEventArgs e)
        {

            // ヘッダーセルや無効な行/列は処理しない
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            // 現在参照中のdatagirdview
            DataGridView data = (DataGridView)sender;
            DateTime targetDate;

            // セルの値（日）を取得
            if (e.Value == null || !int.TryParse(e.Value.ToString(), out int day))
            {
                return;
            }

            // DataGridViewの名前から月を取得
            if (data.Name.StartsWith("dataGridView") && int.TryParse(data.Name.Substring(12), out int month))
            {
                int year = int.Parse(label13.Text);
                try
                {
                    targetDate = new DateTime(year, month, day);
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    // 無効な日付はスキップ
                    return;
                }
            }
            else
            {
                return;
            }

            // 現在の日付であるかチェック
            if (targetDate.Date == today)
            {
                // 1. 標準の描画処理をキャンセル(ここでcalender_clearの処理を回避)
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.Border);

                // 2. 枠線を描画する
                using (Pen thickPen = new Pen(Color.Yellow, (float)THICK_BORDER_WIDTH)) // 黄色の太いペンを使用
                {
                    // 枠線の描画領域 (セル全体)
                    Rectangle rect = e.CellBounds;

                    // 描画をセルの内側に少しずらす (線が途切れないように)
                    double halfPen = THICK_BORDER_WIDTH / 2;

                    // 太い枠線を描画
                    e.Graphics.DrawRectangle(
                        thickPen,
                        (float)(rect.Left + halfPen),
                        (float)(rect.Top + halfPen),
                        (float)(rect.Width - THICK_BORDER_WIDTH),
                        (float)(rect.Height - THICK_BORDER_WIDTH));
                }

                // 3. 標準の描画をキャンセルしたことを伝える
                e.Handled = true;
            }
        }

        private void DataGridViewClicked(object sender, EventArgs e)
        {
   
        }

        // 検索画面遷移処理
        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            // 1. 親フォームの背景（画像）の上に、黒の半透明なOverlayFormを表示
            using (var overlay = new Overlayform()) // OverlayFormのOpacityを0.5などにする
            {
                // 親フォーム全体を覆うように位置とサイズを設定
                overlay.Size = this.Size;
                overlay.Location = this.Location;

                overlay.Show(this); // 親フォームをオーナーとして表示

                // 2. 遷移先のフォームを表示
                Form4 form4 = new Form4(this,user_name);

                form4.TopMost = true;  // form3を最前面に呼び出す
                form4.ShowDialog();    // モーダル表示
            }
        }
    }
}


