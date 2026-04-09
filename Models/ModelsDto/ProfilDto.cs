using SYSGES_MAGs.Models.Enum;
using System.ComponentModel.DataAnnotations;

namespace SYSGES_MAGs.Models.ModelsDto
{
    public class ProfilDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Le nom d'utilisateur est obligatoire")]
        public string Username { get; set; }

        public string Userag { get; set; }

        [Required(ErrorMessage = "L'email est obligatoire")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Le type de profil est obligatoire")]
        [EnumDataType(typeof(EnumProfil), ErrorMessage = "Type de profil invalide")]
        public EnumProfil TypeProfile { get; set; }
        public string TypeProfileString { get; set; }

        [Required(ErrorMessage = "Le statut est obligatoire")]
        [RegularExpression("Actif|Inactif", ErrorMessage = "Statut invalide")]
        public string Status { get; set; }
    }
}
