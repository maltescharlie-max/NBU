using System.ComponentModel.DataAnnotations;
using NBU.Data.Models;

namespace NBU.Desktop;

internal sealed class StudentFormDialog : Form
{
    private static readonly Color Navy = Color.FromArgb(9, 39, 84);
    private readonly int _studentId;
    private readonly Dictionary<string, Control> _fields = new();
    private readonly ComboBox _status = new();

    public Student Student { get; private set; } = new();

    public StudentFormDialog(Student? student = null)
    {
        _studentId = student?.Id ?? 0;
        Text = student is null ? "Registrar estudiante · NBU" : "Editar estudiante · NBU";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(590, 675);
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9.5F);

        var heading = new Label { Text = student is null ? "Nuevo estudiante" : "Editar expediente", ForeColor = Navy, Font = new Font("Segoe UI", 20F, FontStyle.Bold), AutoSize = true, Location = new Point(27, 22) };
        var subtitle = new Label { Text = "Completa la información del estudiante beneficiario.", ForeColor = Color.FromArgb(114, 129, 152), Font = new Font("Segoe UI", 9F), AutoSize = true, Location = new Point(30, 59) };
        Controls.Add(heading);
        Controls.Add(subtitle);

        var layout = new TableLayoutPanel { Location = new Point(30, 104), Size = new Size(530, 480), ColumnCount = 2, RowCount = 9, AutoScroll = true, Padding = new Padding(0), BackColor = Color.White };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        AddTextField(layout, "StudentCode", "Código estudiantil", student?.StudentCode, 0, 0, "Ej. NBU-2026-001");
        AddTextField(layout, "NationalId", "Cédula", student?.NationalId, 0, 1, "Número de cédula");
        AddTextField(layout, "FirstName", "Nombres", student?.FirstName, 1, 0, "Nombres");
        AddTextField(layout, "LastName", "Apellidos", student?.LastName, 1, 1, "Apellidos");
        AddTextField(layout, "Email", "Correo electrónico", student?.Email, 2, 0, "correo@universidad.edu.ni");
        AddTextField(layout, "Phone", "Teléfono", student?.Phone, 2, 1, "Teléfono (opcional)");
        AddTextField(layout, "Career", "Carrera", student?.Career, 3, 0, "Carrera universitaria");
        AddTextField(layout, "University", "Universidad", student?.University, 3, 1, "Universidad");
        _status.Items.AddRange([new StatusItem("Active", "Activo"), new StatusItem("Inactive", "Inactivo")]);
        _status.DisplayMember = nameof(StatusItem.Label);
        _status.ValueMember = nameof(StatusItem.Value);
        _status.SelectedValue = student?.Status ?? "Active";
        _status.DropDownStyle = ComboBoxStyle.DropDownList;
        _status.Font = new Font("Segoe UI", 9F);
        _status.Height = 38;
        AddField(layout, "Estado", _status, 4, 0);
        Controls.Add(layout);

        var cancel = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel, Location = new Point(337, 615), Size = new Size(105, 38), FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Color.FromArgb(72, 94, 117), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
        cancel.FlatAppearance.BorderColor = Color.FromArgb(220, 229, 237);
        var save = new Button { Text = "Guardar", Location = new Point(452, 615), Size = new Size(108, 38), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(81, 218, 210), ForeColor = Navy, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
        save.Click += (_, _) => SaveStudent();
        Controls.Add(cancel);
        Controls.Add(save);
        AcceptButton = save;
        CancelButton = cancel;
    }

    private void AddTextField(TableLayoutPanel layout, string key, string label, string? value, int row, int column, string placeholder)
    {
        var input = new TextBox { Text = value ?? string.Empty, PlaceholderText = placeholder, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 9F), Height = 37, Dock = DockStyle.Top };
        _fields[key] = input;
        AddField(layout, label, input, row, column);
    }

    private static void AddField(TableLayoutPanel layout, string label, Control input, int row, int column)
    {
        var container = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5, 4, 9, 8), Height = 83 };
        var labelControl = new Label { Text = label, ForeColor = Color.FromArgb(67, 86, 109), Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Dock = DockStyle.Top, Height = 24 };
        container.Controls.Add(input);
        container.Controls.Add(labelControl);
        layout.Controls.Add(container, column, row);
    }

    private void SaveStudent()
    {
        var status = _status.SelectedItem is StatusItem selected ? selected.Value : "Active";
        var student = new Student
        {
            Id = _studentId,
            StudentCode = Value("StudentCode"),
            NationalId = Value("NationalId"),
            FirstName = Value("FirstName"),
            LastName = Value("LastName"),
            Email = Value("Email"),
            Phone = Value("Phone"),
            Career = Value("Career"),
            University = Value("University"),
            Status = status
        };
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(student, new ValidationContext(student), results, validateAllProperties: true))
        {
            MessageBox.Show(string.Join(Environment.NewLine, results.Select(item => "• " + item.ErrorMessage)), "Revisa los datos", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        Student = student;
        DialogResult = DialogResult.OK;
        Close();
    }

    private string Value(string key) => _fields[key].Text.Trim();
    private sealed record StatusItem(string Value, string Label) { public override string ToString() => Label; }
}
