using System;
using System.Collections.Generic;
using System.Numerics;

namespace _6502_Emulator
{

    public struct BinConverter
    {

        //Convert To HEX methods for debugging
        public string ConvertToBIN(int value)
        {
            string hex = Convert.ToString(value, 2);
            return hex;

        }
        public string ConvertToBIN(int value, int padding)  // Variable Padding
        {
            string hex = Convert.ToString(value, 2);

            int difference = padding - hex.Length;

            if (hex.Length < padding)
            {

                for (int i = 0; i < difference; i++)
                {
                    hex = "0" + hex;
                }

            }

            return hex;

        }

        public string ConvertToBIN(byte value)  //FOR BYTES 
        {
            string hex = Convert.ToString(value, 2);

            int difference = 2 - hex.Length;

            if (hex.Length < 2)
            {

                for (int i = 0; i < difference; i++)
                {
                    hex = "0" + hex;
                }

            }
            return hex;

        }
        public string ConvertToBIN(ushort value)  //FOR Ushorts 
        {
            string hex = Convert.ToString(value, 16);

            int difference = 2 - hex.Length;

            if (hex.Length < 2)
            {
                for (int i = 0; i < difference; i++)
                {
                    hex = "0" + hex;
                }

            }
            return hex;

        }
        public string ConvertToBIN(ushort value, int padding)  //For Ushorts with custom padding
        {
            string hex = Convert.ToString(value, 2);

            int difference = padding - hex.Length;

            if (hex.Length < padding)
            {

                for (int i = 0; i < difference; i++)
                {
                    hex = "0" + hex;
                }

            }
            return hex;

        }
        public string ConvertToBIN(string number)
        {
            int num = Convert.ToInt32(number);
            string hex = Convert.ToString(num, 2);
            return hex;

        }     //Prolly never gonna acc use this
    }
    public struct HexConverter
    {
        //Convert To HEX methods for debugging
        public string ConvertToHEX(int value)
        {
            string hex = Convert.ToString(value, 16);
            return hex;

        }
        public string ConvertToHEX(int value, int padding)  // Variable Padding
        {
            string hex = Convert.ToString(value, 16);

            int difference = padding - hex.Length;

            if (hex.Length < padding)
            {

                for (int i = 0; i < difference; i++)
                {
                    hex = "0" + hex;
                }

            }

            hex.ToUpper();
            return hex;

        }

        public string ConvertToHEX(byte value)  //FOR BYTES 
        {
            string hex = Convert.ToString(value, 16);

            int difference = 2 - hex.Length;

            if (hex.Length < 2)
            {

                for (int i = 0; i < difference; i++)
                {
                    hex = "0" + hex;
                }

            }

            hex = hex.ToUpper();

            return hex;

        }
        public string ConvertToHEX(ushort value)  //FOR Ushorts 
        {
            string hex = Convert.ToString(value, 16);

            int difference = 2 - hex.Length;

            if (hex.Length < 2)
            {

                for (int i = 0; i < difference; i++)
                {
                    hex = "0" + hex;
                }

            }

            hex = hex.ToUpper();
            return hex;

        }
        public string ConvertToHEX(ushort value, int padding)  //For Ushorts with custom padding
        {
            string hex = Convert.ToString(value, 16);

            int difference = padding - hex.Length;

            if (hex.Length < padding)
            {

                for (int i = 0; i < difference; i++)
                {
                    hex = "0" + hex;
                }

            }

            hex = hex.ToUpper();
            return hex;

        }
        public string ConvertToHEX(string number)
        {
            int num = Convert.ToInt32(number);
            string hex = Convert.ToString(num, 16);


            hex = hex.ToUpper();
            return hex;

        }     //Prolly never gonna acc use this - nvm lmao

        public int ConvertFromHEX(string hexNumber)
        {

            int intValue = Convert.ToInt32(hexNumber, 16);
            return intValue;

        }


    }
    public class D_string
    {
        public string text = "";
        public ConsoleColor colour = ConsoleColor.Gray;


