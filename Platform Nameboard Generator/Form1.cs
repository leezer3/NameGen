using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;


namespace Platform_Nameboard_Generator
{
    public partial class Form1 : Form
    {
        
        //Holds the currently selected texture
        public string texture;
        public string reartexture;
        public Color textcolor;
        //Get the launch path
        public string launchpath = AppDomain.CurrentDomain.BaseDirectory;

        System.Drawing.SolidBrush textbrush = new System.Drawing.SolidBrush(Color.White);

        public static string GetTemporaryDirectory()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }
        public static string temppath = GetTemporaryDirectory();
        //Then attempt to load the default combobox values from textures.ini
        string[] defaulttextures;
        Dictionary<string, string> textures = new Dictionary<string, string>();
        Dictionary<string, string> reartextures = new Dictionary<string, string>();
        public Font selectedfont = new Font("Times New Roman", 36);

        public Form1()
        {
            InitializeComponent();
            //Initilise the image loader dialog
            openFileDialog1.Filter = "Image Files (*.bmp, *.png)|*.bmp;*.png";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.Title = "Select an Image";
            //openFileDialog1.InitialDirectory = launchpath;
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                defaulttextures = System.IO.File.ReadAllLines(launchpath + "textures.ini");
                for (int j = 0; j < defaulttextures.Length; j++)
                {
                    if (!defaulttextures[j].StartsWith(";"))
                    {
                        //This line isn't a comment
                        string[] input = defaulttextures[j].Split(',');
                        
                        if(input.Length == 2)
                        {
                            //Otherwise, add
                            textures.Add(input[0], input[1]);
                            comboBox1.Items.Add(input[0]);
                        }
                        else if (input.Length == 3)
                        {
                            //Otherwise, add
                            textures.Add(input[0], input[1]);
                            reartextures.Add(input[0], input[2]);
                            comboBox1.Items.Add(input[0]);
                        }
                        else
                        {
                            //Discard if the entry is invalid
                            MessageBox.Show("Invalid entry detected in textures.ini");
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("Error loading textures.ini");
            }
            try
            {
                if(!Directory.Exists(Path.Combine(launchpath + "\\Output\\Nameboards\\")))
                {
                    Directory.CreateDirectory(Path.Combine(launchpath + "\\Output\\Nameboards\\"));
                }
            }
            catch
            {
                MessageBox.Show("Unable to Write To Output Directory");
            }


        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Disable the leg length paramater if we are building wooden nameboards with legs in the image
            if ((string)comboBox1.SelectedItem == "GWR Wooden" || (string)comboBox1.SelectedItem == "SR Wooden")
            {
                //Disable legs boxes and fill the height and width
                numericUpDown3.Enabled = false;
                numericUpDown4.Enabled = false;
                numericUpDown1.Value = (decimal)6.0;
                numericUpDown2.Value = (decimal)3.0;
            }
            else
            {
                //Enable legs boxes and fill all values with defaults
                numericUpDown3.Enabled = true;
                numericUpDown4.Enabled = true;
                numericUpDown1.Value = (decimal)4.0;
                numericUpDown2.Value = (decimal)0.6;
                numericUpDown3.Value = (decimal)1.0;
                numericUpDown4.Value = (decimal)1.0;
            }
            //Check to see that the file is in our dictionary and load it
            if (textures.ContainsKey(Convert.ToString(comboBox1.SelectedItem)))
            {
                string fullpath;
                if (!Path.IsPathRooted(textures[Convert.ToString(comboBox1.SelectedItem)]))
                {
                    fullpath = launchpath + textures[Convert.ToString(comboBox1.SelectedItem)];   
                }
                else
                {
                    fullpath = textures[Convert.ToString(comboBox1.SelectedItem)];
                }
                try
                {
                    Bitmap tempimage;
                    using (FileStream myStream = new FileStream(fullpath, FileMode.Open))
                    {
                        tempimage = (Bitmap)Image.FromStream(myStream);

                    }
                    tempimage.MakeTransparent(Color.FromArgb(0, 0, 255));
                    this.pictureBox1.Image = tempimage;
                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                }
                catch
                {
                    MessageBox.Show("An error occured loading the selected image. Please check that it exists and is readable");
                }
            }
            if (reartextures.ContainsKey(Convert.ToString(comboBox1.SelectedItem)))
            {
                string rearpath;
                if (!Path.IsPathRooted(reartextures[Convert.ToString(comboBox1.SelectedItem)]))
                {
                    rearpath = launchpath + reartextures[Convert.ToString(comboBox1.SelectedItem)];   
                }
                else
                {
                    rearpath = reartextures[Convert.ToString(comboBox1.SelectedItem)];
                }
                reartexture = rearpath;
            }
            updatepreview();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    updatepreview();
                    //Append the filename [No extension to the list of possible candidates]
                    string filename = Path.GetFileNameWithoutExtension(openFileDialog1.FileName);
                    comboBox1.Items.Add(filename);
                    textures.Add(filename, openFileDialog1.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading image" + ex.Message);
                }

            }

        }



        private static string MakeValidFileName(string name)
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
        }

