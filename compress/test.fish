#!/bin/fish
set filepath "inputs/Gutenberg 2003 CD"
set total 0
set count 0

for text in $filepath/*.txt
	set textout (path basename $text)
	./compress.py -e $text $textout.encoded
	./compress.py -d $textout.encoded $textout.decoded
	if diff $text $textout.decoded
		set count (math $count + 1)
		set insize (stat -c %s $text) 
		set outsize (stat -c %s $textout.encoded)
		set ratio (math $outsize / $insize)
		set total (math $total + $ratio)
		echo -e "$text\n$insize\n$outsize\n$ratio\n"
	else
		rm $textout.encoded
	end
	rm $textout.decoded
end

echo -e "\naverage:"
echo (math $total / $count)
