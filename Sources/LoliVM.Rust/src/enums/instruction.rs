#[derive(Debug)]
pub enum Instruction {
    Mov(usize, u8),
    Add(usize, usize),
    Halt
}