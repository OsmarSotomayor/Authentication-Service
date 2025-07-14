using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class BlackListToken
    {
        public int Id { get; set; }

        public string Token { get; set; }

        public DateTime ExpirationDate { get; set; }
    }
}
