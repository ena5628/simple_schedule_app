using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;


namespace kojinseisaku
{
    public partial class Form4 : Form
    {
        private Form1 form1;  // form1の情報
        private string name;

        // 接続文字列
        string cnstr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\ckadai\C#\kojinseisaku\Database1.mdf;Integrated Security=True;Connect Timeout=30";

        public Form4(Form1 f1,string user_name)
        {
            InitializeComponent();

            form1 = f1;       // form1の情報

            name = user_name; // ユーザー名
        }

        private void button1_Click(object sender, EventArgs e)
        {
            form1.Calendar_clear();  // カレンダー色付け更新

            this.Close();  // フォームを閉じる
        }

        private void Form4_Load(object sender, EventArgs e)
        {

            dataGridView1.AllowUserToResizeColumns = false;
            dataGridView2.AllowUserToResizeColumns = false;  // ユーザー編集を禁止する
            dataGridView3.AllowUserToResizeColumns = false;

            radioButton4.Checked = true;                     // 全件表示ボタンを選択状態にする

            Datagridview_select();  // 全件検索した結果をdatagridviewに表示

     
        }

        // 全件検索した結果をdatagridviewに表示
        public void Datagridview_select()
        {

            DateTime today = DateTime.Now.Date;  // 現在日時


            // 現在日時後データ検索
            string select_date_future = @"SELECT MemoTable.Date AS 日付,MemoTable.Name AS ユーザー名,MemoTable.Title AS タイトル名,DateTable.Color
                                    FROM [dbo].[MemoTable] AS MemoTable
                                    INNER JOIN [dbo].[Table] AS DateTable
                                    ON MemoTable.Date = DateTable.Date AND MemoTable.Name = DateTable.Name
                                    WHERE MemoTable.Date >= @today AND MemoTable.Name = @Name
                                    ORDER BY MemoTable.Date ASC";

            // 現在日時前データ検索
            string select_date_past = @"SELECT MemoTable.Date AS 日付,MemoTable.Name AS ユーザー名,MemoTable.Title AS タイトル名,DateTable.Color
                                    FROM [dbo].[MemoTable] AS MemoTable
                                    INNER JOIN [dbo].[Table] AS DateTable
                                    ON MemoTable.Date = DateTable.Date AND MemoTable.Name = DateTable.Name
                                    WHERE MemoTable.Date < @today AND MemoTable.Name = @Name
                                    ORDER BY MemoTable.Date ASC";

            // 現在日時データ検索
            string select_date_now = @"SELECT MemoTable.Date AS 日付,MemoTable.Name AS ユーザー名,MemoTable.Title AS タイトル名,DateTable.Color
                                    FROM [dbo].[MemoTable] AS MemoTable
                                    INNER JOIN [dbo].[Table] AS DateTable
                                    ON MemoTable.Date = DateTable.Date AND MemoTable.Name = DateTable.Name
                                    WHERE MemoTable.Date = @today AND MemoTable.Name = @Name";

            // データベース処理
            using (SqlConnection connection = new SqlConnection(cnstr))
            {

                try
                {
                    connection.Open();  // データベース接続

                    // 現在日時前のデータを取得＆表示
                    using (SqlCommand cmd = new SqlCommand(select_date_past, connection))
                    {
                        cmd.Parameters.Add("@today", SqlDbType.Date).Value = today;
                        cmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = name;

                        using (SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dataTable = new DataTable();

                            // データベースから結合結果を取得してDataTableに格納
                            dataAdapter.Fill(dataTable);

                            // DataGridViewにバインド
                            dataGridView1.DataSource = dataTable;

                            foreach (DataGridViewColumn column in dataGridView1.Columns)
                            {
                                // ヘッダーをクリックしてもソートを行わないように設定
                                column.SortMode = DataGridViewColumnSortMode.NotSortable;
                            }

                            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

                            dataGridView1.AllowUserToResizeColumns = false;
                        }

                        dataGridView1.ClearSelection(); // セルの選択を解除 
                    }

                    // 現在日時後のデータを取得＆表示
                    using (SqlCommand cmd = new SqlCommand(select_date_future, connection))
                    {
                        cmd.Parameters.Add("@today", SqlDbType.Date).Value = today;
                        cmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = name;

                        using (SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dataTable = new DataTable();

                            // データベースから結合結果を取得してDataTableに格納
                            dataAdapter.Fill(dataTable);

                            // DataGridViewにバインド
                            dataGridView2.DataSource = dataTable;

                            foreach (DataGridViewColumn column in dataGridView2.Columns)
                            {
                                // ヘッダーをクリックしてもソートを行わないように設定
                                column.SortMode = DataGridViewColumnSortMode.NotSortable;
                            }

                            dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                            dataGridView2.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

                            dataGridView2.AllowUserToResizeColumns = false; 　// ユーザにサイズを変更させないようにする
                        }

                    }

                    // 現在日時のデータを取得＆表示
                    using (SqlCommand cmd = new SqlCommand(select_date_now, connection))
                    {
                        cmd.Parameters.Add("@today", SqlDbType.Date).Value = today;
                        cmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = name;

                        using (SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dataTable = new DataTable();

                            // データベースから結合結果を取得してDataTableに格納
                            dataAdapter.Fill(dataTable);

                            // DataGridViewにバインド
                            dataGridView3.DataSource = dataTable;

                            foreach (DataGridViewColumn column in dataGridView3.Columns)
                            {
                                // ヘッダーをクリックしてもソートを行わないように設定
                                column.SortMode = DataGridViewColumnSortMode.NotSortable;
                            }

                            dataGridView3.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                            dataGridView3.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

                            dataGridView3.AllowUserToResizeColumns = false; // ユーザにサイズを変更させないようにする
                        }

                    }

                    dataGridView1.ClearSelection(); // セルの選択を解除 
                    dataGridView2.ClearSelection(); // セルの選択を解除
                    dataGridView3.ClearSelection(); // セルの選択を解除 

                    // Color列色付け
                    dataGridView1.CellFormatting += new DataGridViewCellFormattingEventHandler(this.dataGridView_CellFormatting);
                    dataGridView2.CellFormatting += new DataGridViewCellFormattingEventHandler(this.dataGridView_CellFormatting);
                    dataGridView3.CellFormatting += new DataGridViewCellFormattingEventHandler(this.dataGridView_CellFormatting);

                }
                catch (Exception ex)
                {
                    MessageBox.Show("データベース接続またはデータ取得エラー: " + ex.Message);
                }
            }
        }


