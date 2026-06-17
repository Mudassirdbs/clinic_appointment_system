using App.Core.Interfaces;
using App.WindowsApp.Forms;

namespace App.WindowsApp.Forms
{
    public partial class MainForm : Form
    {
        private readonly IPatientService     _ps;
        private readonly IDoctorService      _ds;
        private readonly IAppointmentService _as;
        private readonly IReportService      _rs;

        private Label        lblPatients  = null!;
        private Label        lblDoctors   = null!;
        private Label        lblToday     = null!;
        private DataGridView todayGrid    = null!;
        private StatusStrip  statusBar    = null!;
        private ToolStripStatusLabel slblInfo   = null!;
        private ToolStripStatusLabel slblTime   = null!;
        private System.Windows.Forms.Timer clock = null!;

        public MainForm(IPatientService ps, IDoctorService ds,
                        IAppointmentService apts, IReportService rs)
        {
            _ps = ps; _ds = ds; _as = apts; _rs = rs;
            InitializeComponent();
            LoadDashboard();
            StartClock();
        }

        private void InitializeComponent()
        {
            Text          = "🏥 Clinic Appointment System";
            Size          = new Size(1050, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor     = Color.FromArgb(245, 247, 250);
            Font          = new Font("Segoe UI", 9f);
            MinimumSize   = new Size(900, 600);

            // ── Status bar (BONUS) ────────────────────────────────────────────
            statusBar = new StatusStrip { BackColor = Color.FromArgb(30, 58, 95) };
            slblInfo  = new ToolStripStatusLabel("Ready") { ForeColor = Color.White, Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            slblTime  = new ToolStripStatusLabel(DateTime.Now.ToString("dd MMM yyyy  HH:mm:ss")) { ForeColor = Color.LightGray };
            statusBar.Items.AddRange(new ToolStripItem[] { slblInfo, slblTime });

            // ── Sidebar ───────────────────────────────────────────────────────
            var sidebar = new Panel { Dock = DockStyle.Left, Width = 210, BackColor = Color.FromArgb(30, 58, 95) };
            sidebar.Controls.Add(new Label
            {
                Text = "🏥  Clinic System", ForeColor = Color.White,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                Dock = DockStyle.Top, Height = 65, TextAlign = ContentAlignment.MiddleCenter
            });

            var btnDash   = SideBtn("📊  Dashboard");
            var btnPat    = SideBtn("👤  Patients");
            var btnDoc    = SideBtn("👨‍⚕️  Doctors");
            var btnAppt   = SideBtn("📅  Appointments");
            var btnCharts = SideBtn("📈  Charts & Reports");

            btnDash.Click   += (s, e) => LoadDashboard();
            btnPat.Click    += (s, e) => { SetStatus("Opening Patients..."); new PatientForm(_ps).ShowDialog(this); LoadDashboard(); };
            btnDoc.Click    += (s, e) => { SetStatus("Opening Doctors..."); new DoctorForm(_ds).ShowDialog(this); LoadDashboard(); };
            btnAppt.Click   += (s, e) => { SetStatus("Opening Appointments..."); new AppointmentForm(_as, _ps, _ds).ShowDialog(this); LoadDashboard(); };
            btnCharts.Click += (s, e) => { SetStatus("Opening Charts..."); new ChartForm(_rs).ShowDialog(this); };

            sidebar.Controls.Add(btnCharts);
            sidebar.Controls.Add(btnAppt);
            sidebar.Controls.Add(btnDoc);
            sidebar.Controls.Add(btnPat);
            sidebar.Controls.Add(btnDash);

            // ── Main content ──────────────────────────────────────────────────
            var content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };

            var lblTitle = new Label
            {
                Text = "Dashboard", Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 58, 95), AutoSize = true, Location = new Point(15, 15)
            };

            // Stat cards
            lblPatients = new Label { Text = "0" };
            lblDoctors  = new Label { Text = "0" };
            lblToday    = new Label { Text = "0" };

            var card1 = StatCard("Total Patients",       Color.FromArgb(52,152,219), lblPatients, 15,  65);
            var card2 = StatCard("Active Doctors",       Color.FromArgb(46,204,113), lblDoctors,  235, 65);
            var card3 = StatCard("Today's Appointments", Color.FromArgb(231,76,60),  lblToday,    455, 65);

            // Quick Charts button on dashboard
            var btnQuickChart = new Button
            {
                Text = "📈 View Charts", Location = new Point(675, 65), Size = new Size(160, 80),
                BackColor = Color.FromArgb(155,89,182), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11f, FontStyle.Bold)
            };
            btnQuickChart.FlatAppearance.BorderSize = 0;
            btnQuickChart.Click += (s, e) => new ChartForm(_rs).ShowDialog(this);

