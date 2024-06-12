using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubSincronizacao.SeedWork
{
    public static class FormatProdids
    {
        public static string FormatProdIds(List<string> prodIds)
        {
            // Juntar os itens em uma única string com vírgulas
            string formattedProdIds = string.Join(",", prodIds);

            // Adicionar aspas simples no início e no final
            return $"'{formattedProdIds}'";
        }

    }
}