        private void CreateObject_Click(object sender, EventArgs e)
        {
            updatepreview();
            if (!textures.ContainsKey(Convert.ToString(comboBox1.SelectedItem)))
            {
                MessageBox.Show("No texture currently selected!");
                return;
            }
            if (textBox1.Text.Length > 0)
            {
                try
                {
                    //Create Object
                    string objectfile = Path.Combine(launchpath + "\\Output\\Nameboards\\" + MakeValidFileName(textBox1.Text) + ".b3d");
                    string finaltexture = Path.Combine(launchpath + "\\Output\\Nameboards\\" + MakeValidFileName(textBox1.Text) + ".png");
                    string rearcopy = Path.Combine(launchpath + "\\Output\\Nameboards\\" + Path.GetFileName(reartexture));
                    //Cleanup output first
                    if (File.Exists(objectfile))
                    {
                        File.Delete(objectfile);
                    }
                    if (File.Exists(finaltexture))
                    {
                        File.Delete(finaltexture);
                    }
                    //Check that height and width are sensible
                    double width = (double)numericUpDown1.Value / 2;
                    if (width < 1)
                    {
                        MessageBox.Show("Your sign is too small, please use a minimum width of 1m");
                        return;
                    }
                    double height = (double)numericUpDown2.Value / 2;
                    if (height < 0.1)
                    {
                        MessageBox.Show("Your sign is too small, please use a minimum height of 0.2m");
                        return;
                    }

                    double aboveground = (double)numericUpDown3.Value;
                    double leglength = (double)numericUpDown4.Value;
                    //Then open streamwriter
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(objectfile, true))
                    {
                        if ((string)comboBox1.SelectedItem == "GWR Wooden")
                        {
                            //We require an entirely different meshbuilder for Wooden type GWR/ SR signs
                            //First write the front face
                            file.WriteLine(";Platform Nameboard generated by Platform Name Generator v1.0");
                            file.WriteLine(";The output from this utility is public domain content");
                            file.WriteLine();
                            file.WriteLine(";Sign Front");
                            file.WriteLine("[MeshBuilder]");
                            file.WriteLine("Vertex -0.025,{0:f4},{1:f4}", height, 0 - width);
                            file.WriteLine("Vertex -0.025,{0:f4},{1:f4}", height, width);
                            file.WriteLine("Vertex -0.025,{0:f4},{1:f4}", 0, width);
                            file.WriteLine("Vertex -0.025,{0:f4},{1:f4}", 0, 0 - width);
                            file.WriteLine("Face 3,2,1,0");
                            file.WriteLine();
                            file.WriteLine("[Texture]");
                            file.WriteLine("Load {0:f4}", Path.GetFileName(texture));
                            file.WriteLine("Coordinates 0,0,0");
                            file.WriteLine("Coordinates 1,-1,0");
                            file.WriteLine("Coordinates 2,-1,1");
                            file.WriteLine("Coordinates 3,0,1");
                            file.WriteLine("Transparent 0,0,255");
                            file.WriteLine();
                            if (singleside.Checked == true)
                            {
                                file.WriteLine(";Sign Rear");
                                file.WriteLine("[MeshBuilder]");
                                file.WriteLine("Vertex -0.025,{0:f4},{1:f4}", height, 0 - width);
                                file.WriteLine("Vertex -0.025,{0:f4},{1:f4}", height, width);
                                file.WriteLine("Vertex -0.025,{0:f4},{1:f4}", 0, width);
                                file.WriteLine("Vertex -0.025,{0:f4},{1:f4}", 0, 0 - width);
                                file.WriteLine("Face 0,1,2,3");
                                file.WriteLine();
                                file.WriteLine("[Texture]");
                                file.WriteLine("Load {0:f4}", Path.GetFileName(reartexture));
                                file.WriteLine("Coordinates 0,0,0");
                                file.WriteLine("Coordinates 1,-1,0");
                                file.WriteLine("Coordinates 2,-1,1");
                                file.WriteLine("Coordinates 3,0,1");
                                file.WriteLine("Transparent 0,0,255");
                            }
                            else
                            {
                                file.WriteLine(";Sign Rear");
                                file.WriteLine("[MeshBuilder]");
                                file.WriteLine("Vertex -0.025,{0:f4},{1:f4}", height, 0 - width);
                                file.WriteLine("Vertex -0.025,{0:f4},{1:f4}", height, width);
                                file.WriteLine("Vertex -0.025,{0:f4},{1:f4}", 0, width);
                                file.WriteLine("Vertex -0.025,{0:f4},{1:f4}", 0, 0 - width);
                                file.WriteLine("Face 0,1,2,3");
                                file.WriteLine();
                                file.WriteLine("[Texture]");
                                file.WriteLine("Load {0:f4}", Path.GetFileName(texture));
                                file.WriteLine("Coordinates 0,0,0");
                                file.WriteLine("Coordinates 1,1,0");
                                file.WriteLine("Coordinates 2,1,1");
                                file.WriteLine("Coordinates 3,0,1");
                                file.WriteLine("Transparent 0,0,255");
                            }
                            //No legs required for this one.....
                        }
                        else
                        {
                            //First write the front face
                            file.WriteLine(";Platform Nameboard generated by Platform Name Generator v1.0");
                            file.WriteLine(";The output from this utility is public domain content");
                            file.WriteLine();
                            file.WriteLine(";Sign Front");
                            file.WriteLine("[MeshBuilder]");
                            file.WriteLine("Vertex -0.025,{0:f4},{1:f4}", aboveground + height, 0 - width);
                            file.WriteLine("Vertex -0.025,{0:f4},{1:f4}", aboveground + height, width);
                            file.WriteLine("Vertex -0.025,{0:f4},{1:f4}", aboveground, width);
                            file.WriteLine("Vertex -0.025,{0:f4},{1:f4}", aboveground, 0 - width);
                            if (singleside.Checked == true)
                            {
                                file.WriteLine("Face 3,2,1,0");
                                file.WriteLine();
                                file.WriteLine("[Texture]");
                                file.WriteLine("Load {0:f4}", Path.GetFileName(texture));
                                file.WriteLine("Coordinates 0,0,0");
                                file.WriteLine("Coordinates 1,-1,0");
                                file.WriteLine("Coordinates 2,-1,1");
                                file.WriteLine("Coordinates 3,0,1");
                                file.WriteLine("Transparent 0,0,255");
                                //Write the rear face with an opposite face statement
                                file.WriteLine();
                                file.WriteLine(";Sign Rear");
                                file.WriteLine("[MeshBuilder]");
                                file.WriteLine("Vertex -0.025,{0:f4},{1:f4}", aboveground + height, 0 - width);
                                file.WriteLine("Vertex -0.025,{0:f4},{1:f4}", aboveground + height, width);
                                file.WriteLine("Vertex -0.025,{0:f4},{1:f4}", aboveground, width);
                                file.WriteLine("Vertex -0.025,{0:f4},{1:f4}", aboveground, 0 - width);
                                file.WriteLine("Face 0,1,2,3");
                                file.WriteLine();
                                file.WriteLine("[Texture]");
                                file.WriteLine("Load {0:f4}", Path.GetFileName(reartexture));
                                file.WriteLine("Coordinates 0,0,0");
                                file.WriteLine("Coordinates 1,1,0");
                                file.WriteLine("Coordinates 2,1,1");
                                file.WriteLine("Coordinates 3,0,1");
                                file.WriteLine("Transparent 0,0,255");
                            }
                            else
                            {
                                //Double-sided needs a face2
                                file.WriteLine("Face2 0,1,2,3");
                                file.WriteLine();
                                file.WriteLine("[Texture]");
                                file.WriteLine("Load {0:f4}", Path.GetFileName(texture));
                                file.WriteLine("Coordinates 0,0,0");
                                file.WriteLine("Coordinates 1,-1,0");
                                file.WriteLine("Coordinates 2,-1,1");
                                file.WriteLine("Coordinates 3,0,1");
                                file.WriteLine("Transparent 0,0,255");
                                //Then write the rear face
                                file.WriteLine();
                                file.WriteLine(";Sign Rear");
                                file.WriteLine("[MeshBuilder]");
                                file.WriteLine("Vertex 0.025,{0:f4},{1:f4}", aboveground + height, 0 - width);
                                file.WriteLine("Vertex 0.025,{0:f4},{1:f4}", aboveground + height, width);
                                file.WriteLine("Vertex 0.025,{0:f4},{1:f4}", aboveground, width);
                                file.WriteLine("Vertex 0.025,{0:f4},{1:f4}", aboveground, 0 - width);
                                file.WriteLine("Face2 3,2,1,0");
                                file.WriteLine();
                                file.WriteLine("[Texture]");
                                file.WriteLine("Load {0:f4}", Path.GetFileName(texture));
                                file.WriteLine("Coordinates 0,0,0");
                                file.WriteLine("Coordinates 1,1,0");
                                file.WriteLine("Coordinates 2,1,1");
                                file.WriteLine("Coordinates 3,0,1");
                                file.WriteLine("Transparent 0,0,255");
                            }
                            //Handle the legs
                            if (radioButton1.Checked == true)
                            {
                                //Cube Type Legs
                                file.WriteLine();
                                file.WriteLine(";Leg Left");
                                file.WriteLine("[MeshBuilder]");
                                file.WriteLine("Cube 0.024,{0:f4},0.049", leglength);
                                file.WriteLine("Color 80,80,80");
                                file.WriteLine("Translate 0,{0:f4},{1:f4}", (aboveground / 1.5) - 0.3, (0.5 - width));
                                file.WriteLine();
                                file.WriteLine(";Leg Right");
                                file.WriteLine("[MeshBuilder]");
                                file.WriteLine("Cube 0.024,{0:f4},0.049", leglength);
                                file.WriteLine("Color 80,80,80");
                                file.WriteLine("Translate 0,{0:f4},{1:f4}", (aboveground / 1.5) - 0.3, (width - 0.5));
                            }
                            if (radioButton2.Checked == true)
                            {
                                //Cylinder Type Legs
                                file.WriteLine();
                                file.WriteLine(";Leg Left");
                                file.WriteLine("[MeshBuilder]");
                                file.WriteLine("Cylinder 8,0.025,0.025,{0:f4}", leglength * 1.7);
                                file.WriteLine("Translate 0,{0:f4},{1:f4}", (aboveground / 1.5) - 0.3, (0.5 - width));
                                file.WriteLine("Color 80,80,80");
                                file.WriteLine();
                                file.WriteLine(";Leg Right");
                                file.WriteLine("[MeshBuilder]");
                                file.WriteLine("Cylinder 8,0.025,0.025,{0:f4}", leglength * 1.7);
                                file.WriteLine("Translate 0,{0:f4},{1:f4}", (aboveground / 1.5) - 0.3, (width - 0.5));
                                file.WriteLine("Color 80,80,80");
                            }
                            else
                            {
                                //Twiddle, no legs to be generated
                            }
                        }
                    }

                    File.Copy(texture, finaltexture, true);
                    File.Copy(reartexture, rearcopy, true);

                    //Cleanup temporary textures
                    string[] filePaths = Directory.GetFiles(temppath);
                    foreach (string filePath in filePaths)
                    {
                        var name = new FileInfo(filePath).Name;
                        name = name.ToLower();
                        File.Delete(filePath);
                    } 
                }
                catch
                {
                    MessageBox.Show("An error occured whilst creating your object. Please check you have write permissions.");
                }
            }
            else
            {
                MessageBox.Show("Please set a station name!");
                return;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            updatepreview();
            
        }

