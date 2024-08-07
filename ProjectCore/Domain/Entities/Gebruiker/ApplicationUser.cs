using Microsoft.AspNetCore.Identity;

namespace ProjectCore.Domain.Entities.Gebruiker
{
    public class ApplicationUser
    {

        public string Id { get; set; }
        public string VoorNaam { get; set; } = string.Empty;
        public string AchterNaam { get; set; } = string.Empty;

        public string FullName => VoorNaam + " " + AchterNaam;

        public string Email { get; set; }

        //public string tel_werk_vast { get; set; } = string.Empty;
        //public string tel_werk_verkort { get; set; } = string.Empty;
        //public string? tel_werk_gsm { get; set; }

        //public string? tel_privé_gsm { get; set; }

        //public string? Subdivisie { get; set; } = string.Empty;
        //public bool Actief { get; set; }
        public int Balans { get; set; } = 0;
    }
}
