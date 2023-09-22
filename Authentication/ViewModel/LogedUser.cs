using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authentication.ViewModel
{
    public record LogedUser
    {
        public string UserName { get; set; }
        public string Token { get; set; }
    }
}
