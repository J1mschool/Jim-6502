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
            console.PrintInstructions(cpu.totalCycles, cpu.cyclesRemaining, cpu.currentInstruction, cpu.abs_address, cpu.fetchedByte, cpu.InstructionBaseAddress, cpu.rel_address, cpu.start_address, instructionQueue);


        }
        public void UpdateConsole_Plus()
        {
            Console.Clear();
            //Console.Beep();
            Console.CursorVisible = false;

            console.PrintMemory(cpu.ram.RAM_Array, 16, 16, cpu.start_address, 0X00 , 0, "RAM -");    // Zero Page
            console.PrintMemory(cpu.ram.RAM_Array, 16, 16, (ushort)(cpu.STKptr + 0x0100),0x0100, 17, "");    
            // console.PrintMemory(cpu.ram.RAM_Array, 16, 16, cpu.start_address, 0x8000, 19, "");  // Page 80 (0x8000)
        

             console.PrintRegisters(cpu.ACC, cpu.X, cpu.Y, cpu.STKptr, cpu.SR, cpu.PC);
            console.PrintInstructions(cpu.totalCycles, cpu.cyclesRemaining, cpu.currentInstruction, cpu.abs_address, cpu.fetchedByte, cpu.InstructionBaseAddress, cpu.rel_address, cpu.start_address, instructionQueue);



        }
        public void Main(string program = "00 00 00 ")
        {
            cpu.DebugReset();
            cpu.ram.Reset();
             
            peter.LoadProgram(program, 0, cpu.ram.RAM_Array);
            cpu.ram.RAM_Array[0XFFFC] = 0x00;
            cpu.ram.RAM_Array[0XFFFD] = 0x80;

            cpu.ram.RAM_Array[0XFFFE] = 0x05;
            cpu.ram.RAM_Array[0XFFFF] = 0x80;

            peter.LoadProgram("EE 03 80 FE ", 0, cpu.ram.RAM_Array);
            peter.LoadProgram("40 ", 0x8005, cpu.ram.RAM_Array);

            //INITIAL PRINT;
            UpdateConsole_Plus();
            Console.ReadLine();

            //CPU MAIN LOOP

            while (true)
            {
                cpu.Clock();

                if (cpu.cyclesRemaining == 0)
                {

                    UpdateConsole_Plus();
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
        public void RunProgram()
        {
            while (cpu.currentInstruction.assemblerName != "???")
            {
                cpu.Clock();
            }
        }

        public List<Test> AllTests = new List<Test>();

        public Test DS_Flags;

        public Test ASL_ACC;
        public Test ASL_ZP0;
        public Test ASL_BEQ;

        public Test INC_ZP0;
        public Test INC_ZPX;
        public Test INC_ABS;
        public Test INC_ABX;
        public Test CMP_BEQ;


        public Test INTERRUPTS;


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
            DS_Flags = SetTest("DesetFlags", DesetFlags, cpu);

            AllTests.Add(DS_Flags);
            string ADC_Test = "65 FF 65 23 75 12 6D 02 00 ";

            //ASL
            ASL_ACC = SetTest("ASL_ACC", ASLACC, cpu);
            ASL_ZP0 = SetTest("ASL_ZP0", ASLZP0, cpu);

            AllTests.Add(ASL_ACC);
            AllTests.Add(ASL_ZP0);

            //BEQ        
            CMP_BEQ = SetTest("CMP_BEQ", CMPBEQ, cpu);
            AllTests.Add(CMP_BEQ);

            //INC
            INC_ZP0 = SetTest("INC_ZP0", INCZP0, cpu);
            INC_ZPX = SetTest("INC_ZPX", INCZPX, cpu);
            INC_ABS = SetTest("INC_ABS", INCABS, cpu);
            INC_ABX = SetTest("INC_ABX", INCABX, cpu);

            AllTests.Add(INC_ZP0);
            AllTests.Add(INC_ZPX);
            AllTests.Add(INC_ABS);
            AllTests.Add(INC_ABX);

            //Interrupts /reset

            INTERRUPTS = SetTest("RESET_TEST ", RESET_TEST, cpu);
            AllTests.Add(INTERRUPTS);

        }


        //ALL TESTS
        bool DesetFlags() // DESet D V AND I FLAGS 
        {
            cpu.SetFlag(CPU_6502.SR_Flags.V);
            cpu.SetFlag(CPU_6502.SR_Flags.D);
            cpu.SetFlag(CPU_6502.SR_Flags.I);


            peter.LoadProgram("D8 58 B8 ", 0, cpu.ram.RAM_Array);
            RunProgram();


            if (cpu.SR == 0)        //Ensures Flags Can Be Cleared
                return true;

            else
            {
                return false;
            }



        }

        //INC
        bool INCZP0()
        {
            peter.LoadProgram("E6 05 ", 0, cpu.ram.RAM_Array);
            cpu.ram.RAM_Array[0x05] = 1;

            RunProgram();

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

            RunProgram();

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

            RunProgram();

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

            RunProgram();

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

            RunProgram();

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

            RunProgram();

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

            peter.LoadProgram("C9 02 F0 09 ", 0, cpu.ram.RAM_Array);
            RunProgram();


            if (cpu.PC  == 0x0E)
                return true;

            else
                return false;




        }

        bool RESET_TEST()
        {
            cpu.DebugReset();
         
            cpu.ram.RAM_Array[0XFFFC] = 0x00;
            cpu.ram.RAM_Array[0XFFFD] = 0x80;

            peter.LoadProgram("EE 03 80 ", 0, cpu.ram.RAM_Array);
            peter.LoadProgram("FE 03 80 ", 0x8000, cpu.ram.RAM_Array);

            RunProgram();
            UpdateConsole_Plus();
            Console.ReadLine();
            cpu.Reset();
            RunProgram();
            UpdateConsole_Plus();
            Console.ReadLine();
            Console.Clear();


            return true;
        }
    }
}

    
