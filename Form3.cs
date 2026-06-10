using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace kojinseisaku
{
    public partial class Form3 : Form
    {
        // グローバル変数
        Form1 form1;
        Form4 form4;
        public DataGridView datagrid;
        public int x;
        public int y;
        public int year;
        public string name;
        public string back_color;
        public string memo;
        public string title;
        public int day_value_clicked;
        public int targetmonth;
        public DateTime update_date_clicked;

        // 接続文字列
        private string cnstr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\ckadai\C#\kojinseisaku\Database1.mdf;Integrated Security=True;Connect Timeout=30";

        // form1からform3に遷移時のForm3
        public Form3(Form1 f1, DataGridView data, int x1, int y1, string year_text,string user_name,string memo_text,string title_text,string color)
        {
            InitializeComponent();

            datagrid = data;                 //
            x = x1;                          //
            y = y1;                          //
            year = int.Parse(year_text);     // 別フォームから渡された情報をグローバル変数で定義
            name = user_name;                // 
            memo = memo_text;                // 
            title = title_text;              //
            form1 = f1;                      //
            back_color = color;              // 

            // 選択されたセルから日を取得
            day_value_clicked = int.Parse(datagrid[x, y].Value.ToString());
            targetmonth = 0;  // 選択月

            // 現在参照中のdatagridviewの月を取得
            if (datagrid.Name.StartsWith("dataGridView") && int.TryParse(datagrid.Name.Substring(12), out int parsedmonth))
            {
                targetmonth = parsedmonth;  // 選択月をtargetmonthに代入
            }

            // 現在選択中の年月日
            update_date_clicked = new DateTime(year, targetmonth, day_value_clicked);

            this.StartPosition = FormStartPosition.CenterScreen;
        }

        // form4からform3に遷移時のForm3(オーバーロード)
        public Form3(Form1 f1,Form4 f4, DateTime date_clicked, string user_name, string memo_text, string title_text,string color)
        {
            InitializeComponent();

            form1 = f1;                               // 
            form4 = f4;                               // 
            update_date_clicked = date_clicked.Date;  // 
            year = date_clicked.Year;                 // 
            targetmonth = date_clicked.Month;         // グローバル変数にform4から渡された情報を格納 
            day_value_clicked = date_clicked.Day;     // 
            name = user_name;                         //
            memo = memo_text;                         //
            title = title_text;                       //
            back_color = color;                       // 

            // どちらのformから遷移したのかを識別するための情報
            datagrid = null;

            this.StartPosition = FormStartPosition.CenterScreen;
        }


        private void Form3_Load(object sender, EventArgs e)
        {
            textBox1.Text = memo;                                      // メモデータ
            textBox2.Text = title;                                     // タイトル名
            label1.Text = update_date_clicked.ToString("yyyy/MM/dd");  // 日付

            /*-------------------重要度表示--------------------*/
            Color form_color = Color.Empty;
            Color button_color = Color.Empty;
            switch (back_color)
            {
                case "Red":
                    form_color = Color.LightCoral;
                    button_color = Color.IndianRed;
                    radioButton1.Checked = true;
                    break;
                case "RoyalBlue":
                    form_color = Color.LightSkyBlue;
                    button_color = Color.LightBlue;
                    radioButton2.Checked = true;
                    break;
                case "Green":
                    form_color = Color.LightGreen;
                    button_color = Color.DarkSeaGreen;
                    radioButton3.Checked = true;
                    break;
            }
            this.BackColor = form_color;
            this.button1.BackColor = button_color;
            this.button2.BackColor = button_color;
            this.button3.BackColor = button_color;
            /*--------------------------------------------------*/

            toolTip1.SetToolTip(textBox2, textBox2.Text);              // カーソルをあわせると全文表示
        }


        private void button2_Click(object sender, EventArgs e)
        {
            Color targetColor = Color.Empty;  // 初期値設定

            // radioButtonの分岐処理
            if (radioButton1.Checked || radioButton2.Checked || radioButton3.Checked)
            {
                if (radioButton1.Checked)
                {
                    // 取得したDataGridViewと座標を使って色を設定
                    targetColor = Color.Red;

                }
                else if (radioButton2.Checked)
                {
                    // 取得したDataGridViewと座標を使って色を設定
                    targetColor = Color.RoyalBlue;

                }
                else if (radioButton3.Checked)
                {
                    // 取得したDataGridViewと座標を使って色を設定
                    targetColor = Color.Green;
                    
                }
            }
            else
            {
                MessageBox.Show("ラジオボタンを選択してください");
                return;
            }

            // form1から遷移した場合
            if (datagrid != null)
            {
                // 背景色を変更(form1のdatagridview)
                datagrid[x, y].Style.BackColor = targetColor;

                try
                {

                    DateTime start_Date = new DateTime(year, 1, 1);  // 年始
                    DateTime end_Date = new DateTime(year, 12, 31);  // 年末

                    SqlTransaction tran = null;  // トランザクション処理

                    using (SqlConnection connection = new SqlConnection())
                    {
                        DataGridView dgv = null;
                        DateTime date;
                        int day_value;
                        int month;
                        int row;
                        int col;
                        string memo = textBox1.Text;
                        string title = textBox2.Text;

                        if (string.IsNullOrEmpty(title))
                        {
                            MessageBox.Show("タイトルを設定してください");
                        }
                        else
                        {
                            connection.ConnectionString = cnstr;

                            connection.Open();  // データベース接続
                            tran = connection.BeginTransaction(); // トランザクションを開始

                            using (SqlCommand cmd = new SqlCommand())
                            {
                                cmd.Transaction = tran;
                                cmd.Connection = connection;

                                // タイトルが空欄の場合
                                if (string.IsNullOrEmpty(title))
                                {
                                    // 既存のレコードを削除
                                    cmd.CommandText = "DELETE FROM [dbo].[MemoTable] WHERE Date = @Date AND Name = @Name";
                                }
                                else
                                {
                                    // データベースに存在している(更新処理)、存在しない(挿入処理)
                                    cmd.CommandText =
                                        @"IF EXISTS (SELECT 1 FROM [dbo].[MemoTable] WHERE Date = @Date AND Name = @Name)
                                             UPDATE [dbo].[MemoTable] SET Memo = @Memo, Title = @Title WHERE Date = @Date AND Name = @Name
                                          ELSE
                                             INSERT INTO [dbo].[MemoTable] (Date, Name, Memo, Title) VALUES (@Date, @Name, @Memo, @Title);";

                                    cmd.Parameters.Add(new SqlParameter("@Memo", memo));
                                    cmd.Parameters.Add(new SqlParameter("@Title", title));
                                }

                                // 共通のパラメータを設定
                                cmd.Parameters.Add(new SqlParameter("@Date", update_date_clicked));
                                cmd.Parameters.Add(new SqlParameter("@Name", name));

                                cmd.ExecuteNonQuery();
                            }

                            // その年のデータを全削除
                            using (SqlCommand cmd = new SqlCommand())
                            {

                                day_value = int.Parse(datagrid[x, y].Value.ToString());  // 選択されているDatagridViewの日付

                                cmd.Transaction = tran;
                                cmd.Connection = connection;
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandText = "DELETE FROM [dbo].[Table] WHERE Date >= @start_Date " + "AND Date <= @end_Date " + "AND Name = @name";
                                cmd.Parameters.Add(new SqlParameter("@start_Date", start_Date));
                                cmd.Parameters.Add(new SqlParameter("@end_Date", end_Date));
                                cmd.Parameters.Add(new SqlParameter("@name", name));

                                cmd.ExecuteNonQuery();

                            }

                            // データを更新(挿入)
                            using (SqlCommand cmd = new SqlCommand())
                            {
                                cmd.Transaction = tran;
                                cmd.Connection = connection;

                                // 1～12月まで
                                for (month = 1; month <= 12; month++)
                                {
                                    // 現在参照中のコントロールを探す
                                    Control[] cs = form1.Controls.Find("dataGridView" + string.Format("{0:}", month), true);
                                    if (cs.Length > 0)
                                    {
                                        dgv = (DataGridView)cs[0];
                                    }
                                    else
                                    {
                                        continue;
                                    }

                                    // 参照しているdatagridviewの行数分繰り返す
                                    for (row = 0; row < dgv.RowCount; row++)
                                    {
                                        // 参照しているdatagridviewの行数分繰り返す
                                        for (col = 0; col < dgv.ColumnCount; col++)
                                        {
                                            // 空のセルの場合
                                            if (dgv[col, row].Value == null)
                                            {
                                                continue;
                                            }

                                            // セルが色付けされていない場合
                                            if (dgv[col, row].Style.BackColor == Color.Empty)
                                            {
                                                continue;
                                            }

                                            // 日付が数字かチェックする
                                            if (int.TryParse(dgv[col, row].Value.ToString(), out day_value))
                                            {
                                                // cellcolorに現在ループで参照しているセルの背景色を取得
                                                Color cellColor = dgv[col, row].Style.BackColor;
                                                string color;  // 色(データベース用)

                                                // セルの背景色が赤の場合
                                                if (cellColor.ToArgb() == Color.Red.ToArgb() || cellColor.ToArgb() == System.Drawing.Color.FromArgb(255, 156, 151).ToArgb())
                                                {
                                                    color = "Red";
                                                }
                                                // セルの背景色が青の場合
                                                else if (cellColor.ToArgb() == Color.RoyalBlue.ToArgb() || cellColor.ToArgb() == System.Drawing.Color.FromArgb(173, 224, 238).ToArgb())
                                                {
                                                    color = "RoyalBlue";
                                                }
                                                // セルの背景色が緑の場合
                                                else if (cellColor.ToArgb() == Color.Green.ToArgb() || cellColor.ToArgb() == System.Drawing.Color.FromArgb(206, 226, 193).ToArgb())
                                                {
                                                    color = "Green";
                                                }
                                                else
                                                {
                                                    // 想定外の色またはデフォルトの灰色の場合、DBには保存しない (null)
                                                    color = null;
                                                    continue; // nullならINSERTせずに次のセルへ
                                                }

                                                date = new DateTime(year, month, day_value);  // 色付けされているセルの日付を取得

                                                cmd.CommandType = CommandType.Text;
                                                // Tableの内容を更新(挿入)
                                                cmd.CommandText = "INSERT INTO [dbo].[Table] (Date,Name,Color) VALUES (@Date,@Name,@Color)";
                                                cmd.Parameters.Clear();
                                                cmd.Parameters.Add(new SqlParameter("@Date", date)); // パラメータを再追加
                                                cmd.Parameters.Add(new SqlParameter("@Name", name)); // パラメータを再追加
                                                cmd.Parameters.Add(new SqlParameter("@Color", color)); // パラメータを再追加

                                                cmd.ExecuteNonQuery();

                                            }


                                        }
                                    }

                                }
                            }
                            tran.Commit();  // コミット

                            // 正常終了メッセージ（デバッグ用）
                            MessageBox.Show("データの保存が完了しました。");

                            this.TopMost = false;
                            this.Close();
                        }


                    }



                }
                catch (Exception en)
                {
                    MessageBox.Show("接続エラーが発生しました: " + en.Message);
                }

            }
            else　　// form4から遷移した場合
            {
                try
                {
                    string color = "";  // 色(データベース用)

                    if (targetColor == Color.Red)
                    {
                        color = "Red";
                    }
                    else if (targetColor == Color.RoyalBlue)
                    {
                        color = "RoyalBlue";

                    }
                    else if (targetColor == Color.Green)
                    {
                        color = "Green";
                    }
                    else
                    {
                        return;
                    }


                    SqlTransaction tran = null;

                    using (SqlConnection connection = new SqlConnection())
                    {
                        // タイトルが空白の場合
                        if (string.IsNullOrEmpty(title))
                        {
                            MessageBox.Show("タイトルを設定してください");
                            return;
                        }
                        else
                        {
                            connection.ConnectionString = cnstr;

                            connection.Open();  // sql接続
                            tran = connection.BeginTransaction(); // トランザクションを開始

                            using (SqlCommand cmd = new SqlCommand())
                            {
                                cmd.Transaction = tran;
                                cmd.Connection = connection;

                                // タイトルが空欄の場合
                                if (string.IsNullOrEmpty(title))
                                {
                                    // 既存のレコードを削除
                                    cmd.CommandText = "DELETE FROM [dbo].[MemoTable] WHERE Date = @Date AND Name = @Name";
                                }
                                else
                                {
                                    // データベースに存在している(更新処理)、存在しない(挿入処理)
                                    cmd.CommandText =
                                        @"IF EXISTS (SELECT 1 FROM [dbo].[MemoTable] WHERE Date = @Date AND Name = @Name)
                                             UPDATE [dbo].[MemoTable] SET Memo = @Memo, Title = @Title WHERE Date = @Date AND Name = @Name
                                          ELSE
                                             INSERT INTO [dbo].[MemoTable] (Date, Name, Memo, Title) VALUES (@Date, @Name, @Memo, @Title);";

                                    cmd.Parameters.Add(new SqlParameter("@Memo", memo));
                                    cmd.Parameters.Add(new SqlParameter("@Title", title));
                                }

                                // 共通のパラメータを設定
                                cmd.Parameters.Add(new SqlParameter("@Date", update_date_clicked));
                                cmd.Parameters.Add(new SqlParameter("@Name", name));

                                cmd.ExecuteNonQuery();
                            }

                            using (SqlCommand cmd = new SqlCommand())
                            {
                                cmd.Transaction = tran;
                                cmd.Connection = connection;

                                DateTime date;
                                date = new DateTime(year, targetmonth, day_value_clicked);  // 色付けされているセルの日付を取得

                                cmd.CommandType = CommandType.Text;
                                // Tableに存在しない場合(挿入)、存在する場合(更新)
                                cmd.CommandText =
                                      @"IF NOT EXISTS (SELECT 1 FROM [dbo].[Table] WHERE Date = @Date AND Name = @Name)
                                            INSERT INTO [dbo].[Table] (Date,Name,Color) VALUES (@Date,@Name,@Color)
                                        ELSE
                                            UPDATE  [dbo].[Table] SET Color = @Color WHERE Date = @Date AND Name = @Name;";
                                cmd.Parameters.Clear();
                                cmd.Parameters.Add(new SqlParameter("@Date", date)); // パラメータを再追加
                                cmd.Parameters.Add(new SqlParameter("@Name", name)); // パラメータを再追加
                                cmd.Parameters.Add(new SqlParameter("@Color", color)); // パラメータを再追加

                                cmd.ExecuteNonQuery();
                            }

                            tran.Commit();

                            // 正常終了メッセージ（デバッグ用）
                            MessageBox.Show("データの保存が完了しました。");

                            form4.Datagridview_select();
                            this.TopMost = false;
                            this.Close();

                        }
                    }
                }
                catch(OperationAbortedException ez)
                {
                    MessageBox.Show("データベースエラー" + ez.Message);
                }
                


            }
        }












        // 処理の終了
        private void button1_Click_1(object sender, EventArgs e)
        {
            // form4から呼ばれた場合
            if (datagrid == null)
            {
                form4.dataGridView1.ClearSelection(); // セルの選択を解除
                form4.dataGridView2.ClearSelection(); // セルの選択を解除
                form4.dataGridView3.ClearSelection(); // セルの選択を解除
            }
            // Form3を閉じ、Form1を再度表示
            this.TopMost = false;
            this.Close();
        }



        // データ削除のメッセージ
        private void button3_Click(object sender, EventArgs e)
        {
            // 確認ダイアログを表示
            DialogResult result = MessageBox.Show(
                "データを削除してよろしいですか？\n(現在参照中のデータが完全に削除されます)", // メッセージ本文
                "※確認※",                           // ダイアログのタイトル
                MessageBoxButtons.OKCancel,      // OKとキャンセルのボタンを表示
                MessageBoxIcon.Question           // 疑問符のアイコンを表示
            );

            // ユーザーの選択を判定
            if (result == DialogResult.OK)
            {
                // ユーザーが「OK」を選択した場合の処理
                Table_Delete();
            }
            else
            {
                MessageBox.Show("処理をキャンセルしました。");
            }

        }

        // データの削除処理
        private void Table_Delete()
        {
            SqlTransaction tran = null;

            try
            {
                using (SqlConnection connection = new SqlConnection(cnstr))
                {
                    connection.Open();

                    tran = connection.BeginTransaction();


                    // コマンドも using で囲む (破棄を保証)
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = connection;
                        cmd.Transaction = tran;

                        // パラメータ設定
                        cmd.Parameters.Add(new SqlParameter("@Date", update_date_clicked));
                        cmd.Parameters.Add(new SqlParameter("@Name", name));

                        // MemoTableから選択されている日のデータを削除
                        cmd.CommandText = "DELETE FROM [dbo].[MemoTable] WHERE Date = @Date AND Name = @Name";
                        cmd.ExecuteNonQuery(); // 1回目の実行

                        // Tableから選択されている日のデータを削除
                        cmd.CommandText = "DELETE FROM [dbo].[Table] WHERE Date = @Date AND Name = @Name";
                        cmd.ExecuteNonQuery(); // 2回目の実行

                    }

                    // 両方の DELETE が成功したらコミット
                    tran.Commit();


                }

                MessageBox.Show("データを削除しました");

                // form4から遷移し実行した場合
                if (form4 != null)
                {
                    form4.Datagridview_select();  // 一覧表示を更新(データ削除後)
                }
                

            }
            catch (SqlException en)
            {
                // コミット失敗時
                if (tran != null)
                {
                    try
                    {
                        tran.Rollback();  // ロールバック
                    }
                    catch (Exception rollbackEx)
                    {
                        // ロールバック失敗時の処理
                        MessageBox.Show($"ロールバック中にエラーが発生しました: {rollbackEx.Message}");
                    }
                }
                MessageBox.Show("データベースエラー：" + en.Message);
            }
            finally
            {
                this.TopMost = false;
                this.Close();
            }

        }


        //  テキストボックスの内容を表示
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(textBox2, textBox2.Text);
        }
    }

}

        
       



            




        
    
    




