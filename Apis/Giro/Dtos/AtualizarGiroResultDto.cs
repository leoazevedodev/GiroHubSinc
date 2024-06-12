using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubSincronizacao.Apis.Giro.Dtos
{
    public class AtualizarGiroResultDto
    {
        public string LojaId { get; set; }
        public string ProdId { get; set; }
        public string Existe { get; set; }
    }
}
