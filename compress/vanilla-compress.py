#!/bin/python3

# compress.py with bigram dictionary generated from vanilla.nki
# 24% savings (18% with compress.py)

from sys import argv

bigrams_rev = {
	b's ': 0,
	b'e ': 1,
	b' a': 2,
	b'in': 3,
	b' t': 4,
	b'he': 5,
	b'er': 6,
	b' s': 7,
	b'th': 8,
	b'n ': 9,
	b' o': 10,
	b't ': 11,
	b're': 12,
	b'd ': 13,
	b'te': 14,
	b'.A': 15,
	b'an': 16,
	b'A ': 17,
	b' b': 18,
	b'it': 19,
	b'a ': 20,
	b"'s": 21,
	b'f ': 22,
	b' i': 23,
	b'ou': 24,
	b'of': 25,
	b'is': 26,
	b"t'": 27,
	b'on': 28,
	b'en': 29,
	b'ar': 30,
	b'It': 31,
	b'le': 127
}

bigrams = {val: key for key, val in bigrams_rev.items()}

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