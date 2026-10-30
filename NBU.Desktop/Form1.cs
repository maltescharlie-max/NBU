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
    private readonly TableLayoutPanel _content = new();
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
        var workingArea = Screen.FromPoint(Cursor.Position).WorkingArea;
        var availableWidth = Math.Max(640, workingArea.Width - 24);
        var availableHeight = Math.Max(520, workingArea.Height - 24);
        MinimumSize = new Size(Math.Min(1000, availableWidth), Math.Min(680, availableHeight));
        Size = new Size(Math.Min(1320, availableWidth), Math.Min(820, availableHeight));
        WindowState = FormWindowState.Maximized;
        BackColor = Canvas;
        Font = new Font("Segoe UI", 10F);

        var sidebar = new Panel { Dock = DockStyle.Fill, BackColor = Navy, Padding = new Padding(17, 22, 17, 16) };
        var brandLogo = new PictureBox { Image = NbuBranding.LoadMark(), SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.Transparent, Location = new Point(18, 19), Size = new Size(54, 54) };
        var brand = new Label { Text = "NBU", ForeColor = Color.White, Font = new Font("Segoe UI", 22F, FontStyle.Bold), AutoSize = true, Location = new Point(80, 19) };
        var brandSub = new Label { Text = "NICARAGUA BECAS\nUNIVERSITARIAS", ForeColor = Color.FromArgb(212, 229, 245), Font = new Font("Segoe UI", 7.5F, FontStyle.Bold), AutoSize = true, Location = new Point(82, 51) };
        var divider = new Panel { BackColor = Color.FromArgb(36, 77, 119), Height = 1, Dock = DockStyle.Top, Margin = new Padding(0, 14, 0, 13) };
        _sideMenu.Dock = DockStyle.Top;
        _sideMenu.AutoSize = true;
        _sideMenu.FlowDirection = FlowDirection.TopDown;
        _sideMenu.WrapContents = false;
        _sideMenu.Margin = new Padding(0, 22, 0, 0);
        _sideMenu.Padding = new Padding(0, 18, 0, 0);

        AddNavigation("Inicio", () => { ShowDashboard(); return Task.CompletedTask; });
        AddNavigation("Estudiantes", async () => await ShowStudentsAsync());
        AddNavigation("Registros pendientes", ShowRegistrationRequestsAsync);
        AddNavigation("Becas", () => OpenManager(ModuleForms.Scholarships(_dbOptions)));
        AddNavigation("Solicitudes de beca", () => OpenManager(ModuleForms.Applications(_dbOptions)));
        AddNavigation("Pagos", () => OpenManager(ModuleForms.Payments(_dbOptions)));
        AddNavigation("Noticias y eventos", () => OpenManager(ModuleForms.Publications(_dbOptions)));
        AddNavigation("Notificaciones", () => OpenManager(ModuleForms.Notifications(_dbOptions)));

        var motto = new Label { Text = "TU ESFUERZO\nABRE OPORTUNIDADES", ForeColor = Color.FromArgb(117, 222, 219), Font = new Font("Segoe UI", 9F, FontStyle.Bold), AutoSize = true, Dock = DockStyle.Bottom, Padding = new Padding(4, 16, 0, 4) };
        sidebar.Controls.Add(motto);
        sidebar.Controls.Add(_sideMenu);
        sidebar.Controls.Add(divider);
        sidebar.Controls.Add(brandSub);
        sidebar.Controls.Add(brand);
        sidebar.Controls.Add(brandLogo);

        var header = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(30, 15, 30, 12), Margin = Padding.Empty };
        _pageTitle.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
        _pageTitle.ForeColor = Navy;
        _pageTitle.AutoSize = true;
        _pageTitle.Location = new Point(29, 7);
        _pageSubtitle.Font = new Font("Segoe UI", 9F);
        _pageSubtitle.ForeColor = Muted;
        _pageSubtitle.AutoSize = true;
        _pageSubtitle.Location = new Point(31, 53);
        header.Controls.Add(_pageTitle);
        header.Controls.Add(_pageSubtitle);
        _content.Dock = DockStyle.Fill;
        _content.ColumnCount = 1;
        _content.RowCount = 2;
        _content.Margin = Padding.Empty;
        _content.Padding = Padding.Empty;
        _content.BackColor = Canvas;
        _content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _content.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
        _content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _pageBody.Dock = DockStyle.Fill;
        _pageBody.Padding = new Padding(30, 26, 30, 30);
        _pageBody.BackColor = Canvas;
        _pageBody.AutoScroll = true;
        _content.Controls.Add(header, 0, 0);
        _content.Controls.Add(_pageBody, 0, 1);

        var shell = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty, Padding = Padding.Empty, BackColor = Canvas };
        shell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240));
        shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        shell.Controls.Add(sidebar, 0, 0);
        shell.Controls.Add(_content, 1, 0);
        Controls.Add(shell);
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
        button.Click += async (_, _) =>
        {
            SetActiveNavigation(_navigationButtons.IndexOf(button));
            await action();
        };
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
        ClearPageBody();
        var panel = MakeCard(new Rectangle(30, 30, Math.Max(360, _pageBody.ClientSize.Width - 60), 250));
        panel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        panel.BackColor = Color.FromArgb(11, 55, 99);
        var title = new Label { Text = "Tu esfuerzo abre\noportunidades.", ForeColor = Color.White, Font = new Font("Segoe UI", 25F, FontStyle.Bold), AutoSize = false, Location = new Point(28, 23), Height = 78, TextAlign = ContentAlignment.MiddleLeft };
        var subtitle = new Label { Text = "Consulta becas, solicitudes, noticias y eventos en un solo lugar.", ForeColor = Color.FromArgb(242, 248, 255), Font = new Font("Segoe UI", 10F), AutoSize = false, Location = new Point(31, 111), Height = 46, TextAlign = ContentAlignment.TopLeft };
        var action = MakeButton("Abrir estudiantes  →", true);
        action.Location = new Point(30, 179);
        action.Size = new Size(205, 42);
        action.Click += async (_, _) => await ShowStudentsAsync();
        var heroLogo = new PictureBox { Image = NbuBranding.LoadMark(), SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.Transparent, Anchor = AnchorStyles.Top | AnchorStyles.Right };
        void LayoutHero()
        {
            var showLogo = panel.ClientSize.Width >= 540;
            heroLogo.Visible = showLogo;
            heroLogo.Size = new Size(showLogo ? 150 : 0, showLogo ? 150 : 0);
            if (showLogo) heroLogo.Location = new Point(panel.ClientSize.Width - 184, 42);
            var textWidth = Math.Max(240, panel.ClientSize.Width - (showLogo ? 225 : 58));
            title.Width = textWidth;
            subtitle.Width = textWidth;
            title.Font = new Font("Segoe UI", panel.ClientSize.Width < 620 ? 22F : 25F, FontStyle.Bold);
        }
        panel.SizeChanged += (_, _) => LayoutHero();
        LayoutHero();
        panel.Controls.Add(title);
        panel.Controls.Add(subtitle);
        panel.Controls.Add(action);
        panel.Controls.Add(heroLogo);
        _pageBody.Controls.Add(panel);

        var section = new Label { Text = "MÓDULOS DEL SISTEMA", ForeColor = Color.FromArgb(21, 153, 164), Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), AutoSize = true, Location = new Point(32, 312) };
        _pageBody.Controls.Add(section);
        var twoColumns = _pageBody.ClientSize.Width >= 620;
        var secondColumn = twoColumns ? 322 : 30;
        var thirdRow = twoColumns ? 515 : 690;
        AddDashboardCard("Estudiantes", "Registra y actualiza expedientes.", 30, 340, () => ShowStudentsAsync());
        AddDashboardCard("Becas y solicitudes", "Convocatorias y decisiones.", secondColumn, 340, () => OpenManager(ModuleForms.Scholarships(_dbOptions)));
        AddDashboardCard("Pagos", "Registro de depósitos.", 30, thirdRow, () => OpenManager(ModuleForms.Payments(_dbOptions)));
        AddDashboardCard("Noticias y eventos", "Avisos para la comunidad.", secondColumn, thirdRow, () => OpenManager(ModuleForms.Publications(_dbOptions)));
    }

    private void AddDashboardCard(string title, string description, int x, int y, Func<Task> action)
    {
        var card = MakeCard(new Rectangle(x, y, 270, 155));
        var heading = new Label { Text = title, ForeColor = Navy, Font = new Font("Segoe UI", 12F, FontStyle.Bold), AutoSize = true, Location = new Point(18, 19) };
        var body = new Label { Text = description, ForeColor = Muted, Font = new Font("Segoe UI", 9F), AutoSize = false, Size = new Size(225, 45), Location = new Point(19, 52), TextAlign = ContentAlignment.TopLeft };
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
        ClearPageBody();

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
        card.Margin = Padding.Empty;
        card.Controls.Add(grid);
        var studentsLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = Padding.Empty, Padding = Padding.Empty, BackColor = Canvas };
        studentsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        studentsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        studentsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        studentsLayout.Controls.Add(tools, 0, 0);
        studentsLayout.Controls.Add(card, 0, 1);
        _pageBody.Controls.Add(studentsLayout);

        async Task Reload(string? term = null)
        {
            try
            {
                await using var db = new AppDbContext(_dbOptions);
                var query = db.Students.AsNoTracking().Where(item => item.Status == "Active" || item.Status == "Inactive");
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

    private Task ShowRegistrationRequestsAsync()
    {
        _pageTitle.Text = "Registros pendientes";
        _pageSubtitle.Text = "Revisa las solicitudes enviadas desde el portal estudiantil";
        ReplacePageContent(new StudentRegistrationsForm(_dbOptions));
        return Task.CompletedTask;
    }

    private Task OpenManager<T, TRow>(DesktopCrudManager<T, TRow> manager) where T : class, new() where TRow : class
    {
        var navigationLabel = manager.PageTitle == "Solicitudes" ? "Solicitudes de beca" : manager.PageTitle;
        var navigationIndex = _navigationButtons.FindIndex(button => button.Text.Trim() == navigationLabel);
        if (navigationIndex >= 0) SetActiveNavigation(navigationIndex);
        _pageTitle.Text = manager.PageTitle;
        _pageSubtitle.Text = manager.PageSubtitle;
        ReplacePageContent(manager);
        return Task.CompletedTask;
    }

    private void ReplacePageContent(Control content)
    {
        ClearPageBody();
        content.Dock = DockStyle.Fill;
        content.Margin = Padding.Empty;
        _pageBody.Controls.Add(content);
    }

    private void ClearPageBody()
    {
        foreach (Control control in _pageBody.Controls.Cast<Control>().ToArray())
        {
            _pageBody.Controls.Remove(control);
            control.Dispose();
        }
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
