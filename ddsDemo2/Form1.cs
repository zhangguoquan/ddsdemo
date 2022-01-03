using DDS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ddsDemo2
{
    public partial class Form1 : Form
    {
        private Point MouseDownLocation;
     
//        private CustomCtrl ctl;
        private Rectangle rec = new Rectangle(0, 0, 0, 0);

        private ShapeTypeDataWriter writer;
        Topic shapeTopic;

        private ShapeType pubInstance;

        public ShapeType subInstance;
        public static Form1 staticForm1;
        private Brush localBrush;

        private ShapesDDS shapesDDS = null;
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            DoubleBuffered = true;
            pubInstance = new ShapeType();
            subInstance = new ShapeType();

      
    
            staticForm1 =this;
            startDDS();

            bttn_quit.Select();


            localBrush = Brushes.DeepSkyBlue;
            radioButton1.Checked = true;
        }

        public Boolean startDDS()
        {
            if (shapesDDS != null)
            {
                return true;
            }

            /* Get some settings from preference manager */
            shapesDDS = new ShapesDDS();
            if (!shapesDDS.initialize(0))
            {
                shapesDDS = null;
                return false;
            }

            shapeTopic = shapesDDS.get_Topic();


            return true;
        }

        public Boolean stopDDS()
        {
            /* Clear all objects that have been created previously */

            shapesDDS.stop();

            shapesDDS = null;

            return true;
        }
        public void addPublication(Topic topic)
        {
            // Create object of shape/color if not created already
            // Create writer 
            writer = shapesDDS.create_writer(topic);


        }
        public void addSubscription(Topic topic)
        {
            // Create reader if not created already


            ShapeTypeListener reader_listener =  new ShapeTypeListener();

            /* To customize the data reader QoS, use
            the configuration file USER_QOS_PROFILES.xml */
            DDS.DataReader reader = shapesDDS.create_reader(topic, reader_listener);
            if (reader == null)
            {
                reader_listener = null;
                throw new ApplicationException("create_datareader error");
            }
        }
        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                rec = new Rectangle(e.X, e.Y, 0, 0);
                Invalidate();
            }
            if (e.Button == MouseButtons.Right)
            {
                MouseDownLocation = e.Location;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                rec.Width = e.X - rec.X;
                rec.Height = e.Y - rec.Y;
                this.Invalidate();
            }
            if (e.Button == MouseButtons.Right)
            {
                rec.Location = new Point(e.X - MouseDownLocation.X + rec.Left, e.Y - MouseDownLocation.Y + rec.Top);
                MouseDownLocation = e.Location;
                this.Invalidate();
            }
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(localBrush, rec);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
      

            rec.X = rec.X + 1;
            rec.Y = rec.Y;

            if (rec.X >=ClientSize.Width)
            {
                rec.X = 0;
            }

            pubInstance.x = rec.X;
            pubInstance.y = rec.Y;

            pubInstance.width = rec.Width;
            pubInstance.length = rec.Height;
            DDS.InstanceHandle_t instance_handle = DDS.InstanceHandle_t.HANDLE_NIL;
            writer.write(pubInstance, ref instance_handle);

            this.Invalidate();

        
           
        }


        public void drawrec(int x,int y,int width,int height)
        {
            rec.X = x;
            rec.Y = y;
            rec.Width = width;
            rec.Height = height;


            this.Invalidate();
        }

        private void bttn_sub_Click(object sender, EventArgs e)
        {
          addSubscription(shapeTopic);
            bttn_sub.Select();
     
        }

        private void bttn_pub_Click(object sender, EventArgs e)
        {
            addPublication(shapeTopic);
            timer1.Start();
            bttn_pub.Select();

        }

        private void bttn_quit_Click(object sender, EventArgs e)
        {
           if (shapesDDS !=null)
             stopDDS();
             Close();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            localBrush = Brushes.DeepSkyBlue;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            localBrush = Brushes.DarkOliveGreen;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            localBrush = Brushes.PaleVioletRed;
        }
    }

    public class ShapeTypeListener : DDS.DataReaderListener
    {
      
       public override void on_requested_deadline_missed(
            DDS.DataReader reader,
            ref DDS.RequestedDeadlineMissedStatus status)
        { }

        public override void on_requested_incompatible_qos(
            DDS.DataReader reader,
            DDS.RequestedIncompatibleQosStatus status)
        { }

        public override void on_sample_rejected(
            DDS.DataReader reader,
            ref DDS.SampleRejectedStatus status)
        { }

        public override void on_liveliness_changed(
            DDS.DataReader reader,
            ref DDS.LivelinessChangedStatus status)
        { }

        public override void on_sample_lost(
            DDS.DataReader reader,
            ref DDS.SampleLostStatus status)
        { }

        public override void on_subscription_matched(
            DDS.DataReader reader,
            ref DDS.SubscriptionMatchedStatus status)
        { }

        public override void on_data_available(DDS.DataReader reader)
        {
            ShapeTypeDataReader ShapeType_reader =
            (ShapeTypeDataReader)reader;

            try
            {
                ShapeType_reader.take(
                    data_seq,
                    info_seq,
                    DDS.ResourceLimitsQosPolicy.LENGTH_UNLIMITED,
                    DDS.SampleStateKind.ANY_SAMPLE_STATE,
                    DDS.ViewStateKind.ANY_VIEW_STATE,
                    DDS.InstanceStateKind.ANY_INSTANCE_STATE);
            }
            catch (DDS.Retcode_NoData)
            {
                return;
            }
            catch (DDS.Exception e)
            {
                Console.WriteLine("take error {0}", e);
                return;
            }

            System.Int32 data_length = data_seq.length;
            for (int i = 0; i < data_length; ++i)
            {
                if (info_seq.get_at(i).valid_data)
                {
    
                    Form1.staticForm1.drawrec(data_seq.get_at(i).x, data_seq.get_at(i).y, data_seq.get_at(i).width, data_seq.get_at(i).length);
                }
            }

            try
            {
                ShapeType_reader.return_loan(data_seq, info_seq);
            }
            catch (DDS.Exception e)
            {
                Console.WriteLine("return loan error {0}", e);
            }
        }

        public ShapeTypeListener()
        {
            data_seq = new ShapeTypeSeq();
            info_seq = new DDS.SampleInfoSeq();
        }

        private ShapeTypeSeq data_seq;
        private DDS.SampleInfoSeq info_seq;
    };


    public class refreshData
    {
       

        public int local_x;
        public int local_y;
        public int local_width;
        public int local_height;



     
    }

    //public class CustomCtrl : Control
    //{
    //    public int x;
    //    public int y;
    //    public CustomCtrl(int x, int y)
    //    {
    //        this.x = x;
    //        this.y = y;
    //        Rectangle rect = new Rectangle(x, y, 200, 200);
    //        this.Region = new Region(rect);

    //    }


    //}
}
