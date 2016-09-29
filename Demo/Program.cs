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
            Console.ReadLine();   
        }
    }
    public class TestProgram
    {
        public ZloClient Client { get; set; }
        public TestProgram()
        {
            Start();
        }
        public  void Start()
        {
            Client = new ZloClient();
            Client.Connect();
            Client.SendRequest(ZloClient.ZloRequest.Stats , ZloClient.ZloGame.BF_3);
        }

      
    }
}
