using App.Core.Interfaces;
using App.Core.Models;

namespace App.WindowsApp.Forms
{
    /// <summary>
    /// Charting Module — draws 4 charts using GDI+ (no third-party library required).
    /// Charts: Pie (by status), Bar (by doctor), Line (last 7 days), Bar (by specialization).
    /// </summary>
    public class ChartForm : Form
    {
        private readonly IReportService _report;

        // Chart panels
        private Panel pnlPie   = null!;
        private Panel pnlBar   = null!;
        private Panel pnlLine  = null!;
        private Panel pnlSpec  = null!;

        // Chart data
        private List<ChartDataPoint> _statusData  = new();
        private List<ChartDataPoint> _doctorData  = new();
        private List<ChartDataPoint> _dailyData   = new();
        private List<ChartDataPoint> _specData    = new();

        // Palette
        private static readonly Color[] Palette = new[]
        {
            Color.FromArgb(52,152,219),   // blue
            Color.FromArgb(46,204,113),   // green
            Color.FromArgb(231,76,60),    // red
            Color.FromArgb(241,196,15),   // yellow
            Color.FromArgb(155,89,182),   // purple
            Color.FromArgb(26,188,156),   // teal
            Color.FromArgb(230,126,34),   // orange
            Color.FromArgb(52,73,94)      // dark
        };

        public ChartForm(IReportService report)
        {
            _report = report;
            BuildUI();
            LoadData();
        }

        private void BuildUI()
        {
            Text            = "📊 Charts & Reports";
            Size            = new Size(1100, 700);
            StartPosition   = FormStartPosition.CenterParent;
            BackColor       = Color.FromArgb(245, 247, 250);
            Font            = new Font("Segoe UI", 9f);

            // ── Title bar ─────────────────────────────────────────────────────
            var titleBar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 50,
                BackColor = Color.FromArgb(30, 58, 95)
            };
            titleBar.Controls.Add(new Label
            {
                Text      = "📊  Clinic Analytics & Reports",
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 14f, FontStyle.Bold),
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            });

            // ── Refresh button ────────────────────────────────────────────────
            var btnRefresh = new Button
            {
                Text      = "🔄 Refresh",
                Size      = new Size(100, 32),
                Location  = new Point(980, 9),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadData();
            titleBar.Controls.Add(btnRefresh);

            // ── Chart grid (2x2) ──────────────────────────────────────────────
            var grid = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 2,
                Padding     = new Padding(10),
                BackColor   = Color.FromArgb(245, 247, 250)
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            pnlPie  = MakeChartPanel("Appointments by Status (Pie)");
            pnlBar  = MakeChartPanel("Appointments by Doctor (Bar)");
            pnlLine = MakeChartPanel("Daily Appointments — Last 7 Days (Line)");
            pnlSpec = MakeChartPanel("Appointments by Specialization (Bar)");

            pnlPie.Paint  += PaintPieChart;
            pnlBar.Paint  += PaintDoctorBar;
            pnlLine.Paint += PaintLineChart;
            pnlSpec.Paint += PaintSpecBar;

            grid.Controls.Add(pnlPie,  0, 0);
            grid.Controls.Add(pnlBar,  1, 0);
            grid.Controls.Add(pnlLine, 0, 1);
            grid.Controls.Add(pnlSpec, 1, 1);

            Controls.Add(grid);
            Controls.Add(titleBar);
        }