        // Color列の色づけ
        private void dataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // DataGridView を特定
            DataGridView dgv = sender as DataGridView;


            // Color 列のインデックスを取得(nullならば-1を返す)
            int colorColumnIndex = dgv.Columns["Color"]?.Index ?? -1;

            // Color列が存在しない、または行がデータ行でない場合は処理しない
            if (colorColumnIndex == -1 || e.RowIndex < 0 || dgv.Rows[e.RowIndex].IsNewRow)
            {
                return;
            }


            // 現在処理中のセルが Color列 であるかを確認
            if (e.ColumnIndex == colorColumnIndex)
            {
                // Color列のセルの値を取得
                object colorValue = dgv.Rows[e.RowIndex].Cells[colorColumnIndex].Value;

                // Color列に値が入っている場合
                if (colorValue != null)
                {
                    string colorName = colorValue.ToString();

                    // 赤だと見ずらいからなんとなく見やすい色で
                    if (colorName == "Red")
                    {
                        colorName = "Crimson";
                    }

                    try
                    {
                        // ColorTranslator.FromHtml で色名から Color 構造体を作成(動的に変数を色名に変換)
                        Color bgColor = System.Drawing.ColorTranslator.FromHtml(colorName);

                        // 現在処理中のセルの背景色を設定
                        e.CellStyle.BackColor = bgColor;

                        // Formattingが完了したことを通知
                        e.FormattingApplied = true;
                    }
                    catch
                    {
                        // 無効な色名の場合は、デフォルトの色に戻す
                        e.CellStyle.BackColor = dgv.DefaultCellStyle.BackColor;
                        e.CellStyle.ForeColor = dgv.DefaultCellStyle.ForeColor;
                    }
                }
            }

        }

