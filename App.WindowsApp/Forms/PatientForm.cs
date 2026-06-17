using App.Core.Interfaces;
using App.Core.Models;

namespace App.WindowsApp.Forms
{
    public class PatientForm : Form
    {
        private readonly IPatientService _service;

        private DataGridView   grid       = null!;
        private TextBox        txtSearch  = null!;
        private ComboBox       cmbGender  = null!;   // filter dropdown
        private TextBox        txtName    = null!;
        private ComboBox       cmbGenderF = null!;
        private DateTimePicker dtpDOB     = null!;
        private TextBox        txtPhone   = null!;
        private TextBox        txtEmail   = null!;
        private TextBox        txtAddress = null!;
        private ComboBox       cmbBlood   = null!;
        private Button         btnSave    = null!;
        private Button         btnDelete  = null!;
        private Button         btnClear   = null!;
        private Label          lblRecords = null!;

        private int _selectedId = 0;

        public PatientForm(IPatientService service)
        {
            _service = service;
            BuildUI();
            LoadPatients();
        }

        private void BuildUI()
        {
            Text          = "Patient Management";
            Size          = new Size(1050, 650);
            StartPosition = FormStartPosition.CenterParent;
            BackColor     = Color.FromArgb(245, 247, 250);
            Font          = new Font("Segoe UI", 9f);
            MinimumSize   = new Size(900, 600);

            // ── Title ─────────────────────────────────────────────────────────
            var titleBar = new Panel { Dock = DockStyle.Top, Height = 45, BackColor = Color.FromArgb(30,58,95) };
            titleBar.Controls.Add(new Label
            {
                Text = "👤  Patient Management", ForeColor = Color.White,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter
            });

            // ── Search & filter bar ───────────────────────────────────────────
            var searchPanel = new Panel { Location = new Point(10, 55), Size = new Size(610, 40) };
            txtSearch = new TextBox { PlaceholderText = "🔍 Search by name, phone or blood group...", Width = 320, Location = new Point(0, 8) };
            txtSearch.TextChanged += (s, e) => ApplyFilter();

            cmbGender = new ComboBox { Location = new Point(330, 8), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbGender.Items.AddRange(new[] { "All", "Male", "Female", "Other" });
            cmbGender.SelectedIndex = 0;
            cmbGender.SelectedIndexChanged += (s, e) => ApplyFilter();

            lblRecords = new Label { Location = new Point(445, 12), AutoSize = true, ForeColor = Color.Gray };
            searchPanel.Controls.AddRange(new Control[] { txtSearch, cmbGender, lblRecords });

            // ── Grid ──────────────────────────────────────────────────────────
            grid = new DataGridView
            {
                Location            = new Point(10, 100),
                Size                = new Size(610, 500),
                ReadOnly            = true,
                AllowUserToAddRows  = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode       = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor     = Color.White,
                RowHeadersVisible   = false,
                MultiSelect         = false,
                AllowUserToOrderColumns = true,   // BONUS: sortable columns
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(30,58,95),
                    ForeColor = Color.White,
                    Font      = new Font("Segoe UI", 9f, FontStyle.Bold)
                }
            };
            grid.Columns.AddRange(
                new DataGridViewTextBoxColumn { HeaderText = "ID",       DataPropertyName = "PatientId",   Width = 45, SortMode = DataGridViewColumnSortMode.Automatic },
                new DataGridViewTextBoxColumn { HeaderText = "Full Name",DataPropertyName = "FullName",    SortMode = DataGridViewColumnSortMode.Automatic },
                new DataGridViewTextBoxColumn { HeaderText = "Gender",   DataPropertyName = "Gender",      Width = 65, SortMode = DataGridViewColumnSortMode.Automatic },
                new DataGridViewTextBoxColumn { HeaderText = "Age",      DataPropertyName = "Age",         Width = 50, SortMode = DataGridViewColumnSortMode.Automatic },
                new DataGridViewTextBoxColumn { HeaderText = "Phone",    DataPropertyName = "PhoneNumber", SortMode = DataGridViewColumnSortMode.Automatic },
                new DataGridViewTextBoxColumn { HeaderText = "Blood",    DataPropertyName = "BloodGroup",  Width = 60, SortMode = DataGridViewColumnSortMode.Automatic }
            );
            grid.SelectionChanged += Grid_SelectionChanged;
            grid.ClearSelection();

            // ── Right form ────────────────────────────────────────────────────
            var rightPanel = new Panel
            {
                Location  = new Point(630, 45),
                Size      = new Size(400, 565),
                BackColor = Color.White
            };

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(12),
                AutoSize = true
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            int row = 0;
            void R(string lbl, Control ctrl)
            {
                tbl.Controls.Add(new Label { Text = lbl, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 9f, FontStyle.Bold) }, 0, row);
                ctrl.Dock = DockStyle.Fill;
                tbl.Controls.Add(ctrl, 1, row);
                tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
                row++;
            }

