using System;
using static _6502_Emulator.Instruction;

namespace _6502_Emulator
{

    public struct Instruction // OPCODE STRUCT
    {
        public string assemblerName;
        public int byteCount;
        public int cycleCount;
        public ushort OpCode;
        public bool canUseExtraCycles ;
        public delegate void function();
        public function func;

        //ADDRESSING MODES
        public enum AddressingMode //13* addressing Modes
        {
          
            SYS,  // SYS (SYSTEM) IS NOT A REAL ADDRESSING MODE BUT IS USED FOR HARDWARE ROUTINES ;
            A,    // Basicaly just IMP        
            IMP,        
            ABS,        
            ABX,
            ABY,
            IND,
            IZX,
            IZY,
            IMM,
            ZP0,
            ZPX,
            ZPY,
            REL,

        }
        public AddressingMode adrMode;

    }

    public class CPU_6502
    {
        //RAM - Implemented as Part of The
        //CPU.
        public const int RAMSIZE = 0x10000; // 64KB 
        public RAM ram = new RAM(RAMSIZE);

        //PC
        public ushort PC = 0x00;
        public void Write(ushort address, byte data)
        {
            if (address >= 0x000 && address <= 0xFFFF)
                ram.RAM_Array[address] = data;

        }
        public byte Read(ushort address)
        {
            if (address >= 0x000 && address <= 0xFFFF)
                return (byte)ram.RAM_Array[address];

            else
                return 0;

        }
        public byte Read(int address)
        {
            if (address >= 0x000 && address <= 0xFFFF)
                return (byte)ram.RAM_Array[address];

            else
                return 0;

        }
        public byte Fetch()
        {
            if (currentInstruction.adrMode != AddressingMode.IMP && currentInstruction.adrMode != AddressingMode.A)
            {
                fetchedByte = Read(abs_address);
                return fetchedByte;
            }

            else
            {
                fetchedByte = ACC;
                return fetchedByte;

            }

        }


        //THE NES USES THE 2AO3 CPU , a variation if the MOS 65(C)02 CPU with a built in audio processing unit (APU).
        //It has an 8 Bit Word Length and a 116 bit Addressible Range.

        // Clock Cycle Variables

        public int totalCycles = 0;

        public ushort abs_address = 0x00;
        public ushort start_address = 0x00;

        public ushort rel_address = 0x00;
        public byte fetchedByte = 0x00;

        ushort opcode = 0x00;
        public ushort InstructionBaseAddress;

        public int cyclesRemaining = 0;
        public Instruction currentInstruction;

        public int extraCycles = 0;