        private void updatepreview()
        {
            //Sleep for 100ms to stop overspeed typing causing issues
            Thread.Sleep(100);
            string tempfile = Path.Combine(temppath + "\\" + MakeValidFileName(textBox1.Text) + ".png");
            
            //Dispose of the original image
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
                pictureBox1.Image = null;
                
                if (File.Exists(tempfile))
                {
                    File.Delete(tempfile);
                }
            }

            if (textBox1.Text.Length > 0 && textures.ContainsKey(Convert.ToString(comboBox1.SelectedItem)))
            {
                using (var prevbitmap = new Bitmap(textures[Convert.ToString(comboBox1.SelectedItem)]))
                {
                    
                    Graphics g = Graphics.FromImage(prevbitmap);
                    StringFormat strFormat = new StringFormat();
                    strFormat.Alignment = StringAlignment.Center;
                    strFormat.LineAlignment = StringAlignment.Center;
                    string currenttexture = (string)comboBox1.SelectedItem;
                    //Handle alternate sign types with a different sized text box
                    if (currenttexture == "GWR Wooden" || (string)comboBox1.SelectedItem == "SR Wooden")
                    {
                        //GWR & SR Wooden signs
                        g.DrawString(textBox1.Text, selectedfont, textbrush,
                        new RectangleF(21, 35, 468, 107), strFormat);
                    }
                    else if (currenttexture == "SR Target" || (string)comboBox1.SelectedItem == "SR Target (Junction)")
                    {
                        //SR Targets
                        g.DrawString(textBox1.Text, selectedfont, textbrush,
                        new RectangleF(0, 77, 512, 100), strFormat);
                    }
                    else if (currenttexture == "London Underground (Modern)")
                    {
                        //LU Modern sign
                        g.DrawString(textBox1.Text, selectedfont, textbrush,
                            new RectangleF(8, 206, 496, 100), strFormat);
                    }
                    else if (currenttexture == "British Railways White Double Arrow")
                    {
                        //BR White Double Arrow
                        g.DrawString(textBox1.Text, selectedfont, textbrush,
                            new RectangleF(115, 0, 397, 128), strFormat);
                    }
                    else if (currenttexture.Contains("Totem"))
                    {
                        //BR Totems
                        g.DrawString(textBox1.Text, selectedfont, textbrush,
                            new RectangleF(40, 30, 432, 68), strFormat);
                    }
                    else
                    {
                        g.DrawString(textBox1.Text, selectedfont, textbrush,
                            new RectangleF(0, 0, 512, 128), strFormat);
                    }
                    
                    prevbitmap.Save(tempfile, System.Drawing.Imaging.ImageFormat.Png);
                }

                Bitmap tempimage;
                using (FileStream myStream = new FileStream(tempfile, FileMode.Open))
                {
                    tempimage = (Bitmap)Image.FromStream(myStream);
                    
                }
                tempimage.MakeTransparent(Color.FromArgb(0, 0, 255));
                this.pictureBox1.Image = tempimage;
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                texture = tempfile;
            }

        }

        

        private void fontbutton_Click(object sender, EventArgs e)
        {
            if (fontDialog1.ShowDialog() == DialogResult.OK)
            {
                selectedfont = fontDialog1.Font;
                updatepreview();
            }
        }

        private void singleside_CheckedChanged(object sender, EventArgs e)
        {
            if(singleside.Checked == true)
            {
                doublesided.Checked = false;
            }
        }

        private void doublesided_CheckedChanged(object sender, EventArgs e)
        {
            if (doublesided.Checked == true)
            {
                singleside.Checked = false;
            }
        }

        private void colorbutton_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                textbrush.Color = colorDialog1.Color;
                updatepreview();
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked == true)
            {
                radioButton2.Checked = false;
                radioButton3.Checked = false;
                numericUpDown4.Enabled = true;
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked == true)
            {
                radioButton1.Checked = false;
                radioButton3.Checked = false;
                numericUpDown4.Enabled = true;
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked == true)
            {
                radioButton1.Checked = false;
                radioButton2.Checked = false;
                numericUpDown4.Enabled = false;
            }
        }


    }
}
