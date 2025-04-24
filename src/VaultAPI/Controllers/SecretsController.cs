[Authorize]  // Asegura que solo los usuarios autenticados accedan a este controlador
public class SecretsController : Controller
{
    private readonly GuardianDbContext _context;

    public SecretsController(GuardianDbContext context)
    {
        _context = context;
    }

    // GET: /Secrets
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Verificar que el usuario esté autenticado
        if (!User.Identity.IsAuthenticated)
        {
            return Unauthorized();  // Devuelve 401 si el usuario no está autenticado
        }

        // Verificar si el usuario tiene acceso a los secretos
        var userId = int.Parse(User.Identity.Name);
        var secrets = await _context.Secrets
            .Where(s => _context.SecretAccesses.Any(sa => sa.UserId == userId && sa.SecretId == s.Id))
            .Include(s => s.Company)
            .ToListAsync();

        // Si no hay secretos para mostrar, redirigir a una página de acceso denegado o mostrar mensaje
        if (!secrets.Any())
        {
            return Forbid();  // Devuelve 403 si no se tiene permiso para ver los secretos
        }

        return View(secrets);
    }

    // GET: /Secrets/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Secrets/Create
    [HttpPost]
    public async Task<IActionResult> Create(VaultAPI.Models.Dto.CreateSecretDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var vaultPath = $"grupo{dto.CompanyId}/empresa{dto.CompanyId}/{dto.Name.ToLower().Replace(" ", "-")}";
        bool vaultSuccess = false;

        if (dto.Type == "password")
        {
            vaultSuccess = await _vaultKvService.WriteSecretAsync(vaultPath, dto.Value!);
        }
        else if (dto.Type == "fiel" && dto.Files != null)
        {
            using var memoryStream = new MemoryStream();
            await dto.Files[0].CopyToAsync(memoryStream);
            var base64File = Convert.ToBase64String(memoryStream.ToArray());

            var fileData = new Dictionary<string, object>
            {
                { "filename", dto.Files[0].FileName },
                { "data", base64File }
            };

            vaultSuccess = await _vaultKvService.WriteSecretRawAsync(vaultPath, fileData);
        }

        if (!vaultSuccess)
        {
            ModelState.AddModelError("", "Hubo un error al guardar el secreto en Vault.");
            return View(dto);
        }

        // Guardar el secreto en la base de datos
        var secret = new Secret
        {
            Name = dto.Name,
            Type = dto.Type,
            VaultPath = vaultPath,
            Expiration = dto.Expiration,
            RequiresApproval = dto.RequiresApproval,
            CompanyId = dto.CompanyId
        };

        _context.Secrets.Add(secret);
        await _context.SaveChangesAsync();

        var userId = int.Parse(User.Identity.Name);
        var secretAccess = new SecretAccess
        {
            UserId = userId,
            SecretId = secret.Id,
            Permission = "read"
        };

        _context.SecretAccesses.Add(secretAccess);
        await _context.SaveChangesAsync();

        TempData["LoginMessage"] = "¡Secreto creado correctamente!";
        return RedirectToAction("Index", "Secrets");
    }

    // GET: /Secrets/View/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> View(int id)
    {
        var secret = await _context.Secrets.FindAsync(id);
        if (secret == null)
            return NotFound();

        var secretValue = await _vaultKvService.ReadSecretAsync(secret.VaultPath);
        return View(secretValue);
    }

    // POST: /Secrets/Delete/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var secret = await _context.Secrets.FindAsync(id);
        if (secret == null)
            return NotFound();

        _context.Secrets.Remove(secret);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