        public void Clock()
        {
            if (cyclesRemaining == 0) //New Instruction
            {
                extraCycles = 0;

                //FETCH

                opcode = Read(PC);
                start_address = PC;
                
                PC++;

                #region Memory Variables
                byte lo;
                byte hi;

                byte lo_ptr;
                byte hi_ptr;

                ushort adr_ptr;
                ushort baseadr;

                #endregion

                //DECODE

                // a - OPCODE

                currentInstruction = Opcodes[opcode];

                // b - ADDRESSING MODE

                switch (currentInstruction.adrMode)     //ALL addressing modes [X] - Working
                {

                    case AddressingMode.A:
                        fetchedByte = ACC;
                        break;

                    case AddressingMode.IMP: // Either NOTHING or Operate on the Accumulator (ACC)  [X]
                        fetchedByte = ACC;
                        break;

                    case AddressingMode.IMM: // Immediate Addressing  e.g. AND #$A9                 [X]
                        abs_address = (ushort)(start_address + 1);
                        break;

                   // If ZERO PAGE modes dont work it might be because I didnt increment the PC inside it but i dont think thats a problem

                    case AddressingMode.ZP0: //Zero Page                                            [X]
                        abs_address = Read(PC);
                        abs_address &= 0x00FF;  //Read the Lo Bite of the 0th Page  
                        break;                  // 0x00LO


                    case AddressingMode.ZPX:    //Zero Page  with X offset                          [X]
                        InstructionBaseAddress = (ushort)(Read(PC));  
                        abs_address = (ushort)(InstructionBaseAddress + X);
                        abs_address &= 0x00FF;
                        break;

                    case AddressingMode.ZPY: //Zero Page with Y offset                              [X]
                        InstructionBaseAddress = (ushort)(Read(PC));  // For Debugging
                        abs_address = (ushort)(InstructionBaseAddress + Y);
                        abs_address &= 0x00FF;
                        break;

                    case AddressingMode.ABS: //                                                     [X]      

                        lo = Read(PC);   // [AA]
                        PC++;
                        hi = Read(PC);   // [BB]

                        abs_address = (ushort)((hi << 8) | lo); // pushes hi and lo together for a 16 bit  (2 byte) address 
                        break;                                  // [$BBAA]   ->  little-endian (derogatory)


                    case AddressingMode.ABX://                                                      [X]

                        lo = Read(PC);
                        PC++;
                        hi = Read(PC);


                        abs_address = (ushort)((hi << 8) | lo);
                        abs_address += X;  //Add X offset

                        if ((abs_address & 0xFF00) != (hi << 8)) // if page changes due to X OVERFLOW
                        {
                            if (currentInstruction.canUseExtraCycles)
                            { 
                                extraCycles = 1;      //Signal for Extra Cycle
                            }
                        }

                        break;

                    case AddressingMode.ABY: // Same as ABX                                         [X]
                        lo = Read(PC);
                        PC++;
                        hi = Read(PC);


                        abs_address = (ushort)((hi << 8) | lo);
                        abs_address += Y;       //  Y offset

                        if ((abs_address & 0xFF00) != (hi << 8)) // if page changes due to Y OVERFLOW
                        {

                            if (currentInstruction.canUseExtraCycles)
                            {
                                extraCycles = 1;      //Signal for Extra Cycle
                            }
                        }

                        break;


                    case AddressingMode.IND:  //Indirect addressing (Pointers)

                        lo_ptr = Read(PC);   // Little endian again (sigh)
                        PC++;
                        hi_ptr = Read(PC);

                        adr_ptr = (ushort)((hi_ptr << 8) | lo_ptr);

                        abs_address = (ushort)(Read((adr_ptr + 1) << 8) | Read(adr_ptr));

                        break;


                    case AddressingMode.IZX: //Indirect Zero Page X                                 [X]

                        baseadr = Read(PC);
                        InstructionBaseAddress = baseadr;  // for debugging

                        lo = Read((baseadr + X) & 0x00FF);
                        hi = Read((baseadr + (X + 1)) & 0x00FF);


                        abs_address = (ushort)((hi << 8) | lo);
                        break;


                    case AddressingMode.IZY: // Different to X  :0                                  [X]

                        baseadr = Read(PC);
                        InstructionBaseAddress = baseadr;  // for debugging#

                        lo = Read(baseadr & 0x00FF);
                        hi = Read((baseadr + 1) & 0x00FF);

                        abs_address = (ushort)((hi << 8) | lo);
                        abs_address += Y;                                 //Adds Y to the abs address 

                        if ((abs_address & 0xFF00) != (hi << 8))    // if Page changes (due to Y OVERFLOW)
                        {

                            if (currentInstruction.canUseExtraCycles)
                            {
                                extraCycles = 1;      //Signal for Extra Cycle
                            }      
                        }

                        break;

                    case AddressingMode.REL:                                               //      [X]

                        rel_address = Read(PC);             
                        PC++;                               //Offset begins from Operand ++;

                        if ((rel_address & 0x80) == 1)      // if bigger than 128 , modulus divide - basically
                        {
                            rel_address |= 0xFF00;
                        }

                        break;

                }


                //EXECUTE 

                cyclesRemaining = currentInstruction.cycleCount;
                
                if (currentInstruction.canUseExtraCycles)
                    cyclesRemaining += extraCycles;

                //Execute instruction on final cycle (inaccurate)
                if (currentInstruction.func != null)
                    currentInstruction.func();


            }


            //Iterate
            totalCycles++;
            cyclesRemaining--;

        }

