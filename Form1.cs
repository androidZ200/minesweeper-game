using System;
using System.Drawing;
using System.Windows.Forms;
using ImageWork;
using MinesweeperLibrary;
using System.Diagnostics;
using System.Threading;

namespace minesweeper
{
    public partial class Form : System.Windows.Forms.Form
    {
        object Lock = new object();
        byte Mode = 0;
        Graphics field, field1;
        Stopwatch StartTime;
        Thread t1;
        Point beginning;
        bool Game = true;
        Bitmap image, image1;
        MinesweeperGame New;
        int X, Y;
        byte[,] arrayField;
        byte[,] arrayOpen;
        public Form()
        {
            InitializeComponent();
        }
        private void start_Click(object sender, EventArgs e)
        {
            if (t1 != null)
                t1.Abort();
            Game = true;
            beginning = new Point(0, 0);
            if (textBoxX.Text.Length == 0 || Convert.ToInt32(textBoxX.Text) < 5) textBoxX.Text = "5";
            if (textBoxY.Text.Length == 0 || Convert.ToInt32(textBoxY.Text) < 4) textBoxY.Text = "4";
            X = Convert.ToInt32(textBoxX.Text);
            Y = Convert.ToInt32(textBoxY.Text);
            arrayField = new byte[X, Y];
            arrayOpen = new byte[X, Y];
            image = new Bitmap(21 * X, 21 * Y);
            field = Graphics.FromImage(image);
            if (domainUpDown2.Text == "обычный")
                Mode = 0;
            else if (domainUpDown2.Text == "зеркальный")
                Mode = 1;
            else if (domainUpDown2.Text == "зацыклиный")
                Mode = 2;
            else if (domainUpDown2.Text == "без флагов")
                Mode = 3;
            if (Mode == 2)
            {
                image1 = new Bitmap(21 * X, 21 * Y);
                field1 = Graphics.FromImage(image1);
                t1 = new Thread(move);
            }
            if (domainUpDown1.Text == "хардкор")
                New = new MinesweeperGame(X, Y, X * Y / 4);
            else if (domainUpDown1.Text == "сложно")
                New = new MinesweeperGame(X, Y, X * Y / 5);
            else if (domainUpDown1.Text == "средне")
                New = new MinesweeperGame(X, Y, X * Y / 6);
            else
                New = new MinesweeperGame(X, Y, X * Y / 8);
            Brush p = new SolidBrush(Color.Gray);
            field.Clear(Color.White);
            for (int i = 0; i < X; i++)
                for (int j = 0; j < Y; j++)
                    field.FillRectangle(p, i * 21, j * 21, 20, 20);
            int MinimumX = image.Width, MinimumY = image.Height;
            if (MinimumX < 4110)
            {
                MinimumY = MinimumY * 4110 / MinimumX;
                MinimumX = 4110;
            }
            if (MinimumY < 1750)
            {
                MinimumX = MinimumX * 1750 / MinimumY;
                MinimumY = 1750;
            }
            this.MinimumSize = new Size(MinimumX / 10 + 40, MinimumY / 10 + 90);
            picture.Image = GraphicsWork.ImageResize(image, picture.Width, picture.Height);
            StartTime = Stopwatch.StartNew();
            if (Mode == 2)
                t1.Start();
        }
        private void textBoxX_TextChanged(object sender, EventArgs e)
        {
            string num = "1234567890";
            string text = null;
            for (int i = 0; i < textBoxX.Text.Length; i++)
                for (int j = 0; j < 10; j++)
                    if (textBoxX.Text[i] == num[j]) text += textBoxX.Text[i];
            if (Convert.ToInt32(text) > 180)
            {
                text = "180";
                textBoxX.Text = text;
            }
            else textBoxX.Text = text;
            textBoxX.SelectionStart = textBoxX.Text.Length;
        }
        private void textBoxY_TextChanged(object sender, EventArgs e)
        {
            string num = "1234567890";
            string text = null;
            for (int i = 0; i < textBoxY.Text.Length; i++)
                for (int j = 0; j < 10; j++)
                    if (textBoxY.Text[i] == num[j]) text += textBoxY.Text[i];
            if (Convert.ToInt32(text) > 90)
            {
                text = "90";
                textBoxY.Text = text;
            }
            else textBoxY.Text = text;
            textBoxY.SelectionStart = textBoxY.Text.Length;
        }
        private void picture_MouseDown(object sender, MouseEventArgs e)
        {
            if (arrayField != null)
            {
                int x = X, y = Y;
                lock (Lock)
                {
                    for (int i = 0; i < X; i++)
                        if ((e.Location.X - (beginning.X * 1.0 * picture.Image.Width / image.Width) + picture.Image.Width) % picture.Image.Width > i * (double)(picture.Image.Width * 1.0 / X) && (e.Location.X - (beginning.X * 1.0 * picture.Image.Width / image.Width) + picture.Image.Width) % picture.Image.Width <= (i + 1) * (double)(picture.Image.Width * 1.0 / X))
                            x = i;
                    for (int i = 0; i < Y; i++)
                        if ((e.Location.Y - (beginning.Y * 1.0 * picture.Image.Height / image.Height) + picture.Image.Height) % picture.Image.Height > i * (double)(picture.Image.Height * 1.0 / Y) && (e.Location.Y - (beginning.Y * 1.0 * picture.Image.Height / image.Height) + picture.Image.Height) % picture.Image.Height <= (i + 1) * (double)(picture.Image.Height * 1.0 / Y))
                            y = i;
                }
                if (Mode == 1)
                {
                    x = X - 1 - x;
                    y = Y - 1 - y;
                }
                if (e.Location.X <= picture.Image.Width && e.Location.Y <= picture.Image.Height)
                {
                    if (e.Button == MouseButtons.Right && Mode != 3)
                    {
                        if (Game)
                        {
                            if (arrayOpen[x, y] == 0)
                            {
                                lock (Lock)
                                    field.DrawImage(Properties.Resources._10, x * 21, y * 21);
                                arrayOpen[x, y] = 1;
                            }
                            else if (arrayOpen[x, y] == 1)
                            {
                                Brush p = new SolidBrush(Color.Gray);
                                lock (Lock)
                                    field.FillRectangle(p, x * 21, y * 21, 20, 20);
                                arrayOpen[x, y] = 0;
                            }
                            if (Mode != 2)
                                picture.Image = GraphicsWork.ImageResize(image, picture.Width, picture.Height);
                        }
                    }
                    if (e.Button == MouseButtons.Left)
                    {
                        if (New.chek)
                        {
                            if (Game)
                            {
                                if (arrayOpen[x, y] == 0)
                                {
                                    lock (Lock)
                                        if (arrayField[x, y] == 0)
                                        {
                                            Brush p = new SolidBrush(Color.White);
                                            field.FillRectangle(p, x * 21, y * 21, 20, 20);
                                            arrayOpen[x, y] = 2;
                                            OpenZero();
                                        }
                                        else if (arrayField[x, y] == 1)
                                            field.DrawImage(Properties.Resources._1, x * 21, y * 21);
                                        else if (arrayField[x, y] == 2)
                                            field.DrawImage(Properties.Resources._2, x * 21, y * 21);
                                        else if (arrayField[x, y] == 3)
                                            field.DrawImage(Properties.Resources._3, x * 21, y * 21);
                                        else if (arrayField[x, y] == 4)
                                            field.DrawImage(Properties.Resources._4, x * 21, y * 21);
                                        else if (arrayField[x, y] == 5)
                                            field.DrawImage(Properties.Resources._5, x * 21, y * 21);
                                        else if (arrayField[x, y] == 6)
                                            field.DrawImage(Properties.Resources._6, x * 21, y * 21);
                                        else if (arrayField[x, y] == 7)
                                            field.DrawImage(Properties.Resources._7, x * 21, y * 21);
                                        else if (arrayField[x, y] == 8)
                                            field.DrawImage(Properties.Resources._8, x * 21, y * 21);
                                        else if (arrayField[x, y] == 9)
                                        {
                                            field.DrawImage(Properties.Resources._9, x * 21, y * 21);
                                            GameOver();
                                        }
                                    if (Mode != 2)
                                        picture.Image = GraphicsWork.ImageResize(image, picture.Width, picture.Height);
                                    arrayOpen[x, y] = 2;
                                }
                                else if (arrayOpen[x, y] == 2)
                                {
                                    OpenNum(x, y);
                                }
                            }
                        }
                        else
                        {
                            if (arrayOpen[x, y] == 0)
                            {
                                arrayField = New.NewFiled(x, y, (Mode == 2));
                                lock (Lock)
                                    if (arrayField[x, y] == 0)
                                    {
                                        Brush p = new SolidBrush(Color.White);
                                        field.FillRectangle(p, x * 21, y * 21, 20, 20);
                                        arrayOpen[x, y] = 2;
                                        OpenZero();
                                    }
                                    else if (arrayField[x, y] == 1)
                                        field.DrawImage(Properties.Resources._1, x * 21, y * 21);
                                    else if (arrayField[x, y] == 2)
                                        field.DrawImage(Properties.Resources._2, x * 21, y * 21);
                                    else if (arrayField[x, y] == 3)
                                        field.DrawImage(Properties.Resources._3, x * 21, y * 21);
                                    else if (arrayField[x, y] == 4)
                                        field.DrawImage(Properties.Resources._4, x * 21, y * 21);
                                    else if (arrayField[x, y] == 5)
                                        field.DrawImage(Properties.Resources._5, x * 21, y * 21);
                                    else if (arrayField[x, y] == 6)
                                        field.DrawImage(Properties.Resources._6, x * 21, y * 21);
                                    else if (arrayField[x, y] == 7)
                                        field.DrawImage(Properties.Resources._7, x * 21, y * 21);
                                    else if (arrayField[x, y] == 8)
                                        field.DrawImage(Properties.Resources._8, x * 21, y * 21);
                                if (Mode != 2)
                                    picture.Image = GraphicsWork.ImageResize(image, picture.Width, picture.Height);
                                arrayOpen[x, y] = 2;
                            }
                        }
                    }
                    Finish();
                }
            }
        }
        private void Form_SizeChanged(object sender, EventArgs e)
        {
            if (picture.Image != null)
            {
                if (Mode != 2)
                    picture.Image = GraphicsWork.ImageResize(image, picture.Width, picture.Height);
            }
        }
        private void OpenZero()
        {
            bool time = true;
            while (time)
            {
                time = false;
                for (int i = 0; i < X; i++)
                    for (int j = 0; j < Y; j++)
                    {
                        if (arrayField[i, j] == 0 && arrayOpen[i, j] == 2)
                        {
                            for (int i1 = -1; i1 < 2; i1++)
                                for (int j1 = -1; j1 < 2; j1++)
                                    if (Mode == 2)
                                    {
                                        if (arrayOpen[(i + i1 + X) % X, (j + j1 + Y) % Y] != 2)
                                        {
                                            time = true;
                                            arrayOpen[(i + i1 + X) % X, (j + j1 + Y) % Y] = 2;
                                        }
                                    }
                                    else
                                    {
                                        if (i + i1 >= 0 && i + i1 < X && j + j1 >= 0 && j + j1 < Y)
                                            if (arrayOpen[i + i1, j + j1] != 2)
                                            {
                                                time = true;
                                                arrayOpen[i + i1, j + j1] = 2;
                                            }
                                    }
                        }
                    }
            }
            for (int i = 0; i < X; i++)
                for (int j = 0; j < Y; j++)
                    if (arrayOpen[i, j] == 2)
                    {
                        lock (Lock)
                            if (arrayField[i, j] == 0)
                            {
                                Brush p = new SolidBrush(Color.White);
                                field.FillRectangle(p, i * 21, j * 21, 20, 20);
                            }
                            else if (arrayField[i, j] == 1)
                                field.DrawImage(Properties.Resources._1, i * 21, j * 21);
                            else if (arrayField[i, j] == 2)
                                field.DrawImage(Properties.Resources._2, i * 21, j * 21);
                            else if (arrayField[i, j] == 3)
                                field.DrawImage(Properties.Resources._3, i * 21, j * 21);
                            else if (arrayField[i, j] == 4)
                                field.DrawImage(Properties.Resources._4, i * 21, j * 21);
                            else if (arrayField[i, j] == 5)
                                field.DrawImage(Properties.Resources._5, i * 21, j * 21);
                            else if (arrayField[i, j] == 6)
                                field.DrawImage(Properties.Resources._6, i * 21, j * 21);
                            else if (arrayField[i, j] == 7)
                                field.DrawImage(Properties.Resources._7, i * 21, j * 21);
                            else if (arrayField[i, j] == 8)
                                field.DrawImage(Properties.Resources._8, i * 21, j * 21);
                    }
            if (Mode != 2)
                picture.Image = GraphicsWork.ImageResize(image, picture.Width, picture.Height);
        }
        private void OpenNum(int x, int y)
        {
            byte k = 0;
            for (int i = -1; i < 2; i++)
                for (int j = -1; j < 2; j++)
                {
                    if (Mode == 2)
                    {
                        if (arrayOpen[(x + i + X) % X, (y + j + Y) % Y] == 1) k++;
                    }
                    else
                    {
                        if (x + i >= 0 && x + i < X && y + j >= 0 && y + j < Y)
                            if (arrayOpen[x + i, y + j] == 1) k++;
                    }
                }
            if (arrayField[x, y] == k)
            {
                for (int i = -1; i < 2; i++)
                    for (int j = -1; j < 2; j++)
                    {
                        if (Mode == 2)
                        {
                            if (arrayOpen[(x + i + X) % X, (y + j + Y) % Y] != 1)
                            {
                                arrayOpen[(x + i + X) % X, (y + j + Y) % Y] = 2;
                                if (arrayField[(x + i + X) % X, (y + j + Y) % Y] == 9)
                                    GameOver();
                            }
                        }
                        else
                        {
                            if (x + i >= 0 && x + i < X && y + j >= 0 && y + j < Y)
                            {
                                if (arrayOpen[x + i, y + j] != 1)
                                {
                                    arrayOpen[x + i, y + j] = 2;
                                    if (arrayField[x + i, y + j] == 9)
                                        GameOver();
                                }
                            }
                        }
                    }
                OpenZero();
                if (Mode != 2)
                    picture.Image = GraphicsWork.ImageResize(image, picture.Width, picture.Height);
            }
        }
        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (t1 != null)
                t1.Abort();
        }
        private void Finish()
        {
            bool finish = true;
            for (int i = 0; i < X; i++)
                for (int j = 0; j < Y; j++)
                {
                    if (arrayField[i, j] < 9 && arrayOpen[i, j] != 2)
                        finish = false;
                }
            if (finish)
            {
                StartTime.Stop();
                Game = false;
                MessageBox.Show("победа! ваше время: " + StartTime.Elapsed);
                if (t1 != null)
                    t1.Abort();
            }
        }
        private void GameOver()
        {
            if (t1 != null)
                t1.Abort();
            Game = false;
            for (int i = 0; i < X; i++)
                for (int j = 0; j < Y; j++)
                    if (arrayField[i, j] == 9)
                    {
                        arrayOpen[i, j] = 2;
                        field.DrawImage(Properties.Resources._9, i * 21, j * 21);
                    }
            picture.Image = GraphicsWork.ImageResize(image, picture.Width, picture.Height);
        }
        private void move()
        {
            Point beginning1 = new Point(0, 0);
            Random rand = new Random();
            Point prevSpeed = new Point(0, 0);
            while (true)
            {
                Point speed = new Point(rand.Next(-80, 81), rand.Next(-80, 81));
                while (true)
                {
                    if (prevSpeed.X < speed.X) prevSpeed.X++;
                    if (prevSpeed.X > speed.X) prevSpeed.X--;
                    if (prevSpeed.Y < speed.Y) prevSpeed.Y++;
                    if (prevSpeed.Y > speed.Y) prevSpeed.Y--;
                    lock (Lock)
                    {
                        beginning1.X = (beginning1.X + prevSpeed.X + image.Width * 100) % (image.Width * 100);
                        beginning1.Y = (beginning1.Y + prevSpeed.Y + image.Height * 100) % (image.Height * 100);
                        beginning.X = beginning1.X / 100;
                        beginning.Y = beginning1.Y / 100;
                        field1.DrawImage(image, beginning);
                        field1.DrawImage(image, beginning.X - image.Width, beginning.Y);
                        field1.DrawImage(image, beginning.X, beginning.Y - image.Height);
                        field1.DrawImage(image, beginning.X - image.Width, beginning.Y - image.Height);
                        picture.Image = GraphicsWork.ImageResize(image1, picture.Width, picture.Height);
                    }
                    Thread.Sleep(10);
                    if (prevSpeed.X == speed.X && prevSpeed.Y == speed.Y) break;
                }
                for (int i = 0; i < rand.Next(50, 201); i++)
                {
                    lock (Lock)
                    {
                        beginning1.X = (beginning1.X + prevSpeed.X + image.Width * 100) % (image.Width * 100);
                        beginning1.Y = (beginning1.Y + prevSpeed.Y + image.Height * 100) % (image.Height * 100);
                        beginning.X = beginning1.X / 100;
                        beginning.Y = beginning1.Y / 100;
                        field1.DrawImage(image, beginning);
                        field1.DrawImage(image, beginning.X - image.Width, beginning.Y);
                        field1.DrawImage(image, beginning.X, beginning.Y - image.Height);
                        field1.DrawImage(image, beginning.X - image.Width, beginning.Y - image.Height);
                        picture.Image = GraphicsWork.ImageResize(image1, picture.Width, picture.Height);
                    }
                    Thread.Sleep(10);
                }
            }
        }
    }
}