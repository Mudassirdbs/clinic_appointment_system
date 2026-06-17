using App.Core.Interfaces;
using App.Core.Models;

namespace App.WindowsApp.Forms
{
    public class DoctorForm : Form
    {
        private readonly IDoctorService _service;

        private DataGridView   grid      = null!;
        private TextBox        txtName   = null!;
        private TextBox        txtSpec   = null!;
        private TextBox        txtPhone  = null!;
        private TextBox        txtEmail  = null!;
        private TextBox        txtQual   = null!;
        private TextBox        txtDays   = null!;
        private DateTimePicker dtpStart  = null!;
        private DateTimePicker dtpEnd    = null!;
        private NumericUpDown  nudFee    = null!;
        private CheckBox       chkActive = null!;
        private Button         btnSave   = null!;
        private Button         btnDelete = null!;
        private Button         btnClear  = null!;
        private Label          lblCount  = null!;

        private int _selectedId = 0;

        public DoctorForm(IDoctorService service)
        {
            _service = service;
            BuildUI();
            LoadDoctors();
        }

        private void BuildUI()
        {
            Text          = "Doctor Management";
            Size          = new Size(1100, 660);
            StartPosition = FormStartPosition.CenterParent;
            BackColor     = Color.FromArgb(245, 247, 250);
            Font          = new Font("Segoe UI", 9f);
            MinimumSize   = new Size(1000, 600);

            // Title
            var titleBar = new Panel { Dock = DockStyle.Top, Height = 45, BackColor = Color.FromArgb(30, 58, 95) };
            titleBar.Controls.Add(new Label
            {
                Text = "👨‍⚕️  Doctor Management", ForeColor = Color.White,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter
            });

            // ── Grid (left) ───────────────────────────────────────────────────
            var leftPanel = new Panel { Location = new Point(10, 55), Size = new Size(590, 560) };

            lblCount = new Label { Location = new Point(0, 0), AutoSize = true, ForeColor = Color.Gray };

            grid = new DataGridView
            {
                Location            = new Point(0, 25),
                Size                = new Size(590, 535),
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
                new DataGridViewTextBoxColumn { HeaderText = "ID",            DataPropertyName = "DoctorId",        Width = 40 },
                new DataGridViewTextBoxColumn { HeaderText = "Name",          DataPropertyName = "FullName" },
                new DataGridViewTextBoxColumn { HeaderText = "Specialization",DataPropertyName = "Specialization" },
                new DataGridViewTextBoxColumn { HeaderText = "Phone",         DataPropertyName = "PhoneNumber" },
                new DataGridViewTextBoxColumn { HeaderText = "Fee (Rs.)",     DataPropertyName = "ConsultationFee", Width = 70 },
                new DataGridViewCheckBoxColumn { HeaderText = "Active",       DataPropertyName = "IsActive",        Width = 55 }
            );
            grid.SelectionChanged += Grid_SelectionChanged;
            grid.ClearSelection();

            leftPanel.Controls.Add(lblCount);
            leftPanel.Controls.Add(grid);

            // ── Form (right) ──────────────────────────────────────────────────
            var rightPanel = new Panel
            {
                Location  = new Point(610, 55),
                Size      = new Size(470, 560),
                BackColor = Color.White
            };

            var tbl = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                Padding     = new Padding(12)
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 135));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            int row = 0;
            void R(string lbl, Control ctrl, int height = 36)
            {
                tbl.Controls.Add(new Label
                {
                    Text = lbl, Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold)
                }, 0, row);
                ctrl.Dock = DockStyle.Fill;
                tbl.Controls.Add(ctrl, 1, row);
                tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
                row++;
            }

            txtName  = new TextBox();
            txtSpec  = new TextBox();
            txtPhone = new TextBox();
            txtEmail = new TextBox();
            txtQual  = new TextBox();
            txtDays  = new TextBox { PlaceholderText = "e.g. Mon,Wed,Fri" };
            dtpStart = new DateTimePicker { Format = DateTimePickerFormat.Time, ShowUpDown = true };
            dtpEnd   = new DateTimePicker { Format = DateTimePickerFormat.Time, ShowUpDown = true };
            nudFee   = new NumericUpDown  { Minimum = 0, Maximum = 999999 };
            chkActive = new CheckBox { Text = "Doctor is Active", Checked = true };

            R("Full Name *",      txtName);
            R("Specialization *", txtSpec);
            R("Phone",            txtPhone);
            R("Email",            txtEmail);
            R("Qualification",    txtQual);
            R("Available Days",   txtDays);
            R("Start Time",       dtpStart);
            R("End Time",         dtpEnd);
            R("Fee (Rs.)",        nudFee);

            // Checkbox row
            tbl.Controls.Add(new Label(), 0, row);
            chkActive.Dock = DockStyle.Fill;
            tbl.Controls.Add(chkActive, 1, row);
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            row++;

            // Buttons
            var btnFlow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Dock = DockStyle.Fill, Padding = new Padding(0, 6, 0, 0)
            };
            btnSave   = Btn("💾 Save",   Color.FromArgb(52, 152, 219));
            btnDelete = Btn("🗑 Delete", Color.FromArgb(231, 76, 60));
            btnClear  = Btn("✖ Clear",  Color.FromArgb(149, 165, 166));
            btnDelete.Enabled = false;
            btnSave.Click    += BtnSave_Click;
            btnDelete.Click  += BtnDelete_Click;
            btnClear.Click   += (s, e) => ClearForm();
            btnFlow.Controls.AddRange(new Control[] { btnSave, btnDelete, btnClear });
            tbl.SetColumnSpan(btnFlow, 2);
            tbl.Controls.Add(btnFlow, 0, row);
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

            rightPanel.Controls.Add(tbl);
            Controls.Add(leftPanel);
            Controls.Add(rightPanel);
            Controls.Add(titleBar);
        }

        private void LoadDoctors()
        {
            var list = _service.GetAllDoctors();
            grid.DataSource = list;
            grid.ClearSelection();
            lblCount.Text = $"{list.Count} doctor(s)";
        }

        private void Grid_SelectionChanged(object? sender, EventArgs e)
        {
            if (grid.CurrentRow?.DataBoundItem is Doctor d)
            {
                _selectedId       = d.DoctorId;
                txtName.Text      = d.FullName;
                txtSpec.Text      = d.Specialization;
                txtPhone.Text     = d.PhoneNumber;
                txtEmail.Text     = d.Email;
                txtQual.Text      = d.Qualification;
                txtDays.Text      = d.AvailableDays;
                dtpStart.Value    = DateTime.Today.Add(d.StartTime);
                dtpEnd.Value      = DateTime.Today.Add(d.EndTime);
                nudFee.Value      = d.ConsultationFee;
                chkActive.Checked = d.IsActive;
                btnDelete.Enabled = true;
                btnSave.Text      = "💾 Update";
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(txtName.Text)) errors.Add("• Full Name is required.");
            if (string.IsNullOrWhiteSpace(txtSpec.Text)) errors.Add("• Specialization is required.");
            if (dtpEnd.Value.TimeOfDay <= dtpStart.Value.TimeOfDay)
                errors.Add("• End Time must be after Start Time.");
            if (errors.Count > 0)
            {
                MessageBox.Show("Please fix the following:\n\n" + string.Join("\n", errors),
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var d = new Doctor
            {
                DoctorId        = _selectedId,
                FullName        = txtName.Text.Trim(),
                Specialization  = txtSpec.Text.Trim(),
                PhoneNumber     = txtPhone.Text.Trim(),
                Email           = txtEmail.Text.Trim(),
                Qualification   = txtQual.Text.Trim(),
                AvailableDays   = txtDays.Text.Trim(),
                StartTime       = dtpStart.Value.TimeOfDay,
                EndTime         = dtpEnd.Value.TimeOfDay,
                ConsultationFee = nudFee.Value,
                IsActive        = chkActive.Checked
            };

            bool ok = _selectedId == 0 ? _service.AddDoctor(d) : _service.UpdateDoctor(d);
            if (ok)
            {
                MessageBox.Show(_selectedId == 0 ? "Doctor added successfully." : "Doctor updated successfully.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm(); LoadDoctors();
            }
            else MessageBox.Show("Operation failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (_selectedId == 0) return;
            var d = grid.CurrentRow?.DataBoundItem as Doctor;
            if (MessageBox.Show($"Delete Dr. {d?.FullName}?\nThis will also remove their appointments.",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                if (_service.DeleteDoctor(_selectedId))
                {
                    MessageBox.Show("Doctor deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm(); LoadDoctors();
                }
            }
        }

        private void ClearForm()
        {
            _selectedId = 0;
            txtName.Text = txtSpec.Text = txtPhone.Text =
            txtEmail.Text = txtQual.Text = txtDays.Text = "";
            nudFee.Value = 0; chkActive.Checked = true;
            btnDelete.Enabled = false;
            btnSave.Text = "💾 Save";
            grid.ClearSelection();
        }

        private static Button Btn(string text, Color color) => new Button
        {
            Text = text, Size = new Size(118, 36), BackColor = color,
            ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Margin = new Padding(0, 0, 6, 0),
            FlatAppearance = { BorderSize = 0 }
        };
    }
}
