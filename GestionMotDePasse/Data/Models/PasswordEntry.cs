using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionMotDePasse.Data.Models
{
    public class PasswordEntry
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Value { get; set; } = "";
        public DateTime InsertDate { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
