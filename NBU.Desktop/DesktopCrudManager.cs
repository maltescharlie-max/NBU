using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using NBU.Data;

namespace NBU.Desktop;

internal enum EditorKind { Text, Multiline, Decimal, Date, DateTime, Choice, Boolean }

internal sealed record EditorChoice(string Value, string Label);
internal sealed record EditorField(string PropertyName, string Label, EditorKind Kind = EditorKind.Text,
    IReadOnlyList<EditorChoice>? Choices = null, bool Optional = false);
internal sealed record DisplayColumn(string PropertyName, string Label, int Width = 100);

internal sealed class DesktopCrudManager<T, TRow> : UserControl where T : class, new() where TRow : class
{
    private static readonly Color Navy = Color.FromArgb(9, 39, 84);
    private static readonly Color Muted = Color.FromArgb(114, 129, 152);
    private static readonly Color Cyan = Color.FromArgb(81, 218, 210);
    private static readonly Color Canvas = Color.FromArgb(245, 248, 252);
    private readonly DbContextOptions<AppDbContext> _options;
    private readonly string _title;
    private readonly string _subtitle;
    private readonly Func<AppDbContext, Task<List<T>>> _load;
    private readonly Func<T, TRow> _toRow;
    private readonly Func<AppDbContext, T, Task<IReadOnlyList<EditorField>>> _fields;
    private readonly Func<AppDbContext, T, bool, Task> _save;
    private readonly Func<AppDbContext, T, Task> _delete;
    private readonly Func<TRow, string> _searchText;
    private readonly IReadOnlyList<DisplayColumn> _columns;
    private readonly DataGridView _grid = new();
    private readonly TextBox _search = new();
    private List<T> _items = [];

    public string PageTitle => _title;
    public string PageSubtitle => _subtitle;

