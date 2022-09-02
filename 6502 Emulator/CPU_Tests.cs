using System;
using System.Collections.Generic;
using System.Text;
using static _6502_Emulator.Test;

namespace _6502_Emulator
{
    public struct Test
    {
        public CPU_6502 CPU;
        ConsoleExtension console;
        BinConverter rosa;
        HexConverter wanda;

        public string TestName;
        public string Program;
        string Error;
        string TestDescription;

        public bool testPassed;

        public delegate bool TestFunction();
        public TestFunction testFunction;

        public bool RunTest()
        {
            testPassed = testFunction();
            console.ColourWrite(ConsoleColor.Gray, "{" + TestName + "}");

            if (testPassed)
                console.ColourWriteLine(ConsoleColor.Green, " - Passed");

            else
            {
                console.ColourWriteLine(ConsoleColor.Red, " - Failed");
                Console.WriteLine("SR: " + rosa.ConvertToBIN(CPU.SR, 8) + " X: " + wanda.ConvertToHEX(CPU.X) + " Y: " + wanda.ConvertToHEX(CPU.Y) + " ACC: " + wanda.ConvertToHEX(CPU.ACC));
                Console.WriteLine("PC: " + wanda.ConvertToHEX(CPU.PC));
                Console.WriteLine("RunCycles: " + (CPU.totalCycles - 1));
                Console.ReadLine();

            }

            return testPassed;
        }



    }
    class CpuTests
    {
        public CPU_6502 cpu = new CPU_6502();
        ConsoleExtension console = new ConsoleExtension();
        ProgramParser peter;
        Queue<List<D_string>> instructionQueue = new Queue<List<D_string>>();

        public Test SetTest(string _name, TestFunction _func, CPU_6502 _cpu)
        {
            Test Dummy = new Test();
            Dummy.CPU = _cpu;
            Dummy.testFunction = _func;
            Dummy.TestName = _name;
            Dummy.testPassed = false;

            return Dummy;

        }
        public void UpdateConsole()
        {
            Console.Clear();
            //Console.Beep();
            Console.CursorVisible = false;

            console.PrintMemory(cpu.ram.RAM_Array, 16, 16, cpu.start_address, 0, 0, "RAM");    // Zero Page
            console.PrintMemory(cpu.ram.RAM_Array, 16, 16, cpu.start_address, 0x8000, 17, "");  // Page 80 (0x8000)
            console.PrintRegisters(cpu.ACC, cpu.X, cpu.Y, cpu.STKptr, cpu.SR, cpu.PC);
            console.PrintInstructions(cpu.totalCycles, cpu.cyclesRemaining, cpu.currentInstruction, cpu.abs_address, cpu.fetchedByte, cpu.InstructionBaseAddress, cpu.rel_address, cpu.start_address, instructionQueue, cpu.branch);


        }
        public void UpdateConsole_Plus()
        {
            Console.Clear();
            //Console.Beep();
            Console.CursorVisible = false;

            console.PrintMemory(cpu.ram.RAM_Array, 16, 16, cpu.start_address, 0X00 , 0, "RAM");    // Zero Page
            console.PrintMemory(cpu.ram.RAM_Array, 16, 16, (ushort)(cpu.STKptr + 0x0100),0x0100, 17, "");    
            // console.PrintMemory(cpu.ram.RAM_Array, 16, 16, cpu.start_address, 0x8000, 19, "");  // Page 80 (0x8000)
        

             console.PrintRegisters(cpu.ACC, cpu.X, cpu.Y, cpu.STKptr, cpu.SR, cpu.PC);
            console.PrintInstructions(cpu.totalCycles, cpu.cyclesRemaining, cpu.currentInstruction, cpu.abs_address, cpu.fetchedByte, cpu.InstructionBaseAddress, cpu.rel_address, cpu.start_address, instructionQueue, cpu.branch);



        }
        public void Main(string program = "A0 FF BE 03 00 69 FF 65 03 75 12 6D 02 ")
        {
            cpu.DebugReset();
            cpu.ram.Reset();

            cpu.ram.RAM_Array[0XFFFE] = 00;
            cpu.ram.RAM_Array[0XFFFF] = 0x80;
            
            cpu.ACC = 2;

            peter.LoadProgram(program, 0, cpu.ram.RAM_Array);
            peter.LoadProgram("A0 FF BE 03 00 69 FF 65 03 75 12 6D 02 ", 0x8000, cpu.ram.RAM_Array);
            //INITIAL PRINT;
            UpdateConsole();
            Console.ReadLine();

            //CPU MAIN LOOP

            while (true)
            {
                cpu.Clock();

                if (cpu.cyclesRemaining == 0)
                {

                    UpdateConsole();
                    Console.SetCursorPosition(00, 39);

                    string f = Console.ReadLine();

                    if (f.ToUpper() == "RESET")
                        cpu.Reset();

                    if (f.ToUpper() == "IRQ")
                        cpu.Irq();

                    if (f.ToUpper() == "NMI")
                        cpu.Nmi();

                }



            }

        }
        public void RunProgram(int instructions)
        {
            for (int i = 0; i < instructions; i = 0)
            {
               if (cpu.cyclesRemaining >= 0)
                {
                    cpu.Clock();

                    if (cpu.cyclesRemaining == 0)
                    {
                        instructions -= 1;
                    }
                    
                }
           
            }


        }

