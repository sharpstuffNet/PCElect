using System;
using IO=System.IO;

namespace PCElect.Tool
{
    class Program
    {
        static void Main(string[] args)
        {            
            
            if (args.Length == 0)
            {
                Console.WriteLine("i    Init    Create Key and Initial Files");
                Console.WriteLine("a    Add Voter IDs");
                Console.WriteLine("r    Process Results");
            }
            else
            {
                if (!PCElect.Lib.MainHelper.Init(args))
                    return;
                
                switch (args[0])
                {
                    case "i":
                        {
                            var pce = new PCElect.Lib.PCElect(args.Length > 1 ? args[1] : null, PCElect.Lib.MainHelper.Config, PCElect.Lib.MainHelper.Logger);
                            pce.Init();
                        }
                        break;
                    case "a":
                        {
                            var pce = new PCElect.Lib.PCElect(args.Length > 1 ? args[1] : null, PCElect.Lib.MainHelper.Config, PCElect.Lib.MainHelper.Logger);
                            pce.AddVotes(args.Length > 2 ? args.Length > 3 ? int.Parse(args[3]) : int.Parse(args[2]) : 10);
                        }
                        break;
                    case "r":
                        {
                            var pce = new Lib.PCElect(args.Length > 1 ? args[1] : null, PCElect.Lib.MainHelper.Config, PCElect.Lib.MainHelper.Logger);
                            pce.Results(args.Length > 2 ? args[2] : null);
                        }
                        break;
                }
            }
        }
    }
}