    public DesktopCrudManager(DbContextOptions<AppDbContext> options, string title, string subtitle,
        IReadOnlyList<DisplayColumn> columns,
        Func<AppDbContext, Task<List<T>>> load,
        Func<T, TRow> toRow,
        Func<AppDbContext, T, Task<IReadOnlyList<EditorField>>> fields,
        Func<AppDbContext, T, bool, Task> save,
        Func<AppDbContext, T, Task> delete,
        Func<TRow, string> searchText,
        string? deletePrompt = null)
    {
        _options = options;
        _title = title;
        _subtitle = subtitle;
        _load = load;
        _toRow = toRow;
        _fields = fields;
        _save = save;
        _delete = delete;
        _searchText = searchText;
        _columns = columns;
        DeletePrompt = deletePrompt ?? $"¿Eliminar el registro seleccionado de {title}?";
        Dock = DockStyle.Fill;
        BackColor = Canvas;
        Font = new Font("Segoe UI", 9.5F);

        var toolbar = new TableLayoutPanel { Dock = DockStyle.Top, Height = 52, ColumnCount = 2, RowCount = 1, BackColor = Canvas, Margin = Padding.Empty, Padding = Padding.Empty };
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        toolbar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var searchTools = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Margin = Padding.Empty, Padding = Padding.Empty, BackColor = Canvas };
        _search.Margin = new Padding(0, 5, 8, 0);
        _search.Size = new Size(270, 35);
        _search.PlaceholderText = "Buscar…";
        _search.Font = new Font("Segoe UI", 9.5F);
        var searchButton = MakeButton("Buscar", false);
        searchButton.Margin = new Padding(0, 3, 0, 0);
        searchButton.Size = new Size(94, 38);
        var addButton = MakeButton("＋ Nuevo", true);
        addButton.Margin = new Padding(0, 3, 8, 0);
        addButton.Size = new Size(112, 38);
        var editButton = MakeButton("Editar", false);
        editButton.Margin = new Padding(0, 3, 8, 0);
        editButton.Size = new Size(94, 38);
        var deleteButton = MakeButton("Eliminar", false);
        deleteButton.Margin = new Padding(0, 3, 0, 0);
        deleteButton.Size = new Size(105, 38);
        searchTools.Controls.Add(_search);
        searchTools.Controls.Add(searchButton);
        var recordTools = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Margin = Padding.Empty, Padding = Padding.Empty, BackColor = Canvas };
        recordTools.Controls.Add(deleteButton);
        recordTools.Controls.Add(editButton);
        recordTools.Controls.Add(addButton);
        toolbar.Controls.Add(searchTools, 0, 0);
        toolbar.Controls.Add(recordTools, 1, 0);

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
        _grid.GridColor = Color.FromArgb(232, 237, 242);
        _grid.EnableHeadersVisualStyles = false;
        _grid.ColumnHeadersHeight = 42;
        _grid.RowTemplate.Height = 40;
        _grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(246, 249, 252), ForeColor = Muted, Font = new Font("Segoe UI", 8F, FontStyle.Bold), SelectionBackColor = Color.FromArgb(246, 249, 252), SelectionForeColor = Muted };
        _grid.DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.White, ForeColor = Color.FromArgb(24, 43, 67), SelectionBackColor = Color.FromArgb(222, 245, 244), SelectionForeColor = Navy, Padding = new Padding(6, 3, 6, 3) };
        _grid.AutoGenerateColumns = false;
        foreach (var column in columns)
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = column.Label, DataPropertyName = column.PropertyName, FillWeight = column.Width });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", DataPropertyName = "Id", Visible = false });

        var card = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Margin = Padding.Empty };
        card.Controls.Add(_grid);
        var body = new Panel { Dock = DockStyle.Fill, BackColor = Canvas, Padding = new Padding(0, 0, 0, 0) };
        body.Controls.Add(card);
        Controls.Add(body);
        Controls.Add(toolbar);

        searchButton.Click += async (_, _) => await LoadRowsAsync();
        _search.KeyDown += async (_, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; await LoadRowsAsync(); } };
        addButton.Click += async (_, _) => await EditRecordAsync(null);
        editButton.Click += async (_, _) => await EditRecordAsync(SelectedRecord());
        _grid.CellDoubleClick += async (_, _) => await EditRecordAsync(SelectedRecord());
        deleteButton.Click += async (_, _) => await DeleteRecordAsync();
        Load += async (_, _) => await LoadRowsAsync();
    }

    private string DeletePrompt { get; }

    private async Task LoadRowsAsync()
    {
        try
        {
            await using var db = new AppDbContext(_options);
            _items = await _load(db);
            var rows = _items.Select(_toRow).ToList();
            if (!string.IsNullOrWhiteSpace(_search.Text))
            {
                var term = _search.Text.Trim();
                rows = rows.Where(row => _searchText(row).Contains(term, StringComparison.CurrentCultureIgnoreCase)).ToList();
            }
            _grid.DataSource = rows;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            MessageBox.Show("No se pudo cargar la información. Revisa la cadena de conexión y que el script SQL de NBU esté aplicado.\n\n" + ex.Message, "Conexión a la base de datos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private T? SelectedRecord()
    {
        if (_grid.CurrentRow?.Cells["Id"]?.Value is not int id) return null;
        return _items.FirstOrDefault(item => (int?)typeof(T).GetProperty("Id")?.GetValue(item) == id);
    }

    private static T CopyEditableValues(T source)
    {
        var copy = new T();
        foreach (var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!property.CanRead || !property.CanWrite || (property.PropertyType.IsClass && property.PropertyType != typeof(string))) continue;
            property.SetValue(copy, property.GetValue(source));
        }
        return copy;
    }

    private async Task EditRecordAsync(T? existing)
    {
        var record = existing is null ? new T() : CopyEditableValues(existing);
        try
        {
            await using var db = new AppDbContext(_options);
            var fields = await _fields(db, record);
            var notify = fields.Any(field => field.PropertyName == "__NotifyStudents");
            fields = fields.Where(field => field.PropertyName != "__NotifyStudents").ToList();
            using var dialog = new EntityEditorDialog<T>(record, fields, notify, _title);
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            await _save(db, dialog.Entity, dialog.NotifyStudents);
            await db.SaveChangesAsync();
            await LoadRowsAsync();
        }
        catch (DbUpdateException ex)
        {
            MessageBox.Show("No se pudo guardar. Comprueba los datos y que no exista un registro duplicado.\n\n" + ex.GetBaseException().Message, "No se guardó el registro", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Revisa los datos", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private async Task DeleteRecordAsync()
    {
        var record = SelectedRecord();
        if (record is null) return;
        if (MessageBox.Show(DeletePrompt, "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try
        {
            await using var db = new AppDbContext(_options);
            await _delete(db, record);
            await db.SaveChangesAsync();
            await LoadRowsAsync();
        }
        catch (DbUpdateException)
        {
            MessageBox.Show("El registro está relacionado con información del historial y no se puede eliminar.", "No se puede eliminar", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    internal static void ValidateModel(object model)
    {
        var errors = new List<ValidationResult>();
        if (!Validator.TryValidateObject(model, new ValidationContext(model), errors, validateAllProperties: true))
            throw new InvalidOperationException(string.Join(Environment.NewLine, errors.Select(error => "• " + error.ErrorMessage)));
    }

    private static Button MakeButton(string text, bool primary) => new()
    {
        Text = text,
        FlatStyle = FlatStyle.Flat,
        BackColor = primary ? Cyan : Color.White,
        ForeColor = primary ? Navy : Color.FromArgb(72, 94, 117),
        Font = new Font("Segoe UI", 9F, FontStyle.Bold),
        Cursor = Cursors.Hand,
        UseVisualStyleBackColor = false,
        FlatAppearance = { BorderSize = primary ? 0 : 1, BorderColor = Color.FromArgb(220, 229, 237) }
    };
}

internal sealed class EntityEditorDialog<T> : Form where T : class
{
    private static readonly Color Navy = Color.FromArgb(9, 39, 84);
    private readonly IReadOnlyList<EditorField> _fields;
    private readonly Dictionary<string, Control> _controls = new();
    private readonly bool _showNotify;
    private readonly CheckBox _notify = new() { Text = "Notificar a estudiantes activos", AutoSize = true, Checked = true };

    public T Entity { get; }
    public bool NotifyStudents => _showNotify && _notify.Checked;

    public EntityEditorDialog(T entity, IReadOnlyList<EditorField> fields, bool showNotify = false, string? entityTitle = null)
    {
        Entity = entity;
        _fields = fields;
        _showNotify = showNotify;
        Text = "Editar registro · NBU";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        Width = 620;
        Height = Math.Min(790, Math.Max(430, 200 + fields.Count * 86 + (showNotify ? 55 : 0)));
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9.5F);

        var title = new Label { Text = entityTitle ?? typeof(T).Name, ForeColor = Navy, Font = new Font("Segoe UI", 19F, FontStyle.Bold), AutoSize = true, Location = new Point(25, 18) };
        var sub = new Label { Text = "Completa los datos y guarda los cambios.", ForeColor = Color.FromArgb(114, 129, 152), AutoSize = true, Location = new Point(28, 55) };
        Controls.Add(title);
        Controls.Add(sub);
        var layout = new TableLayoutPanel { Location = new Point(27, 91), Size = new Size(550, Height - 190), ColumnCount = 2, AutoScroll = true, Padding = new Padding(0) };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        for (var i = 0; i < fields.Count; i++)
        {
            var field = fields[i];
            var control = MakeEditor(field, typeof(T).GetProperty(field.PropertyName)?.GetValue(entity));
            _controls[field.PropertyName] = control;
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5, 4, 9, 8), Height = field.Kind == EditorKind.Multiline ? 140 : 77 };
            panel.Controls.Add(control);
            panel.Controls.Add(new Label { Text = field.Label, ForeColor = Color.FromArgb(67, 86, 109), Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Dock = DockStyle.Top, Height = 23 });
            var span = field.Kind == EditorKind.Multiline ? 2 : 1;
            layout.Controls.Add(panel, i % 2 == 0 ? 0 : 1, i / 2);
            if (span == 2) layout.SetColumnSpan(panel, 2);
        }
        Controls.Add(layout);
        if (showNotify)
        {
            _notify.Location = new Point(32, Height - 87);
            _notify.ForeColor = Color.FromArgb(54, 81, 103);
            _notify.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            Controls.Add(_notify);
        }
        var cancel = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel, Location = new Point(365, Height - 57), Size = new Size(96, 36), FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Color.FromArgb(72, 94, 117) };
        cancel.FlatAppearance.BorderColor = Color.FromArgb(220, 229, 237);
        var save = new Button { Text = "Guardar", Location = new Point(468, Height - 57), Size = new Size(109, 36), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(81, 218, 210), ForeColor = Navy, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
        save.Click += (_, _) => SaveEntity();
        Controls.Add(cancel);
        Controls.Add(save);
        AcceptButton = save;
        CancelButton = cancel;
    }

    private static Control MakeEditor(EditorField field, object? value)
    {
        Control control;
        switch (field.Kind)
        {
            case EditorKind.Decimal:
                var number = new NumericUpDown { DecimalPlaces = 2, Maximum = 999999999, Minimum = 0, Value = Math.Clamp(value is decimal d ? d : 0m, 0m, 999999999m), ThousandsSeparator = true };
                control = number;
                break;
            case EditorKind.Date:
            case EditorKind.DateTime:
                var rawDate = value switch { DateOnly dateOnly => dateOnly.ToDateTime(TimeOnly.MinValue), DateTime dateTime => dateTime, _ => DateTime.Today };
                if (rawDate < DateTimePicker.MinimumDateTime || rawDate > DateTimePicker.MaximumDateTime) rawDate = DateTime.Today;
                var date = new DateTimePicker { Format = field.Kind == EditorKind.Date ? DateTimePickerFormat.Short : DateTimePickerFormat.Custom, CustomFormat = "dd MMM yyyy · HH:mm", Value = rawDate };
                if (field.Optional) { date.ShowCheckBox = true; date.Checked = value is not null; }
                control = date;
                break;
            case EditorKind.Choice:
                var combo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = nameof(EditorChoice.Label), ValueMember = nameof(EditorChoice.Value) };
                combo.DataSource = field.Choices?.ToList() ?? [];
                if (value is not null) combo.SelectedValue = Convert.ToString(value);
                control = combo;
                break;
            case EditorKind.Boolean:
                control = new CheckBox { Text = field.Label, Checked = value is true, AutoSize = true };
                break;
            default:
                var textBox = new TextBox { Text = value?.ToString() ?? string.Empty, BorderStyle = BorderStyle.FixedSingle, Multiline = field.Kind == EditorKind.Multiline, ScrollBars = field.Kind == EditorKind.Multiline ? ScrollBars.Vertical : ScrollBars.None };
                if (field.Kind == EditorKind.Multiline) textBox.Size = new Size(520, 92);
                control = textBox;
                break;
        }
        control.Dock = DockStyle.Top;
        control.Font = new Font("Segoe UI", 9F);
        return control;
    }

    private void SaveEntity()
    {
        foreach (var field in _fields)
        {
            var property = typeof(T).GetProperty(field.PropertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property is null || !property.CanWrite) continue;
            var control = _controls[field.PropertyName];
            object? value = control switch
            {
                TextBox text => text.Text.Trim(),
                NumericUpDown numeric => numeric.Value,
                DateTimePicker date when field.Optional && !date.Checked => null,
                DateTimePicker date when property.PropertyType == typeof(DateOnly) => DateOnly.FromDateTime(date.Value),
                DateTimePicker date => date.Value,
                ComboBox combo => ConvertChoice(combo.SelectedValue, property.PropertyType),
                CheckBox checkBox => checkBox.Checked,
                _ => null
            };
            property.SetValue(Entity, value);
        }
        var errors = new List<ValidationResult>();
        if (!Validator.TryValidateObject(Entity, new ValidationContext(Entity), errors, validateAllProperties: true))
        {
            MessageBox.Show(string.Join(Environment.NewLine, errors.Select(item => "• " + item.ErrorMessage)), "Revisa los datos", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        DialogResult = DialogResult.OK;
        Close();
    }

    private static object? ConvertChoice(object? value, Type targetType)
    {
        if (value is null) return null;
        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;
        return underlying == typeof(int) ? int.Parse(Convert.ToString(value)!) : Convert.ToString(value);
    }
}
