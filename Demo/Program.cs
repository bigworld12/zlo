using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zlo;
namespace Demo
{
    class Program
    {
     
        public static void Main(string[] args)
        {
            new TestProgram();          
            
        }
    }
    public class TestProgram
    {
        public ZloClient Client { get; set; }
        public TestProgram()
        {

        }
    }
}
