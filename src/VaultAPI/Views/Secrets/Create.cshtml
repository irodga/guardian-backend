@model VaultAPI.Models.Dto.CreateSecretDto

@{
    ViewData["Title"] = "Crear Secreto";
}

<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="utf-8" />
    <title>@ViewData["Title"]</title>
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet" />
</head>
<body class="bg-light">

    <div class="container d-flex align-items-center justify-content-center min-vh-100">
        <div class="col-md-5">
            <div class="card shadow-lg border-0">
                <div class="card-body p-4">
                    <h3 class="text-center mb-4">🔐 Crear Secreto</h3>

                    @if (TempData["LoginMessage"] != null)
                    {
                        <div class="alert alert-success">
                            @TempData["LoginMessage"]
                        </div>
                    }

                    @if (!ViewData.ModelState.IsValid)
                    {
                        <div class="alert alert-danger">
                            Hubo un error en el formulario.
                        </div>
                    }

                    <form method="post" asp-action="Create" enctype="multipart/form-data">
                        @Html.AntiForgeryToken()

                        <div class="mb-3">
                            <label asp-for="Name" class="form-label">Nombre del Secreto</label>
                            <input asp-for="Name" class="form-control" />
                        </div>

                        <div class="mb-3">
                            <label asp-for="Type" class="form-label">Tipo</label>
                            <select asp-for="Type" class="form-control">
                                <option value="password">Password</option>
                                <option value="fiel">Archivo (Fiel)</option>
                            </select>
                        </div>

                        <!-- Cambiar la lista desplegable por un campo INT donde se ingresa el ID de la empresa -->
                        <div class="mb-3">
                            <label asp-for="CompanyId" class="form-label">ID de la Empresa</label>
                            <input asp-for="CompanyId" class="form-control" type="number" />
                        </div>

                        <div class="mb-3">
                            <label asp-for="Expiration" class="form-label">Fecha de Expiración</label>
                            <input asp-for="Expiration" class="form-control" type="datetime-local" />
                        </div>

                        <div class="mb-3 form-check">
                            <input asp-for="RequiresApproval" class="form-check-input" type="checkbox" />
                            <label class="form-check-label">Requiere aprobación</label>
                        </div>

                        <div class="mb-3">
                            <label asp-for="Value" class="form-label">Valor del Secreto</label>
                            <input asp-for="Value" class="form-control" />
                        </div>

                        <div class="mb-3">
                            <label class="form-label">Archivos (si es de tipo "fiel")</label>
                            <input asp-for="Files" class="form-control" type="file" multiple />
                        </div>

                        <button type="submit" class="btn btn-primary w-100">Crear Secreto</button>
                    </form>
                </div>
            </div>
        </div>
    </div>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>