            txtName   = new TextBox();
            cmbGenderF = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cmbGenderF.Items.AddRange(new[] { "Male", "Female", "Other" });
            dtpDOB    = new DateTimePicker { Format = DateTimePickerFormat.Short };
            txtPhone  = new TextBox();
            txtEmail  = new TextBox();
            txtAddress = new TextBox { Multiline = true, Height = 55 };
            cmbBlood  = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cmbBlood.Items.AddRange(new[] { "A+","A-","B+","B-","AB+","AB-","O+","O-" });

            R("Full Name *", txtName);
            R("Gender *",    cmbGenderF);
            R("Date of Birth",dtpDOB);
            R("Phone *",     txtPhone);
            R("Email",       txtEmail);
            R("Address",     txtAddress);
            R("Blood Group", cmbBlood);

            // Buttons
            var btnRow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Dock          = DockStyle.Fill, Padding = new Padding(0, 6, 0, 0)
            };
            btnSave   = Btn("💾 Save",   Color.FromArgb(52,152,219));
            btnDelete = Btn("🗑 Delete", Color.FromArgb(231,76,60));
            btnClear  = Btn("✖ Clear",  Color.FromArgb(149,165,166));
            btnDelete.Enabled = false;
            btnSave.Click    += BtnSave_Click;
            btnDelete.Click  += BtnDelete_Click;
            btnClear.Click   += (s,e) => ClearForm();
            btnRow.Controls.AddRange(new Control[] { btnSave, btnDelete, btnClear });
            tbl.SetColumnSpan(btnRow, 2);
            tbl.Controls.Add(btnRow, 0, row);
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

            rightPanel.Controls.Add(tbl);

            Controls.Add(rightPanel);
            Controls.Add(grid);
            Controls.Add(searchPanel);
            Controls.Add(titleBar);
        }

        private void LoadPatients(List<Patient>? list = null)
        {
            var data = list ?? _service.GetAllPatients();
            grid.DataSource = data;
            grid.ClearSelection();
            lblRecords.Text = $"{data.Count} record(s)";
        }

        private void ApplyFilter()
        {
            string kw     = txtSearch.Text.Trim();
            string gender = cmbGender.Text;

            var list = string.IsNullOrWhiteSpace(kw)
                ? _service.GetAllPatients()
                : _service.SearchPatients(kw);

            if (gender != "All")
                list = list.Where(p => p.Gender == gender).ToList();

            LoadPatients(list);
        }

        private void Grid_SelectionChanged(object? sender, EventArgs e)
        {
            if (grid.CurrentRow?.DataBoundItem is Patient p)
            {
                _selectedId        = p.PatientId;
                txtName.Text       = p.FullName;
                cmbGenderF.Text    = p.Gender;
                dtpDOB.Value       = p.DateOfBirth;
                txtPhone.Text      = p.PhoneNumber;
                txtEmail.Text      = p.Email;
                txtAddress.Text    = p.Address;
                cmbBlood.Text      = p.BloodGroup;
                btnDelete.Enabled  = true;
                btnSave.Text       = "💾 Update";
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            // ── Validation ────────────────────────────────────────────────────
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(txtName.Text))   errors.Add("• Full Name is required.");
            if (cmbGenderF.SelectedIndex < 0)              errors.Add("• Gender is required.");
            if (string.IsNullOrWhiteSpace(txtPhone.Text))  errors.Add("• Phone Number is required.");
            if (txtPhone.Text.Trim().Length < 7)           errors.Add("• Phone Number is too short.");
            if (!string.IsNullOrWhiteSpace(txtEmail.Text) && !txtEmail.Text.Contains("@"))
                                                           errors.Add("• Email format is invalid.");
            if (errors.Count > 0)
            {
                MessageBox.Show("Please fix the following:\n\n" + string.Join("\n", errors),
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var p = new Patient
            {
                PatientId   = _selectedId,
                FullName    = txtName.Text.Trim(),
                Gender      = cmbGenderF.Text,
                DateOfBirth = dtpDOB.Value.Date,
                PhoneNumber = txtPhone.Text.Trim(),
                Email       = txtEmail.Text.Trim(),
                Address     = txtAddress.Text.Trim(),
                BloodGroup  = cmbBlood.Text
            };

            bool ok = _selectedId == 0 ? _service.AddPatient(p) : _service.UpdatePatient(p);
            if (ok)
            {
                MessageBox.Show(_selectedId == 0 ? "Patient added successfully." : "Patient updated successfully.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                LoadPatients();
            }
            else MessageBox.Show("Operation failed. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (_selectedId == 0) return;
            var p = grid.CurrentRow?.DataBoundItem as Patient;
            if (MessageBox.Show($"Delete patient '{p?.FullName}'?\nThis will also delete their appointments.",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                if (_service.DeletePatient(_selectedId))
                {
                    MessageBox.Show("Patient deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm(); LoadPatients();
                }
            }
        }

        private void ClearForm()
        {
            _selectedId = 0;
            txtName.Text = txtPhone.Text = txtEmail.Text = txtAddress.Text = "";
            cmbGenderF.SelectedIndex = cmbBlood.SelectedIndex = -1;
            dtpDOB.Value = DateTime.Today;
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
