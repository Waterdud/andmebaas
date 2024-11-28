using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Andmebass_TARpv23
{
    public partial class Form1 : Form
    {
        SqlConnection conn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\opilane\Source\Repos\Andmebass_TARpv23\Andmebaas1.mdf;Integrated Security=True");
        SqlCommand cmd;
        SqlDataAdapter adapter;
        OpenFileDialog open;
        SaveFileDialog save;
        Form popupForm;
        DataTable laotable;
        string extension;
        private byte[] imageData;
        int ID = 0;

        public Form1()
        {
            InitializeComponent();
            NaitaAndmed();
            NaitaLaod();
        }

        private void NaitaLaod()
        {
            conn.Open();
            cmd = new SqlCommand("SELECT Id, LaoNimetus FROM Ladu", conn);
            adapter = new SqlDataAdapter(cmd);
            laotable = new DataTable();
            adapter.Fill(laotable);
            foreach (DataRow item in laotable.Rows)
            {
                Ladu_cb.Items.Add(item["LaoNimetus"]);
            }
            conn.Close();
        }

        public void NaitaAndmed()
        {
            conn.Open();
            DataTable dt = new DataTable();
            cmd = new SqlCommand("SELECT * FROM Toode", conn);
            adapter = new SqlDataAdapter(cmd);
            adapter.Fill(dt);
            dataGridView1.DataSource = dt;
            conn.Close();
        }

        private void Emaldamine()
        {
            Nimetus_txt.Text = "";
            Kogus_txt.Text = "";
            Hind_txt.Text = "";
            pictureBox1.Image = Image.FromFile(Path.Combine(Path.GetFullPath(@"..\..\Pildid"), "pilt.jpg"));
        }

        private void Pildi_otsing_btn_Click(object sender, EventArgs e)
        {
            open = new OpenFileDialog();
            open.InitialDirectory = @"C:\Users\opilane\Pictures\";
            open.Multiselect = false;
            open.Filter = "Images Files(*.jpeg;*.png;*.bmp;*.jpg)|*.jpeg;*.png;*.bmp;*.jpg";
            if (open.ShowDialog() == DialogResult.OK && Nimetus_txt.Text != null)
            {
                save = new SaveFileDialog();
                save.InitialDirectory = Path.GetFullPath(@"..\..\Pildid");
                extension = Path.GetExtension(open.FileName);
                save.FileName = Nimetus_txt.Text + extension;
                save.Filter = "Images" + Path.GetExtension(open.FileName) + "|" + Path.GetExtension(open.FileName);
                if (save.ShowDialog() == DialogResult.OK)
                {
                    File.Copy(open.FileName, save.FileName);
                    pictureBox1.Image = Image.FromFile(save.FileName);
                }
            }
            else
            {
                MessageBox.Show("Puudub toode nimetus või ole Cancel vajutatud");
            }
        }

        private void Lisa_btn_Click(object sender, EventArgs e)
        {
            if (Nimetus_txt.Text.Trim() != string.Empty && Kogus_txt.Text.Trim() != string.Empty && Hind_txt.Text.Trim() != string.Empty)
            {
                try
                {
                    conn.Open();

                    cmd = new SqlCommand("SELECT Id FROM Ladu WHERE LaoNimetus=@ladu", conn);
                    cmd.Parameters.AddWithValue("@ladu", Ladu_cb.Text);
                    cmd.ExecuteNonQuery();
                    ID = Convert.ToInt32(cmd.ExecuteScalar());

                    cmd = new SqlCommand("Insert into Toode(Nimetus, Kogus, Hind, Pilt) Values (@toode,@kogus,@hind,@pilt)", conn);
                    cmd.Parameters.AddWithValue("@toode", Nimetus_txt.Text);
                    cmd.Parameters.AddWithValue("@kogus", Kogus_txt.Text);
                    cmd.Parameters.AddWithValue("@hind", Hind_txt.Text);
                    cmd.Parameters.AddWithValue("@pilt", Nimetus_txt.Text + extension);
                    cmd.Parameters.AddWithValue("@ladu", ID);

                    cmd.ExecuteNonQuery();
                    conn.Close();
                    NaitaAndmed();
                }
                catch (Exception)
                {
                    MessageBox.Show("Andmebaasiga viga");
                }
            }
            else
            {
                MessageBox.Show("Sisesta andmeid");
            }
        }

        private void dataGridView1_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (popupForm != null && !popupForm.IsDisposed)
            {
                popupForm.Close();
            }
        }

        private void Uuenda_btn_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Nimetus_txt.Text) &&
                !string.IsNullOrWhiteSpace(Kogus_txt.Text) &&
                !string.IsNullOrWhiteSpace(Hind_txt.Text))
            {
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    try
                    {
                        ID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["Id"].Value);

                        conn.Open();

                        cmd = new SqlCommand(
                            "UPDATE Toode SET Nimetus = @toode, Kogus = @kogus, Hind = @hind, Pilt = @pilt WHERE Id = @id",
                            conn);
                        cmd.Parameters.AddWithValue("@id", ID);
                        cmd.Parameters.AddWithValue("@toode", Nimetus_txt.Text);
                        cmd.Parameters.AddWithValue("@kogus", int.Parse(Kogus_txt.Text));
                        cmd.Parameters.AddWithValue("@hind", decimal.Parse(Hind_txt.Text));
                        cmd.Parameters.AddWithValue("@pilt", Nimetus_txt.Text + extension);

                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Andmed on edukalt uuendatud!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Viga andmete uuendamisel: " + ex.Message);
                    }
                    finally
                    {
                        conn.Close();
                        NaitaAndmed();
                        Emaldamine();
                    }
                }
                else
                {
                    MessageBox.Show("Valige kirje, mida soovite uuendada.");
                }
            }
            else
            {
                MessageBox.Show("Palun täitke kõik väljad!");
            }
        }
 

        private void LoopIt(Image image, int r)
        {
            popupForm = new Form();
            popupForm.FormBorderStyle = FormBorderStyle.None;
            popupForm.StartPosition = FormStartPosition.Manual;
            popupForm.Size = image.Size;

            PictureBox pictureBox = new PictureBox
            {
                Image = image,
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            popupForm.Controls.Add(pictureBox);

            Rectangle cellRectangle = dataGridView1.GetCellDisplayRectangle(4, r, true);
            Point popupLocation = dataGridView1.PointToScreen(cellRectangle.Location);

            popupForm.Location = new Point(popupLocation.X + cellRectangle.Width, popupLocation.Y);

            popupForm.Show();
        }

        private void Kustuta_btn_Click(object sender, EventArgs e)
        {
            try
            {
                ID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["Id"].Value);
                if (ID != 0)
                {
                    conn.Open();
                    cmd = new SqlCommand("DELETE FROM Toode WHERE Id=@id", conn);
                    cmd.Parameters.AddWithValue("@id", ID);
                    cmd.ExecuteNonQuery();
                    conn.Close();

                    Kustuta_fail(dataGridView1.SelectedRows[0].Cells["Pilt"].Value.ToString());

                    Emaldamine();
                    NaitaAndmed();
                    MessageBox.Show("Kirje kustutatud edukalt", "Kustuta");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Viga kirje kustutamisel: {ex.Message}");
            }
        }

        private void Kustuta_fail(string file)
        {
            try
            {
                string filePath = Path.Combine(Path.GetFullPath(@"..\..\Pildid"), file);

                if (File.Exists(filePath))
                {
                    pictureBox1.Image?.Dispose();
                    pictureBox1.Image = null;
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Viga faili kustutamisel: {ex.Message}");
            }
        }
    }
}
