using System;
using System.Windows.Forms;
using System.Net.Http;

namespace SmartX.Dashboard
{
    public partial class Form1 : Form
    {
        public static readonly HttpClient ApiClient = new HttpClient { BaseAddress = new Uri("https://localhost:7103/") };

        public Form1()
        {
            InitializeComponent();
            SetupMainMenu();
        }

        private void SetupMainMenu()
        {
            // Configure Form
            this.Text = "Smart-X Ecosystem Gateway";
            this.Size = new System.Drawing.Size(450, 300);

            // 1. Button: Sensor Data Ingestion
            Button btnOpenIngestion = new Button();
            btnOpenIngestion.Text = "Sensor Data Ingestion and Telemetry";
            btnOpenIngestion.Location = new System.Drawing.Point(50, 40);
            btnOpenIngestion.Size = new System.Drawing.Size(320, 40);
            btnOpenIngestion.Click += new EventHandler(btnOpenIngestion_Click);
            this.Controls.Add(btnOpenIngestion);

            // 2. Button: Real-Time Command Stream
            Button btnOpenCommandStream = new Button();
            btnOpenCommandStream.Text = "Real-Time Command Stream and History";
            btnOpenCommandStream.Location = new System.Drawing.Point(50, 100);
            btnOpenCommandStream.Size = new System.Drawing.Size(320, 40);
            btnOpenCommandStream.Click += new EventHandler(btnOpenCommandStream_Click);
            this.Controls.Add(btnOpenCommandStream);

            // 3. Button: Network Topology
            Button btnOpenTopology = new Button();
            btnOpenTopology.Text = "Network Topology and Mesh Routing";
            btnOpenTopology.Location = new System.Drawing.Point(50, 160);
            btnOpenTopology.Size = new System.Drawing.Size(320, 40);
            btnOpenTopology.Click += new EventHandler(btnOpenTopology_Click);
            this.Controls.Add(btnOpenTopology);
        }

        private void btnOpenIngestion_Click(object sender, EventArgs e)
        {
            var ingestionForm = new IngestionForm();
            ingestionForm.Show();
        }

        private void btnOpenCommandStream_Click(object sender, EventArgs e)
        {
            var commandForm = new RealTimeCommandForm();
            commandForm.Show();
        }

        private void btnOpenTopology_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Network Topology & Mesh Routing will be implemented in the final PoE.");
        }
    }
}