        public D_string()
        {

        }

        public D_string(string _text)
        {
            text = _text;
        }

        public D_string(string _text, ConsoleColor _colour) // dissasembly string (stores colour AND text))  // Idea from TheLearner on msdn forum
        {
            text = _text;
            colour = _colour;

        }

        public void Clear()
        {
            text = "";
        }

    }
    public struct ConsoleExtension
    {
        HexConverter wanda;
        BinConverter rosa;

        const int REG_PADDING = 58;
        const int INS_PADDING = 80;
        const int RAM_PADDING = 24;
        const int RegisterValuePadding = 6;

        //FORMATTING
        public void NewLine(int x, ref int y)
        {
            Console.SetCursorPosition(x, y);
            y++;


        }
        public void ColourWriteLine(ConsoleColor colour, string text) // For writing lines in colour
        {
            Console.ForegroundColor = colour;
            Console.WriteLine(text);
            Console.WriteLine("");
            Console.ResetColor();
        }
        public void ColourWrite(ConsoleColor colour, string text) // For writing lines in colour
        {
            Console.ForegroundColor = colour;
            Console.Write(text);
            Console.ResetColor();
        }


        public void PrintTable(int[] values, int sliceWidth)
        {

            for (int i = 0; i < values.Length; i++)
            {
                if (i % sliceWidth == 0)
                    Console.WriteLine(values[i]);

                else
                    Console.Write(values[i] + ",");

            }

        } // Generic table

        //COMPONENT DEBUG DISPLAYS
        public void PrintMemory(byte[] values, int columns, int rows, ushort dataTracer, int startIndex, int yCoord, string MemoryName)
        {
            NewLine(RAM_PADDING, ref yCoord);

            ColourWriteLine(ConsoleColor.Red, MemoryName);

            int row = 0;
            int displayIndex = startIndex;

            for (int i = startIndex; i < startIndex + (columns * rows); i++)
            {

                //Adds Padding to HEX display Values.
                string displayValue = wanda.ConvertToHEX(values[i], 2).ToUpper();
                string pageValue = wanda.ConvertToHEX(row, 4);


                if ((i + 1) % columns == 0)   // if on last item in row
                {

                    if (i == dataTracer)
                    {
                        ColourWrite(ConsoleColor.Yellow, displayValue);
                        Console.WriteLine("");

                    }

                    else
                    {
                        Console.WriteLine(displayValue);  //starta a new one
                    }


                    if ((row + 1) % rows == 0 && row != 0)
                        Console.WriteLine("");


                    row++;
                    displayIndex += 16;
                }


                else
                {
                    if (i % columns == 0) // if on first line
                    {

                        ColourWrite(ConsoleColor.White, "$" + wanda.ConvertToHEX(displayIndex, 4) + ":");

                    }

                    if (i == dataTracer)
                    {
                        ColourWrite(ConsoleColor.Yellow, displayValue + " ");
                    }

                    else
                        Console.Write(displayValue + " ");

                }



            }

        }

        public void PrintMemorySlice(byte[] values, int startIndex, int columns , int yCoord, string MemoryName)
        {
            NewLine(0, ref yCoord);

            int displayIndex = startIndex;
            ColourWrite(ConsoleColor.White, "$" + wanda.ConvertToHEX(displayIndex, 4) + ":");

            for (int i = 0; i < columns; i++)
            {
                string displayValue = wanda.ConvertToHEX(values[startIndex + i], 2).ToUpper();
                ColourWrite(ConsoleColor.Gray, displayValue);
                Console.Write(" ");

            }


        }


