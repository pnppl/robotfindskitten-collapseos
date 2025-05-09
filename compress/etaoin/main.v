module main
import os

/*
*  another take on basic ascii compression
*  
*  like the python version, we set the high bit and encode bigrams in the lower 7 bits,
*    or emit the original ascii character if we can't find a match.
*  however, instead of a set of 33 preset bigrams and 95 doubled ascii characters,
*    we encode the 7 most common english letters (+ SPC) in the top half,
*    and the 15 most common english letters (+ SPC) in the bottom half.
*    ie: 1hhhllll
*
*  this has most of the same advantages as the previous method,
*    but is more suited to english text.
*  it generally shaves 30% rather than 20%.
*  that's approaching useful!
*/

const lower := [ ' ', 'e', 't', 'a', 'o', 'i', 'n', 's', 'r', 'h', 'l', 'd', 'c', 'u', 'm', 'f']
const upper := lower[..8]

fn encode_bytes(bytes string) string {
	char0 := upper.index(bytes[0].ascii_str())
	char1 := lower.index(bytes[1].ascii_str())
	if bytes.len > 2 || char0 == -1 || char1 == -1 {
		return bytes
	}
	else {
		result := u8(0b1000_0000 | (char0 << 4) | char1)
		return result.ascii_str()
	}
}

fn decode_byte(byte string) string {
	if byte.len > 1  ||  byte[0] != (byte[0] | 0b1000_0000) {
		return byte
	}
	else {
		byte0 := (byte[0] & 0b0111_0000) >>> 4
		byte1 := byte[0] & 0b0000_1111
		char0 := upper[byte0]
		char1 := lower[byte1]
		return char0 + char1
	}
}

fn encode_file(f_in string, f_out string) {
	file_in := os.open(f_in) or { panic(err) }
	mut file_out := os.create(f_out) or { panic(err) }
	mut bytes := ''
	mut pos := u64(0)
	defer { 
		file_out.write(bytes.bytes()) or { panic(err) }
		file_out.flush()
		file_out.close()
	}
	for pos < os.file_size(f_in) {
		bytes += file_in.read_bytes_at(1, pos)[0].ascii_str()
		pos++
		if bytes.len == 2 {
			new_byte := encode_bytes(bytes)
			if new_byte == bytes {
				file_out.write([bytes[0]]) or { panic(err) }
				bytes = bytes[1..]
			} else {
				file_out.write(new_byte.bytes()) or { panic(err) }
				bytes = ''
			}
		}
	}
}

fn decode_file(f_in string, f_out string) {
	file_in := os.open(f_in) or { panic(err) }
	mut file_out := os.create(f_out) or { panic(err) }
	mut pos := u64(0)
	defer {
		file_out.flush()
		file_out.close()
	}
	for pos < os.file_size(f_in) {
		byte := file_in.read_bytes_at(1, pos)[0].ascii_str()
		pos++
		file_out.write(decode_byte(byte).bytes()) or { panic(err) }
	}
}

fn main() {	
	match os.args[1] {
		'-d' { decode_file(os.args[2], os.args[3]) }
		'-e' { encode_file(os.args[2], os.args[3]) }
		'-t' { 
			result := encode_bytes(os.args[2])
			println(result)
			println(decode_byte(result))
		}
		else { }
	}	
}
