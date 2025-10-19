using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;
using Whisper.net;
using Whisper.net.Ggml;

namespace SpeechToText
{
    public partial class Form1 : Form
    {
        private WhisperProcessor processor;
        private WaveInEvent waveInDevice;
        private WaveFileWriter waveFileWriter;
        private bool isRecording = false;
        private TextBox txtResult;
        private Button btnToggle;
        private Button btnClear;
        private Label lblStatus;
        private ComboBox cmbDil;
        private const string audioFileName = "recording.wav";
        private const string modelFileName = "ggml-small.bin";

        public Form1()
        {
            InitializeComponent();
            CreateControls();
            InitializeWhisper();
        }

        private void CreateControls()
        {
            this.Text = "Konuşmadan Metne Çevirici (Whisper)";
            this.Size = new System.Drawing.Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;

            var lblInfo = new Label
            {
                Text = "Dil Seçin:",
                Location = new System.Drawing.Point(20, 10),
                Size = new System.Drawing.Size(100, 25),
                Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold)
            };
            this.Controls.Add(lblInfo);

            cmbDil = new ComboBox
            {
                Location = new System.Drawing.Point(120, 10),
                Size = new System.Drawing.Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbDil.Items.Add("Türkçe (tr)");
            cmbDil.Items.Add("İngilizce (en)");
            cmbDil.SelectedIndex = 0;
            this.Controls.Add(cmbDil);

            txtResult = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new System.Drawing.Point(20, 50),
                Size = new System.Drawing.Size(540, 300),
                Font = new System.Drawing.Font("Arial", 12)
            };
            this.Controls.Add(txtResult);

            btnToggle = new Button
            {
                Text = "Kayda Başla",
                Location = new System.Drawing.Point(20, 370),
                Size = new System.Drawing.Size(150, 40)
            };
            btnToggle.Click += BtnToggle_Click;
            this.Controls.Add(btnToggle);

            btnClear = new Button
            {
                Text = "Temizle",
                Location = new System.Drawing.Point(180, 370),
                Size = new System.Drawing.Size(100, 40)
            };
            btnClear.Click += (s, e) => txtResult.Clear();
            this.Controls.Add(btnClear);

            lblStatus = new Label
            {
                Text = "Hazır",
                Location = new System.Drawing.Point(20, 420),
                Size = new System.Drawing.Size(540, 20),
                Font = new System.Drawing.Font("Arial", 10),
                ForeColor = System.Drawing.Color.Green
            };
            this.Controls.Add(lblStatus);
        }

        private async void InitializeWhisper()
        {
            try
            {
                lblStatus.Text = "Model yükleniyor...";
                lblStatus.ForeColor = System.Drawing.Color.Orange;

                if (!System.IO.File.Exists(modelFileName))
                {
                    using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(GgmlType.Small);
                    using var fileWriter = System.IO.File.Create(modelFileName);
                    await modelStream.CopyToAsync(fileWriter);
                }

                var factory = WhisperFactory.FromPath(modelFileName);
                processor = factory.CreateBuilder()
                    .WithLanguage("auto")
                    .Build();

                lblStatus.Text = "Hazır - Dil Seçin";
                lblStatus.ForeColor = System.Drawing.Color.Green;
                btnToggle.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Whisper başlatılamadı: " + ex.Message);
                lblStatus.Text = "Hata!";
                lblStatus.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void BtnToggle_Click(object sender, EventArgs e)
        {
            if (!isRecording)
                StartRecording();
            else
                StopRecording();
        }

        private void StartRecording()
        {
            try
            {
                if (System.IO.File.Exists(audioFileName))
                    System.IO.File.Delete(audioFileName);

                waveInDevice = new WaveInEvent();
                waveInDevice.WaveFormat = new WaveFormat(16000, 1); // 16kHz, mono ✅
                waveFileWriter = new WaveFileWriter(audioFileName, waveInDevice.WaveFormat);

                waveInDevice.DataAvailable += (s, e) =>
                    waveFileWriter.Write(e.Buffer, 0, e.BytesRecorded);

                waveInDevice.StartRecording();
                isRecording = true;
                btnToggle.Text = "Kayda Son Ver";
                lblStatus.Text = "🎤 Kayıt yapılıyor... Konuşun!";
                lblStatus.ForeColor = System.Drawing.Color.Green;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kayıt başlatılamadı: " + ex.Message);
            }
        }

        private async void StopRecording()
        {
            try
            {
                waveInDevice?.StopRecording();
                waveInDevice?.Dispose();
                waveFileWriter?.Dispose();
                isRecording = false;
                btnToggle.Text = "Kayda Başla";
                lblStatus.Text = "⏳ Çeviriliyor...";
                lblStatus.ForeColor = System.Drawing.Color.Orange;

                string selectedLanguage = cmbDil.SelectedIndex == 0 ? "tr" : "en";

                var factory = WhisperFactory.FromPath(modelFileName);
                var processorWithLang = factory.CreateBuilder()
                    .WithLanguage(selectedLanguage)
                    .Build();

                using (var fileStream = System.IO.File.OpenRead(audioFileName))
                {
                    string transcription = "";

                    await foreach (var result in processorWithLang.ProcessAsync(fileStream))
                    {
                        transcription += result.Text + " ";
                    }

                    txtResult.AppendText(transcription.Trim() + "\n");
                }

                lblStatus.Text = "✓ Tamamlandı";
                lblStatus.ForeColor = System.Drawing.Color.Green;
                processorWithLang?.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Çevirme başarısız: " + ex.Message);
                lblStatus.Text = "❌ Hata!";
                lblStatus.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            waveInDevice?.Dispose();
            waveFileWriter?.Dispose();
            processor?.Dispose();
            base.OnFormClosing(e);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
    }
}
