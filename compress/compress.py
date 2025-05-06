#!/bin/python3

# an experiment with simple ascii compression
# ./compress.py -edt file_in file_out
# 				encode decode test
#
# ascii only uses 7 bits
# we turn on the top bit to represent another 128 text units of 2 chars each
# SPC (32) through ~ (126) represent themselves doubled
# NUL (0)-US (31), DEL (127) are repurposed as the 33 most common bigrams
#
# advantages:
#	very simple to implement
#	minimal overhead
#	the encoded file is legible enough to tell what it is, it's just garbled
#	line breaks are preserved
#	you can test for negative to see if you can just emit or need to decode first
#	byte alignment maintained
#	seems to be pretty consistent with different sorts of texts
#
# the main downside is... it only shaves off about 20% (max 50%)
# it is dubious whether this is worth the increase in annoyance wrt Collapse OS,
# 	mainly because a consistent line length is tremendously convenient
#	also you can't decode with a syscall
#
# other downsides:
# 	no CRLF or multiple newline compression
# 	unicode gets mangled obviously
# 	you can't play with the rest of the codepage
# 	lots of the doubled letter units are rare, at least in natural language
#		you could replace them with 95 more bigrams to improve ratio
# 	only lowercase bigrams

from sys import argv

bigrams = {
	0: b'th',
	1: b'he',
	2: b'in',
	3: b'er',
	4: b'an',
	5: b're',
	6: b'es',
	7: b'on',
	8: b'st',
	9: b'nt',
	10: b'en',
	11: b'at',
	12: b'ed',
	13: b'nd',
	14: b'to',
	15: b'or',
	16: b'ea',
	17: b'ti',
	18: b'ar',
	19: b'te',
	20: b'ng',
	21: b'al',
	22: b'it',
	23: b'as',
	24: b'is',
	25: b'ha',
	26: b'et',
	27: b'se',
	28: b'ou',
	29: b'of',
	30: b'le',
	31: b'sa',
	127: b've'
}

bigrams_rev = {val: key for key, val in bigrams.items()}

msb1 = 0b1000_0000
msb0 = 0b0111_1111

# in: bytearray   out: bytearray with one item
def encode_byte(chars):
	if chars in bigrams_rev:
		return (bigrams_rev[chars] | msb1).to_bytes()
	elif len(chars) == 2 and chars[0] == chars[1] and chars[0] > 31 and chars[0] < 127 :
		return (chars[0] | msb1).to_bytes()
	else:
		return chars

def encode_file(file_in, file_out):
	buffer = b''
	output = open(file_out, 'wb')
	with open(file_in, 'rb') as file:
		while (byte := file.read(1)):
			buffer += byte
			# buffer full
			if len(buffer) == 2:
				encoded = encode_byte(buffer)
				if encoded != buffer:
					buffer = b''
				else:
					encoded = buffer[0:1]
					buffer = buffer[1:]
				output.write(encoded)
	output.write(buffer)
	output.flush()
	output.close()
	file.close()


# in: bytearray with one item   out: bytearray
def decode_byte(byte):
	if len(byte) > 1:
		print("we only decode one byte at a time round these parts")
		exit(1)
	byte = byte[0]
	if byte >= msb1 :
		if byte & msb0 in bigrams:
			return bigrams[byte & msb0]
		else:
			return (byte & msb0).to_bytes() + (byte & msb0).to_bytes()
	else:
		return byte.to_bytes()

def decode_file(file_in, file_out):
	output = open(file_out, 'wb')
	with open(file_in, 'rb') as file:
		while (byte := file.read(1)):
			output.write(decode_byte(byte))
	output.flush()
	output.close()

match argv[1]:
	case '-d':
		decode_file(argv[2], argv[3])
	case '-e':
		encode_file(argv[2], argv[3])
	case '-t':
		test = encode_byte(argv[2].encode('ascii'))
		print(test)
		print(decode_byte(test))