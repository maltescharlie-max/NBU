using Microsoft.EntityFrameworkCore;
using NBU.Data;
using NBU.Data.Models;

namespace NBU.Desktop;

public partial class Form1 : Form
{
    private static readonly Color Navy = Color.FromArgb(9, 39, 84);
    private static readonly Color Cyan = Color.FromArgb(32, 182, 195);
    private static readonly Color Ink = Color.FromArgb(24, 43, 67);
    private static readonly Color Muted = Color.FromArgb(114, 129, 152);
    private static readonly Color Canvas = Color.FromArgb(245, 248, 252);

    private readonly DbContextOptions<AppDbContext> _dbOptions;
    private readonly Panel _content = new();
    private readonly Label _pageTitle = new();
    private readonly Label _pageSubtitle = new();
    private readonly Panel _pageBody = new();
    private readonly FlowLayoutPanel _sideMenu = new();
    private readonly List<Button> _navigationButtons = [];

    public Form1(DbContextOptions<AppDbContext> dbOptions)
    {
        InitializeComponent();
        _dbOptions = dbOptions;
        BuildShell();
        ShowDashboard();
    }

    private void BuildShell()
    {
        Text = "NBU · Nicaragua Becas Universitarias";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1000, 680);
        Size = new Size(1320, 820);
        BackColor = Canvas;
        Font = new Font("Segoe UI", 10F);

        var sidebar = new Panel { Dock = DockStyle.Left, Width = 240, BackColor = Navy, Padding = new Padding(17, 22, 17, 16) };
        var brand = new Label { Text = "NBU", ForeColor = Color.White, Font = new Font("Segoe UI", 24F, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };
        var brandSub = new Label { Text = "NICARAGUA BECAS\nUNIVERSITARIAS", ForeColor = Color.FromArgb(175, 202, 221), Font = new Font("Segoe UI", 8F, FontStyle.Bold), AutoSize = true, Location = new Point(22, 60) };
        var divider = new Panel { BackColor = Color.FromArgb(36, 77, 119), Height = 1, Dock = DockStyle.Top, Margin = new Padding(0, 14, 0, 13) };
        _sideMenu.Dock = DockStyle.Top;
        _sideMenu.AutoSize = true;
        _sideMenu.FlowDirection = FlowDirection.TopDown;
        _sideMenu.WrapContents = false;
        _sideMenu.Margin = new Padding(0, 22, 0, 0);
        _sideMenu.Padding = new Padding(0, 18, 0, 0);

        AddNavigation("Inicio", () => { ShowDashboard(); return Task.CompletedTask; });
        AddNavigation("Estudiantes", async () => await ShowStudentsAsync());
        AddNavigation("Becas", () => OpenManager(ModuleForms.Scholarships(_dbOptions)));
        AddNavigation("Solicitudes", () => OpenManager(ModuleForms.Applications(_dbOptions)));
        AddNavigation("Pagos", () => OpenManager(ModuleForms.Payments(_dbOptions)));
        AddNavigation("Noticias y eventos", () => OpenManager(ModuleForms.Publications(_dbOptions)));
        AddNavigation("Notificaciones", () => OpenManager(ModuleForms.Notifications(_dbOptions)));

        var motto = new Label { Text = "TU ESFUERZO\nABRE OPORTUNIDADES", ForeColor = Color.FromArgb(117, 222, 219), Font = new Font("Segoe UI", 9F, FontStyle.Bold), AutoSize = true, Dock = DockStyle.Bottom, Padding = new Padding(4, 16, 0, 4) };
        sidebar.Controls.Add(motto);
        sidebar.Controls.Add(_sideMenu);
        sidebar.Controls.Add(divider);
        sidebar.Controls.Add(brandSub);
        sidebar.Controls.Add(brand);
        Controls.Add(sidebar);

        var header = new Panel { Dock = DockStyle.Top, Height = 83, BackColor = Color.White, Padding = new Padding(30, 15, 30, 12) };
        _pageTitle.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
        _pageTitle.ForeColor = Navy;
        _pageTitle.AutoSize = true;
        _pageTitle.Location = new Point(29, 12);
        _pageSubtitle.Font = new Font("Segoe UI", 9F);
        _pageSubtitle.ForeColor = Muted;
        _pageSubtitle.AutoSize = true;
        _pageSubtitle.Location = new Point(31, 48);
        header.Controls.Add(_pageTitle);
        header.Controls.Add(_pageSubtitle);
        _content.Dock = DockStyle.Fill;
        _content.BackColor = Canvas;
        _content.Controls.Add(header);
        _pageBody.Dock = DockStyle.Fill;
        _pageBody.Padding = new Padding(30, 26, 30, 30);
        _pageBody.BackColor = Canvas;
        _content.Controls.Add(_pageBody);
        header.BringToFront();
        Controls.Add(_content);
    }