        public void DispayRegisterValue(string registerName, string registerValue, ref int yCoord, bool inline, ConsoleColor ValueColour)
        {
            Console.Write(registerName + ":");

            if ((registerValue.Length + registerName.Length) < RegisterValuePadding && inline == true) // Even Padding
            {
                int paddingDifference = RegisterValuePadding - (registerValue.Length + registerName.Length);

                for (int i = 0; i < paddingDifference; i++)
                {
                    Console.Write(" ");
                }
            }
            ColourWrite(ValueColour, "$" + registerValue);
            NewLine(REG_PADDING, ref yCoord);

        }
        public void DispayRegisterValue(string registerName, string registerValue, ref int yCoord, bool inline)
        {
            ColourWrite(ConsoleColor.White, registerName + ":");

            if ((registerValue.Length + registerName.Length) < RegisterValuePadding && inline == true) // Even Padding
            {
                int paddingDifference = RegisterValuePadding - (registerValue.Length + registerName.Length);

                for (int i = 0; i < paddingDifference; i++)
                {
                    Console.Write(" ");
                }
            }
            ColourWrite(ConsoleColor.Gray, "$" + registerValue.ToUpper() + " ");
            ColourWrite(ConsoleColor.White, "[" + Convert.ToInt32(Convert.ToString(Convert.ToInt32(registerValue, 16)), 10) + "]");
            NewLine(REG_PADDING, ref yCoord);


        }
        public void PrintRegisters(byte _ACC, byte _X, byte _Y, byte _STKP, ushort _SR, ushort _PC)
        {

            int Y_coord = 0;

            string A_Val = wanda.ConvertToHEX(_ACC);
            string X_Val = wanda.ConvertToHEX(_X);
            string Y_Val = wanda.ConvertToHEX(_Y);
            string STKP_Val = wanda.ConvertToHEX(_STKP);
            string PC_Val = wanda.ConvertToHEX(_PC, 4);
            string SR_Val = rosa.ConvertToBIN(_SR, 8);

            int SRValue = Convert.ToInt32(_SR);


            NewLine(REG_PADDING, ref Y_coord);
            ColourWrite(ConsoleColor.Blue, "REGISTERS");

            NewLine(REG_PADDING, ref Y_coord);
            NewLine(REG_PADDING, ref Y_coord);

            //PC
            DispayRegisterValue("PC", PC_Val, ref Y_coord, false);

            // Byte Registers
            DispayRegisterValue("ACC", A_Val, ref Y_coord, true);
            DispayRegisterValue("X", X_Val, ref Y_coord, true);
            DispayRegisterValue("Y", Y_Val, ref Y_coord, true);

            NewLine(REG_PADDING, ref Y_coord);

            //Status Register

            ColourWrite(ConsoleColor.White, "STATUS: ");

            NewLine(REG_PADDING, ref Y_coord);

            for (int i = 0; i < 8; i++)
            {

                if (SR_Val[i] != '0')
                    ColourWrite(ConsoleColor.White, Convert.ToString(SR_Val[i]));
                else
                    ColourWrite(ConsoleColor.Gray, Convert.ToString(SR_Val[i]));
            }
            Console.Write("(B)");
            ColourWrite(ConsoleColor.White, " [$" + wanda.ConvertToHEX(_SR, 2) + "]");


            string[] flags = { "N - Negative", "V - Overflow", "-", "B - Break", "D - Decimal", "I - !Interrupt", "Z - Zero", "C - Carry" };
            //  8  > 1  

            NewLine(REG_PADDING, ref Y_coord);

            for (int i = 8; i > 0; i--) // goes down
            {
                int positionShift = i - 1;
                int mask = 1 << (positionShift);
                int XORresult = SRValue ^ mask;
                int ANDresult = XORresult & mask;

                //NewLine(RX_padding, ref Y_coord); //< uncomment if testing if its still working

                if (_SR != 0 && ANDresult == 0)
                {
                    ColourWrite(ConsoleColor.Cyan, flags[8 - i] + " ");
                }

                else
                {
                    ColourWrite(ConsoleColor.DarkGray, flags[8 - i] + " ");

                }

                NewLine(REG_PADDING, ref Y_coord);




                //Debug Line - proof it works 

                //Console.Write( wanda.ConvertToHEX(_SR,2) + " XOR " + wanda.ConvertToHEX(mask, 2) + " = " + rosa.ConvertToBIN(XORresult,8));
                //NewLine(RX_padding, ref Y_coord);

                // Console.Write(rosa.ConvertToBIN(XORresult, 8) + " AND " + rosa.ConvertToBIN(mask, 8) + " = " + rosa.ConvertToBIN(ANDresult, 8));
                //NewLine(RX_padding, ref Y_coord);

            }
            
            NewLine(REG_PADDING, ref Y_coord);
            NewLine(REG_PADDING, ref Y_coord);
            ColourWrite(ConsoleColor.White, "STK: ");
            ColourWrite(ConsoleColor.White, STKP_Val);



        }



