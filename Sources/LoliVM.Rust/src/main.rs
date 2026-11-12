mod enums;
mod models;

use models::Cpu;
use enums::Instruction;

fn main() {
    let mut cpu = Cpu::new();

    let instructions = vec!(
        Instruction::Mov(0, 10),
        Instruction::Mov(1, 20),
        Instruction::Add(0, 1),
        Instruction::Add(0, 1),
        Instruction::Halt);

    cpu.run(&instructions);
}