        // 絞り込み条件で内容を変える処理
        private void button2_Click(object sender, EventArgs e)
        {
            string select_date_radio_past = "";
            string select_date_radio_future = "";

            DateTime today = DateTime.Now.Date;  // 現在日時取得

            
            /* Radio_button選択処理(データ絞り込み) */
            if (radioButton1.Checked)
            {
                // 現在日時後データ検索
                select_date_radio_future = @"SELECT MemoTable.Date AS 日付,MemoTable.Name AS ユーザー名,MemoTable.Title AS タイトル名,DateTable.Color
                                    FROM [dbo].[MemoTable] AS MemoTable
                                    INNER JOIN [dbo].[Table] AS DateTable
                                    ON MemoTable.Date = DateTable.Date AND MemoTable.Name = DateTable.Name
                                    WHERE MemoTable.Date >= @today AND DateTable.Color = 'Red' AND MemoTable.Name = @Name
                                    ORDER BY MemoTable.Date ASC";

                // 現在日時前データ検索
                select_date_radio_past = @"SELECT MemoTable.Date AS 日付,MemoTable.Name AS ユーザー名,MemoTable.Title AS タイトル名,DateTable.Color
                                    FROM [dbo].[MemoTable] AS MemoTable
                                    INNER JOIN [dbo].[Table] AS DateTable
                                    ON MemoTable.Date = DateTable.Date AND MemoTable.Name = DateTable.Name
                                    WHERE MemoTable.Date < @today AND DateTable.Color = 'Red' AND MemoTable.Name = @Name
                                    ORDER BY MemoTable.Date ASC";
            }
            else if (radioButton2.Checked)
            {
                // 現在日時後データ検索
                select_date_radio_future = @"SELECT MemoTable.Date AS 日付,MemoTable.Name AS ユーザー名,MemoTable.Title AS タイトル名,DateTable.Color
                                    FROM [dbo].[MemoTable] AS MemoTable
                                    INNER JOIN [dbo].[Table] AS DateTable
                                    ON MemoTable.Date = DateTable.Date AND MemoTable.Name = DateTable.Name
                                    WHERE MemoTable.Date >= @today AND DateTable.Color = 'RoyalBlue' AND MemoTable.Name = @Name
                                    ORDER BY MemoTable.Date ASC";

                // 現在日時前データ検索
                select_date_radio_past = @"SELECT MemoTable.Date AS 日付,MemoTable.Name AS ユーザー名,MemoTable.Title AS タイトル名,DateTable.Color
                                    FROM [dbo].[MemoTable] AS MemoTable
                                    INNER JOIN [dbo].[Table] AS DateTable
                                    ON MemoTable.Date = DateTable.Date AND MemoTable.Name = DateTable.Name
                                    WHERE MemoTable.Date < @today AND DateTable.Color = 'RoyalBlue' AND MemoTable.Name = @Name
                                    ORDER BY MemoTable.Date ASC";
            }
            else if (radioButton3.Checked)
            {
                // 現在日時後データ検索
                select_date_radio_future = @"SELECT MemoTable.Date AS 日付,MemoTable.Name AS ユーザー名,MemoTable.Title AS タイトル名,DateTable.Color
                                    FROM [dbo].[MemoTable] AS MemoTable
                                    INNER JOIN [dbo].[Table] AS DateTable
                                    ON MemoTable.Date = DateTable.Date AND MemoTable.Name = DateTable.Name
                                    WHERE MemoTable.Date >= @today AND DateTable.Color = 'Green' AND MemoTable.Name = @Name
                                    ORDER BY MemoTable.Date ASC";

                // 現在日時前データ検索
                select_date_radio_past = @"SELECT MemoTable.Date AS 日付,MemoTable.Name AS ユーザー名,MemoTable.Title AS タイトル名,DateTable.Color
                                    FROM [dbo].[MemoTable] AS MemoTable
                                    INNER JOIN [dbo].[Table] AS DateTable
                                    ON MemoTable.Date = DateTable.Date AND MemoTable.Name = DateTable.Name
                                    WHERE MemoTable.Date < @today AND DateTable.Color = 'Green' AND MemoTable.Name = @Name
                                    ORDER BY MemoTable.Date ASC";
            }
            else if (radioButton4.Checked)
            {
                select_date_radio_future = @"SELECT MemoTable.Date AS 日付,MemoTable.Name AS ユーザー名,MemoTable.Title AS タイトル名,DateTable.Color
                                    FROM [dbo].[MemoTable] AS MemoTable
                                    INNER JOIN [dbo].[Table] AS DateTable
                                    ON MemoTable.Date = DateTable.Date AND MemoTable.Name = DateTable.Name
                                    WHERE MemoTable.Date >= @today AND MemoTable.Name = @Name
                                    ORDER BY MemoTable.Date ASC";

                // 現在日時前データ検索
                select_date_radio_past = @"SELECT MemoTable.Date AS 日付,MemoTable.Name AS ユーザー名,MemoTable.Title AS タイトル名,DateTable.Color
                                    FROM [dbo].[MemoTable] AS MemoTable
                                    INNER JOIN [dbo].[Table] AS DateTable
                                    ON MemoTable.Date = DateTable.Date AND MemoTable.Name = DateTable.Name
                                    WHERE MemoTable.Date < @today AND MemoTable.Name = @Name
                                    ORDER BY MemoTable.Date ASC";
            }
            else
            {
                // Radio_buttonが選択されていない場合
                MessageBox.Show("ラジオボタンを選択してください");
                return;
            }

            // データベース処理
            using (SqlConnection connection = new SqlConnection(cnstr))
            {

                try
                {
                    connection.Open();  // データベース接続

                    // 現在日時前のデータを取得＆表示
                    using (SqlCommand cmd = new SqlCommand(select_date_radio_past, connection))
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@today", SqlDbType.Date).Value = today;
                        cmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = name;

                        using (SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dataTable = new DataTable();

                            // データベースから結合結果を取得してDataTableに格納
                            dataAdapter.Fill(dataTable);

                            // DataGridViewにバインド
                            dataGridView1.DataSource = dataTable;

                            foreach (DataGridViewColumn column in dataGridView1.Columns)
                            {
                                // ヘッダーをクリックしてもソートを行わないように設定
                                column.SortMode = DataGridViewColumnSortMode.NotSortable;
                            }

                            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

                            dataGridView1.AllowUserToResizeColumns = false;  // ユーザにサイズを変更させないようにする
                        }

                        dataGridView1.ClearSelection(); // セルの選択を解除 
                    }

                    // 現在日時後のデータを取得＆表示
                    using (SqlCommand cmd = new SqlCommand(select_date_radio_future, connection))
                    {
                        cmd.Parameters.Add("@today", SqlDbType.Date).Value = today;
                        cmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = name;

                        using (SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dataTable = new DataTable();

                            // データベースから結合結果を取得してDataTableに格納
                            dataAdapter.Fill(dataTable);

                            // DataGridViewにバインド
                            dataGridView2.DataSource = dataTable;

                            foreach (DataGridViewColumn column in dataGridView2.Columns)
                            {
                                // ヘッダーをクリックしてもソートを行わないように設定
                                column.SortMode = DataGridViewColumnSortMode.NotSortable;
                            }

                            dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                            dataGridView2.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

                            dataGridView2.AllowUserToResizeColumns = false;   // ユーザにサイズを変更させないようにする  
                        }

                        dataGridView1.ClearSelection(); // セルの選択を解除 
                        dataGridView2.ClearSelection(); // セルの選択を解除 

                        // Color列色付け
                        dataGridView1.CellFormatting += new DataGridViewCellFormattingEventHandler(this.dataGridView_CellFormatting);
                        dataGridView2.CellFormatting += new DataGridViewCellFormattingEventHandler(this.dataGridView_CellFormatting);
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("データベース接続またはデータ取得エラー: " + ex.Message);
                }
            }
        }



