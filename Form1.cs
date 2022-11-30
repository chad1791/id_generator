using MySqlX.XDevAPI.Relational;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace id_gen
{
    public partial class Form1 : Form
    {
        DataSet students, classes;
        int s_index;
        int s_limit;
        string connection, hrSaveDir, sGlobalId;
        readonly List<Bitmap> single = new();
        readonly List<Bitmap> multi = new();

        //get template for the id
        Bitmap raw = new(@"E:\hs files\students\20223.png");

        //keys to calculate the width of the names
        Dictionary<char, int> keys = new()
        {
            {'a', 110},
            {'b', 130},
            {'c', 115},
            {'d', 115},
            {'e', 115},
            {'f', 90},
            {'g', 130},
            {'h', 120},
            {'i', 70},
            {'j', 70},
            {'k', 115},
            {'l', 65},
            {'m', 175},
            {'n', 130},
            {'o', 135},
            {'p', 125},
            {'q', 125},
            {'r', 105},
            {'s', 125},
            {'t', 95},
            {'u', 135},
            {'v', 130},
            {'w', 160},
            {'x', 120},
            {'y', 130},
            {'z', 110},
            {'A', 120},
            {'B', 150},
            {'C', 160},
            {'D', 160},
            {'E', 140},
            {'F', 140},
            {'G', 170},
            {'H', 160},
            {'I', 80},
            {'J', 120},
            {'K', 165},
            {'L', 140},
            {'M', 180},
            {'N', 130},
            {'O', 170},
            {'P', 145},
            {'Q', 170},
            {'R', 150},
            {'S', 140},
            {'T', 150},
            {'U', 120},
            {'V', 160},
            {'W', 210},
            {'X', 150},
            {'Y', 160},
            {'Z', 160},
            {' ', 10},
            {'.', 60},
            {'-', 80}
        };

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //create a dataset for students, classes
            students = new DataSet();
            classes = new DataSet();

            //define connection parameter
            connection = "Datasource=localhost;port=3306;username=root;password=;database=hs_students";

            //define queries for students and classes
            string qStudents = "SELECT * FROM students";
            string qClasses = "SELECT DISTINCT homeroom FROM students ORDER BY homeroom ASC;";

            //create an instance of the dbManager class
            dbManager myDb = new(connection);

            //test connection
            if (myDb.testDB(qStudents))
            {
                MessageBox.Show("Connection to database was successful!");
            } else
            {
                MessageBox.Show("Error connecting to the database!");
            }

            //fetch classes data from the database
            classes = myDb.getClasses(qClasses);

            //link classes dataset to classes combobox;
            comboBox1.DataSource = classes.Tables[0];
            comboBox1.DisplayMember = "homeroom";

            //populate the groupbox with the initial list of students
            string? currentHm = comboBox1.Text;
            string? qStdByHm = "SELECT * FROM students WHERE homeroom='" + currentHm + "';";
            students = myDb.getStudentsByHomeroom(qStdByHm);
            generateStudentControls(students);

            //assign global student index
            s_index = 0;
            s_limit = students.Tables[0].Rows.Count;

            //load first student to the id view..
            redrawId();

            //create class directory to save the student id
            foreach(DataRow? row in classes.Tables[0].Rows)
            {
                string? homeroom = row[0]?.ToString();
                createClassDirectory(homeroom);
            }            

        }

        private void button2_Click(object sender, EventArgs e)
        {
            s_limit = students.Tables[0].Rows.Count;
            s_index++;

            if (s_index <= s_limit - 1)
            {
                //load next student to the id view..
                //redrawId();
            }

            if (s_index >= s_limit - 1)
            {
                s_index = s_limit - 1;
                MessageBox.Show("No more students in the database");
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            s_limit = students.Tables[0].Rows.Count;
            s_index--;

            if (s_index <= 0)
            {
                s_index = 0;
                MessageBox.Show("No more students in the database");
            }
        }

        public void generateStudentControls(DataSet studentsByClass)
        {
            int sgnx = 11;
            int sgny = 0;  //add 25 on each y
            //height = 15, width = 60; default values

            flowLayoutPanel1.Controls.Clear();

            foreach (DataRow row in studentsByClass.Tables[0].Rows)
            {
                //create panel group container for each student
                Panel studentNameGroup = new()
                {
                    BackColor = Color.FromArgb(33, 31, 44),
                    BorderStyle = BorderStyle.FixedSingle
                };
                flowLayoutPanel1.Controls.Add(studentNameGroup);
                studentNameGroup.Tag = row[0]?.ToString();
                studentNameGroup.SetBounds(sgnx, sgny, 405, 60);                

                //increase the value of y for the other student name containter
                sgny += 75;

                //create click event for each Student Name Group
                studentNameGroup.Click += StudentNameGroup_Click;
                studentNameGroup.MouseEnter += StudentNameGroup_MouseHover;
                studentNameGroup.MouseLeave += StudentNameGroup_MouseLeave;

                //absolut coordinates for student names
                int snx = 19;
                int sny = 8;

                //create name labels for each student
                Label stdName = new()
                {
                    Text = toSentenceCase(row[1].ToString()) + " " + toSentenceCase(row[3].ToString()),
                    //BackColor = Color.FromArgb(33, 31, 44),
                    BackColor = Color.Transparent,
                    BorderStyle = BorderStyle.None,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 14.0f, FontStyle.Bold)  //00925
                };
                stdName.SetBounds(snx, sny, 250, 25);
                stdName.Click += ChildClick;
                stdName.MouseEnter += Child_MouseHover;
                studentNameGroup.Controls.Add(stdName);


                //absolut coordinates for student names
                int sidx = 21;
                int sidy = 34;

                //create id labels for each student
                Label stdId = new()
                {
                    Text = row[0]?.ToString(),
                    //BackColor = Color.FromArgb(33, 31, 44),
                    BackColor = Color.Transparent,
                    BorderStyle = BorderStyle.None,
                    ForeColor = SystemColors.ButtonShadow,
                    Font = new Font("Segoe UI", 9.75f, FontStyle.Regular)
                };
                stdId.SetBounds(sidx, sidy, 78, 17); 
                stdId.Click += ChildClick;
                stdId.MouseEnter += Child_MouseHover;
                studentNameGroup.Controls.Add(stdId);


                //absolut coordinates for student homeroom - actual value
                int shrvx = 278;
                int shrvy = 11;

                //create homeroom labels for each student
                Label stdHrv = new()
                {
                    Text = row[4]?.ToString(),
                    //BackColor = Color.FromArgb(33, 31, 44),
                    BackColor= Color.Transparent,
                    BorderStyle = BorderStyle.None,
                    ForeColor = SystemColors.ButtonShadow,
                    Font = new Font("Segoe UI", 14.25f, FontStyle.Bold)
                };
                stdHrv.SetBounds(shrvx, shrvy, 45, 25);
                stdHrv.Click += ChildClick;
                stdHrv.MouseEnter += Child_MouseHover;
                studentNameGroup.Controls.Add(stdHrv);


                //absolut coordinates for student homeroom label
                int shrlblx = 262;
                int shrlbly = 32;

                //create homeroom labels for each student
                Label stdHrLbl = new()
                {
                    Text = "Homeroom",
                    //BackColor = Color.FromArgb(33, 31, 44),
                    BackColor= Color.Transparent,
                    BorderStyle = BorderStyle.None,
                    ForeColor = SystemColors.ButtonShadow,
                    Font = new Font("Segoe UI", 9.75f, FontStyle.Regular)
                };
                stdHrLbl.SetBounds(shrlblx, shrlbly, 75, 17);
                stdHrLbl.Click += ChildClick;
                stdHrLbl.MouseEnter += Child_MouseHover;
                studentNameGroup.Controls.Add(stdHrLbl);


                //absolut coordinates for student homeroom - actual value
                int dotsx = 378;
                int dotsy = 5;

                //create homeroom labels for each student
                Label dots = new()
                {
                    Text = "...",
                    //BackColor = Color.FromArgb(33, 31, 44),
                    BackColor= Color.Transparent,
                    BorderStyle = BorderStyle.None,
                    ForeColor = SystemColors.ButtonShadow,
                    Font = new Font("Segoe UI", 12.0f, FontStyle.Bold)
                };
                dots.SetBounds(dotsx, dotsy, 16, 42);
                studentNameGroup.Controls.Add(dots);
            }
        }

        private void StudentNameGroup_MouseLeave(object? sender, EventArgs e)
        {
            foreach (Control parent in flowLayoutPanel1.Controls)
            {
                if (parent.BackColor == Color.Teal)
                {
                    parent.BackColor = Color.FromArgb(33, 31, 44);
                    foreach (Control child in parent.Controls)
                    {
                        //child.BackColor = Color.FromArgb(33, 31, 44);
                        child.BackColor = Color.Transparent;
                    }
                }
            }
        }

        private void Child_MouseHover(object? sender, EventArgs e)
        {
            Control? ctl = sender as Control;
            Control? childPanel = ctl?.Parent;

            StudentNameGroup_MouseHover(childPanel, e);
        }

        private void StudentNameGroup_MouseHover(object? sender, EventArgs e)
        {

            foreach (Control parent in flowLayoutPanel1.Controls)
            {
                if(parent.BackColor == Color.Teal)
                {
                    parent.BackColor = Color.FromArgb(33, 31, 44);
                    foreach (Control child in parent.Controls)
                    {
                        //child.BackColor = Color.FromArgb(33, 31, 44);
                        child.BackColor = Color.Transparent;
                    }
                }
            }

            Panel? stdGr = sender as Panel;
            stdGr.BackColor = Color.Teal;

            foreach(Control s in stdGr.Controls)
            {
                s.BackColor = Color.Teal;
            }
        }

        private void ChildClick(object? sender, EventArgs e)
        {
            Control? ctl = sender as Control;
            Control? childPanel = ctl?.Parent;

            StudentNameGroup_Click(childPanel, e);
        }

        private void StudentNameGroup_Click(object? sender, EventArgs e)
        {
            Control? lb = sender as Control;

            //find the student record on the dataset...
            string? id = lb?.Tag?.ToString();

            //loop through the students records
            foreach (DataRow row1 in students.Tables[0].Rows)
            {
                string? stdId = row1[0].ToString();

                if (stdId == id)
                {
                    drawSingleStudentId(row1);
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //clear the list of students in the students dataset
            students.Clear();

            //get current class selected
            string currentHm = comboBox1.Text;

            //create an instance of the dbManager class
            dbManager myDb = new(connection);

            //define queries for students and classes
            string qStdByHm = "SELECT * FROM students WHERE homeroom='" + currentHm + "';";

            //fetch students by homeroom            
            students = myDb.getStudentsByHomeroom(qStdByHm);

            //generate a link label for each student
            generateStudentControls(students);

        }

        private void redrawId()
        {
            // get the first row on the current homeroom dataset
            DataRow row1 = students.Tables[0].Rows[s_index];

            string? id = row1[0].ToString();
            string? name = toSentenceCase(row1[1]?.ToString()) + " " + toSentenceCase(row1[3]?.ToString());
            string? homeroom = row1[4].ToString();
            

            //call function to generate the student id
            Bitmap singleStudent = displayStudentID(id, name, homeroom);

            //show image to the picture box first
            pictureBox1.Image = singleStudent;

        }

        /*private void loadStudentImage(string studentId)
        {
            if (!File.Exists(@"E:\hs files\students\" + studentId + ".jpg"))
            {
                _ = Image.FromFile(@"E:\hs files\students\1A\408210022.jpg");
            }
            else
            {
                //Image image = Image.FromFile(@"E:\hs files\students\" + studentId + ".jpg");

            }
        }*/

        private void button3_Click(object sender, EventArgs e)
        {
            foreach(DataRow row in students.Tables[0].Rows)
            {
                string? id = row[0].ToString();
                string? name = toSentenceCase(row[1].ToString()) + " " + toSentenceCase(row[3].ToString());
                string? homeroom = row[4].ToString();

                //create bitmap with student ids...
                Bitmap studentId = displayStudentID(id, name, homeroom);

                //save the student id to the homeroom directory...
                studentId.Save(@"E:\hs files\processed\" + homeroom + "\\" + id + ".png", ImageFormat.Png);

                //pictureBox1.Image = studentId;
            }
        }

        public void createClassDirectory(string homeroom)
        {
            //create directories for the different homerooms
            Directory.CreateDirectory(@"E:\hs files\processed\"+homeroom);
        }

        private void PrintDocument1_PrintPage(object sender, PrintPageEventArgs e)
        {

            //create bitmap files, raw image and modified size bitmap.
            Bitmap rawI = new(@"E:\hs files\students\20223.png");
            Bitmap processedI = new(207, 325, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            //use graphics to draw from raw to processed image with new heith and width
            Graphics g = Graphics.FromImage(processedI);
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(rawI, 0, 0, processedI.Width, processedI.Height);

            //draw processed image into the print document object to be printed
            e.Graphics?.DrawImage(processedI, 0, 0);

            rawI.Dispose();
            g.Dispose();
            processedI.Dispose();

        }

        private async void button4_Click(object sender, EventArgs e)
        {
            // get the bitmap image on the single list
            Bitmap singleStudent = single[0];

            //save the student id to the homeroom directory...
            singleStudent.Save(@"E:\hs files\processed\" + hrSaveDir + "\\" + sGlobalId + ".png", ImageFormat.Png);

            singleStudent.Dispose();
        }

        public Task drawSingleStudentId(DataRow d) 
        {
            return Task.Factory.StartNew(() => {

                //empty single list
                single.Clear();

                // assign values to the student data variables... 
                string? id = d[0].ToString();
                string? name = toSentenceCase(d[1].ToString()) + " " + toSentenceCase(d[3].ToString());
                string? homeroom = d[4].ToString();

                //create bitmap with student ids...
                Bitmap studentId = displayStudentID(id, name, homeroom);

                //show student id on the picturebox
                pictureBox1.Image = studentId;

                //set global variables for student id...
                hrSaveDir = homeroom;
                sGlobalId = id;

                //save the bitmap to a list of bitmaps...
                single.Add(studentId);

            });
        }

        public int getNameWidth(string name)
        {
            int width = 0;

            foreach(char l in name)
            {
                width += keys[l];
            }

            /*string nameLower = name.ToLower();
            width = keys[nameLower[0]];*/

            return width;
        }

        public Bitmap displayStudentID(string sid, string name, string homeroom)
        {
            #region id_template

                raw.Dispose();          
                raw = new Bitmap(@"E:\hs files\students\20223.png");

                //create graphics object of image to draw on..
                System.Drawing.Graphics canvas = System.Drawing.Graphics.FromImage(raw);

            #endregion

            #region student_image 

                //define a new pen for the circle
                Pen whitePen = new(Color.FromKnownColor(KnownColor.White), 50);
                Pen blackPen = new(Color.FromKnownColor(KnownColor.Black), 30);

                //black border around student image
                canvas.DrawEllipse(blackPen, 1610, 818, 1320, 1320);

                //white border around the student image
                canvas.DrawEllipse(whitePen, 1620, 830, 1300, 1300);

                //clean up memory for the brushes that draw the circle id
                whitePen.Dispose();
                blackPen.Dispose();

                try
                {
                    //crop the image extra edges and get the student image in circular shape
                    Bitmap student = new(@"E:\hs files\students\" + homeroom + "\\" + sid + ".jpg");
                    Bitmap resized = cropAtRect(student, new Rectangle(Convert.ToInt16(student.Width / 4), 200, (student.Width - student.Width / 2), student.Height - student.Height / 4));
                    Bitmap studentCircular = ClipToCircle(resized, new PointF(resized.Width / 2, resized.Height / 2), resized.Width / 2);

                    //add student image to template id
                    canvas.DrawImage(studentCircular, 1620, 830, 1300, 1300);

                    //clean up memory for the temporary bitmaps
                    student.Dispose();
                    resized.Dispose();
                    studentCircular.Dispose();

                }
                catch(Exception ex)
                {
                    //MessageBox.Show(ex.ToString());
                    MessageBox.Show("No image found for " + name + "\n" + ex.ToString());
                }

            #endregion

            #region student_name

                //determine the width of the id..
                int edge = 20;
                int id_width = raw.Width;
                Font s_font = new("Arial", 45, FontStyle.Bold);
                int s_nameW = getNameWidth(name);
                int s_nameH = 300;              
                int s_nameY = 2300;
                int s_nameX = 0;

            /*if (name.Length == 10)
            s_nameW += 100;*/

            //MessageBox.Show(name.Length.ToString());

                if (name.Length == 9)
                {
                    s_font = new Font("Arial", 44, FontStyle.Bold);
                    s_nameW += 60;
                }
                else
                if (name.Length == 10)
                {
                    s_font = new Font("Arial", 44, FontStyle.Bold);
                    s_nameW += 80;
                }
                else
                if (name.Length == 11)
                {
                    s_font = new Font("Arial", 43, FontStyle.Bold);
                    s_nameW += 20;
                }
                else
                if (name.Length > 11 && name.Length < 13)
                {
                    s_font = new Font("Arial", 45, FontStyle.Bold);
                    if (name.Length == 11)
                        s_nameW += 200;

                    if (name.Length == 12)
                        s_nameW += 50;
                }
                else
                if (name.Length >= 13 && name.Length < 20)
                {
                    s_font = new Font("Arial", 43, FontStyle.Bold);

                    if (name.Length == 14)
                        s_nameW -= 30;

                    if (name.Length == 15)
                        s_nameW -= 80;

                    if (name.Length == 16)
                        s_nameW -= 120;

                    if (name.Length == 17)
                        s_nameW -= 180;

                    if (name.Length >= 18 && name.Length <= 19)
                        s_nameW -= 150;
                }
                if (name.Length >= 20)
                {
                    s_font = new Font("Arial", 42, FontStyle.Bold);
                    edge = 10;

                    if (name.Length >= 21 && name.Length < 24)
                        s_nameW -= 150;

                    if (name.Length >= 24 && name.Length < 25)
                        s_nameW -= 200;

                    if (name.Length >= 25)
                    {
                        s_font = new Font("Arial", 40, FontStyle.Bold);
                        s_nameW -= 360;
                    }

                    if (name.Length >= 28)
                    {
                        s_font = new Font("Arial", 37, FontStyle.Bold);
                        edge -= 200;
                        //s_nameW -= 400;
                    }
                }

                //set the x coordinate for the name
                s_nameX = id_width - (s_nameW + edge);

                //SELECT * FROM `students` WHERE CHAR_LENGTH(CONCAT(first, " ", last)) = 18;
                drawString(new Rectangle(s_nameX, s_nameY, s_nameW, s_nameH), canvas, name, s_font, Brushes.White);

                //clean font object to release memory
                s_font.Dispose();

            #endregion

            #region student_class
                Font s_class = new("Arial", 50);
                drawString(new Rectangle(2300, 2550, 2000, 300), canvas, homeroom, s_class, Brushes.White);

                s_class.Dispose();
            #endregion

            #region id_number_text
                Font id_font = new("Agency FB", 44, FontStyle.Bold);
                drawString(new Rectangle(1970, 3650, 900, 300), canvas, "ID number", id_font, Brushes.Black);

                id_font.Dispose();
            #endregion

            #region create_&_add_barcode

               PictureBox barCode = new();

                try
                {
                    Zen.Barcode.Code128BarcodeDraw brCode = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum;
                    barCode.Image = brCode.Draw(sid, 100);
                    Bitmap bCode = (Bitmap)barCode.Image;

                    //add student image to template id
                    canvas.DrawImage(bCode, 1750, 3880, 1100, 600);

                    bCode.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            #endregion

            #region student_id
                StringBuilder lineBuilder = new StringBuilder();

                int count = 0;
                int size = sid.Length;

                while (count < size)
                {
                    lineBuilder.Append($"{sid[count]} ");
                    count++;
                }

                string id_w_spaces = lineBuilder.ToString();

                Font student_id = new("Agency FB", 40, FontStyle.Bold);
                drawString(new Rectangle(1715, 4480, 1200, 300), canvas, id_w_spaces, student_id, Brushes.Black);

                student_id.Dispose();
            #endregion

            #region h_name
                Font name_font = new("Agency FB", 34, FontStyle.Bold);
                drawString(new Rectangle(340, 3260, 900, 300), canvas, "Mr. Fidel Pol", name_font, Brushes.Black);

                name_font.Dispose();
            #endregion

            #region h_title
                Font title_font = new("Agency FB", 33, FontStyle.Bold);
                drawString(new Rectangle(318, 3405, 900, 300), canvas, "HEADMASTER", title_font, Brushes.Black);

                title_font.Dispose();
            #endregion

            #region clean_canvas_&_output_id

                //clean the canvas
                canvas.Dispose();

                //generate id for the student
                //raw.Save(@"E:\hs files\processed\"+sid+".png", ImageFormat.Png);

            #endregion

            return raw;
        }

        public void drawString(Rectangle r, Graphics canvas, String text, Font f, Brush b)
        {
            canvas.DrawString(text, f, b, r);
        }

        public Bitmap ClipToCircle(Bitmap original, PointF center, float radius)
        {
            Bitmap bm = new(original);
            Bitmap bt = new(bm.Width, bm.Height);
            Graphics g = Graphics.FromImage(bt);
            GraphicsPath gp = new();
            gp.AddEllipse(10, 10, bm.Width - 20, bm.Height - 20);
            g.Clear(Color.Magenta);
            g.SetClip(gp);
            g.DrawImage(bm, new Rectangle(0, 0, bm.Width, bm.Height), 0, 0, bm.Width, bm.Height, GraphicsUnit.Pixel);
            g.Dispose();
            bt.MakeTransparent(Color.Magenta);

            //clean up memory bitmaps
            bm.Dispose();
            g.Dispose();

            return bt;
        }

        public Bitmap cropAtRect(Bitmap b, Rectangle r)
        {
            Bitmap original = new(b);
            Bitmap bmpCrop = original.Clone(r, original.PixelFormat);

            //clean up memory bitmaps
            original.Dispose();

            return bmpCrop;
        }

        private void label2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            MessageBox.Show("I am button 7");
        }

        /*private void button5_Click(object sender, EventArgs e)
        {
            Bitmap student = new Bitmap(@"E:\hs files\students\1A\408210022.jpg");
            Bitmap resized = cropAtRect(student, new Rectangle(Convert.ToInt16(student.Width / 4), 0, (student.Width - student.Width /2), student.Height-student.Height/4));

            //generate id for the student
            resized.Save(@"E:\hs files\processed\student.png", ImageFormat.Png);

        }*/

        public string toSentenceCase(string s)
        {
            string sLower = s.ToLower();
            return sLower[0].ToString().ToUpper() + sLower.Substring(1);
        }
    }
}