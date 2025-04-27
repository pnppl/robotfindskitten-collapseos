#!/bin/python3
# in out length
import sys
fileout = open(sys.argv[2], 'w')
length=int(sys.argv[3])
new_line=""
count=0
exit_code=0

with open(sys.argv[1], 'r') as filein:
    for line in filein:
        if len(line) <= length + 1:
            for char in line:
                if char != '\n':
                    new_line += char
            fileout.write(new_line)
            if len(new_line) < length:
                for i in range(length - len(new_line)):
                    fileout.write(' ')
            fileout.flush()
            count+=1
        else:
            print("line too long: " + line)
            exit_code=1
        new_line=""

for i in range(length):
    fileout.write(' ')
fileout.flush()
filein.close()
fileout.close()
print(str(count) + " lines converted")
exit(exit_code)