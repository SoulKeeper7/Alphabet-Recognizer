using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        Image img;
        double score = 0.0;
        Bitmap bitmap;
        Graphics g;
        Graphics h;
        int a, b,linenumbcount;
        bool[,] visited;
        byte[,] temp,forkmap;
        double[,] lineregion;
        GraphicsState start;
       
        Queue<Point> nodes = new Queue<Point>();
        Queue<Point> queue = new Queue<Point>();
        Queue<Point> path =new Queue<Point>();
         Queue<Point> fork =new Queue<Point>();
        Stack<Point> dirstack = new Stack<Point>();
        Stack<Point> sl2 = new Stack<Point>();
        Stack<Point> line1,line2,line3,line4,line5;
        
        double sx=0, sx2=0, sxy=0, sy=0,rescount=0;
        double resa = 0, resb = 0;
        Point thelastpoint = new Point();
        public Form1()
        {
            InitializeComponent();
            bitmap = new Bitmap(panel1.Width, panel1.Height);
            img = bitmap;
            panel1.BackgroundImage = img;
            g = Graphics.FromImage(img);
            h = panel1.CreateGraphics();
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            label1.Text = Convert.ToString(e.X);
            label2.Text = Convert.ToString(e.Y);
            if (e.Button == MouseButtons.Left)
            {
                Pen p = new Pen(Color.Black, 21);
         
                g.DrawLine(p, e.X, e.Y, a, b);
                g.FillRectangle(new SolidBrush(Color.Black), e.X - 10f, e.Y - 10f, 21f, 21f);
                h.DrawLine(p, e.X, e.Y, a, b);
                h.FillRectangle(new SolidBrush(Color.Black), e.X - 10f, e.Y - 10f, 21f, 21f);
                a = e.X;
                b = e.Y;
                p.Dispose();
                

            }
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
           
            a = e.X;
            b = e.Y;
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            panel1.Invalidate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            linenumbcount=0;
            sx = 0; sx2 = 0; sxy = 0; sy = 0; resa = 0; resb = 0; rescount = 0; score = 0.0;
            path.Clear();
            fork.Clear();
            dirstack.Clear();
            //h.DrawLine(Pens.Blue, 500, 500, 167,290);
            
            button1.Text = "processing...";
           

            button1.Refresh();
            processbitmap();
            //serialise the data//
           // Stream stream = File.Open("test.txt", FileMode.Create);
           // BinaryFormatter bFormatter = new BinaryFormatter();
           // bFormatter.Serialize(stream, path);
           // stream.Close();
           // button1.Text = "all set";
            Point firstpoint = new Point();

            foreach (Point element in path)
                 {
                     if (element.X > firstpoint.X)
                        firstpoint = element;
                     if (element.Y == firstpoint.Y)
                         if (element.X < firstpoint.X)
                            firstpoint = element;
                 }
            post_path(firstpoint,1,1);
            resa = (double)((double)sx * sxy - sx2 * sy) / (double)((double)rescount * sx2 - (sx * sx));
            resb = (double)((double)rescount * sxy - sx * sy) / (double)((double)rescount*sx2-(sx*sx));
           // label15.Text = "y = " + Convert.ToString(Math.Round(1 / resb, 5)) + "x + " + Convert.ToString(Math.Round(resa, 3));//y=bx+a
            label15.Text = Convert.ToString(Math.Round( Math.Atan(1 / resb)*180/3.1415926535,3))+ "     +      " + Convert.ToString(resa) ;
            
            score += 100 - Math.Abs(65 -Math.Abs( Math.Round(Math.Atan(1 / resb) * 180 / 3.1415926535, 3)));
            

           h.FillRectangle(Brushes.Gold, firstpoint.Y, firstpoint.X, 15, 15);
           fillline(1);
           firstpoint=findregionalpoint(90);
           h.FillRectangle(Brushes.DarkRed, firstpoint.Y, firstpoint.X, 15, 25);
           post_path(firstpoint, -1, 1);
           fillline(2);
           firstpoint = findregionalpoint(50);
           h.FillRectangle(Brushes.DarkOliveGreen, firstpoint.Y, firstpoint.X, 25, 15);
           post_path(firstpoint, -10, 1);
        }

        private unsafe void processbitmap()
        {

            
           
            System.Drawing.Imaging.BitmapData bmpd = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bitmap.PixelFormat);
            IntPtr ptr = bmpd.Scan0;
            int bytes = Math.Abs(bmpd.Stride) * bitmap.Height;
            byte[] datarray = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, datarray, 0, bytes);
            int width = Math.Abs(bmpd.Width);
            temp = new byte[bitmap.Height, width];
            forkmap = new byte[bitmap.Height, width];
            lineregion = new double[bitmap.Height, width];
            int counterh = 0, rootc = 0, rootr = 0;
            bool flag = new bool();
            int abc = bmpd.Width;
            visited = new bool[bitmap.Height, Width];
            for (counterh = 0; counterh < bitmap.Height; counterh++)
            {
                int counterw = 0;
                for (counterw = 0; counterw < width; counterw++)
                {
                    temp[counterh, counterw] = datarray[counterh * bmpd.Stride + (counterw * 4) + 3];
                    if (flag == false && temp[counterh, counterw] == 255)
                    {
                        rootc = counterw;
                        rootr = counterh;
                        flag = true;
                       queue.Enqueue(new Point(rootr, rootc));
                        
                       
                    }
                }
            }
            bitmap.UnlockBits(bmpd);
            Queue<Point> processed = new Queue<Point>();
            findnodes();



        }
        private unsafe void findnodes()
        {
           

            Point currentpoint = new Point();
            Stack<Point> s= new Stack<Point>();
            Queue<Point> boundary = new Queue<Point>();
            try
            {
                do
                {
                    while (queue.Count != 0)
                    {
                        int size = 1, i = 0, j = 0;

                        currentpoint = queue.Dequeue();

                        int count = 0;
                        int curx = currentpoint.X, cury = currentpoint.Y;
                        if (visited[curx, cury] == true) continue;



                    again: i = (-1) * size;


                        int tempcount = 0;
                        for (j = (-1) * size; j < size; j++)
                        {
                            if (curx + i >= 0 && cury + j >= 0)
                            {
                                if (temp[curx + i, cury + j] == 0 && temp[curx + i, cury + j + 1] == 255)
                                    count++;
                                if (temp[curx + i, cury + j] == 255)
                                {
                                    boundary.Enqueue(new Point(curx + i, cury + j));

                                    tempcount++;
                                }
                            }
                        }
                        j = size;

                        for (; i < size; i++)
                        {
                            if (curx + i >= 0 && cury + j >= 0)
                            {
                                if (temp[curx + i, cury + j] == 0 && temp[curx + i + 1, cury + j] == 255)
                                    count++;
                                if (temp[curx + i, cury + j] == 255)
                                {
                                    boundary.Enqueue(new Point(curx + i, cury + j));

                                    tempcount++;

                                }
                            }
                        }
                        for (; j > (-1) * size; j--)
                        {
                            if (curx + i >= 0 && cury + j >= 0)
                            {
                                if (temp[curx + i, cury + j] == 0 && temp[curx + i, cury + j - 1] == 255)
                                    count++;
                                if (temp[curx + i, cury + j] == 255)
                                {
                                    boundary.Enqueue(new Point(curx + i, cury + j));
                                    tempcount++;
                                }
                            }
                        }
                        for (; i > (-1) * size; i--)
                        {
                            if (curx + i >= 0 && cury + j >= 0)
                            {
                                if (temp[curx + i, cury + j] == 0 && temp[curx + i - 1, cury + j] == 255)
                                    count++;
                                if (temp[curx + i, cury + j] == 255)
                                {
                                    boundary.Enqueue(new Point(curx + i, cury + j));
                                    tempcount++;
                                }
                            }
                        }
                        if (count == 0)
                        {


                            while (boundary.Count != 0)
                                boundary.Dequeue();
                            size = size + 1;
                            goto again;
                        }

                        if (count == 1)
                        {
                            int atempcount = 0;
                            visited[curx, cury] = true;
                            count = 0;
                            size = size + 1;
                            i = (-1) * size;



                            for (j = (-1) * size; j < size; j++)
                            {
                                if (curx + i >= 0 && cury + j >= 0)
                                {
                                    if (temp[curx + i, cury + j] == 0 && temp[curx + i, cury + j + 1] == 255)
                                        count++;
                                    if (temp[curx + i, cury + j] == 255) atempcount++;
                                }
                            }
                            j = size;

                            for (; i < size; i++)
                            {
                                if (curx + i >= 0 && cury + j >= 0)
                                {
                                    if (temp[curx + i, cury + j] == 0 && temp[curx + i + 1, cury + j] == 255)
                                        count++;
                                    if (temp[curx + i, cury + j] == 255) atempcount++;

                                }
                            }
                            for (; j > (-1) * size; j--)
                            {
                                if (curx + i >= 0 && cury + j >= 0)
                                {
                                    if (temp[curx + i, cury + j] == 0 && temp[curx + i, cury + j - 1] == 255)
                                        count++;
                                    if (temp[curx + i, cury + j] == 255) atempcount++;
                                }
                            }
                            for (; i > (-1) * size; i--)
                            {
                                if (curx + i >= 0 && cury + j >= 0)
                                {
                                    if (temp[curx + i, cury + j] == 0 && temp[curx + i - 1, cury + j] == 255)
                                        count++;
                                    if (temp[curx + i, cury + j] == 255) atempcount++;
                                }
                            }
                            size = size - 1;
                            if (count == 1)
                            {
                                //   h.FillRectangle(Brushes.RoyalBlue, currentpoint.Y, currentpoint.X, 1, 1);
                                Point p = new Point();
                                // next line to track the line end points;
                                int factor = Convert.ToInt32(textBox1.Text);
                                
                               if (atempcount <= (factor* size)-5) h.FillRectangle(Brushes.RoyalBlue, currentpoint.Y, currentpoint.X, 5, 5);
                                while (boundary.Count != 0)
                                {
                                    p = boundary.Dequeue();
                                    queue.Enqueue(p);
                                }
                            }
                            if (count == 2)
                            {

                                path.Enqueue(new Point(curx, cury));
                                temp[curx, cury] = 200;

                                 h.FillRectangle(Brushes.Indigo, currentpoint.Y, currentpoint.X, 1, 1);
                                Point poi = new Point();
                                while (boundary.Count != 0)
                                {
                                    poi = boundary.Dequeue();
                                    if (visited[poi.X, poi.Y] == false)
                                        s.Push(poi);


                                }
                                visited[curx, cury] = true;


                            }

                            if (count > 2)
                            {
                                fork.Enqueue(new Point(curx, cury));
                                forkmap[curx, cury] = 1;
                                temp[curx, cury] = 200;
                                h.FillRectangle(Brushes.Silver, currentpoint.Y, currentpoint.X, 2, 2);
                                Point poi = new Point();
                                while (boundary.Count != 0)
                                {
                                    poi = boundary.Dequeue();
                                    if (visited[poi.X, poi.Y] == false)
                                        s.Push(poi);
                                }
                                visited[curx, cury] = true;
                            }
                        }
                        ////////////////////////////////////////////////
                        if (count == 2)
                        {

                            path.Enqueue(new Point(curx, cury));
                            temp[curx, cury] = 200;
                            //continuous path
                            h.FillRectangle(Brushes.Green, currentpoint.Y, currentpoint.X, 1, 1);
                            Point poi = new Point();
                            while (boundary.Count != 0)
                            {
                                poi = boundary.Dequeue();
                                if (visited[poi.X, poi.Y] == false)
                                    s.Push(poi);


                            }
                            visited[curx, cury] = true;


                        }

                        if (count > 2)
                        {
                            fork.Enqueue(new Point(curx, cury));
                            temp[curx, cury] = 200;
                            forkmap[curx, cury] = 1;
                            //fork
                            h.FillRectangle(Brushes.Red, currentpoint.Y, currentpoint.X, 5, 5);
                            Point poi = new Point();
                            while (boundary.Count != 0)
                            {
                                poi = boundary.Dequeue();
                                if (visited[poi.X, poi.Y] == false)
                                    s.Push(poi);
                            }
                            visited[curx, cury] = true;
                        }


                    }
                    Point t = new Point();
                    t = s.Pop();
                    queue.Enqueue(t);




                } while (s.Count != 0);
            }
            catch
            {
                MessageBox.Show("Error, maybe the drawing touches the boundary.");
            }
            button1.Text = "done";
         }

     

        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
           
                       
        }
        private void post_path(Point firstpoint,int x=1, int y=1)
        {
            linenumbcount++;
            rescount = 0;
            lineregion.Initialize();
            int flag = 1, i = -1, j = -1, size = Convert.ToInt32(textBox2.Text), maxmatch = 999, tolerance = Convert.ToInt32(textBox3.Text), maxmatchsum = 0, maxmatchcount = 0;
            int maximum = 0;
            dirstack.Clear();
            forkmap.Initialize();
            Point checkpixel, nextpoint = new Point();
            //  path.TrimExcess();
            // fork.TrimExcess();

            
            
            dirstack.Push(firstpoint);
            forkmap[firstpoint.X, firstpoint.Y] = 1;
            sl2.Push(firstpoint);
            while (sl2.Count != 0)
            {
                while (flag == 1)
                {
                    checkpixel = dirstack.Peek();
                    flag = 0;
                    //////////////////// simply typed during debugging//////////////// check properly!!!!/////////////////////////////
                    if (checkpixel.X == 0 || checkpixel.Y == 0)
                        break;
                    //////////////////// simply typed during debugging//////////////// check properly!!!!/////////////////////////////  ^  //  
                    //for(all black surrounding pixels) find best satisfying pixel for the eqn 2x - 3y = 0
                    for (j = (-1) * size; j < size; j++)
                    {
                        
                    //    h.FillRectangle(Brushes.Chocolate, checkpixel.Y + j, checkpixel.X + i, 1, 1);
                        if (temp[checkpixel.X + i, checkpixel.Y + j] == 200)
                        {

                            if (Math.Abs(x * i + y * j) <= Math.Abs(maxmatch))
                            {
                                maxmatch = (x * i + y * j);
                                nextpoint.X = checkpixel.X + i;
                                nextpoint.Y = checkpixel.Y + j;


                            }
                            if (Math.Abs(x * i + y * j) <= Math.Abs(tolerance))
                            {

                                Point pushpoint = new Point();
                                pushpoint.X = checkpixel.X + i;
                                pushpoint.Y = checkpixel.Y + j;
                                sl2.Push(pushpoint);

                            }


                        }

                    }
                    j = size;

                    for (i = -1 * size; i < size; i++)
                    {
                      //  h.FillRectangle(Brushes.Chocolate, checkpixel.Y + j, checkpixel.X + i, 1, 1);
                        if (temp[checkpixel.X + i, checkpixel.Y + j] == 200)
                        {

                            if (Math.Abs(x * i + y * j) <= Math.Abs(maxmatch))
                            {
                                maxmatch = (x * i + y * j);
                                nextpoint.X = checkpixel.X + i;
                                nextpoint.Y = checkpixel.Y + j;
                              

                            }
                            if (Math.Abs(x * i + y * j) <= Math.Abs(tolerance))
                            {

                                Point pushpoint = new Point();
                                pushpoint.X = checkpixel.X + i;
                                pushpoint.Y = checkpixel.Y + j;
                                sl2.Push(pushpoint);

                            }

                        }
                    }
                    for (j = size; j > (-1) * size; j--)
                    {
                      //  h.FillRectangle(Brushes.Chocolate, checkpixel.Y + j, checkpixel.X + i, 1, 1);
                        if (temp[checkpixel.X + i, checkpixel.Y + j] == 200)
                        {

                            if (Math.Abs(x * i + y * j) <= Math.Abs(maxmatch))
                            {
                                maxmatch = (x * i + y * j);
                                nextpoint.X = checkpixel.X + i;
                                nextpoint.Y = checkpixel.Y + j;
                                
                            }
                            if (Math.Abs(x * i + y * j) <= Math.Abs(tolerance))
                            {

                                Point pushpoint = new Point();
                                pushpoint.X = checkpixel.X + i;
                                pushpoint.Y = checkpixel.Y + j;
                                sl2.Push(pushpoint);

                            }

                        }

                    }
                    for (i = size; i > (-1) * size; i--)
                    {
                      //  h.FillRectangle(Brushes.Chocolate, checkpixel.Y + j, checkpixel.X + i, 1, 1);
                        if (temp[checkpixel.X + i, checkpixel.Y + j] == 200)
                        {

                            if (Math.Abs(x * i + y * j) <= Math.Abs(maxmatch))
                            {
                                maxmatch = (x * i + y * j);
                                nextpoint.X = checkpixel.X + i;
                                nextpoint.Y = checkpixel.Y + j;
                                
                            }
                            if (Math.Abs(x * i + y * j) <= Math.Abs(tolerance))
                            {

                                Point pushpoint = new Point();
                                pushpoint.X = checkpixel.X + i;
                                pushpoint.Y = checkpixel.Y + j;
                                sl2.Push(pushpoint);

                            }

                        }

                    }
                    if (Math.Abs(maxmatch) < Math.Abs(tolerance) & Convert.ToBoolean(cic()))//cc=check if nextpoint and checkpixel are connected
                    {
                        dirstack.Push(nextpoint);
                        forkmap[nextpoint.X, nextpoint.Y] = 1;
                        size = Convert.ToInt32(textBox2.Text);
                     
                        sx += nextpoint.X;
                        sx2 += (nextpoint.X * nextpoint.X);
                        sxy += (nextpoint.X * (nextpoint.Y));
                        sy += ( nextpoint.Y);
                        rescount++;
                        lineregion[nextpoint.X, nextpoint.Y] = rescount;
                        //h.FillRectangle(Brushes.DarkRed, nextpoint.Y, nextpoint.X,5, 5);
                        
                        mapac(checkpixel, size, 200, 201);//mark all points in the area for not visiting again,  200= previous value, 201= new value
                        //temp[checkpixel.X, checkpixel.Y] = 201;// mark only the pushed point as used
                        flag = 1;
                        maxmatchsum += maxmatch;
                        maxmatchcount++;
                        if (Math.Abs(maxmatch) >= Math.Abs(maximum))
                        {
                            maximum = maxmatch;
                            label13.Text = Convert.ToString(maximum);
                        }
                        maxmatch = 999;
                    }
                    else if (Math.Abs(maxmatch) >= Math.Abs(tolerance) & Convert.ToBoolean(cic()))
                    {
                        size--;
                        // continue;
                    }
                    //where are the already used being taken out of loop???
                    //how does continue effect the pp???




                   
                    if (maxmatchcount != 0)
                        label9.Text = Convert.ToString(maxmatchsum / maxmatchcount);

                    if (maxmatchcount == 0)
                        label9.Text = ("No point could be located that can satisfy the eqn");

          
                }
                
                Point pp = new Point();
                pp = sl2.Pop();
                dirstack.Push(pp);
                flag = 1;
            }
            label6.Text = Convert.ToString(dirstack.Count);
            fillline(linenumbcount);
           
           
        }

        private void fillline(int linenumb)
        {
            if (linenumb == 1)
            {
                line1 = new Stack<Point>();
                foreach (Point mn in dirstack)
                {
                    line1.Push(mn);
                    h.FillRectangle(Brushes.DarkRed, mn.Y, mn.X, 5, 5);
                }
            }
            else if (linenumb == 2)
            {
                line2 = new Stack<Point>();
                foreach (Point mn in dirstack)
                {
                    line2.Push(mn);
                    h.FillRectangle(Brushes.DeepPink, mn.Y, mn.X, 5, 5);
                }
                }
            else if (linenumb == 3)
            {
                line3 = new Stack<Point>();
                foreach (Point mn in dirstack)
                {
                    line3.Push(mn);
                    h.FillRectangle(Brushes.Purple, mn.Y, mn.X, 5, 5);
                }
                }
            else if (linenumb == 4)
            {
                line4 = new Stack<Point>();
                foreach (Point mn in dirstack)
                    line4.Push(mn);
            }
            else if (linenumb == 5)
            {
                line5 = new Stack<Point>();
                foreach (Point mn in dirstack)
                    line5.Push(mn);
            }
            dirstack.Clear();
        }

        private int cic()
        {
            return (1);
        }

        private void mapac(Point checkpixel , int size , int previousvalue , int newvalue)
        {
           
            
                while (size > 0)
                {
                    int i = (-1) * size, j = (-1) * size;
                    try
                     {

                    for (j = (-1) * size; j < size; j++)
                    {


                        if (temp[checkpixel.X + i, checkpixel.Y + j] == previousvalue)
                        {
                            temp[checkpixel.X + i, checkpixel.Y + j] = (byte)newvalue;
                            lineregion[checkpixel.X + i, checkpixel.Y + j] = rescount;
                           // h.FillRectangle(Brushes.BurlyWood, checkpixel.Y + j, checkpixel.X + i, 1, 1);
                        }

                    }
                    j = size;

                    for (i=(-1)*size; i < size; i++)
                    {

                        if (temp[checkpixel.X + i, checkpixel.Y + j] == previousvalue)
                        {
                            temp[checkpixel.X + i, checkpixel.Y + j] = (byte)newvalue;
                            lineregion[checkpixel.X + i, checkpixel.Y + j] = rescount;
                            //h.FillRectangle(Brushes.BurlyWood, checkpixel.Y + j, checkpixel.X + i, 1, 1);
                        }
                    }
                    i = size;
                    for (j=size; j > (-1) * size; j--)
                    {

                        if (temp[checkpixel.X + i, checkpixel.Y + j] == previousvalue)
                        {
                            temp[checkpixel.X + i, checkpixel.Y + j] = (byte)newvalue;
                            lineregion[checkpixel.X + i, checkpixel.Y + j] = rescount;
                           // h.FillRectangle(Brushes.BurlyWood, checkpixel.Y + j, checkpixel.X + i, 1, 1);
                        }

                    }
                    j = -1 * size;
                    for (i=size; i > (-1) * size; i--)
                    {

                        if (temp[checkpixel.X + i, checkpixel.Y + j] == previousvalue)
                        {
                            temp[checkpixel.X + i, checkpixel.Y + j] = (byte)newvalue;
                            lineregion[checkpixel.X + i, checkpixel.Y + j] = rescount;//mark the regions for identifying the line portion
                            //h.FillRectangle(Brushes.BurlyWood, checkpixel.Y + j, checkpixel.X + i, 1, 1);
                        }

                    }
                    i = -1 * size;
                    size--;
                }
                    catch
                    {
                        continue;
                    }

                    temp[checkpixel.X, checkpixel.Y] = (byte)newvalue;
                    lineregion[checkpixel.X + i, checkpixel.Y + j] = rescount;//mark the regions



                }
            
           

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

            panel1.Update();
            panel1.Refresh();
         
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            start = h.Save();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            thelastpoint = findregionalpoint(75);

            h.FillRectangle(Brushes.HotPink, thelastpoint.Y, thelastpoint.X, 15, 15);
            mapac(thelastpoint, Convert.ToInt32(textBox2.Text), 201, 200);
            sx = 0; sx2 = 0; sxy = 0; sy = 0; resa = 0; resb = 0; rescount = 0;
            post_path(thelastpoint);
            resa = (double)((double)sx * sxy - sx2 * sy) / (double)((double)rescount * sx2 - (sx * sx));
            resb = (double)((double)rescount * sxy - sx * sy) / (double)((double)rescount * sx2 - (sx * sx));
            label15.Text += "\n" + Convert.ToString(Math.Round(Math.Atan(1 / resb) * 180 / 3.1415926535, 3)) + "     +      " + Convert.ToString(resa);
            try
            {
                h.DrawLine(Pens.IndianRed, Convert.ToInt32(-resa / resb) + 20, -20, 20, -20 + Convert.ToInt32(resa));

            }

            catch { }
            score += 100 - Math.Abs((-65 + Math.Round(Math.Atan(1 / resb) * 180 / 3.1415926535, 3)));
            label16.Text = Convert.ToString(score)+"  " + score/2 + " % ";
        }

        private Point findregionalpoint(int point)
        {
            Point selectedpoint = new Point();
            int falggg = 0;
            foreach (Point mn in fork)
            {
                
                if (forkmap[mn.X, mn.Y] == 1)
                {

                    label17.Text = label17.Text + "\n" + Convert.ToString(mn.X) + "  " + Convert.ToString(mn.Y);

                    if ((100 - Math.Abs((point - lineregion[mn.X, mn.Y] * 100 / rescount))) >= (100 - Math.Abs((point - lineregion[selectedpoint.X, selectedpoint.Y] * 100 / rescount))))
                    {
                        selectedpoint = mn;
                        falggg = 1;
                    }
                }
            }
            if (falggg == 1)
            {

                h.FillRectangle(Brushes.DarkGoldenrod, selectedpoint.Y, selectedpoint.X,5, 5);
                label18.Text += "\n" + lineregion[selectedpoint.X, selectedpoint.Y] + "/" + rescount +" = "+ lineregion[selectedpoint.X, selectedpoint.Y] *100 /rescount + "selected point\n";
                return selectedpoint;
            }
            else
            {
                h.FillRectangle(Brushes.Brown, selectedpoint.Y, selectedpoint.X, 5, 5);
                label18.Text += "\n" + lineregion[selectedpoint.X, selectedpoint.Y]  + " the last point\n";
                return (thelastpoint);
            }
        }
  
        
      
    }
}
