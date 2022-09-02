using System;
using static _6502_Emulator.Instruction;
using static _6502_Emulator.Instruction.AddressingMode;

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
        public bool branch = false;
        int extraCycles = 0;

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

        Instruction[] Opcodes = new Instruction[256];
        Instruction SetOP(string _name, int _runCycles, ushort _opcode, function _instructionMethod, AddressingMode _AM)
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
        Instruction SetOP(string _name, int _runCycles, ushort _opcode, function _instructionMethod, AddressingMode _AM, bool _extraCycles)
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

        //  Instruction Methods
        void ADC_() // ADD with CARRY
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
        void AND_() // ACC = OP AND ACC 
        {

            byte f = Fetch();

            PC++;

            ACC = (byte)(f & ACC);  //  ACC = fetched data & ACC

            SetFlag(SR_Flags.Z , ACC == 0);                // if reult is zero
            SetFlag(SR_Flags.N, (ACC & 0x80) == 0x80);     //0b 10000000   if most sig bit is set

        }
        void ASL_() // Arithmetic Shift Left One BIt
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
        void BCS_() // Branch if Carry 
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
        void BIT_() //Test Memory Bits with ACC
        {
            byte f = Fetch();

            byte testbits = (byte)(f & 0xC0);// test with bits 6 and 7
            SR |= testbits;

            SetFlag(SR_Flags.Z, (f & ACC) == 0);
        }  
        void BEQ_() // Branch if  Zero 
        {
            if (IsFlagSet(SR_Flags.Z) ) // If Zero Flag set
            {
                branch = true;

                cyclesRemaining++;
                abs_address = (ushort)(PC + rel_address);

                if ((abs_address & 0xFF00) != (PC & 0xFF00))  // if page boundary crossed
                {
                    cyclesRemaining++; // 1 more cycle
                }

                PC = abs_address; // jump to new address
            }

            else
                branch = false;
        } 
        void BMI_() // Branch if Negative
        {
            if (IsFlagSet(SR_Flags.N) ) // If N flag NOT set
            {
                branch = true;

                cyclesRemaining++;
                abs_address = (ushort)(PC + rel_address);

                if ((abs_address & 0xFF00) != (PC & 0xFF00))  // if page boundary crossed
                {
                    cyclesRemaining++; // 1 more cycle
                }

                PC = abs_address; // jump to new address
               
            }
            else
                branch = false;
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
                branch = true;
            }
            else
                branch = false;


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
                branch = true;
            }
            else
                branch = false;

        }
        void BRK_() //Break - software interrupt
        {
            PC++;

            //WRITE PC to Stack 
            Write((ushort)(0x0100 + STKptr), (byte)((PC >> 8) & 0x00FF)); // hi byte
            STKptr -= 1;

            Write((ushort)(0x0100 + STKptr), (byte)(PC & 0x00FF)); // lo byte  
            STKptr -= 1;

            //PUSH SR TO STACK
            SetFlag(SR_Flags.B);
            SetFlag(SR_Flags.I);
            Write((ushort)(0x0100 + STKptr), (byte)SR);

            abs_address = 0XFFFE;  // JUMP TO  Interrupt Vector
            ushort lo = Read(abs_address);
            ushort hi = Read(abs_address + 1);

            PC = (ushort)(hi << 8 | lo);

        }
        void BVC_() // Branch if Not Overflow
        {
            // Branch if Overflow (V)
            {
                if (IsFlagSet(SR_Flags.V) == false) // If flag  set
                {
                    branch = true;
                    cyclesRemaining++;
                    abs_address = (ushort)(PC + rel_address);

                    if ((abs_address & 0xFF00) != (PC & 0xFF00))  // if page boundary crossed
                    {
                        cyclesRemaining++; // 1 more cycle
                    }

                    PC = abs_address; // jump to new address
                }

                else
                    branch = false;
            }
        }
        void BVS_() // Branch if Overflow (V)
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
                branch = true;
            }

            else
                branch = false;
        }
        void CLC_() //* Clear Carry Flag
        {
            DeSetFlag(SR_Flags.C);
        
        }
        void CLD_() // Clear Decimal Flag
        {
            DeSetFlag(SR_Flags.D);

        }
        void CLI_() // Clear Intrrupt Disable Flag
        {
            DeSetFlag(SR_Flags.I);
        }
        void CLV_() // Clear Overflow
        {
            DeSetFlag(SR_Flags.V);
        }
        void CMP_() // Compare ACC with Memory (subtract)
        {
           byte f = Fetch();
            PC++;
           ushort difference = (ushort)(ACC - f);

            SetFlag(SR_Flags.C, ACC >= f); 
            SetFlag(SR_Flags.Z, (difference & 0x00FF) == 0); // same number
            SetFlag(SR_Flags.N, (difference & 0x80) == 0x80);
        }
        void CPX_() // Compare X with Memory (subtract)
        {
            byte f = Fetch();
            PC++;
            ushort difference = (ushort)(X - f);

            SetFlag(SR_Flags.C, X >= f);
            SetFlag(SR_Flags.Z, (difference & 0x00FF) == 0); // same number
            SetFlag(SR_Flags.N, (difference & 0x80) == 0x80);
        }
        void CPY_() // Compare Y with Memory (subtract)
        {
            byte f = Fetch();
            PC++;
            ushort difference = (ushort)(Y - f);

            SetFlag(SR_Flags.C, Y >= f);
            SetFlag(SR_Flags.Z, (difference & 0x00FF) == 0); // same number
            SetFlag(SR_Flags.N, (difference & 0x80) == 0x80);
        }
        void DEC_() // Decrement Memory
        {
            byte f = Fetch();
            f -= 1;

            Write(abs_address, f);
            PC++;

            SetFlag(SR_Flags.Z, f == 0);
            SetFlag(SR_Flags.N, (f & 0x80) == 0x80);

        }
        void DEX_() // Decrement X
        {
            X -= 1;
            SetFlag(SR_Flags.Z, X == 0);
            SetFlag(SR_Flags.N, (X & 0x80) == 0x08);

        }
        void DEY_() // Decrement Y
        {
            Y -= 1;
            SetFlag(SR_Flags.Z, Y == 0);
            SetFlag(SR_Flags.N, (Y & 0x80) == 0x08);

        }
        void EOR_() // XOR Memory with ACC
        {
            byte f = Fetch();
            PC++;

            ACC = (byte)(f ^ ACC);

            SetFlag(SR_Flags.Z, (ACC & 0xFF) == 0); 
            SetFlag(SR_Flags.N, (ACC & 0x80) == 0x80);

        }
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
        void INY_() // Increment Y by one
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
            //Stack is FILO so HI,LO in = LO,HI OUT.

            PC -= 1; // Step Behind Instruction
         
            // Push PC to stack
            Write((ushort)(STKptr + 0X0100), (byte)(PC >> 8)); // hi
            STKptr -= 1;

            Write((ushort)(STKptr + 0X0100), (byte)(PC & 0xFF));  //lo
            STKptr -= 1;

            PC = abs_address;

        }
        void LDA_() // Load Memory into ACC
        {
            ACC = Fetch();
            PC++;
            SetFlag(SR_Flags.Z, ACC == 0);
            SetFlag(SR_Flags.N, (ACC & 0x80) == 0x80);
        }
        void LDX_() // Load Memory into X
        {
            X = Fetch();
            PC ++;
            SetFlag(SR_Flags.Z, X == 0);
            SetFlag(SR_Flags.N, (X & 0x80) == 0x80);

        }
        void LDY_() // Load Memory into Y
        {
            Y = Fetch();
            PC++;
            SetFlag(SR_Flags.Z, Y == 0);
            SetFlag(SR_Flags.N, (Y & 0x80) == 0x80);
        }
        void LSR_() // Logical Shift Right
        {
            byte f = Fetch();
            f = (byte)(f >> 1);

            SetFlag(SR_Flags.C, (f & 0x01) == 1);
            DeSetFlag(SR_Flags.N);
            SetFlag(SR_Flags.Z, f == 0);

            if (currentInstruction.adrMode == AddressingMode.IMP)
                ACC = (byte)(f & 0xFF);

            else
                Write(abs_address, (byte)(f & 0xFF));

        }
        void NOP_() { return; } // Do Nothing :)
        void ORA_() //  OR Memory with ACC
        {
            byte f = Fetch();
            PC++;
            ACC = (byte)(f | ACC);

            SetFlag(SR_Flags.Z, (ACC & 0xFF) == 0);
            SetFlag(SR_Flags.N, (ACC & 0x80) == 0x80);
        }
        void PHA_() // PUSH ACC to stack
        {
            Write( (ushort)(STKptr + 0X0100) , ACC);
            STKptr -= 1;
        }
        void PHP_() // PUSH SR to stack
        {
            SetFlag(SR_Flags.B);
            SetFlag(SR_Flags.E);
            Write((ushort)(STKptr + 0X0100), ACC);

            DeSetFlag(SR_Flags.B);
            DeSetFlag(SR_Flags.E);

            STKptr -= 1;

        }
        void PLA_() // POP stack to ACC
        {
            STKptr += 1;

            ACC = Read((ushort)STKptr + 0X0100);
            SetFlag(SR_Flags.Z, ACC == 0);
            SetFlag(SR_Flags.N, (ACC & 0X80) == 0X80);

        }
        void PLP_() // PULL SR FROM STACK
        {
            STKptr += 1;

            SR = Read((ushort)STKptr + 0x0100);
            SetFlag(SR_Flags.E);

        }
        void ROL_() // ROTATE LEFT 
        {
            byte f = Fetch();
            f = (byte)(f << 1);
            f |= GetFlag(SR_Flags.C);


            SetFlag(SR_Flags.C, (fetchedByte & 0x01) == 1);
            SetFlag(SR_Flags.Z, (f) == 0);
            SetFlag(SR_Flags.N, (f & 0x80) == 0x80);

            if (currentInstruction.adrMode == A)
                ACC = f;

            else
                Write(abs_address, f);

        }
        void ROR_() // ROTATE RIGHT 
        {
            SetFlag(SR_Flags.C);
            byte f = Fetch(); 

            f = (byte)(f >> 1);
            f = (byte)(f | (GetFlag(SR_Flags.C) << 7)); // if Carry is set - make it 7th bit

            SetFlag(SR_Flags.C, (fetchedByte & 0x01) == 1);
            SetFlag(SR_Flags.Z, (f) == 0);
            SetFlag(SR_Flags.N, (f & 0x80) == 0x80);

            // f is a byte so it has to be less that 255 anyway OBVIOUSLLYYY...
            if (currentInstruction.adrMode == A)
                ACC = f;

            else
                Write(abs_address, f);

        }
        void RTI_() // Return From Interrupt
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
        void RTS_() // Return From Subroutine
        {
            //POP PC from Stack

            STKptr++;
            PC = (ushort)Read(0x0100 + STKptr); // lo

            STKptr++;
            PC |= (ushort)Read((0x0100 + STKptr) << 8); // hi

            //Go to NEXT instruction.
            PC++;

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
        void SEI_() // Set Interrupt Disable
        {
            SetFlag(SR_Flags.I);
        }
        void STA_() // Store ACC in Memory
        {
            Write(abs_address, ACC);
        }
        void STX_() // Store X in Memory
        {
            Write(abs_address, X);
        }
        void STY_() // Store Y in Memory
        {
            Write(abs_address, Y);
          
        }
        void TAX_() // Transfer ACC -> X
        {
            X = ACC;
            SetFlag(SR_Flags.Z, X == 0);
            SetFlag(SR_Flags.N, (X & 0x80) == 0x80);

        }
        void TAY_() // Transfer ACC -> Y 
        {
            Y = ACC;
            SetFlag(SR_Flags.Z, Y == 0);
            SetFlag(SR_Flags.N, (Y & 0x80) == 0x80);
        }
        void TSX_() // Transfer STKptr -> X
        {
            X = STKptr;
            SetFlag(SR_Flags.Z, X == 0);
            SetFlag(SR_Flags.N, (X & 0x80) == 0x80);


        }
        void TXA_() // Transfer X -> ACC
        {
            ACC = X;
            SetFlag(SR_Flags.Z, ACC == 0);
            SetFlag(SR_Flags.N, (ACC & 0x80) == 0x80);
        }
        void TXS_() // Transfer X -> STKptr
        {
            STKptr = X;
           
        }
        void TYA_() // Transfer Y -> ACC
        {
            ACC = Y;
            SetFlag(SR_Flags.Z, ACC == 0);
            SetFlag(SR_Flags.N, (ACC & 0x80) == 0x80);
        }
        void XXX_() { return; } // Illegal so i wont worry abt it for now
        #endregion
        public void initOPCODES()
        {

            //CPU INSTRUCTIONS ( Alphabetical A-Z)

            //ADD WITH CARRY (ADC)

            Opcodes[0x69] = SetOP("ADC", 2, 0x69, ADC_, AddressingMode.IMM);
 
            Opcodes[0x65] = SetOP("ADC", 3, 0x65, ADC_, AddressingMode.ZP0);
    
            Opcodes[0x75] = SetOP("ADC", 4, 0x75, ADC_, AddressingMode.ZPX);

            Opcodes[0x6D] = SetOP("ADC", 4, 0x6D, ADC_, AddressingMode.ABS);
    
            Opcodes[0x7D] = SetOP("ADC", 4, 0x7D, ADC_, AddressingMode.ABX, true);
    
            Opcodes[0x79] = SetOP("ADC", 4, 0x79, ADC_, AddressingMode.ABY, true);

            Opcodes[0x61] = SetOP("ADC", 6, 0x61, ADC_, AddressingMode.IZX);
 
            Opcodes[0x71] = SetOP("ADC", 5, 0x71, ADC_, AddressingMode.IZY, true);

            //AND WITH ACCUMLATOR (AND)

            Opcodes[0x29] = SetOP("AND", 2, 0x29, AND_ , AddressingMode.IMM);
            
            Opcodes[0x25] = SetOP("AND", 3, 0x25, AND_, AddressingMode.ZP0);
            
            Opcodes[0x35] = SetOP("AND", 4, 0x35, AND_, AddressingMode.ZPX);

            Opcodes[0x2D] = SetOP("AND", 4, 0x2D, AND_, AddressingMode.ABS);

            Opcodes[0x3D] = SetOP("AND", 4, 0x3D, AND_, AddressingMode.ABX, true);

            Opcodes[0x39] = SetOP("AND", 4, 0x39, AND_, AddressingMode.ABY, true);
            
            Opcodes[0x21] = SetOP("AND", 6, 0x21, AND_, AddressingMode.IZX);
            
            Opcodes[0x31] = SetOP("AND", 5, 0x31, AND_,  AddressingMode.IZY,true);

            //ARITHMETIC SHIFT LEFT (ASL)

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

            //BREAK (BRK)

            Opcodes[0x00] = SetOP("BRK", 7, 0x00, BRK_, AddressingMode.IMP);


            // CLEAR FLAGS - Fully Tested :D

            Opcodes[0x18] = SetOP("CLC", 2, 0xD8, CLC_, AddressingMode.IMP);

            Opcodes[0xD8] = SetOP("CLD", 2, 0xD8, CLD_, AddressingMode.IMP);

            Opcodes[0x58] = SetOP("CLI", 2, 0x58, CLI_, AddressingMode.IMP);

            Opcodes[0xB8] = SetOP("CLV", 2, 0xB8, CLV_, AddressingMode.IMP);


            //COMPARE ACC (CMP)

            Opcodes[0xC9] = SetOP("CMP", 2, 0xC9, CMP_, AddressingMode.IMM);

            Opcodes[0xC5] = SetOP("CMP", 3, 0xC5, CMP_, AddressingMode.ZP0);

            Opcodes[0xD5] = SetOP("CMP", 4, 0xD5, CMP_, AddressingMode.ZPX);

            Opcodes[0xCD] = SetOP("CMP", 4, 0xCD, CMP_, AddressingMode.ABS);

            Opcodes[0xDD] = SetOP("CMP", 4, 0xDD, CMP_, AddressingMode.ZPX, true);

            Opcodes[0xD9] = SetOP("CMP", 4, 0xD9, CMP_, AddressingMode.ABS, true);

            Opcodes[0xC1] = SetOP("CMP", 6, 0xC1, CMP_, AddressingMode.IZX);

            Opcodes[0xD1] = SetOP("CMP", 5, 0xD1, CMP_, AddressingMode.IZY, true);

            //COMPARE X (CPX)

            Opcodes[0xE0] = SetOP("CPX", 2, 0xE0, CPX_, AddressingMode.IMM);

            Opcodes[0xE4] = SetOP("CPX", 3, 0xE4, CPX_, AddressingMode.ZP0);

            Opcodes[0xEC] = SetOP("CPX", 4, 0xEC, CPX_, AddressingMode.ABS);

            //COMPARE Y (CPY)

            Opcodes[0xC0] = SetOP("CPY", 2, 0xE0, CPY_, AddressingMode.IMM);

            Opcodes[0xC4] = SetOP("CPY", 3, 0xE4, CPY_, AddressingMode.ZP0);

            Opcodes[0xCC] = SetOP("CPY", 4, 0xEC, CPY_, AddressingMode.ABS);


            //DECREMENT MEMORY (DEC)

            Opcodes[0xC6] = SetOP("DEC", 5, 0xC6, DEC_, AddressingMode.ZP0);

            Opcodes[0xD6] = SetOP("DEC", 6, 0xD6, DEC_, AddressingMode.ZPX);

            Opcodes[0xCE] = SetOP("DEC", 6, 0xCE, DEC_, AddressingMode.ABS);

            Opcodes[0xDE] = SetOP("DEC", 7, 0xDE, DEC_, AddressingMode.ABX);

            //DECREMENT X - (DEY)

            Opcodes[0xCA] = SetOP("DEX", 2, 0xCA, DEX_, AddressingMode.IMP);

            //DECREMENT Y (DEY)

            Opcodes[0x88] = SetOP("DEX", 2, 0x88, DEY_, AddressingMode.IMP);

            // EXCLUSIVE OR (XOR / "EOR")

            Opcodes[0x49] = SetOP("EOR", 2, 0x49, EOR_, AddressingMode.IMM);

            Opcodes[0x45] = SetOP("EOR", 3, 0x45, EOR_, AddressingMode.ZP0);

            Opcodes[0x55] = SetOP("EOR", 4, 0x55, EOR_, AddressingMode.ZPX);

            Opcodes[0x4D] = SetOP("EOR", 4, 0x4D, EOR_, AddressingMode.ABS);

            Opcodes[0x5D] = SetOP("EOR", 4, 0x5D, EOR_, AddressingMode.ABX, true);

            Opcodes[0x59] = SetOP("EOR", 4, 0x59, EOR_, AddressingMode.ABY, true);

            Opcodes[0x41] = SetOP("EOR", 6, 0x41, EOR_, AddressingMode.IZX);

            Opcodes[0x51] = SetOP("EOR", 5, 0x51, EOR_, AddressingMode.IZY, true);


            //INCREMENT MEMORY (INC) - Fully Tested :D

            Opcodes[0xE6] = SetOP("INC", 5, 0xE6, INC_, AddressingMode.ZP0);

            Opcodes[0xF6] = SetOP("INC", 6, 0xF6, INC_, AddressingMode.ZPX);

            Opcodes[0xEE] = SetOP("INC", 6, 0xEE, INC_, AddressingMode.ABS);

            Opcodes[0xFE] = SetOP("INC", 7, 0xFE, INC_, AddressingMode.ABX);


            //INCREMENT X (INX)

            Opcodes[0xE8] = SetOP("INX", 2, 0xE8, INX_, AddressingMode.IMP);

            //INCREMENT Y (INY)

            Opcodes[0xC8] = SetOP("INY", 2, 0xC8, INY_, AddressingMode.IMP);


            //JUMP (JMP)

            Opcodes[0x4C] = SetOP("JMP", 3, 0x4C, JMP_, AddressingMode.ABS);

            Opcodes[0x6C] = SetOP("JMP", 5, 0x6C, JMP_, AddressingMode.IND);

            // JUMP TO SUBROUTIE
            
            Opcodes[0x20] = SetOP("JSR", 5, 0x6C, JSR_, AddressingMode.ABS);

            //LOAD ACC (LDA)

            Opcodes[0xA9] = SetOP("LDA", 2, 0xA9, LDA_, AddressingMode.IMM);

            Opcodes[0xA5] = SetOP("LDA", 3, 0xA5, LDA_, AddressingMode.ZP0);

            Opcodes[0xB5] = SetOP("LDA", 4, 0xB5, LDA_, AddressingMode.ZPX);

            Opcodes[0xAD] = SetOP("LDA", 4, 0xAD, LDA_, AddressingMode.ABS);

            Opcodes[0xBD] = SetOP("LDA", 4, 0xBD, LDA_, AddressingMode.ABX, true);

            Opcodes[0xB9] = SetOP("LDA", 4, 0xB9, LDA_, AddressingMode.ABY, true);

            Opcodes[0xA1] = SetOP("LDA", 6, 0xA1, LDA_, AddressingMode.IZX);

            Opcodes[0xB1] = SetOP("LDA", 5, 0xB1, LDA_, AddressingMode.IZY, true);


            //LOAD X (LDX)

            Opcodes[0xA2] = SetOP("LDX", 2, 0xA2, LDX_, AddressingMode.IMM);

            Opcodes[0xA6] = SetOP("LDX", 3, 0xA6, LDX_, AddressingMode.ZP0);

            Opcodes[0xB6] = SetOP("LDX", 4, 0xB6, LDX_, AddressingMode.ZPY);

            Opcodes[0xAE] = SetOP("LDX", 4, 0xAE, LDX_, AddressingMode.ABS);

            Opcodes[0xBE] = SetOP("LDX", 4, 0xBE, LDX_, AddressingMode.ABY, true);

            //LOAD Y (LDY)

            Opcodes[0xA0] = SetOP("LDY", 2, 0xA0, LDY_, AddressingMode.IMM);

            Opcodes[0xA4] = SetOP("LDY", 3, 0xA4, LDY_, AddressingMode.ZP0);

            Opcodes[0xB4] = SetOP("LDY", 4, 0xB4, LDY_, AddressingMode.ZPY);

            Opcodes[0xAC] = SetOP("LDY", 4, 0xAC, LDY_, AddressingMode.ABS);

            Opcodes[0xBC] = SetOP("LDY", 4, 0xBC, LDY_, AddressingMode.ABY, true);

            // LOGICAL SHIFT RIGHT (LSR)

            Opcodes[0x4A] = SetOP("LSR", 2, 0x4A, LSR_, AddressingMode.A);

            Opcodes[0x46] = SetOP("LSR", 5, 0x46, LSR_, AddressingMode.ZP0);

            Opcodes[0x56] = SetOP("LSR", 6, 0x56, LSR_, AddressingMode.ZPX);

            Opcodes[0x4E] = SetOP("LSR", 6, 0x4E, LSR_, AddressingMode.ABS);

            Opcodes[0x5E] = SetOP("LSR", 7, 0x5E, LSR_, AddressingMode.ZPX);


            // NO OPERATION (NOP)

            Opcodes[0xEA] = SetOP("NOP", 2, 0xEA, NOP_, AddressingMode.IMP);


            // OR WITH ACCUMULATOR (ORA)

            Opcodes[0x09] = SetOP("ORA", 2, 0x09, ORA_, AddressingMode.IMM);

            Opcodes[0x05] = SetOP("ORA", 3, 0x05, ORA_, AddressingMode.ZP0);

            Opcodes[0x15] = SetOP("ORA", 4, 0x15, ORA_, AddressingMode.ZPX);

            Opcodes[0x0D] = SetOP("ORA", 4, 0x0D, ORA_, AddressingMode.ABS);

            Opcodes[0x1D] = SetOP("ORA", 4, 0x1D, ORA_, AddressingMode.ABX, true);

            Opcodes[0x19] = SetOP("ORA", 4, 0x19, ORA_, AddressingMode.ABY, true);

            Opcodes[0x01] = SetOP("ORA", 6, 0x01, ORA_, AddressingMode.IZX);

            Opcodes[0x11] = SetOP("ORA", 5, 0x11, ORA_, AddressingMode.IZY, true);

            //PUSH

            Opcodes[0x48] = SetOP("PHA", 3, 0x48, PHA_, AddressingMode.IMP);

            Opcodes[0x08] = SetOP("PHP", 3, 0x08, PHP_, AddressingMode.IMP);

            // PULL

            Opcodes[0x68] = SetOP("PLA", 3, 0x68, PLA_, AddressingMode.IMP);

            Opcodes[0x28] = SetOP("PLP", 3, 0x28, PLP_, AddressingMode.IMP);

            //ROTATE LEFT (ROL)

            Opcodes[0x2A] = SetOP("ROL", 2, 0x2A, ROL_, AddressingMode.A);

            Opcodes[0x26] = SetOP("ROL", 5, 0x26, ROL_, AddressingMode.ZP0);

            Opcodes[0x36] = SetOP("ROL", 6, 0x36, ROL_, AddressingMode.ZPX);

            Opcodes[0x2E] = SetOP("ROL", 6, 0x2E, ROL_, AddressingMode.ABS);

            Opcodes[0x3E] = SetOP("ROL", 7, 0x3E, ROL_, AddressingMode.ZPX);

            // ROTATE RIGHT (ROR)

            Opcodes[0x6A] = SetOP("ROR", 2, 0x0A, ROR_, AddressingMode.A);

            Opcodes[0x66] = SetOP("ROR", 5, 0x06, ROR_, AddressingMode.ZP0);

            Opcodes[0x76] = SetOP("ROR", 6, 0x16, ROR_, AddressingMode.ZPX);

            Opcodes[0x6E] = SetOP("ROR", 6, 0x0E, ROR_, AddressingMode.ABS);

            Opcodes[0x7E] = SetOP("ROR", 7, 0x0E, ROR_, AddressingMode.ZPX);


            //RETURN FROM INTERRUPT

            Opcodes[0x40] = SetOP("RTI", 6, 0x40, RTI_, AddressingMode.IMP);


            //RETURN FROM SUBROUTINE

            Opcodes[0x60] = SetOP("RTS", 6, 0x60, RTS_, AddressingMode.IMP);


            //SET STATUS

            Opcodes[0x38] = SetOP("SEC", 2, 0x38, SEC_, AddressingMode.IMP);

            Opcodes[0xF8] = SetOP("SED", 2, 0xFE, SED_, AddressingMode.IMP);

            Opcodes[0x78] = SetOP("SEI", 2, 0xFE, SEI_, AddressingMode.IMP);

            //SUBTRACT WITH CARRY (SBC)

            Opcodes[0xE9] = SetOP("SBC", 2, 0xE9, SBC_, AddressingMode.IMM);

            Opcodes[0xE5] = SetOP("SBC", 3, 0xE5, SBC_, AddressingMode.ZP0);

            Opcodes[0xF5] = SetOP("SBC", 4, 0xF5, SBC_, AddressingMode.ZPX);

            Opcodes[0xED] = SetOP("SBC", 4, 0xED, SBC_, AddressingMode.ABS);

            Opcodes[0xFD] = SetOP("SBC", 4, 0xFD, SBC_, AddressingMode.ABX, true);

            Opcodes[0xF9] = SetOP("SBC", 4, 0xF9, SBC_, AddressingMode.ABY, true);

            Opcodes[0xE1] = SetOP("SBC", 6, 0xE1, SBC_, AddressingMode.IZX);

            Opcodes[0xF1] = SetOP("SBC", 5, 0xF1, SBC_, AddressingMode.IZY, true);


            //STORE ACC (STA)

            Opcodes[0x85] = SetOP("STA", 3, 0x85, STA_, AddressingMode.ZP0);

            Opcodes[0x95] = SetOP("STA", 4, 0x95, STA_, AddressingMode.ZPX);

            Opcodes[0x8D] = SetOP("STA", 4, 0x8D, STA_, AddressingMode.ABS);

            Opcodes[0x9D] = SetOP("STA", 5, 0x9D, STA_, AddressingMode.ABX);

            Opcodes[0x99] = SetOP("STA", 5, 0x99, STA_, AddressingMode.ABY);

            Opcodes[0x81] = SetOP("STA", 6, 0x81, STA_, AddressingMode.IZX);

            Opcodes[0x91] = SetOP("STA", 6, 0x91, STA_, AddressingMode.IZY);

            // STORE X

            Opcodes[0x86] = SetOP("STX", 3, 0x86, STX_ , AddressingMode.ZP0);

            Opcodes[0x96] = SetOP("STA", 4, 0x96, STX_, AddressingMode.IZY);

            Opcodes[0x8E] = SetOP("STA", 4, 0x8E, STX_, AddressingMode.ABS);


            // STORE Y

            Opcodes[0x84] = SetOP("STY", 3, 0x84, STY_, AddressingMode.ZP0);

            Opcodes[0x94] = SetOP("STA", 4, 0x94, STY_, AddressingMode.IZX);

            Opcodes[0x8C] = SetOP("STA", 4, 0x8E, STY_, AddressingMode.ABS);


            //TRANSFER 

            Opcodes[0xAA] = SetOP("TAX", 2, 0xAA, TAX_, AddressingMode.IMP);

            Opcodes[0xA8] = SetOP("TAY", 2, 0xAA, TAY_, AddressingMode.IMP);

            Opcodes[0xBA] = SetOP("TSX", 2, 0xBA, TSX_, AddressingMode.IMP);

            Opcodes[0x8A] = SetOP("TXA", 2, 0x8A, TXA_, AddressingMode.IMP);

            Opcodes[0x9A] = SetOP("TXS", 2, 0x9A, TXS_, AddressingMode.IMP);

            Opcodes[0x98] = SetOP("TYA", 2, 0x98, TYA_, AddressingMode.IMP);


        



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
            branch = false;  
        }

        Instruction RESET;
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

        Instruction IRQ;
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

        Instruction NMI;
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
