using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.PerformanceData;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace kojinseisaku
{
    public partial class Form2 : Form
    {

        public Form2()
        {
            InitializeComponent();
        }

        private SqlConnection cn = new SqlConnection();
        SqlCommand cmd = new SqlCommand();        // インスタンス生成用変数
        private SqlDataReader rd;                 // フィールドの宣言

        // 接続文字列
        private string cnstr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\ckadai\C#\kojinseisaku\Database1.mdf;Integrated Security=True;Connect Timeout=30";

        private int SelectedIndex;

        private void Form2_Load(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(textBox1, textBox1.Text);  // カーソルを合わせると内容表示

            toolTip2.SetToolTip(textBox2, textBox2.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            cn.ConnectionString = cnstr;
            cn.Open();

            string num1 = textBox1.Text; // ユーザー名
            string num2 = textBox2.Text; // パスワード

            string name = "";
            int cnt = 0;  // 検索件数

            cmd.Connection = cn;
            cmd.CommandType = CommandType.Text;
            // 入力されたユーザー名とパスワードが存在するか
            cmd.CommandText = @"SELECT UserName FROM [dbo].[Login] WHERE UserName ='" + num1 + "' AND Password ='" + num2 + "'";

            rd = cmd.ExecuteReader();

            // Read(読み込み)
            while (rd.Read())
            {
                name = rd["UserName"].ToString();  // ユーザー名(他formに渡すための変数)
                cnt++;  // 件数カウント
            }


       
            try
            {

                // 1件見つかった場合のみ成功
                if (cnt == 1)
                {
                    MessageBox.Show("認証成功");
                    // ログイン後の画面遷移処理など
                    Form1 form1 = new Form1(this,name);
                    form1.Show();
                    this.Enabled = false;
                    this.Visible = false;
                }
                else  // 1件もないor複数存在している(例外)
                {
                    MessageBox.Show("認証失敗: ユーザー名またはパスワードが正しくありません");
                    textBox1.Text = "";
                    textBox2.Text = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("データベースエラー: " + ex.Message);
            }
            finally
            {
                // 最後に接続を閉じます
                if (cn != null && cn.State == ConnectionState.Open)
                {
                    cn.Dispose();  // データベースの接続を破棄する
                    cn.Close();  // データベースの接続を閉じる
                }
            }




        }

        // アカウント新規登録処理
        private void button1_Click(object sender, EventArgs e)
        {
            string name = textBox1.Text;
            string pass = textBox2.Text;
            int count;
            int addcount;

            cn.ConnectionString = cnstr;
            cn.Open();
            try
            {
                cmd.Connection = cn;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();
                cmd.Parameters.Add(new SqlParameter("@Name", name));
                cmd.Parameters.Add(new SqlParameter("@Password", pass));

                // 入力された情報をログインテーブルと照合して存在するか確認
                cmd.CommandText = @"SELECT COUNT(*) FROM [dbo].[Login] WHERE UserName = @Name AND password = @Password";

                object result = cmd.ExecuteScalar();
                count = (result != DBNull.Value) ? Convert.ToInt32(result) : 0;  // オブジェクト型をint型に変換

                // 既に存在している場合
                if (count > 0)
                {
                    MessageBox.Show("そのアカウントは既に存在しています。");
                    textBox1.Text = "";
                    textBox2.Text = "";
                    this.SelectedIndex = 0;
                }
                else if (name.Length > 4 && pass.Length > 3)
                {
                    cmd.CommandText = "INSERT INTO [dbo].[Login] (UserName,password) VALUES (@Name,@Password)";

                    addcount = cmd.ExecuteNonQuery();

                    if (addcount > 0)
                    {
                        MessageBox.Show("アカウントが追加されました");
                    }
                    else
                    {
                        MessageBox.Show("アカウントの追加に失敗しました");
                    }

                }
                else
                {
                    if (name.Length <= 4 && pass.Length <= 3)
                    {
                        MessageBox.Show("ユーザー名は5文字以上、パスワードは4文字以上入力してください");
                        textBox1.Text = "";
                        textBox2.Text = "";
                    }
                    else if (name.Length <= 4)
                    {
                        MessageBox.Show("ユーザー名は5文字以上入力してください");
                        textBox1.Text = "";
                        textBox2.Text = "";
                    }
                    else if (pass.Length <= 3)
                    {
                        MessageBox.Show("パスワードは4文字以上入力してください");
                        textBox2.Text = "";
                    }
                    else
                    {
                        MessageBox.Show("そのユーザー名は登録することができません");
                        textBox1.Text = "";
                        textBox2.Text = "";
                    }
             
                }

            }
            catch (IOException ex)
            {
                MessageBox.Show("接続エラー：" + ex.Message);
            }
            finally
            {
                cn.Close();
                cn.Dispose();
            }

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(textBox1, textBox1.Text);
            toolTip2.SetToolTip(textBox2, textBox2.Text);
        }
    }
}
