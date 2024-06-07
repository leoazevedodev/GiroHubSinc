using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubSincronizacao.Apis.Fotos.Dtos
{
    public class FotosAlphabiDto
    {
        [Key]
        public int Id { get; set; }
        public string Url { get; set; }
        public string Referencia { get; set; }
        public string Tamanhos { get; set; }
        public string Fabricacao { get; set; }
        public string DescricaoPrincipal { get; set; }
        public string DescricaoSecundaria { get; set; }
        public bool Principal { get; set; }
        public bool ErpPrincipal { get; set; }
    }
}
