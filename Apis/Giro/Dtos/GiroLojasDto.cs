using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubSincronizacao.Apis.Giro.Dtos
{
    [Table("GiroLojas", Schema = "dbo")]
    public class GiroLojasDto
    {
        [Key]
        [Column("Id")]
        [MaxLength(50)]
        public string Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string LojaId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Loja { get; set; }

        [Required]
        [MaxLength(50)]
        public string ProdId { get; set; }

        [Required]
        public int? Vendido { get; set; }

        [Required]
        public int? Comprado { get; set; }

        [Required]
        public int? Saldo { get; set; }

        [MaxLength(15)]
        public string Giro { get; set; }

        [MaxLength(120)]
        public string Descricao { get; set; }

        [MaxLength(120)]
        public string Tamanho { get; set; }

        [MaxLength(120)]
        public string Cor { get; set; }

        [MaxLength(120)]
        public string Colecao { get; set; }

        [MaxLength(120)]
        public string Mapa { get; set; }

        [MaxLength(120)]
        public string Referencia { get; set; }

        [MaxLength(120)]
        public string Grupo { get; set; }

        [MaxLength(120)]
        public string Categoria { get; set; }

        [MaxLength(120)]
        public string Coordenado { get; set; }

        public double? PrecoVenda { get; set; }

        public double? PrecoCusto { get; set; }
    }
}