        public List<Test> AllTests = new List<Test>();
        
      

        public void RunAllTests()
        {
            foreach (Test t in AllTests)
            {
                cpu.DebugReset();
                cpu.ram.Reset();

                bool go = t.RunTest();

                if (go == false)
                { return; }

            }

            Console.ReadLine();

        }
        public void InitTests()
        {

            string ADC_Test = "65 FF 65 23 75 12 6D 02 00 ";
            string LOAD_XY_OVERFLOW = "A0 FF BE 03 00 00 ";
            string LOAD_XYA_OVERFLOW = "A0 FF A2 03 A6 02 ";

            AllTests.Add(SetTest("DesetFlags", DesetFlags, cpu));
            AllTests.Add(SetTest("SetFlags", SetFlags, cpu));

            //ASL
            AllTests.Add(SetTest("ASL_ACC", ASLACC, cpu));
            AllTests.Add(SetTest("ASL_ZP0", ASLZP0, cpu));

            //BEQ        
            AllTests.Add(SetTest("CMP_BEQ", CMPBEQ, cpu));

            //INC
     
            AllTests.Add(SetTest("INC_ZP0", INCZP0, cpu));
            AllTests.Add(SetTest("INC_ZPX", INCZPX, cpu));
            AllTests.Add(SetTest("INC_ABS", INCABS, cpu));
            AllTests.Add(SetTest("INC_ABX", INCABX, cpu));

            //Interrupts /reset

            // AllTests.Add(SetTest("RESET_TEST ", RESET_TEST, cpu));

        }


        //ALL TESTS
        bool DesetFlags() // DESet D V AND I FLAGS 
        {
            cpu.SetFlag(CPU_6502.SR_Flags.V);
            cpu.SetFlag(CPU_6502.SR_Flags.D);
            cpu.SetFlag(CPU_6502.SR_Flags.I);


            peter.LoadProgram("D8 58 B8 ", 0, cpu.ram.RAM_Array);
            RunProgram(3);


            if (cpu.SR == 0)        //Ensures Flags Can Be Cleared
                return true;

            else
            {
                return false;
            }



        }
        bool SetFlags()
        {

            peter.LoadProgram("38 F8 78 ", 0, cpu.ram.RAM_Array);
            RunProgram(3);

            if (cpu.IsFlagSet(CPU_6502.SR_Flags.I) && cpu.IsFlagSet(CPU_6502.SR_Flags.D) && cpu.IsFlagSet(CPU_6502.SR_Flags.C))       
                return true;

            else
            {
                return false;
            }


        }

