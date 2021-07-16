using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;



namespace pjtReconocimientoFacialTutorial
{
    public partial class Form1 : Form
    {

        //variables vectores y Harcascades
        int con = 0;
        Image<Bgr, Byte> currentFrame;
        Capture Grabar;
        HaarCascade face;//Rostro
        MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.4d, 0.4d);
        Image<Gray, byte> result = null;
        Image<Gray, byte> gray = null;
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        List<string> labels = new List<string>();
        List<string> NombrePersonas = new List<string>();
        int ContTrain, numLabels, t;
        string Nombre;
        DataGridView d = new DataGridView();


        public Form1()
        {
            InitializeComponent();


            face = new HaarCascade("haarcascade_frontalface_default.xml");
            try
            {
                clsDA.Consultar(d);

                string[] Labels = clsDA.Nombre;
                numLabels = clsDA.TotalRostros;
                ContTrain = numLabels;

                for (int i = 0; i < numLabels; i++)
                {
                    con = i;
                    Bitmap bmp = new Bitmap(clsDA.ConvertBinaryToImg(con));

                    trainingImages.Add(new Image<Gray, byte>(bmp));
                    labels.Add(Labels[i]);

                }



            }
            catch (Exception e)
            {

                MessageBox.Show("sin rostros para cargar");
            }
            
        }

        /// <summary>
        /// //
        /// </summary>
       
            private void FrameGrabar(object sender, EventArgs e)
        {
            lblCantidad.Text = "0";
            NombrePersonas.Add("");

            try
            {
                currentFrame = Grabar.QueryFrame().Resize(320, 240, INTER.CV_INTER_CUBIC);

                gray = currentFrame.Convert<Gray, Byte>();

                MCvAvgComp[][] RostrosDetectados = gray.DetectHaarCascade(face, 1.2, 10, HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20,20));

                foreach(MCvAvgComp R in RostrosDetectados[0])
                {
                    t = t + 1;
                    result = currentFrame.Copy(R.rect).Convert<Gray, byte>().Resize(100, 100, INTER.CV_INTER_CUBIC);
                    currentFrame.Draw(R.rect, new Bgr(Color.Green), 1);

                    if(trainingImages.ToArray().Length!=0)
                    {
                        MCvTermCriteria Criterio = new MCvTermCriteria(ContTrain, 0.66);

                        EigenObjectRecognizer recogida = new EigenObjectRecognizer(trainingImages.ToArray(), labels.ToArray(), ref Criterio);
                        var fa = new Image<Gray, byte>[trainingImages.Count];

                        Nombre = recogida.Recognize(result);

                        currentFrame.Draw(Nombre, ref font, new Point(R.rect.X - 2, R.rect.Y - 2), new Bgr(Color.Green));
                    }

                    NombrePersonas[t - 1] = Nombre;
                    NombrePersonas.Add("");

                    lblCantidad.Text = RostrosDetectados[0].Length.ToString();
                }
                t = 0;
                imageBox1.Image = currentFrame;
                Nombre = "";
                NombrePersonas.Clear();
            }
            catch (Exception Error)
            {

                MessageBox.Show(Error.Message);
            }

        }

        private void reconocer()
        {
            try
            {
                Grabar = new Capture();
               // Grabar.QueryFrame();
                
               // Application.Idle += new EventHandler(FrameGrabar);
            }
            catch (Exception Error)
            {

                MessageBox.Show(Error.Message);
            }
        }

        private void DetenerReconocer()
        {
            try
            {
               
                Application.Idle -= new EventHandler(FrameGrabar);
                Grabar.Dispose();
            }
            catch (Exception Error)
            {

                MessageBox.Show(Error.Message);
            }

        }
        private void btnregistrar_Click(object sender, EventArgs e)
        {
            frmRegistrar f = new frmRegistrar();

            DetenerReconocer();
            this.Hide();
            f.ShowDialog();
            this.Visible = true;
            reconocer();
        }

       
        private void Form1_Load(object sender, EventArgs e)
        {
            reconocer();
        }
    }
}
