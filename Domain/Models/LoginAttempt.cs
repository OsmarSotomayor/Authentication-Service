using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class LoginAttempt
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int UserId { get; set; }
        public bool IsSuccess { get; set; }
        public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

        // Navegación
        public User? User { get; set; }
    }
}