            var lblTodayTitle = new Label
            {
                Text = "Today's Appointments", Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 58, 95), Location = new Point(15, 165), AutoSize = true
            };

            todayGrid = new DataGridView
            {
                Location            = new Point(15, 195),
                Size                = new Size(820, 430),
                ReadOnly            = true,
                AllowUserToAddRows  = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor     = Color.White,
                BorderStyle         = BorderStyle.None,
                RowHeadersVisible   = false,
                SelectionMode       = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToOrderColumns = true,   // BONUS: column sorting
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(30, 58, 95),
                    ForeColor = Color.White,
                    Font      = new Font("Segoe UI", 9f, FontStyle.Bold)
                }
            };
            todayGrid.Columns.AddRange(
                new DataGridViewTextBoxColumn { HeaderText = "Time",    DataPropertyName = "AppointmentTime" },
                new DataGridViewTextBoxColumn { HeaderText = "Patient", DataPropertyName = "PatientName" },
                new DataGridViewTextBoxColumn { HeaderText = "Doctor",  DataPropertyName = "DoctorName" },
                new DataGridViewTextBoxColumn { HeaderText = "Specialty",DataPropertyName = "Specialization" },
                new DataGridViewTextBoxColumn { HeaderText = "Reason",  DataPropertyName = "Reason" },
                new DataGridViewTextBoxColumn { HeaderText = "Status",  DataPropertyName = "Status" }
            );

            content.Controls.Add(todayGrid);
            content.Controls.Add(lblTodayTitle);
            content.Controls.Add(btnQuickChart);
            content.Controls.Add(card1);
            content.Controls.Add(card2);
            content.Controls.Add(card3);
            content.Controls.Add(lblTitle);

            Controls.Add(content);
            Controls.Add(sidebar);
            Controls.Add(statusBar);
        }

        private void LoadDashboard()
        {
            try
            {
                lblPatients.Text   = _ps.GetTotalPatients().ToString();
                lblDoctors.Text    = _ds.GetTotalDoctors().ToString();
                int todayCnt       = _as.GetTotalAppointmentsToday();
                lblToday.Text      = todayCnt.ToString();
                todayGrid.DataSource = _as.GetTodaysAppointments();
                SetStatus($"Dashboard loaded — {todayCnt} appointment(s) today | {DateTime.Now:HH:mm}");
            }
            catch (Exception ex)
            {
                SetStatus("Error: " + ex.Message);
            }
        }

        private void SetStatus(string msg) =>
            slblInfo.Text = "  " + msg;

        private void StartClock()
        {
            clock = new System.Windows.Forms.Timer { Interval = 1000 };
            clock.Tick += (s, e) => slblTime.Text = DateTime.Now.ToString("dd MMM yyyy  HH:mm:ss");
            clock.Start();
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private static Button SideBtn(string text) => new Button
        {
            Text      = text, Dock = DockStyle.Top, Height = 48,
            FlatStyle = FlatStyle.Flat, ForeColor = Color.White,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding   = new Padding(15, 0, 0, 0),
            Font      = new Font("Segoe UI", 10f),
            FlatAppearance = { BorderSize = 0 }
        };

        private static Panel StatCard(string title, Color color, Label valueLabel, int x, int y)
        {
            var card = new Panel
            {
                BackColor = color, Size = new Size(200, 85), Location = new Point(x, y)
            };
            card.Controls.Add(new Label
            {
                Text = title, ForeColor = Color.White, Font = new Font("Segoe UI", 9f),
                Location = new Point(10, 8), AutoSize = true
            });
            valueLabel.Font = new Font("Segoe UI", 26f, FontStyle.Bold);
            valueLabel.ForeColor = Color.White;
            valueLabel.Location  = new Point(10, 30);
            valueLabel.AutoSize  = true;
            card.Controls.Add(valueLabel);
            return card;
        }
    }
}
