# LoliVM.NET

This is a learning project intended to explore computer architecture and how machines work at a low level.

The project is an opportunity to try different programming languages, including Rust, Go, and Zig. The .NET implementation serves as the reference because it is the language platform I know best. The goal is to build a small emulator of an 8-bit machine with basic functionality, implement the same machine in several languages, and then write a .NET coordinator to launch the virtual machines. A later stage will explore communication between machines using different methods.

## Roadmap

- [x] Create a command language so programs do not have to be written in hexadecimal.
- [ ] Implement the machine in Go.
- [ ] Implement the machine in C#.
- [ ] Implement the machine in Rust.
- [ ] Implement the machine in Zig.
- [x] Implement the coordinator in C#.
- [ ] Consider extending the machine to 16, 32, or 64 bits.
- [ ] Consider implementing instruction set extensions.

## Common contract

- The address space is 16-bit.
- ROM occupies addresses 0000–00FF, and RAM occupies 0200–02FF. The remaining addresses are unmapped.
- The machine has four 8-bit registers, R0–R3.
- Multibyte addresses are little-endian.
- ROM starts with a basic program; RAM starts empty.

| Opcode | Encoding | Meaning |
| --- | --- | --- |
| 00 | NOP | No operation |
| 01 | MOV reg imm8 | Set register |
| 02 | ADD dst src | Add registers, update flags |
| 03 | SUB dst src | Subtract registers, update flags |
| 04 | LOAD reg addr16 | Read memory into register |
| 05 | STORE addr16 reg | Write register to memory |
| 06 | JUMP addr16 | Set program counter |
| ... | ... | ... |
| FF | HALT | Halt execution |

- Arithmetic wraps to eight bits.
- Carry on subtraction denotes borrow.
- Zero and Negative reflect the byte result.
- Overflow reflects signed 8-bit overflow.
- Other instructions do not change flags.