        //Instuction Set (Alphabetical A-Z)

        public Instruction[] Opcodes = new Instruction[256];
        public Instruction SetOP(string _name, int _runCycles, ushort _opcode, function _instructionMethod, AddressingMode _AM)
        {
            Instruction dummy = new Instruction();

            dummy.adrMode = _AM;
            dummy.assemblerName = _name;
            dummy.cycleCount = _runCycles;
            dummy.OpCode = _opcode;
            dummy.func = _instructionMethod;
            dummy.canUseExtraCycles = false;


            return dummy;
        }
        public Instruction SetOP(string _name, int _runCycles, ushort _opcode, function _instructionMethod, AddressingMode _AM, bool _extraCycles)
        {
            Instruction dummy = new Instruction();

            dummy.adrMode = _AM;
            dummy.assemblerName = _name;
            dummy.cycleCount = _runCycles;
            dummy.OpCode = _opcode;
            dummy.func = _instructionMethod;
            dummy.canUseExtraCycles = _extraCycles;


            return dummy;
        }

        #region Instruction Methods

        // Instruction Methods
        void ADC_() //ADD with CARRY
        {
           
            byte operand = Fetch();
            PC++;

            byte carry = GetFlag(SR_Flags.C);

            ushort total = (ushort) (ACC + operand + carry);


              byte sign_A = (byte)((ACC & 0x80) >> 7); // isolates sign bit (1)0000000
              byte sign_O = (byte)((operand & 0x80) >> 7); ;
              byte sing_T = (byte)((total & 0x80) >> 7);

           
          
            ACC = (byte)( total & 0X00FF);

             SetFlag(SR_Flags.C, total > 255);
             SetFlag(SR_Flags.Z, ACC == 0);
             SetFlag(SR_Flags.N, (total & 0x80) == 0x80);   // if 7th bit
             SetFlag(SR_Flags.V, (sign_A == sign_O)  &&  (sing_T != sign_A));  // Set overflow flag if: 
                                                                               //  P + P = N 
                                                                               //  N + N = P 

        }
        void AND_() //ACC = OP AND ACC 
        {

            byte f = Fetch();

            PC++;

            ACC = (byte)(f & ACC);  //  ACC = fetched data & ACC

            SetFlag(SR_Flags.Z , ACC == 0);                // if reult is zero
            SetFlag(SR_Flags.N, (ACC & 0x80) == 0x80);     //0b 10000000   if most sig bit is set

        }
        void ASL_() // Shift Left One BIt
        {
            Fetch();

            ushort result = (ushort)(fetchedByte << 1);

            SetFlag(SR_Flags.Z, (result & 0x00FF) == 0);
            SetFlag(SR_Flags.N, (result & 0x80) == 0x80);
            SetFlag(SR_Flags.C, (result & 0xFF00) > 0);

            if (currentInstruction.adrMode == AddressingMode.A)
            {
                ACC = (byte)(result & 0x00FF); // Puts lowest 2 bits in ACC
            }

            else
                Write(abs_address, (byte)(result & 0x00FF));


        }  
        void BCC_() // Branch if NOT Carry 
        {
            if (IsFlagSet(SR_Flags.C) == false) // If flag NOT set
            {
                cyclesRemaining++;
                abs_address = (ushort)(PC + rel_address);

                if ((abs_address & 0xFF00) != (PC & 0xFF00))  // if page boundary crossed
                {
                    cyclesRemaining++; // 1 more cycle
                }

                PC = abs_address; // jump to new address
            }

       
        } 
        void BCS_() //Branch if Carry 
        {
            if (IsFlagSet(SR_Flags.C))
            {
                extraCycles++; 
                abs_address =(ushort)( PC + rel_address);

                if ((abs_address & 0xFF00) != (PC & 0xFF00))  // if page boundary crossed
                {
                    extraCycles++; // 1 more cycle
                } 
               
                PC = abs_address; // jump to new address

            }



        }
        void BIT_() { }  
        void BEQ_() // Branch if  Zero 
        {
            if (IsFlagSet(SR_Flags.Z) ) // If Zero Flag set
            {
                cyclesRemaining++;
                abs_address = (ushort)(PC + rel_address);

                if ((abs_address & 0xFF00) != (PC & 0xFF00))  // if page boundary crossed
                {
                    cyclesRemaining++; // 1 more cycle
                }

                PC = abs_address; // jump to new address
            }


        } 
        void BMI_() // Branch if Negative
        {
            if (IsFlagSet(SR_Flags.N) ) // If N flag NOT set
            {
                cyclesRemaining++;
                abs_address = (ushort)(PC + rel_address);

                if ((abs_address & 0xFF00) != (PC & 0xFF00))  // if page boundary crossed
                {
                    cyclesRemaining++; // 1 more cycle
                }

                PC = abs_address; // jump to new address
            }

        }
        void BNE_() // Branch if NOT Zero
        {
            if (IsFlagSet(SR_Flags.Z) == false) // If flag NOT set
            {
                cyclesRemaining++;
                abs_address = (ushort)(PC + rel_address);

                if ((abs_address & 0xFF00) != (PC & 0xFF00))  // if page boundary crossed
                {
                    cyclesRemaining++; // 1 more cycle
                }

                PC = abs_address; // jump to new address
            }



        }
        void BPL_() // Branch if Positive
        {
            if (IsFlagSet(SR_Flags.N) == false) // If flag NOT set
            {
                cyclesRemaining++;
                abs_address = (ushort)(PC + rel_address);

                if ((abs_address & 0xFF00) != (PC & 0xFF00))  // if page boundary crossed
                {
                    cyclesRemaining++; // 1 more cycle
                }

                PC = abs_address; // jump to new address
            }



        }
        void BRK_()
        {
            PC++;
        }
        void BVC_()  // Branch if Not Overflow
        {
            // Branch if Overflow (V)
            {
                if (IsFlagSet(SR_Flags.V) == false) // If flag  set
                {
                    cyclesRemaining++;
                    abs_address = (ushort)(PC + rel_address);

                    if ((abs_address & 0xFF00) != (PC & 0xFF00))  // if page boundary crossed
                    {
                        cyclesRemaining++; // 1 more cycle
                    }

                    PC = abs_address; // jump to new address
                }



            }
        }
        void BVS_()   // Branch if Overflow (V)
        {
            if (IsFlagSet(SR_Flags.V)) // If flag  set
            {
                cyclesRemaining++;
                abs_address = (ushort)(PC + rel_address);

                if ((abs_address & 0xFF00) != (PC & 0xFF00))  // if page boundary crossed
                {
                    cyclesRemaining++; // 1 more cycle
                }

                PC = abs_address; // jump to new address
            }



        }
        void CLC_() // Clear Carry Flag
        {
            DeSetFlag(SR_Flags.C);
        
        }
        void CLD_() //Clear Decimal Flag
        {
            DeSetFlag(SR_Flags.D);

        }
        void CLI_() //Clear Intrrupt Disable Flag
        {
            DeSetFlag(SR_Flags.I);
        }
        void CLV_()  // Clear Overflow
        {
            DeSetFlag(SR_Flags.V);
        }
        void CMP_() //Compare ACC with Memory (subtract)
        {
           byte f = Fetch();
            PC++;
           ushort difference = (ushort)(ACC - f);

            SetFlag(SR_Flags.C, ACC >= f); 
            SetFlag(SR_Flags.Z, (difference & 0x00FF) == 0); // same number
            SetFlag(SR_Flags.N, (difference & 0x80) == 0x80);
        }
        void CPX_() { }
        void CPY_() { }
        void DEC_() { }
        void DEX_() { }
        void DEY_() { }
        void EOR_() { }
        void INC_() // Increment memory by one
        {
            byte f = Fetch();
            f += 1;

            Write(abs_address, f ); 
            PC++;
  
            SetFlag(SR_Flags.Z, f == 0);
            SetFlag(SR_Flags.N, (f & 0x80) == 0x80);
  
        }
        void INX_() // Increment X by one
        {
            X += 1;
            SetFlag(SR_Flags.Z, X == 0);
            SetFlag(SR_Flags.N, (X & 0x80) == 0x08);

        }
        void INY_()  // Increment Y by one
        {
            Y += 1;
            SetFlag(SR_Flags.Z, Y == 0);
            SetFlag(SR_Flags.N, (Y & 0x80) == 0x08);


        }
        void JMP_() // Jump to new Location
        {
            PC = abs_address;
        
        }
        void JSR_() // Jump to Subroutine
        {
            PC -= 1;
            // Push Current PC to stack

            Write((ushort)(STKptr + 0X0100), ACC);
            STKptr -= 1;


        }
        void LDA_() // Load ACC
        { }
        void LDX_() { }
        void LDY_() { }
        void LSR_() { }
        void NOP_() { }
        void ORA_() { }
        void PHA_() // PUSH ACC to stack
        {
            Write( (ushort)(STKptr + 0X0100) , ACC);
            STKptr -= 1;
        }
        void PLA_() // POP stack to ACC
        {
            STKptr += 1;

            ACC = Read((ushort)STKptr + 0X0100);
            SetFlag(SR_Flags.Z, ACC == 0);
            SetFlag(SR_Flags.N, (ACC & 0X80) == 0X80);

        }
        void PHP_() 
        {
        
        
        
        }
        void PLP_() { }
        void ROL_() { }
        void ROR_() { }
        void RTI_() // return From Interrupt
        {
            // ( Opposite of IRQ)

            //POP SR from Stack
            SR = Read(0x0100 + STKptr);
            SetFlag(SR_Flags.B, !IsFlagSet(SR_Flags.B));
            SetFlag(SR_Flags.E, !IsFlagSet(SR_Flags.B));

            STKptr++;

            //POP PC from Stack

            PC = (ushort)Read(0x0100 + STKptr); // lo
            STKptr++;

            PC |= (ushort)Read((0x0100 + STKptr) << 8 ); // hi


        }
        void RTS_() 
        {
        
        }
        void SBC_() // Subtraction With Carry (borrow)
        {
            byte N_operand = (byte) (Fetch() ^ 0xFF + 1); // truns negative using 2s compliment
            PC++;

            byte carry = GetFlag(SR_Flags.C);                   // carry then becomes borrow.
            ushort total = (ushort)(ACC + N_operand + carry);

            byte sign_A = (byte)((ACC & 0x80) >> 7); // isolates sign bit (1)0000000
            byte sign_O = (byte)((N_operand & 0x80) >> 7); ;
            byte sing_T = (byte)((total & 0x80) >> 7);



            ACC = (byte)(total & 0X00FF);

            SetFlag(SR_Flags.C, total > 255);
            SetFlag(SR_Flags.Z, ACC == 0);
            SetFlag(SR_Flags.N, (total & 0x80) == 0x80);   // if 7th bit
            SetFlag(SR_Flags.V, (sign_A == sign_O) && (sing_T != sign_A));  // Set overflow flag if: 
                                                                            //  P + P = N 
                                                                            //  N + N = P 



        }
        void SEC_() // Set Carry
        {
            SetFlag(SR_Flags.C);
        }
        void SED_() // Set Decimal Mode - [Wont be used] 
        {
            SetFlag(SR_Flags.D);
        }
        void SEI_()  // Set Interrupt Disa
        {
            SetFlag(SR_Flags.I);
        }
        void STA_() { }
        void STX_() { }
        void STY_() { }
        void TAX_() { }
        void TAY_() { }
        void TSX_() { }
        void TXA_() { }
        void TXS_() { }
        void TYA_() { }
        void XXX_() 
        { }
        #endregion
        public void initOPCODES()
        {

            //CPU INSTRUCTIONS ( Alphabetical A-Z)

            //ADC - Working i think 

            Opcodes[0x69] = SetOP("ADC", 2, 0x69, ADC_, AddressingMode.IMM);
 
            Opcodes[0x65] = SetOP("ADC", 3, 0x65, ADC_, AddressingMode.ZP0);
    
            Opcodes[0x75] = SetOP("ADC", 4, 0x75, ADC_, AddressingMode.ZPX);

            Opcodes[0x6D] = SetOP("ADC", 4, 0x6D, ADC_, AddressingMode.ABS);
    
            Opcodes[0x7D] = SetOP("ADC", 4, 0x7D, ADC_, AddressingMode.ABX, true);
    
            Opcodes[0x79] = SetOP("ADC", 4, 0x79, ADC_, AddressingMode.ABY, true);

            Opcodes[0x61] = SetOP("ADC", 6, 0x61, ADC_, AddressingMode.IZX);
 
            Opcodes[0x71] = SetOP("ADC", 5, 0x71, ADC_, AddressingMode.IZY, true);

            //AND 

            Opcodes[0x29] = SetOP("AND", 2, 0x29, AND_ , AddressingMode.IMM);
            
            Opcodes[0x25] = SetOP("AND", 3, 0x25, AND_, AddressingMode.ZP0);
            
            Opcodes[0x35] = SetOP("AND", 4, 0x35, AND_, AddressingMode.ZPX);

            Opcodes[0x2D] = SetOP("AND", 4, 0x2D, AND_, AddressingMode.ABS);

            Opcodes[0x3D] = SetOP("AND", 4, 0x3D, AND_, AddressingMode.ABX, true);

            Opcodes[0x39] = SetOP("AND", 4, 0x39, AND_, AddressingMode.ABY, true);
            
            Opcodes[0x21] = SetOP("AND", 6, 0x21, AND_, AddressingMode.IZX);
            
            Opcodes[0x31] = SetOP("AND", 5, 0x31, AND_,  AddressingMode.IZY,true);

            //ASL

            Opcodes[0x0A] = SetOP("ASL", 2, 0x0A, ASL_, AddressingMode.A);

            Opcodes[0x06] = SetOP("ASL", 5, 0x06, ASL_, AddressingMode.ZP0);

            Opcodes[0x16] = SetOP("ASL", 6, 0x16, ASL_, AddressingMode.ZPX);

            Opcodes[0x0E] = SetOP("ASL", 6, 0x0E, ASL_, AddressingMode.ABS);

            Opcodes[0x1E] = SetOP("ASL", 7, 0x0E, ASL_, AddressingMode.ZPX);


            //BRANCH

            Opcodes[0x90] = SetOP("BCC", 2, 0x90, BCC_, AddressingMode.REL);

            Opcodes[0xB0] = SetOP("BCS", 2, 0xB0, BCS_, AddressingMode.REL);

            Opcodes[0xF0] = SetOP("BEQ", 2, 0xF0, BEQ_, AddressingMode.REL);

            Opcodes[0x30] = SetOP("BMI", 2, 0x30, BMI_, AddressingMode.REL);

            Opcodes[0xD0] = SetOP("BNE", 2, 0xD0, BNE_, AddressingMode.REL);

            Opcodes[0x10] = SetOP("BPL", 2, 0x10, BPL_, AddressingMode.REL);

            Opcodes[0x50] = SetOP("BVC", 2, 0x50, BVC_, AddressingMode.REL);

            Opcodes[0x70] = SetOP("BVS", 2, 0x70, BVS_, AddressingMode.REL);

            //BREAK


            //Opcodes[0x00] = SetOP("BRK", 7, 0x00, BRK_ AddressingMode.IMP);
            

            // CLEAR FLAGS

            Opcodes[0xD8] = SetOP("CLD", 2, 0xD8, CLD_, AddressingMode.IMP);

            Opcodes[0x58] = SetOP("CLI", 2, 0x58, CLI_, AddressingMode.IMP);

            Opcodes[0xB8] = SetOP("CLV", 2, 0xB8, CLV_, AddressingMode.IMP);

            //COMPARE 

            Opcodes[0xC9] = SetOP("CMP", 2, 0xC9, CMP_, AddressingMode.IMM);

            Opcodes[0xC5] = SetOP("CMP", 3, 0xC5, CMP_, AddressingMode.ZP0);

            Opcodes[0xD5] = SetOP("CMP", 4, 0xD5, CMP_, AddressingMode.ZPX);

            Opcodes[0xCD] = SetOP("CMP", 4, 0xCD, CMP_, AddressingMode.ABS);

            Opcodes[0xDD] = SetOP("CMP", 4, 0xDD, CMP_, AddressingMode.ZPX, true);

            Opcodes[0xD9] = SetOP("CMP", 4, 0xD9, CMP_, AddressingMode.ABS, true);

            Opcodes[0xC1] = SetOP("CMP", 6, 0xC1, CMP_, AddressingMode.IZX);

            Opcodes[0xD1] = SetOP("CMP", 5, 0xD1, CMP_, AddressingMode.IZY, true);


            //INCREMENT - Tested :D

            Opcodes[0xE6] = SetOP("INC", 5, 0xE6, INC_, AddressingMode.ZP0);

            Opcodes[0xF6] = SetOP("INC", 6, 0xF6, INC_, AddressingMode.ZPX);

            Opcodes[0xEE] = SetOP("INC", 6, 0xEE, INC_, AddressingMode.ABS);

            Opcodes[0xFE] = SetOP("INC", 7, 0xFE, INC_, AddressingMode.ABX);


            //RETURN

            Opcodes[0x40] = SetOP("RTI", 6, 0x40, RTI_, AddressingMode.IMP);



            // NO OPERATION

            Opcodes[0xEA] = SetOP("  ", 2, 0xEA, NOP_, AddressingMode.IMP);









            //HARDWARE OPERSATIONS

            RESET = SetOP("RESET", 8 , 0X100 , Reset, AddressingMode.SYS);
            IRQ = SetOP("IRQ", 7, 0X101, Irq , AddressingMode.SYS);
            NMI = SetOP("NMI", 7, 0X110, Nmi , AddressingMode.SYS);


            //Fill in the rest with Mystery Opcode
            for (int i = 0; i < Opcodes.Length; i++)
            {
                if (Opcodes[i].assemblerName == null)
                {
                    Opcodes[i] = SetOP("???", 1, 0xFFF, XXX_ ,AddressingMode.IMP);
               

                }
            }


        }

