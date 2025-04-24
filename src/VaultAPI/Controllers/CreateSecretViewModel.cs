namespace VaultAPI.Models.Dto
{
    public class CreateSecretViewModel
    {
        // Lista de empresas para asociar a un secreto
        public List<Company> Companies { get; set; }
        
        // Lista de grupos de empresas para asociar a un secreto
        public List<CompanyGroup> CompanyGroups { get; set; }
    }
}
