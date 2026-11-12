use Instruction::{Add, Halt, Mov};
use crate::enums::Instruction;

pub struct Cpu {
    registers: [u8; 4],
    pc: usize,
    ram: [u8; 256],
    halted: bool
}

impl Cpu {
    pub fn new() -> Self {
        Self {
            registers: [0u8; 4],
            pc: 0,
            ram: [0u8; 256],
            halted: false
        }
    }

    pub fn reset(&mut self) {
        self.registers = [0u8; 4];
        self.pc = 0;
        self.ram = [0u8; 256];
        self.halted = false;
    }

    pub fn dump(&self) {
        println!("[DUMP]\nRegisters: {0:?},\nProgram Counter: {1},\nHalted: {2}\n",
                 self.registers,
                 self.pc,
                 self.halted
        )
    }

    pub fn run(&mut self, instructions: &[Instruction]) {
        while !self.halted {
            self.step(instructions);
            self.dump()
        }
    }

    pub fn step(&mut self, instructions: &[Instruction]) {

        let Some(instruction) = instructions.get(self.pc) else {
            self.halted = true;
            return;
        };

        self.execute(instruction);

        if !self.halted {
            self.pc += 1;
        }
    }

    pub fn execute(&mut self, instruction: &Instruction) {
        match instruction {
            Mov(register, value) => {
                self.registers[*register] = *value;
            }
            Add(destination, source) => {
                let dst = *destination;
                let src = *source;

                self.registers[dst] = self.registers[dst].wrapping_add(self.registers[src]);
            }
            Halt => {
                self.halted = true;
            }
        }
    }
}