using AutoMapper;
using SYSGES_MAGs.Models;
using SYSGES_MAGs.Models.ModelsDto; 

namespace SYSGES_MAGs.Mapping
{
    // Hérite de AutoMapper.Profile et non de ton modèle Profil
    public class ProfileMapping : Profile
    {
        public ProfileMapping()
        {
            // Mapping bidirectionnel entre Produit et ProduitDto
            CreateMap<Profil, ProfilDto>().ReverseMap();

            // Si tu veux aussi pour Profil
            CreateMap<Profil, ProfilDto>().ReverseMap();
        }
    }
}