        //6502 REGISTERS

        //Can store 1 byte
        public byte ACC = 0x00;    // Accumulator
        public byte X = 0x00;      // x register
        public byte Y = 0x00;      // y register
        public byte STKptr = 0xFF;    //stack pointer

        //Status Register
        public byte SR = 0x00; 
        public enum SR_Flags        //SR FLAGS
        {
            C = (1 << 0),       // Carry  
            Z = (1 << 1),       // Zero Result
            I = (1 << 2),       // Interrupt 
            D = (1 << 3),       // Decimal (Unused in NES)
            B = (1 << 4),       // Break (Doesn't acc do anyhting)
            E = (1 << 5),       // Expansion (Unused Flag) ~ shown as -
            V = (1 << 6),       // Overflow
            N = (1 << 7)        // Negative

        };


        // Flag Methods
        public void SetFlag(SR_Flags flag)
        {

            int SR_ = Convert.ToInt32(SR);

            SR_ |= (int)flag;
            byte newStatus = Convert.ToByte(SR_);
            SR = newStatus;

          
        }
        public void SetFlag(SR_Flags flag , bool condition) // Conditional Overload
        {
            if (condition)
            {
                SetFlag(flag);
            }

            else {
                DeSetFlag(flag);
            }

        }
        public void DeSetFlag(SR_Flags flag)
        {
            int SR_ = Convert.ToInt32(SR);
            int Compare = SR_ | (int)flag;

            // if flag is already set
            if (Compare == SR_)
            {         
                SR_ ^= (int)flag; // XOR -> 1 and 1  = 0;
                byte newStatus = Convert.ToByte(SR_);
                SR = newStatus;
          
            }

            //else dont need to do anthing
        }
        public bool IsFlagSet(SR_Flags flag)
        {
            int _SR = Convert.ToInt32(SR);
            int Mask = _SR & (int)flag;

            int ans = Mask ^ (int)flag;

            if (ans == 0)
            {
                return true;
            }

            else
            {
                return false;
            }

        }
        public byte GetFlag(SR_Flags flag)
        {
            if (IsFlagSet(flag))
                return 1;


            else return 0;

        }



