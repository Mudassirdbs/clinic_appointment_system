using App.Core.Interfaces;
using App.Core.Models;

namespace App.WindowsApp.Forms
{
    public class AppointmentForm : Form
    {
        private readonly IAppointmentService _as;
        private readonly IPatientService     _ps;
        private readonly IDoctorService      _ds;

        private DataGridView   grid       = null!;
        private ComboBox       cmbFilter  = null!;
        private DateTimePicker dtpFilter  = null!;
        private ComboBox       cmbStatus  = null!;   // filter by status
        private Label          lblCount   = null!;

        private ComboBox       cmbPatient = null!;
        private ComboBox       cmbDoctor  = null!;
        private DateTimePicker dtpDate    = null!;
        private DateTimePicker dtpTime    = null!;
        private TextBox        txtReason  = null!;
        private TextBox        txtNotes   = null!;
        private ComboBox       cmbStatusF = null!;
        private Button         btnSave    = null!;
        private Button         btnDelete  = null!;
        private Button         btnClear   = null!;

        private int _selectedId = 0;

        public AppointmentForm(IAppointmentService apts, IPatientService ps, IDoctorService ds)
        {
            _as = apts; _ps = ps; _ds = ds;
            BuildUI();
            LoadDropdowns();
            LoadAppointments();
        }

        private void BuildUI()
        {
            Text          = "Appointment Management";
            Size          = new Size(1150, 680);
            StartPosition = FormStartPosition.CenterParent;
            BackColor     = Color.FromArgb(245, 247, 250);
            Font          = new Font("Segoe UI", 9f);
            MinimumSize   = new Size(1000, 600);

            // Title
            var titleBar = new Panel { Dock = DockStyle.Top, Height = 45, BackColor = Color.FromArgb(30, 58, 95) };
            titleBar.Controls.Add(new Label
            {
                Text = "📅  Appointment Management", ForeColor = Color.White,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter
            });

            // ── Filter bar ────────────────────────────────────────────────────
            var filterPanel = new Panel { Location = new Point(10, 55), Size = new Size(700, 40) };

            filterPanel.Controls.Add(new Label { Text = "Filter:", Location = new Point(0, 12), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) });

