@R2
M=0
@R0
D=M
@INFINITE_LOOP
D;JEQ
@R2
M=1
@R0
D=M-1
@INFINITE_LOOP
D;JEQ
@R0
D=M
@base
M=D
@R1
D=M
@power
M=D
@1
D=A
@ans
M=D
@countPower
M=0
(START_WHILE0)
@countPower
D=M
@power
D=D-M
@END_WHILE0
D;JGE
@countBase
M=1
@ans
D=M
@current
M=D
(START_WHILE1)
@countBase
D=M
@base
D=D-M
@END_WHILE1
D;JGE
@current
D=M
@ans
M=M+D
@countBase
M=M+1
@START_WHILE1
0;JMP
(END_WHILE1)
@countPower
M=M+1
@START_WHILE0
0;JMP
(END_WHILE0)
@ans
D=M
@R2
M=D
(INFINITE_LOOP)
@INFINITE_LOOP
0;JMP