        //Hardware Functions
        public void DebugReset()
        {
            PC = 0;
            X = 0;
            Y = 0;
            ACC = 0;
            SR = 0;
            STKptr = 0xFF;

            fetchedByte = 0;
            abs_address = 0x00;
            start_address = 0x00;
            rel_address = 0x00;
            totalCycles = 0;
            currentInstruction = Opcodes[0xEA];
            cyclesRemaining = 0;
            
        }

        public Instruction RESET;
        public void Reset()
        {
            ACC = 0;
            X = 0;
            Y = 0;
            STKptr = 0xFD;
            SR = 0;

            abs_address = 0XFFFC;   // JUMP TO <--
            ushort lo = Read(abs_address);
            ushort hi = Read(abs_address + 1);

            PC = (ushort)((hi << 8) | lo);

            abs_address = 0x0000;
            rel_address = 0x0000;
            fetchedByte = 0x00;

            currentInstruction = RESET;
            cyclesRemaining = 8;
        }

        public Instruction IRQ;
        public void Irq() // Interrupt Request
        {
            if (GetFlag(SR_Flags.I) == 0)
            {
                //WRITE PC to Stack (STARTS FROM END so big endian)
                Write( (ushort)(0x0100 + STKptr) , (byte)((PC >> 8) & 0x00FF)); // hi byte
                STKptr -= 1;
               
                Write((ushort)(0x0100 + STKptr), (byte)(PC & 0x00FF)); // lo byte  
                STKptr -= 1;

                DeSetFlag(SR_Flags.B);
                SetFlag(SR_Flags.E);
                SetFlag(SR_Flags.I);
                Write( (ushort)(0x0100 + STKptr), (byte)SR);

                abs_address = 0XFFFE;  // JUMP TO <--
                ushort lo = Read(abs_address);
                ushort hi = Read(abs_address + 1);

                PC = (ushort)(hi << 8 | lo);

                abs_address = 0x0000;
                rel_address = 0x0000;
                fetchedByte = 0x00;

                cyclesRemaining = 7;
                currentInstruction = IRQ;
            }
        
        
        
        
        
        }

        public Instruction NMI;
        public void Nmi() // Non Maskable Interrupt
        {
           
                //WRITE PC to Stack (STARTS FROM END so big endian)
                Write((ushort)(0x0100 + STKptr), (byte)((PC >> 8) & 0x00FF)); // hi byte
                STKptr -= 1;

                Write((ushort)(0x0100 + STKptr), (byte)(PC & 0x00FF)); // lo byte  
                STKptr -= 1;

                DeSetFlag(SR_Flags.B);
                SetFlag(SR_Flags.E);
                SetFlag(SR_Flags.I);
                Write((ushort)(0x0100 + STKptr), (byte)SR);

                abs_address = 0XFFFE;  // JUMP TO <--
                ushort lo = Read(abs_address);
                ushort hi = Read(abs_address + 1);

                PC = (ushort)(hi << 8 | lo);

                abs_address = 0x0000;
                rel_address = 0x0000;
                fetchedByte = 0x00;

                cyclesRemaining = 7;
                currentInstruction = NMI;


        }


        public CPU_6502()
        {
            initOPCODES();
        }

    }
}