            cmbFilter = new ComboBox { Location = new Point(50, 8), Width = 110, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbFilter.Items.AddRange(new[] { "All", "Today", "By Date" });
            cmbFilter.SelectedIndex = 0;
            cmbFilter.SelectedIndexChanged += (s, e) => dtpFilter.Visible = cmbFilter.Text == "By Date";

            dtpFilter = new DateTimePicker { Location = new Point(170, 8), Width = 120, Format = DateTimePickerFormat.Short, Visible = false };

            cmbStatus = new ComboBox { Location = new Point(300, 8), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.Add("All Statuses");
            foreach (var s in Enum.GetNames(typeof(AppointmentStatus))) cmbStatus.Items.Add(s);
            cmbStatus.SelectedIndex = 0;

            var btnApply = new Button
            {
                Text = "Apply", Location = new Point(430, 6), Size = new Size(75, 28),
                BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnApply.FlatAppearance.BorderSize = 0;
            btnApply.Click += ApplyFilter;

            lblCount = new Label { Location = new Point(515, 12), AutoSize = true, ForeColor = Color.Gray };
            filterPanel.Controls.AddRange(new Control[] { cmbFilter, dtpFilter, cmbStatus, btnApply, lblCount });

            // ── Grid ──────────────────────────────────────────────────────────
            grid = new DataGridView
            {
                Location            = new Point(10, 100),
                Size                = new Size(700, 535),
                ReadOnly            = true,
                AllowUserToAddRows  = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode       = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor     = Color.White,
                RowHeadersVisible   = false,
                AllowUserToOrderColumns = true,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(30, 58, 95),
                    ForeColor = Color.White,
                    Font      = new Font("Segoe UI", 9f, FontStyle.Bold)
                }
            };
            grid.Columns.AddRange(
                new DataGridViewTextBoxColumn { HeaderText = "#",        DataPropertyName = "AppointmentId", Width = 40 },
                new DataGridViewTextBoxColumn { HeaderText = "Date",     DataPropertyName = "AppointmentDate" },
                new DataGridViewTextBoxColumn { HeaderText = "Time",     DataPropertyName = "AppointmentTime", Width = 65 },
                new DataGridViewTextBoxColumn { HeaderText = "Patient",  DataPropertyName = "PatientName" },
                new DataGridViewTextBoxColumn { HeaderText = "Doctor",   DataPropertyName = "DoctorName" },
                new DataGridViewTextBoxColumn { HeaderText = "Reason",   DataPropertyName = "Reason" },
                new DataGridViewTextBoxColumn { HeaderText = "Status",   DataPropertyName = "Status", Width = 85 }
            );
            grid.SelectionChanged += Grid_SelectionChanged;
            grid.ClearSelection();

            // Color rows by status
            grid.RowPrePaint += (s, e) =>
            {
                if (grid.Rows[e.RowIndex].DataBoundItem is Appointment a)
                {
                    grid.Rows[e.RowIndex].DefaultCellStyle.BackColor = a.Status switch
                    {
                        AppointmentStatus.Completed  => Color.FromArgb(230, 255, 235),
                        AppointmentStatus.Cancelled  => Color.FromArgb(255, 235, 235),
                        AppointmentStatus.NoShow     => Color.FromArgb(255, 248, 220),
                        _                            => Color.White
                    };
                }
            };

            // ── Right form ────────────────────────────────────────────────────
            var rightPanel = new Panel
            {
                Location  = new Point(720, 55),
                Size      = new Size(415, 580),
                BackColor = Color.White
            };

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(12)
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            int row = 0;
            void R(string lbl, Control ctrl, int h = 40)
            {
                tbl.Controls.Add(new Label { Text = lbl, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 9f, FontStyle.Bold) }, 0, row);
                ctrl.Dock = DockStyle.Fill;
                tbl.Controls.Add(ctrl, 1, row);
                tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, h));
                row++;
            }

            cmbPatient = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cmbDoctor  = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            dtpDate    = new DateTimePicker { Format = DateTimePickerFormat.Short };
            dtpTime    = new DateTimePicker { Format = DateTimePickerFormat.Time, ShowUpDown = true };
            txtReason  = new TextBox { Multiline = true };
            txtNotes   = new TextBox { Multiline = true };
            cmbStatusF = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatusF.Items.AddRange(Enum.GetNames(typeof(AppointmentStatus)));
            cmbStatusF.SelectedIndex = 0;

            R("Patient *",  cmbPatient);
            R("Doctor *",   cmbDoctor);
            R("Date *",     dtpDate);
            R("Time *",     dtpTime);
            R("Reason",     txtReason, 55);
            R("Notes",      txtNotes,  55);
            R("Status",     cmbStatusF);