        public void PrintInstructions(int totalCycles, int cyclesRemaining, Instruction currentInstruction, ushort abs_address, byte data, ushort instructionBaseAddress, ushort rel_address, ushort instructionStartAddress, Queue<List<D_string>> instructionQueue)
        {
            int Y_Coord = 0;


            NewLine(INS_PADDING, ref Y_Coord);

            ColourWriteLine(ConsoleColor.Yellow, "INSTRUCTIONS ");


            NewLine(INS_PADDING, ref Y_Coord);
            ColourWriteLine(ConsoleColor.White, "Total Cycles: " + totalCycles);

            NewLine(INS_PADDING, ref Y_Coord);


            int maxY = Y_Coord;
            const int Qlength = 36;

            List<D_string> dissassemblyLine = new List<D_string>();


            D_string AddressingMode = new D_string();
            D_string Data = new D_string(); ;
            D_string targetAddress = new D_string();
            D_string secondaryAddress = new D_string();




            if (totalCycles > 0)
            {

                D_string startAddress = new D_string("$" + wanda.ConvertToHEX(instructionStartAddress, 4) + ":", ConsoleColor.White);
                D_string Name = new D_string(" " + currentInstruction.assemblerName, ConsoleColor.Gray);
                D_string Opcode = new D_string(" [" + wanda.ConvertToHEX(currentInstruction.OpCode) + "]", ConsoleColor.Yellow);


                //Addressing Mode Specific
                switch (currentInstruction.adrMode)
                {
                    case Instruction.AddressingMode.IMM:

                        Data = new D_string(" #" + data, ConsoleColor.Cyan);
                        AddressingMode = new D_string(" {IMM}");

                        break;

                    case Instruction.AddressingMode.IMP:

                        AddressingMode = new D_string(" {IMP}");
                        break;


                    case Instruction.AddressingMode.ZP0:
                        Data = new D_string(" $" + wanda.ConvertToHEX(abs_address, 2) + " -> " + wanda.ConvertToHEX(data), ConsoleColor.Cyan);
                        AddressingMode = new D_string(" {ZP0}");

                        break;

                    case Instruction.AddressingMode.ZPX:
                        targetAddress = new D_string(" $" + wanda.ConvertToHEX(instructionBaseAddress, 2) + ", X", ConsoleColor.Cyan);
                        Data = new D_string("  -> " + wanda.ConvertToHEX(data), ConsoleColor.Cyan);
                        secondaryAddress = new D_string(" [$" + wanda.ConvertToHEX(abs_address, 2) + "]", ConsoleColor.DarkYellow);
                        AddressingMode = new D_string(" {ZPX}");
                        break;

                    case Instruction.AddressingMode.ZPY:
                        targetAddress = new D_string(" $" + wanda.ConvertToHEX(instructionBaseAddress, 2) + ", Y", ConsoleColor.Cyan);
                        Data = new D_string("  -> " + wanda.ConvertToHEX(data), ConsoleColor.Cyan);
                        secondaryAddress = new D_string(" [$" + wanda.ConvertToHEX(abs_address, 2) + "]", ConsoleColor.DarkYellow);
                        AddressingMode = new D_string(" {ZPY}");
                        break;

                    case Instruction.AddressingMode.ABS:
                        Data = new D_string(" $" + wanda.ConvertToHEX(abs_address, 4) + "  -> " + wanda.ConvertToHEX(data), ConsoleColor.Cyan);
                        AddressingMode = new D_string(" {ABS}");
                        break;

                    case Instruction.AddressingMode.ABX:
                        Data = new D_string(" $" + wanda.ConvertToHEX(abs_address, 4) + " -> " + wanda.ConvertToHEX(data), ConsoleColor.Cyan);
                        AddressingMode = new D_string(" {ABX}");
                        break;

                    case Instruction.AddressingMode.IZX:
                        targetAddress = new D_string(" ($" + wanda.ConvertToHEX(instructionBaseAddress, 2) + ", X)", ConsoleColor.Cyan);
                        secondaryAddress = new D_string(" [$" + wanda.ConvertToHEX(abs_address, 4) + "]", ConsoleColor.DarkYellow);
                        Data = new D_string(" -> " + wanda.ConvertToHEX(data), ConsoleColor.Cyan);
                        AddressingMode = new D_string(" {IZX}");
                        break;

                    case Instruction.AddressingMode.IZY:
                        targetAddress = new D_string(" ($" + wanda.ConvertToHEX(instructionBaseAddress, 2) + "), Y");
                        secondaryAddress = new D_string(" [$" + wanda.ConvertToHEX(abs_address, 4) + "]", ConsoleColor.DarkYellow);
                        Data = new D_string(" -> " + wanda.ConvertToHEX(data), ConsoleColor.Cyan);
                        AddressingMode = new D_string(" {IZY}");
                        break;

                    case Instruction.AddressingMode.REL:
                        secondaryAddress = new D_string(" " + wanda.ConvertToHEX(abs_address - rel_address)  + " + $" + wanda.ConvertToHEX(rel_address, 4), ConsoleColor.DarkYellow);
                        Data = new D_string(" -> [$" + wanda.ConvertToHEX(abs_address, 4) + "]", ConsoleColor.Cyan);
                        AddressingMode = new D_string(" {REL}");
                        break;

                    case Instruction.AddressingMode.SYS:
                        Opcode = Data = new D_string("", ConsoleColor.Gray);
               
                        break;



                    default:
                        break;

                }

                //Add information to Line
                dissassemblyLine.Add(startAddress);
                dissassemblyLine.Add(Opcode);
                dissassemblyLine.Add(Name);
                dissassemblyLine.Add(targetAddress);
                dissassemblyLine.Add(secondaryAddress);
                dissassemblyLine.Add(Data);
                dissassemblyLine.Add(AddressingMode);

                instructionQueue.Enqueue(dissassemblyLine);

                foreach (List<D_string> Line in instructionQueue)
                {

                    NewLine(INS_PADDING, ref maxY);

                    if (maxY >= Qlength)
                    {
                        instructionQueue.Dequeue();
                        return;
                    }
                     
                       
                    foreach (D_string d in Line )
                    {
                        ColourWrite(d.colour, d.text);
                    }

                }
              

            }


        }

    }
    public struct ProgramParser
    {

        // 69 FF 65 23 75 12 6D 02 00 <-- ADC program

        public void LoadProgram(string s_program, ushort startAddress, byte[] memory)
        {

            HexConverter wanda;

            uint programLength = (uint) s_program.Length;
            

            string hexByte;
            int value;
            int interval = 3;
            int pos = startAddress;



            for (int i = 0; i < programLength - 2; i += interval)
            {
                interval = 3;

                // For every byte that isnt a 
                if (s_program[i] != ' ')
                {
                    //Formatting Data in AB_AB_AB

                    if (s_program[i + 1] != ' ')
                        hexByte = s_program[i].ToString() + s_program[i + 1].ToString();

                    else
                    { 
                      hexByte = "0" + s_program[1].ToString();
                       interval = 2;
                    }


                    value = wanda.ConvertFromHEX(hexByte);

                    memory[pos] = (byte)value;
                    pos++;
                }
     
            }

        
        
        }
    
        
    
    
    }
       
}
