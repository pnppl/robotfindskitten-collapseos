#!/bin/python3
# experimenting with basic ascii compression
import sys

trigrams = {
	0: b'the',
	1: b'and',
	2: b'ing',
	3: b'ent',
	4: b'ion',
	5: b'her',
	6: b'for',
	7: b'tha',
	8: b'nth',
	9: b'int',
	10: b'ere',
	11: b'tio',
	12: b'ter',
	13: b'est',
	14: b'ers',
	15: b'ati',
	16: b'hat',
	17: b'ate',
	18: b'all',
	19: b'eth',
	20: b'hes',
	21: b'ver',
	22: b'his',
	23: b'oft',
	24: b'ith',
	25: b'fth',
	26: b'sth',
	27: b'oth',
	28: b'res',
	29: b'ont',
	30: b'The',
	31: b'And',
	127: b'Her'
}

trigrams_rev = {val: key for key, val in trigrams.items()}

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
	30: b'Th',
	31: b'He',
	127: b'In'
}

bigrams_rev = {val: key for key, val in bigrams.items()}

msb1 = 0b1000_0000
msb0 = 0b0111_1111

# in: bytearray   out: bytearray with one item
def encode_byte(chars):
	"""
	if chars in trigrams_rev:
		return (trigrams_rev[chars] | msb1).to_bytes()
	"""
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
			"""
			if len(buffer) == 3:
				encoded = encode_byte(buffer)
				# trigram match
				if encoded != buffer:
					buffer = b''
				else:
					encoded = encode_byte(buffer[0:2])
					# bigram match
					if encoded != buffer[0:2]:
						buffer = buffer[2:]
					# no match, spit first char
					else:
						encoded = buffer[0:1]
						buffer = buffer[1:]
			"""
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
#		if byte & msb0 in trigrams:
#			return trigrams[byte & msb0]
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

match sys.argv[1]:
	case '-d':
		decode_file(sys.argv[2], sys.argv[3])
	case '-e':
		encode_file(sys.argv[2], sys.argv[3])
	case '-t':
		test = encode_byte(sys.argv[2].encode('ascii'))
		print(test)
		print(decode_byte(test))