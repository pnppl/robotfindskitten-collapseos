#!/bin/python3

file_in="inputs/vanilla.txt"
file_out="vanilla.txt.monograms"
monograms = dict()

output = open(file_out, 'w')
with open(file_in, 'rb') as file:
	while (byte := file.read(1)):
		if byte[0] >= 32 and byte[0] < 127:
			if byte in monograms:
				monograms[byte] += 1
			else:
				monograms[byte] = 1
monograms_sorted = sorted(monograms.items(), key=lambda kv: (kv[1], kv[0]))
monograms_sorted.reverse()
for i in range(16):
	output.write(str(monograms_sorted[i]))
	output.write('\n')
output.flush()
output.close()
file.close()