            var btnFlow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Dock          = DockStyle.Fill,
                Padding       = new Padding(0, 6, 0, 0)
            };
            btnSave   = Btn("💾 Save",   Color.FromArgb(52,152,219));
            btnDelete = Btn("🗑 Delete", Color.FromArgb(231,76,60));
            btnClear  = Btn("✖ Clear",  Color.FromArgb(149,165,166));
            btnDelete.Enabled = false;
            btnSave.Click    += BtnSave_Click;
            btnDelete.Click  += BtnDelete_Click;
            btnClear.Click   += (s,e) => ClearForm();
            btnFlow.Controls.AddRange(new Control[] { btnSave, btnDelete, btnClear });
            tbl.SetColumnSpan(btnFlow, 2);
            tbl.Controls.Add(btnFlow, 0, row);
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

            rightPanel.Controls.Add(tbl);
            Controls.Add(rightPanel);
            Controls.Add(grid);
            Controls.Add(filterPanel);
            Controls.Add(titleBar);
        }

        private void LoadDropdowns()
        {
            cmbPatient.DisplayMember = "FullName";
            cmbPatient.ValueMember   = "PatientId";
            cmbPatient.DataSource    = _ps.GetAllPatients();

            cmbDoctor.DisplayMember  = "FullName";
            cmbDoctor.ValueMember    = "DoctorId";
            cmbDoctor.DataSource     = _ds.GetActiveDoctors();
        }

        private void LoadAppointments(List<Appointment>? list = null)
        {
            var data = list ?? _as.GetAllAppointments();

            // Apply status filter
            if (cmbStatus != null && cmbStatus.SelectedIndex > 0)
            {
                var st = cmbStatus.Text;
                data = data.Where(a => a.Status.ToString() == st).ToList();
            }

            grid.DataSource = data;
            grid.ClearSelection();
            if (lblCount != null) lblCount.Text = $"{data.Count} record(s)";
        }

        private void ApplyFilter(object? sender, EventArgs e)
        {
            var list = cmbFilter.Text switch
            {
                "Today"   => _as.GetTodaysAppointments(),
                "By Date" => _as.GetAppointmentsByDate(dtpFilter.Value.Date),
                _         => _as.GetAllAppointments()
            };
            LoadAppointments(list);
        }

        private void Grid_SelectionChanged(object? sender, EventArgs e)
        {
            if (grid.CurrentRow?.DataBoundItem is Appointment a)
            {
                _selectedId = a.AppointmentId;
                SetCombo(cmbPatient, a.PatientId, "PatientId");
                SetCombo(cmbDoctor,  a.DoctorId,  "DoctorId");
                dtpDate.Value      = a.AppointmentDate;
                dtpTime.Value      = DateTime.Today.Add(a.AppointmentTime);
                txtReason.Text     = a.Reason;
                txtNotes.Text      = a.Notes;
                cmbStatusF.Text    = a.Status.ToString();
                btnDelete.Enabled  = true;
                btnSave.Text       = "💾 Update";
            }
        }

        private static void SetCombo(ComboBox cmb, int id, string prop)
        {
            foreach (var item in cmb.Items)
            {
                var val = item.GetType().GetProperty(prop)?.GetValue(item);
                if (val is int v && v == id) { cmb.SelectedItem = item; break; }
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            var errors = new List<string>();
            if (cmbPatient.SelectedItem == null) errors.Add("• Please select a Patient.");
            if (cmbDoctor.SelectedItem == null)  errors.Add("• Please select a Doctor.");
            if (string.IsNullOrWhiteSpace(txtReason.Text)) errors.Add("• Reason is required.");
            if (errors.Count > 0)
            {
                MessageBox.Show("Please fix:\n\n" + string.Join("\n", errors),
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var a = new Appointment
            {
                AppointmentId   = _selectedId,
                PatientId       = (int)(cmbPatient.SelectedValue ?? 0),
                DoctorId        = (int)(cmbDoctor.SelectedValue  ?? 0),
                AppointmentDate = dtpDate.Value.Date,
                AppointmentTime = dtpTime.Value.TimeOfDay,
                Reason          = txtReason.Text.Trim(),
                Notes           = txtNotes.Text.Trim(),
                Status          = Enum.Parse<AppointmentStatus>(cmbStatusF.Text)
            };

            bool ok = _selectedId == 0 ? _as.AddAppointment(a) : _as.UpdateAppointment(a);
            if (ok)
            {
                MessageBox.Show(_selectedId == 0 ? "Appointment booked." : "Appointment updated.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm(); LoadAppointments();
            }
            else MessageBox.Show("Operation failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (_selectedId == 0) return;
            if (MessageBox.Show("Delete this appointment?", "Confirm",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                if (_as.DeleteAppointment(_selectedId))
                {
                    MessageBox.Show("Appointment deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm(); LoadAppointments();
                }
            }
        }

        private void ClearForm()
        {
            _selectedId = 0;
            txtReason.Text = txtNotes.Text = "";
            cmbStatusF.SelectedIndex = 0;
            dtpDate.Value = DateTime.Today;
            btnDelete.Enabled = false;
            btnSave.Text = "💾 Save";
            grid.ClearSelection();
        }

        private static Button Btn(string text, Color color) => new Button
        {
            Text = text, Size = new Size(110, 36), BackColor = color,
            ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Margin = new Padding(0, 0, 6, 0),
            FlatAppearance = { BorderSize = 0 }
        };
    }
}
