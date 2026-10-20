using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HCSortingCZP.Models
{
    public class HBYWSocket(Socket socket)
    {
        public Socket Socket { get; } = socket;
    }
}
