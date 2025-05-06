#!/bin/python3
from sys import argv
import ctypes
smaz = ctypes.CDLL("./smaz.so")
smaz.smaz_compress.argtypes = (ctypes.POINTER(ctypes.c_char), ctypes.c_int, ctypes.POINTER(ctypes.c_char), ctypes.c_int)
smaz.smaz_decompress.argtypes = (ctypes.POINTER(ctypes.c_char), ctypes.c_int, ctypes.POINTER(ctypes.c_char), ctypes.c_int)

def smaz_do(func, cin):
	global smaz
	cout = b""
	cin_len = len(cin)
	for i in range(cin_len):
		cout += b"\0"
	if func == "compress":
		result = smaz.smaz_compress(cin, cin_len, cout, cin_len)
	else:
		for i in range(cin_len):
			cout += b"\0"
		result = smaz.smaz_decompress(cin, cin_len, cout, cin_len * 2)
	if result != cin_len + 1:
		return cout[0:result]
	else:
		exit(1)

def read_in(file_in):
	file_in = open(file_in, 'rb')
	cin = file_in.read()
	file_in.close()
	return cin

def smaz_compress(file_in):
	cin = read_in(file_in)
	return smaz_do("compress", cin)

def smaz_decompress(file_in):
	cin = read_in(file_in)
	return smaz_do("decompress", cin)

def write_out(cin, file_out):
	out = open(file_out, 'wb')
	out.write(cin)
	out.flush()
	out.close()

match argv[1]:
	case '-c':
		result = smaz_compress(argv[2])
		write_out(result, argv[3])
	case '-d':
		result = smaz_decompress(argv[2])
		write_out(result, argv[3])
	case '-t':
		comp = smaz_do("compress", bytes(argv[2], "ascii"))
		print(comp)
		comp = smaz_do("decompress", comp)
		print(comp)