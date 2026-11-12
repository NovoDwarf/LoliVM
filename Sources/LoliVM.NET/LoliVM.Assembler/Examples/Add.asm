; Add two numbers, save the result in RAM and read it back.
MOV R0, 7
MOV R1, 3
ADD R0, R1
STORE 0x0200, R0
LOAD R2, 0x0200
HALT
