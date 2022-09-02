using System;
using System.Collections.Generic;
using System.Text;

namespace _6502_Emulator
{
    public class RAM
    {
        ConsoleExtension debugger; // for debug printing

        HexConverter wanda; // beacuse she creates hexs lmao

        public byte[] RAM_Array;  //  ushort = 16 bit unsugned int      (RAM holds 8 kiB of RAM or 8192 bytes)

    
        public void CreateRAM(byte[] RAM) // Initialises every Bit in fake RAM
        {
            for (int i = 0; i < RAM.Length; i++)

            {

                RAM[i] = 0x00;


                //Debug shizzle

                /*
                string hex = wanda.ConvertToHEX(i + 1);
                Console.Write("RAM [");
                debugger.ColourWrite(ConsoleColor.Cyan, "" + hex + "");
                Console.Write("] set to ");
                debugger.ColourWrite(ConsoleColor.Cyan, "" + RAM[i] + " ");
                Console.WriteLine();
                */

            }

            //debugger.ColourWriteLine(ConsoleColor.Green, " RAM Created! ");
            //Console.ReadLine();
           // Console.Clear();

        }
        public RAM(int size) //RAM constructor
        {
            RAM_Array = new byte[size];
            CreateRAM(RAM_Array);


        }

        public void Reset()
        {
            CreateRAM(RAM_Array);
        }
    }
}