        private static Panel MakeChartPanel(string title)
        {
            var outer = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.White,
                Margin    = new Padding(6),
                Padding   = new Padding(8)
            };
            outer.Controls.Add(new Label
            {
                Text      = title,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 58, 95),
                Dock      = DockStyle.Top,
                Height    = 24
            });
            // The panel itself is the drawing surface — tag it with title
            outer.Tag = title;
            return outer;
        }

        private void LoadData()
        {
            try
            {
                _statusData = _report.GetAppointmentsByStatus();
                _doctorData = _report.GetAppointmentsByDoctor();
                _dailyData  = _report.GetDailyAppointmentsLast7Days();
                _specData   = _report.GetAppointmentsBySpecialization();

                pnlPie.Invalidate();
                pnlBar.Invalidate();
                pnlLine.Invalidate();
                pnlSpec.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading chart data:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ── PIE CHART ─────────────────────────────────────────────────────────
        private void PaintPieChart(object? sender, PaintEventArgs e)
        {
            var g    = e.Graphics;
            var rect = GetDrawArea((Panel)sender!);
            if (_statusData.Count == 0) { DrawNoData(g, rect); return; }

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            double total  = _statusData.Sum(d => d.Value);
            float  startA = -90f;
            int    cx     = rect.X + rect.Width / 2;
            int    cy     = rect.Y + rect.Height / 2;
            int    radius = Math.Min(rect.Width, rect.Height) / 2 - 10;
            var    pieRect = new Rectangle(cx - radius, cy - radius, radius * 2, radius * 2);

            for (int i = 0; i < _statusData.Count; i++)
            {
                float sweep = (float)(_statusData[i].Value / total * 360.0);
                using var brush = new SolidBrush(Palette[i % Palette.Length]);
                g.FillPie(brush, pieRect, startA, sweep);
                g.DrawPie(Pens.White, pieRect, startA, sweep);

                // Legend dot
                int ly = rect.Bottom - (_statusData.Count - i) * 18;
                using var lb = new SolidBrush(Palette[i % Palette.Length]);
                g.FillEllipse(lb, rect.X, ly, 12, 12);
                g.DrawString($"{_statusData[i].Label} ({_statusData[i].Value})",
                    new Font("Segoe UI", 8f), Brushes.Black, rect.X + 16, ly);
                startA += sweep;
            }
        }

        // ── BAR CHART (Doctor) ────────────────────────────────────────────────
        private void PaintDoctorBar(object? sender, PaintEventArgs e) =>
            DrawBarChart(e.Graphics, GetDrawArea((Panel)sender!), _doctorData, Color.FromArgb(52, 152, 219));

        // ── LINE CHART ────────────────────────────────────────────────────────
        private void PaintLineChart(object? sender, PaintEventArgs e)
        {
            var g    = e.Graphics;
            var rect = GetDrawArea((Panel)sender!);
            if (_dailyData.Count == 0) { DrawNoData(g, rect); return; }

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            double maxVal = _dailyData.Max(d => d.Value);
            if (maxVal == 0) maxVal = 1;

            int    pad   = 40;
            int    chartW = rect.Width  - pad * 2;
            int    chartH = rect.Height - pad * 2;
            float  stepX  = (float)chartW / Math.Max(_dailyData.Count - 1, 1);

            // Axes
            g.DrawLine(Pens.LightGray, rect.X + pad, rect.Y + pad, rect.X + pad, rect.Y + pad + chartH);
            g.DrawLine(Pens.LightGray, rect.X + pad, rect.Y + pad + chartH, rect.X + pad + chartW, rect.Y + pad + chartH);

            // Grid lines
            for (int i = 1; i <= 4; i++)
            {
                int gy = rect.Y + pad + (int)(chartH - chartH * i / 4.0);
                g.DrawLine(new Pen(Color.FromArgb(230, 230, 230)), rect.X + pad, gy, rect.X + pad + chartW, gy);
                g.DrawString((maxVal * i / 4).ToString("0"), new Font("Segoe UI", 7f), Brushes.Gray, rect.X + 2, gy - 6);
            }

            // Line + points
            var points = new PointF[_dailyData.Count];
            for (int i = 0; i < _dailyData.Count; i++)
            {
                float px = rect.X + pad + stepX * i;
                float py = rect.Y + pad + chartH - (float)(_dailyData[i].Value / maxVal * chartH);
                points[i] = new PointF(px, py);

                // X label
                g.DrawString(_dailyData[i].Label, new Font("Segoe UI", 7f), Brushes.Gray,
                    px - 15, rect.Y + pad + chartH + 4);
            }

            if (points.Length > 1)
            {
                using var linePen = new Pen(Color.FromArgb(52, 152, 219), 2.5f);
                g.DrawLines(linePen, points);
            }

            // Dots + value labels
            foreach (var pt in points)
            {
                g.FillEllipse(new SolidBrush(Color.FromArgb(52, 152, 219)), pt.X - 5, pt.Y - 5, 10, 10);
                g.FillEllipse(Brushes.White, pt.X - 3, pt.Y - 3, 6, 6);
            }
            for (int i = 0; i < _dailyData.Count; i++)
                g.DrawString(_dailyData[i].Value.ToString("0"), new Font("Segoe UI", 8f, FontStyle.Bold),
                    Brushes.Black, points[i].X - 5, points[i].Y - 18);
        }

        // ── BAR CHART (Specialization) ────────────────────────────────────────
        private void PaintSpecBar(object? sender, PaintEventArgs e) =>
            DrawBarChart(e.Graphics, GetDrawArea((Panel)sender!), _specData, Color.FromArgb(46, 204, 113));

        // ── Shared bar chart renderer ─────────────────────────────────────────
        private void DrawBarChart(Graphics g, Rectangle rect, List<ChartDataPoint> data, Color baseColor)
        {
            if (data.Count == 0) { DrawNoData(g, rect); return; }

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            double maxVal  = data.Max(d => d.Value);
            if (maxVal == 0) maxVal = 1;

            int pad     = 45;
            int chartW  = rect.Width  - pad * 2;
            int chartH  = rect.Height - pad * 2;
            int barCount= data.Count;
            int barW    = Math.Max(10, chartW / barCount - 8);

            // Axes
            g.DrawLine(Pens.LightGray, rect.X + pad, rect.Y + pad, rect.X + pad, rect.Y + pad + chartH);
            g.DrawLine(Pens.LightGray, rect.X + pad, rect.Y + pad + chartH, rect.X + pad + chartW, rect.Y + pad + chartH);

            for (int i = 0; i < barCount; i++)
            {
                int   barH  = (int)(data[i].Value / maxVal * chartH);
                int   bx    = rect.X + pad + i * (chartW / barCount) + 4;
                int   by    = rect.Y + pad + chartH - barH;
                Color c     = Palette[i % Palette.Length];

                using var brush = new SolidBrush(c);
                g.FillRectangle(brush, bx, by, barW, barH);

                // Value label on top
                g.DrawString(data[i].Value.ToString("0"),
                    new Font("Segoe UI", 8f, FontStyle.Bold), Brushes.Black, bx, by - 14);

                // X label — truncate long names
                string lbl = data[i].Label.Length > 10 ? data[i].Label[..10] + "…" : data[i].Label;
                g.DrawString(lbl, new Font("Segoe UI", 7f), Brushes.Gray,
                    bx, rect.Y + pad + chartH + 4);
            }

            // Y axis ticks
            for (int i = 1; i <= 4; i++)
            {
                int gy = rect.Y + pad + (int)(chartH - chartH * i / 4.0);
                g.DrawLine(new Pen(Color.FromArgb(230, 230, 230)), rect.X + pad, gy, rect.X + pad + chartW, gy);
                g.DrawString((maxVal * i / 4).ToString("0"), new Font("Segoe UI", 7f), Brushes.Gray, rect.X + 2, gy - 6);
            }
        }

        private static void DrawNoData(Graphics g, Rectangle rect)
        {
            g.DrawString("No data available", new Font("Segoe UI", 10f, FontStyle.Italic),
                Brushes.Gray, rect.X + 10, rect.Y + rect.Height / 2 - 10);
        }

        private static Rectangle GetDrawArea(Panel panel)
        {
            // Offset for the title label at top
            return new Rectangle(5, 28, panel.ClientSize.Width - 10, panel.ClientSize.Height - 32);
        }
    }
}
