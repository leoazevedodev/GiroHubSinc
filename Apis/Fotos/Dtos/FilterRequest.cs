using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubSincronizacao.Apis.Fotos.Dtos
{
    public class FilterRequest
    {
        public string lojaid { get; set; } = string.Empty;
        public string cnpj { get; set; } = string.Empty;
    }
}
