using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NBU.Data;
using NBU.Data.Models;

namespace NBU.Desktop;

public sealed class AdminLoginForm : Form
{
    private readonly AppDbContext _context;
    private readonly TextBox _email = new();
    private readonly TextBox _password = new();
    private readonly Label _message = new();
    private readonly Button _submit = new();
    private int _failedAttempts;

    public AdminLoginForm(AppDbContext context)
    {
        _context = context;
        Text = "NBU · Acceso de administración";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(520, 500);
        BackColor = Color.FromArgb(245, 248, 252);
        Font = new Font("Segoe UI", 10F);

        var card = new Panel { Location = new Point(34, 25), Size = new Size(452, 450), BackColor = Color.White, Padding = new Padding(25) };
        var brand = new Label { Text = "NBU", Location = new Point(32, 22), AutoSize = true, ForeColor = Color.FromArgb(9, 39, 84), Font = new Font("Segoe UI", 25F, FontStyle.Bold) };
        var title = new Label { Text = "Acceso del personal", Location = new Point(32, 79), AutoSize = true, ForeColor = Color.FromArgb(24, 43, 67), Font = new Font("Segoe UI", 14F, FontStyle.Bold) };
        var subtitle = new Label { Text = "Inicia sesión con una cuenta administrativa NBU.", Location = new Point(32, 112), Size = new Size(388, 32), ForeColor = Color.FromArgb(114, 129, 152), Font = new Font("Segoe UI", 9F) };
        var emailLabel = new Label { Text = "Correo electrónico", Location = new Point(32, 159), AutoSize = true, ForeColor = Color.FromArgb(60, 80, 102), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
        _email.SetBounds(32, 182, 388, 38);
        _email.Text = "admin@nbu.test";
        _email.PlaceholderText = "correo@universidad.edu.ni";
        _email.BorderStyle = BorderStyle.FixedSingle;
        _email.Font = new Font("Segoe UI", 10F);
        var passwordLabel = new Label { Text = "Contraseña", Location = new Point(32, 239), AutoSize = true, ForeColor = Color.FromArgb(60, 80, 102), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
        _password.SetBounds(32, 262, 388, 38);
        _password.UseSystemPasswordChar = true;
        _password.BorderStyle = BorderStyle.FixedSingle;
        _password.Font = new Font("Segoe UI", 10F);
        _message.SetBounds(32, 314, 388, 42);
        _message.ForeColor = Color.FromArgb(160, 57, 57);
        _message.Font = new Font("Segoe UI", 8.5F);
        _message.TextAlign = ContentAlignment.MiddleLeft;
        _submit.SetBounds(32, 370, 388, 44);
        _submit.Text = "Ingresar al panel";
        _submit.FlatStyle = FlatStyle.Flat;
        _submit.FlatAppearance.BorderSize = 0;
        _submit.BackColor = Color.FromArgb(32, 182, 195);
        _submit.ForeColor = Color.FromArgb(9, 39, 84);
        _submit.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        _submit.Click += Submit_Click;
        AcceptButton = _submit;
        CancelButton = new Button { DialogResult = DialogResult.Cancel };

        var privacyNote = new Label
        {
            Text = "Acceso exclusivo para personal autorizado",
            Location = new Point(32, 420),
            Size = new Size(388, 22),
            ForeColor = Color.FromArgb(114, 129, 152),
            Font = new Font("Segoe UI", 8F),
            TextAlign = ContentAlignment.MiddleCenter
        };

        card.Controls.AddRange([brand, title, subtitle, emailLabel, _email, passwordLabel, _password, _message, _submit, privacyNote]);
        Controls.Add(card);
    }

    private async void Submit_Click(object? sender, EventArgs e)
    {
        _submit.Enabled = false;
        _message.Text = "Verificando cuenta…";
        try
        {
            var email = _email.Text.Trim();
            var user = await _context.Users
                .SingleOrDefaultAsync(item => item.NormalizedEmail == email.ToUpperInvariant());
            if (user?.LockoutEnd > DateTimeOffset.UtcNow)
            {
                _message.Text = $"Cuenta bloqueada hasta {user.LockoutEnd.Value.ToLocalTime():HH:mm}.";
                return;
            }
            var hasAdminRole = user is not null && await _context.UserRoles.AnyAsync(userRole =>
                userRole.UserId == user.Id && _context.Roles.Any(role => role.Id == userRole.RoleId && role.Name == NbuRoles.Admin));
            var passwordResult = user?.PasswordHash is null
                ? PasswordVerificationResult.Failed
                : new PasswordHasher<NbuUser>().VerifyHashedPassword(user, user.PasswordHash, _password.Text);

            if (user is not null && hasAdminRole && passwordResult is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.AccessFailedCount = 0;
                user.LockoutEnd = null;
                await _context.SaveChangesAsync();
                DialogResult = DialogResult.OK;
                Close();
                return;
            }

            if (user is not null && hasAdminRole)
            {
                user.AccessFailedCount++;
                if (user.AccessFailedCount >= 5)
                {
                    user.AccessFailedCount = 0;
                    user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(5);
                }
                await _context.SaveChangesAsync();
            }

            _failedAttempts++;
            _message.Text = user?.LockoutEnd > DateTimeOffset.UtcNow
                ? "Cuenta bloqueada durante cinco minutos por seguridad."
                : "Correo o contraseña incorrectos, o la cuenta no es administradora.";
            if (_failedAttempts >= 5 && user?.LockoutEnd > DateTimeOffset.UtcNow)
            {
                _submit.Enabled = false;
                _email.Enabled = false;
                _password.Enabled = false;
            }
        }
        catch (Exception)
        {
            _message.Text = "No se pudo conectar con NBU. Abre primero el portal web para instalar la base.";
        }
        finally
        {
            if (_failedAttempts < 5) _submit.Enabled = true;
        }
    }
}
