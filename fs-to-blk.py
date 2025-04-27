#!/bin/python3
# convert Forth source into Collapse OS blocks
# also \\ -> \, single \ not emitted
# arg1: in   arg2: out
import sys

fileout = open(sys.argv[2], 'w')
backslash=False
open_colon=False
semi=False
new_line=""
main_buffer=""
inter_buffer=""
def size(buff):
    return len(buff)//64
def flush_main():
    global main_buffer
    for i in range(16 - size(main_buffer)):
        for k in range(64):
            main_buffer+=' '
    fileout.write(main_buffer)
    fileout.flush()
    main_buffer=""
def inter_to_main():
    global main_buffer
    global inter_buffer
    main_buffer=inter_buffer
    inter_buffer=""
def flush_inter():
    global inter_buffer
    fileout.write(inter_buffer)
    fileout.flush()
    inter_buffer=""
with open(sys.argv[1], 'r') as filein:
    for line in filein:
        for char in line:
            if char == ';':
                semi=True
            if char == '\\':
                if backslash:
                    new_line += '\\'
                    backslash=False
                else:
                    backslash=True
            elif char != '\n':
                new_line += char
        if len(new_line) < 64:
            for i in range(64 - len(new_line)):
                new_line+=' '
        elif len(new_line) > 64:
            print("line too long: ", new_line)
            exit(1)
        if new_line[0:2] == ": " and semi==False:
            open_colon=True
        elif semi:
            open_colon=False
            semi=False

        if not open_colon and size(inter_buffer)==0:
            main_buffer+=new_line
        else:
            inter_buffer+=new_line
        if size(main_buffer) == 16:
            flush_main()
            inter_to_main()
        elif size(main_buffer) + size(inter_buffer) == 16 and not open_colon:
            main_buffer+=inter_buffer
            inter_buffer=""            
            flush_main()
        elif size(main_buffer) + size(inter_buffer) > 16:
            flush_main()
            inter_to_main()
        elif size(main_buffer) + size(inter_buffer) < 16 and not open_colon:
            main_buffer+=inter_buffer
            inter_buffer=""
        new_line=""
flush_main()
flush_inter()