        // form3への画面遷移(登録されているデータごとの移動)
        private async void Data_click_select(object sender, DataGridViewCellMouseEventArgs e)
        {
            // ヘッダー行や無効な行は無視する
            if (e.RowIndex < 0)
            {
                return;
            }

            DataGridView dgv = sender as DataGridView;  // 現在参照しているdatagridview

            // データに値が入っていない場合
            if (dgv == null)
            {
                return;
            }


            // 選択された行のデータを取得(日付,ユーザ名,タイトル名)
            DataGridViewRow selectedRow = dgv.Rows[e.RowIndex];

            // DataGridViewからForm3に渡す情報を取得
            DateTime date = (DateTime)selectedRow.Cells["日付"].Value;
            string name = selectedRow.Cells["ユーザー名"].Value.ToString();
            string title = selectedRow.Cells["タイトル名"].Value.ToString();
            string color = selectedRow.Cells["Color"].Value.ToString();

            // メモ本文を取得するための変数
            string memo = "";

            // データベースからメモ本文（Memo）を取得
            string selectMemoQuery = @"SELECT Memo FROM [dbo].[MemoTable] WHERE Date = @Date AND Name = @Name";

            using (SqlConnection connection = new SqlConnection(cnstr))
            {
                try
                {
                    await connection.OpenAsync(); // 非同期処理
                    using (SqlCommand cmd = new SqlCommand(selectMemoQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@Date", date);
                        cmd.Parameters.AddWithValue("@Name", name);

                        object result = await cmd.ExecuteScalarAsync(); // 1つの値（Memo本文）を取得

                        if (result != null)
                        {
                            memo = result.ToString();  // 値をmemoに格納
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("メモ本文の取得中にエラーが発生しました: " + ex.Message);
                    return;
                }
            }

            try
            {
                // 1. 親フォームの背景（画像）の上に、黒の半透明なOverlayFormを表示
                using (var overlay = new Overlayform())
                {
                    // 親フォーム全体を覆うように位置とサイズを設定
                    overlay.Size = form1.Size;
                    overlay.Location = form1.Location;

                    overlay.Show(form1); // 親フォームをオーナーとして表示

                    // 2. 子フォーム（遷移先のフォーム）を最前面に表示
                    Form3 form3 = new Form3(form1,this, date,name, memo, title,color);
                    form3.TopMost = true;  // form3を最前面に呼び出す
                    form3.ShowDialog();    // モーダル表示
                    
                }

            }
            catch
            {

            }
        }
    }
}




