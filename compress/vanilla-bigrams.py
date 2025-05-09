#!/bin/python3

file_in="inputs/vanilla.txt"
file_out="vanilla.bi"
bigrams = dict()

buffer = b''
output = open(file_out, 'w')
with open(file_in, 'rb') as file:
	while (byte := file.read(1)):
		if byte[0] >= 32 and byte[0] < 127:
			buffer += byte
		# buffer full
		if len(buffer) == 2:
			if buffer in bigrams:
				bigrams[buffer] += 1
			else:
				bigrams[buffer] = 1
			buffer = buffer[1:]
bigrams_sorted = sorted(bigrams.items(), key=lambda kv: (kv[1], kv[0]))
bigrams_sorted.reverse()
for i in range(33):
	output.write(str(bigrams_sorted[i]))
	output.write('\n')
output.flush()
output.close()
file.close()