    private void AddNavigation(string label, Func<Task> action)
    {
        var button = new Button
        {
            Text = "   " + label,
            Width = 202,
            Height = 45,
            FlatStyle = FlatStyle.Flat,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(207, 221, 237),
            BackColor = Navy,
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 0, 4),
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(21, 67, 111);
        button.Click += async (_, _) => await action();
        _sideMenu.Controls.Add(button);
        _navigationButtons.Add(button);
    }

    private void SetActiveNavigation(int index)
    {
        for (var i = 0; i < _navigationButtons.Count; i++)
        {
            _navigationButtons[i].BackColor = i == index ? Color.FromArgb(17, 73, 118) : Navy;
            _navigationButtons[i].ForeColor = i == index ? Color.White : Color.FromArgb(207, 221, 237);
            _navigationButtons[i].FlatAppearance.BorderColor = i == index ? Cyan : Navy;
            _navigationButtons[i].FlatAppearance.BorderSize = i == index ? 2 : 0;
        }
    }

    private void ShowDashboard()
    {
        SetActiveNavigation(0);
        _pageTitle.Text = "Bienvenido al panel NBU";
        _pageSubtitle.Text = "Tu esfuerzo abre oportunidades · Administración universitaria";
        _pageBody.Controls.Clear();
        var panel = MakeCard(new Rectangle(30, 30, Math.Max(700, _pageBody.ClientSize.Width - 75), 230));
        panel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        panel.BackColor = Color.FromArgb(11, 55, 99);
        var title = new Label { Text = "Acompañamos el\ntalento universitario.", ForeColor = Color.White, Font = new Font("Segoe UI", 24F, FontStyle.Bold), AutoSize = true, Location = new Point(28, 29) };
        var subtitle = new Label { Text = "Gestiona becas, beneficiarios y avisos desde un mismo espacio.", ForeColor = Color.FromArgb(202, 224, 239), Font = new Font("Segoe UI", 10F), AutoSize = true, Location = new Point(31, 104) };
        var action = MakeButton("Abrir estudiantes  →", true);
        action.Location = new Point(30, 157);
        action.Click += async (_, _) => await ShowStudentsAsync();
        var accent = new Label { Text = "✦", ForeColor = Color.FromArgb(255, 194, 63), Font = new Font("Segoe UI", 68F, FontStyle.Bold), AutoSize = true, Location = new Point(Math.Max(540, panel.Width - 160), 45), Anchor = AnchorStyles.Top | AnchorStyles.Right };
        panel.Controls.Add(title);
        panel.Controls.Add(subtitle);
        panel.Controls.Add(action);
        panel.Controls.Add(accent);
        _pageBody.Controls.Add(panel);

        var section = new Label { Text = "MÓDULOS DEL SISTEMA", ForeColor = Color.FromArgb(21, 153, 164), Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), AutoSize = true, Location = new Point(32, 292) };
        _pageBody.Controls.Add(section);
        AddDashboardCard("Estudiantes", "Registra y actualiza expedientes.", 30, 320, () => ShowStudentsAsync());
        AddDashboardCard("Becas y solicitudes", "Convocatorias y decisiones.", 322, 320, () => OpenManager(ModuleForms.Scholarships(_dbOptions)));
        AddDashboardCard("Pagos", "Registro de depósitos.", 30, 495, () => OpenManager(ModuleForms.Payments(_dbOptions)));
        AddDashboardCard("Noticias y eventos", "Avisos para la comunidad.", 322, 495, () => OpenManager(ModuleForms.Publications(_dbOptions)));
    }

    private void AddDashboardCard(string title, string description, int x, int y, Func<Task> action)
    {
        var card = MakeCard(new Rectangle(x, y, 270, 155));
        var heading = new Label { Text = title, ForeColor = Navy, Font = new Font("Segoe UI", 12F, FontStyle.Bold), AutoSize = true, Location = new Point(18, 19) };
        var body = new Label { Text = description, ForeColor = Muted, Font = new Font("Segoe UI", 9F), AutoSize = false, Size = new Size(220, 40), Location = new Point(19, 53) };
        var open = new LinkLabel { Text = "Abrir módulo  →", LinkColor = Color.FromArgb(17, 143, 151), Font = new Font("Segoe UI", 9F, FontStyle.Bold), AutoSize = true, Location = new Point(19, 111) };
        open.LinkClicked += async (_, _) => await action();
        card.Controls.Add(heading);
        card.Controls.Add(body);
        card.Controls.Add(open);
        _pageBody.Controls.Add(card);
    }

    private async Task ShowStudentsAsync()
    {
        SetActiveNavigation(1);
        _pageTitle.Text = "Estudiantes";
        _pageSubtitle.Text = "Directorio de estudiantes y beneficiarios";
        _pageBody.Controls.Clear();

        var tools = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Canvas };
        var add = MakeButton("＋  Nuevo estudiante", true);
        add.Dock = DockStyle.Right;
        add.Width = 185;
        var delete = MakeButton("Eliminar", false);
        delete.Dock = DockStyle.Right;
        delete.Width = 110;
        delete.Margin = new Padding(0, 0, 10, 0);
        var edit = MakeButton("Editar", false);
        edit.Dock = DockStyle.Right;
        edit.Width = 95;
        edit.Margin = new Padding(0, 0, 10, 0);
        var search = new TextBox { Width = 280, Height = 36, Font = new Font("Segoe UI", 10F), PlaceholderText = "Buscar estudiante…", BorderStyle = BorderStyle.FixedSingle, Location = new Point(0, 9) };
        var searchButton = MakeButton("Buscar", false);
        searchButton.Location = new Point(290, 6);
        tools.Controls.Add(add);
        tools.Controls.Add(delete);
        tools.Controls.Add(edit);
        tools.Controls.Add(search);
        tools.Controls.Add(searchButton);
        _pageBody.Controls.Add(tools);

        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false,
            Font = new Font("Segoe UI", 9F),
            GridColor = Color.FromArgb(232, 237, 242),
            EnableHeadersVisualStyles = false,
            ColumnHeadersHeight = 42,
            RowTemplate = { Height = 40 }
        };
        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(246, 249, 252), ForeColor = Muted, Font = new Font("Segoe UI", 8F, FontStyle.Bold), SelectionBackColor = Color.FromArgb(246, 249, 252), SelectionForeColor = Muted };
        grid.DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.White, ForeColor = Ink, SelectionBackColor = Color.FromArgb(222, 245, 244), SelectionForeColor = Navy, Padding = new Padding(6, 3, 6, 3) };
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", DataPropertyName = "Id", FillWeight = 35 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Estudiante", DataPropertyName = "Name", FillWeight = 125 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Código", DataPropertyName = "StudentCode", FillWeight = 65 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Carrera", DataPropertyName = "Career", FillWeight = 100 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Universidad", DataPropertyName = "University", FillWeight = 90 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Estado", DataPropertyName = "Status", FillWeight = 50 });
        grid.Columns["Id"]!.Visible = false;
        var card = MakeCard(new Rectangle(0, 0, 1000, 100));
        card.Dock = DockStyle.Fill;
        card.Padding = new Padding(12);
        card.Controls.Add(grid);
        _pageBody.Controls.Add(card);
        tools.BringToFront();

        async Task Reload(string? term = null)
        {
            try
            {
                await using var db = new AppDbContext(_dbOptions);
                var query = db.Students.AsNoTracking();
                if (!string.IsNullOrWhiteSpace(term))
                {
                    var value = term.Trim();
                    query = query.Where(item => item.StudentCode.Contains(value) || item.FirstName.Contains(value) || item.LastName.Contains(value) || item.Email.Contains(value));
                }
                var students = await query.OrderBy(item => item.LastName).ThenBy(item => item.FirstName).ToListAsync();
                grid.DataSource = students.Select(item => new StudentRow(item.Id, $"{item.FirstName} {item.LastName}", item.StudentCode, item.Career, item.University, item.Status == "Active" ? "Activo" : "Inactivo")).ToList();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                MessageBox.Show("No se pudo conectar con SQL Server. Revisa la cadena de conexión en appsettings.json y confirma que la base de datos esté creada.\n\n" + ex.Message, "Conexión a la base de datos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        searchButton.Click += async (_, _) => await Reload(search.Text);
        search.KeyDown += async (_, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; await Reload(search.Text); } };
        await Reload();

        async Task CreateStudentAsync()
        {
            using var form = new StudentFormDialog();
            if (form.ShowDialog(this) != DialogResult.OK) return;
            try
            {
                await using var db = new AppDbContext(_dbOptions);
                db.Students.Add(form.Student);
                await db.SaveChangesAsync();
                await Reload(search.Text);
            }
            catch (DbUpdateException)
            {
                MessageBox.Show("No se pudo guardar. Comprueba que el código, la cédula y el correo no estén registrados.", "Datos duplicados", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        async Task EditStudentAsync()
        {
            if (grid.CurrentRow?.Cells["Id"]?.Value is not int id) return;
            await using var db = new AppDbContext(_dbOptions);
            var student = await db.Students.FindAsync(id);
            if (student is null) return;
            using var form = new StudentFormDialog(student);
            if (form.ShowDialog(this) != DialogResult.OK) return;
            try
            {
                db.Students.Update(form.Student);
                await db.SaveChangesAsync();
                await Reload(search.Text);
            }
            catch (DbUpdateException)
            {
                MessageBox.Show("No se pudo actualizar. Comprueba que el código, la cédula y el correo no estén registrados en otro expediente.", "Datos duplicados", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        async Task DeleteStudentAsync()
        {
            if (grid.CurrentRow?.Cells["Id"]?.Value is not int id) return;
            var answer = MessageBox.Show("¿Eliminar el expediente seleccionado?", "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (answer != DialogResult.Yes) return;
            try
            {
                await using var db = new AppDbContext(_dbOptions);
                var student = await db.Students.FindAsync(id);
                if (student is not null) { db.Students.Remove(student); await db.SaveChangesAsync(); }
                await Reload(search.Text);
            }
            catch (DbUpdateException)
            {
                MessageBox.Show("No se puede eliminar este estudiante porque tiene solicitudes o notificaciones relacionadas.", "Historial relacionado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        add.Click += async (_, _) => await CreateStudentAsync();
        delete.Click += async (_, _) => await DeleteStudentAsync();
        edit.Click += async (_, _) => await EditStudentAsync();
        grid.CellDoubleClick += async (_, _) => await EditStudentAsync();
    }

    private Task OpenManager<T, TRow>(DesktopCrudManager<T, TRow> manager) where T : class, new() where TRow : class
    {
        using (manager) manager.ShowDialog(this);
        return Task.CompletedTask;
    }

    private static Panel MakeCard(Rectangle bounds) => new() { Bounds = bounds, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

    private static Button MakeButton(string text, bool primary) => new()
    {
        Text = text,
        Height = 40,
        FlatStyle = FlatStyle.Flat,
        BackColor = primary ? Color.FromArgb(81, 218, 210) : Color.White,
        ForeColor = primary ? Navy : Color.FromArgb(72, 94, 117),
        Font = new Font("Segoe UI", 9F, FontStyle.Bold),
        Cursor = Cursors.Hand,
        UseVisualStyleBackColor = false,
        FlatAppearance = { BorderSize = primary ? 0 : 1, BorderColor = Color.FromArgb(220, 229, 237) }
    };

    private sealed record StudentRow(int Id, string Name, string StudentCode, string Career, string University, string Status);
}