        //INC
        bool INCZP0()
        {
            peter.LoadProgram("E6 05 EA ", 0, cpu.ram.RAM_Array);
            cpu.ram.RAM_Array[0x05] = 1;

            RunProgram(3);

            if (cpu.ram.RAM_Array[0x05] == 2)
            {
                return true;
            }

            else
                return false;


        }
        bool INCZPX()
        {

            cpu.X = 3;
            cpu.ram.RAM_Array[0x08] = 1;

            peter.LoadProgram("F6 05 ", 0, cpu.ram.RAM_Array);

            RunProgram(1);

            if (cpu.ram.RAM_Array[0x08] == 2)
            {
                return true;
            }

            else
                return false;


        }
        bool INCABS()
        {
            cpu.DebugReset();
            peter.LoadProgram("EE 03 80 ", 0, cpu.ram.RAM_Array);

            RunProgram(1);

            if (cpu.ram.RAM_Array[0x8003] == 1)
            {
                return true;
            }

            else
                return false;


        }
        bool INCABX()
        {
            cpu.DebugReset();
            cpu.X = 1;
            peter.LoadProgram("FE 03 80 ", 0, cpu.ram.RAM_Array);

            RunProgram(1);

            if (cpu.ram.RAM_Array[0x8004] == 1 && (cpu.totalCycles - 1) == 7)
            {
                return true;
            }

            else
                //UpdateConsole();
                //Console.ReadLine();

                return false;


        }
  
        //ASL
        bool ASLACC()
        {
            cpu.ACC = 1;
            peter.LoadProgram("0A 0A 0A ", 0, cpu.ram.RAM_Array); // shift Acc 3 times

            RunProgram(3);

            if (cpu.ACC == 8)
            {
                return true;
            }

            else
                return false;


        }
        bool ASLZP0() 
        {
            cpu.ram.RAM_Array[3] = 128;
            peter.LoadProgram("06 03 ", 0, cpu.ram.RAM_Array);

            RunProgram(1);

            if (cpu.ram.RAM_Array[3] == 0 && cpu.IsFlagSet(CPU_6502.SR_Flags.C) == true) //Test for C
            {
                return true;
            }

            else
                return false;



        }

        //BEQ
        bool CMPBEQ()
        {
            cpu.ACC = 2;
            //COMPARE ACC WITH 2 > THE ARE THE SAME SO BRANCH
            peter.LoadProgram("C9 02 F0 09 ", 0, cpu.ram.RAM_Array);
            RunProgram(2);


            if (cpu.PC  == 0x0E)
                return true;

            else
                return false;




        }


        //HardWare
        bool RESET_TEST()
        {
            cpu.DebugReset();
         
            cpu.ram.RAM_Array[0XFFFC] = 0x00;
            cpu.ram.RAM_Array[0XFFFD] = 0x80;

            peter.LoadProgram("EE 03 80 ", 0, cpu.ram.RAM_Array);
            peter.LoadProgram("FE 03 80 ", 0x8000, cpu.ram.RAM_Array);

            RunProgram(1);

            cpu.Reset();
            RunProgram(1);

            return true;
        }

        bool RESET_TEST2()
        {
            cpu.DebugReset();

            // SET Reset Vectors
            cpu.ram.RAM_Array[0XFFFC] = 0x00;   
            cpu.ram.RAM_Array[0XFFFD] = 0x80;

            cpu.ram.RAM_Array[0XFFFE] = 0x05;
            cpu.ram.RAM_Array[0XFFFF] = 0x80;

            //Run -
            peter.LoadProgram("EE 03 80 FE ", 0, cpu.ram.RAM_Array);
            peter.LoadProgram("40 ", 0x8005, cpu.ram.RAM_Array);

            RunProgram(2);
            UpdateConsole_Plus();
            Console.ReadLine();
            cpu.Irq();
            RunProgram(1);
            UpdateConsole_Plus();
            Console.ReadLine();
            Console.Clear();


            return true;
        }
    }
}

    
