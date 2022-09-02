
// Made using the NESDEV Wiki, The olc6502 tutorial series by javidx9 , Masswerk 6502 Instruction Set, MassWerk Virtual 6502,
// the EmulationDev Subreddit , Dave Poo's 6502 Instruction Set video series.

namespace _6502_Emulator
{
    class Program
    {
        static void Main(string[] args)
        {
            CpuTests C = new CpuTests();

            //C.InitTests();
            
            //C.RunAllTests();
            
           C.Main(); // <- Normal Operation

        }
    }
}
