using Microsoft.EntityFrameworkCore;
using NBU.Data;

namespace NBU.Desktop;

internal sealed class StudentRegistrationsForm : UserControl
{
    private static readonly Color Navy = Color.FromArgb(9, 39, 84);
    private static readonly Color Cyan = Color.FromArgb(32, 182, 195);
    private static readonly Color Ink = Color.FromArgb(24, 43, 67);
    private static readonly Color Muted = Color.FromArgb(114, 129, 152);
    private static readonly Color Canvas = Color.FromArgb(245, 248, 252);

    private readonly DbContextOptions<AppDbContext> _dbOptions;
    private readonly DataGridView _grid = new();
    private readonly Label _count = new();
    private readonly Button _approve = new();
    private readonly Button _reject = new();

    public StudentRegistrationsForm(DbContextOptions<AppDbContext> dbOptions)
    {
        _dbOptions = dbOptions;
        Dock = DockStyle.Fill;
        BackColor = Canvas;
        Font = new Font("Segoe UI", 9F);

        _grid.Dock = DockStyle.Fill;
        _grid.BackgroundColor = Color.White;
        _grid.BorderStyle = BorderStyle.FixedSingle;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.ReadOnly = true;
        _grid.MultiSelect = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.RowHeadersVisible = false;
        _grid.Font = new Font("Segoe UI", 9F);
        _grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        _grid.GridColor = Color.FromArgb(232, 237, 242);
        _grid.EnableHeadersVisualStyles = false;
        _grid.ColumnHeadersHeight = 42;
        _grid.RowTemplate.Height = 40;
        _grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.FromArgb(246, 249, 252),
            ForeColor = Muted,
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            SelectionBackColor = Color.FromArgb(246, 249, 252),
            SelectionForeColor = Muted
        };
        _grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.White,
            ForeColor = Ink,
            SelectionBackColor = Color.FromArgb(222, 245, 244),
            SelectionForeColor = Navy,
            Padding = new Padding(6, 5, 6, 5),
            WrapMode = DataGridViewTriState.True
        };
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", DataPropertyName = "Id", FillWeight = 30 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nombre", DataPropertyName = "Name", FillWeight = 140 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cédula", DataPropertyName = "NationalId", FillWeight = 80 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Correo", DataPropertyName = "Email", FillWeight = 125 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Teléfono", DataPropertyName = "Phone", FillWeight = 75 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Carrera", DataPropertyName = "Career", FillWeight = 105 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Universidad", DataPropertyName = "University", FillWeight = 95 });
        _grid.Columns["Id"]!.Visible = false;

        _count.AutoSize = true;
        _count.ForeColor = Navy;
        _count.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        _count.Anchor = AnchorStyles.Left;
        ConfigureButton(_approve, "Aceptar registro", primary: true);
        ConfigureButton(_reject, "Rechazar", primary: false);
        var refresh = new Button();
        ConfigureButton(refresh, "Actualizar", primary: false);
        _approve.Enabled = false;
        _reject.Enabled = false;

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 62,
            Padding = new Padding(0, 8, 0, 0),
            BackColor = Canvas,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };
        actions.Controls.Add(_count);
        actions.Controls.Add(_approve);
        actions.Controls.Add(_reject);
        actions.Controls.Add(refresh);
        actions.Resize += (_, _) => _count.Margin = new Padding(0, 11, Math.Max(12, actions.ClientSize.Width - 685), 0);

        var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 4, 0, 0), BackColor = Canvas };
        body.Controls.Add(_grid);
        body.Controls.Add(actions);
        Controls.Add(body);

        _grid.SelectionChanged += (_, _) => UpdateActionState();
        _approve.Click += async (_, _) => await ReviewAsync(approve: true);
        _reject.Click += async (_, _) => await ReviewAsync(approve: false);
        refresh.Click += async (_, _) => await ReloadAsync();
        Load += async (_, _) => await ReloadAsync();
    }

    private static void ConfigureButton(Button button, string text, bool primary)
    {
        button.Text = text;
        button.Width = primary ? 155 : 115;
        button.Height = 38;
        button.FlatStyle = FlatStyle.Flat;
        button.BackColor = primary ? Color.FromArgb(81, 218, 210) : Color.White;
        button.ForeColor = primary ? Navy : Color.FromArgb(72, 94, 117);
        button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        button.Cursor = Cursors.Hand;
        button.UseVisualStyleBackColor = false;
        button.FlatAppearance.BorderSize = primary ? 0 : 1;
        button.FlatAppearance.BorderColor = Color.FromArgb(220, 229, 237);
        button.Margin = new Padding(8, 0, 0, 0);
    }

    private async Task ReloadAsync()
    {
        try
        {
            await using var db = new AppDbContext(_dbOptions);
            var students = await db.Students.AsNoTracking()
                .Where(student => student.Status == "Pending")
                .OrderBy(student => student.LastName)
                .ThenBy(student => student.FirstName)
                .ToListAsync();
            _grid.DataSource = students.Select(student => new RegistrationRow(
                student.Id,
                $"{student.FirstName} {student.LastName}",
                student.NationalId,
                student.Email,
                student.Phone ?? "—",
                student.Career,
                student.University)).ToList();
            _count.Text = students.Count == 1 ? "1 solicitud pendiente" : $"{students.Count} solicitudes pendientes";
            UpdateActionState();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            MessageBox.Show("No se pudieron cargar los registros. Confirma que SQL Server esté encendido y que la base NBU exista.\n\n" + ex.Message,
                "Registros pendientes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void UpdateActionState()
    {
        var selected = _grid.CurrentRow is not null && _grid.CurrentRow.Cells["Id"].Value is int;
        _approve.Enabled = selected;
        _reject.Enabled = selected;
    }

    private async Task ReviewAsync(bool approve)
    {
        if (_grid.CurrentRow?.Cells["Id"].Value is not int id) return;

        string? reason = null;
        if (approve)
        {
            var confirm = MessageBox.Show("Al aceptar, la persona podrá ingresar al portal estudiantil. ¿Deseas continuar?",
                "Aceptar registro", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;
        }
        else
        {
            reason = AskRejectionReason();
            if (reason is null) return;
        }

        try
        {
            await using var db = new AppDbContext(_dbOptions);
            var student = await db.Students.SingleOrDefaultAsync(item => item.Id == id && item.Status == "Pending");
            if (student is null)
            {
                MessageBox.Show("Este registro ya fue revisado. Actualiza la lista.", "Registro actualizado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await ReloadAsync();
                return;
            }

            student.Status = approve ? "Active" : "Rejected";
            student.RejectionReason = approve ? null : reason;
            await db.SaveChangesAsync();
            MessageBox.Show(approve ? "Registro aceptado. La persona ya puede ingresar." : "Registro rechazado. La persona podrá corregir sus datos y volver a enviarlo.",
                "Revisión completada", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await ReloadAsync();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            MessageBox.Show("No se pudo guardar la decisión. Revisa la conexión con SQL Server.\n\n" + ex.Message,
                "No se guardó", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private string? AskRejectionReason()
    {
        using var dialog = new Form
        {
            Text = "Motivo del rechazo",
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            ClientSize = new Size(470, 255),
            BackColor = Color.White,
            Font = new Font("Segoe UI", 9F)
        };
        var label = new Label
        {
            Text = "Escribe qué dato debe corregir la persona:",
            Location = new Point(20, 20),
            Size = new Size(425, 25),
            ForeColor = Ink,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        var reason = new TextBox
        {
            Location = new Point(20, 52),
            Size = new Size(425, 115),
            Multiline = true,
            MaxLength = 500,
            ScrollBars = ScrollBars.Vertical,
            BorderStyle = BorderStyle.FixedSingle
        };
        var cancel = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel, Location = new Point(235, 195), Size = new Size(100, 38) };
        var save = new Button { Text = "Guardar motivo", Location = new Point(345, 195), Size = new Size(100, 38), BackColor = Color.FromArgb(81, 218, 210), ForeColor = Navy, FlatStyle = FlatStyle.Flat };
        save.FlatAppearance.BorderSize = 0;
        save.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(reason.Text))
            {
                MessageBox.Show(dialog, "Escribe un motivo para que la persona sepa qué corregir.", "Motivo requerido", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            dialog.DialogResult = DialogResult.OK;
            dialog.Close();
        };
        dialog.AcceptButton = save;
        dialog.CancelButton = cancel;
        dialog.Controls.AddRange([label, reason, cancel, save]);
        return dialog.ShowDialog(this) == DialogResult.OK ? reason.Text.Trim() : null;
    }

    private sealed record RegistrationRow(int Id, string Name, string NationalId, string Email, string Phone, string Career, string University);
}
