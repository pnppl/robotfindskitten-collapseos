\\ --------------------- robotfindskitten -----------------------
alias clrscr clear
: negate  ( n -- -n , 2's complement )  $FFFF xor 1+ ;
 COLS 1- value x_max
LINES 1- value y_max
LINES 2 - value y_max-1
y_max COLS * value bottom-left
COLS y_max-1 * value grid_len
grid_len LINES 3 * / value item_total  \\ <-- tweakable 
42069 value seed0                       \\ <-- values
666 value seed1                          \\ <-- here
2 consts 2 delay_count 64 str_size        \\
5 values text_state str_start str_total str_end_blk str_end_i
1024 str_size / value str_per_blk
create items.pos item_total cells allot0
create items.taken item_total cells allot0
create items.str item_total str_size * allot0
create title_str
   ," You are robot (#)." COLS allot0
create title_str2
   ," Your job is to find kitten." COLS allot0
create controls_str 
   ," move - wasd/hjkl   quit - q   any - start" COLS allot0
create win_str 
   ," You found kitten! Way to go, robot!" COLS allot0
create empty_str str_size allot0
\\ drawing to last cell in grid scrolls screen
: y->abs  ( n -- n )  COLS * ;
: print!  ( pos c -- )  swap cell! ;
: hr  ( y -- , horizontal rule ) 
   y->abs  COLS for 
      dup '-' print! 
   1 + next drop ;
: vr  ( x -- , vertical rule ) 
   COLS +  y_max-1 for 
      dup '|' print! 
   COLS + next drop ;
: corners  ( -- , print corners ) 
   '+' 0 cell! '+' x_max cell!
   '+' y_max-1 y->abs cell!
   '+' y_max-1 y->abs x_max + cell! ;
: border  ( -- , print border )
   0 vr x_max vr 
   y_max hr 0 hr 
   corners ;
\\ https://wiki.forth-ev.de/doku.php/
\\ en:pfw:random_generators_xorshift
: random  ( -- n )
  seed0  seed1 to seed0
  dup 7 lshift xor
  dup 9 rshift xor
  dup 13 lshift xor
  dup seed1 xor to seed1 ;
: random_init  ( -- )  8 for random drop next ;
: rand  ( floor ceil -- n , floor <= n < ceil )  
   over -  random swap mod  + ;
: []  ( n a -- 'a[n] )  swap cells + ;
: =[]  ( val n a -- f , f=1 if a[n]=val ) 
   [] @ = if 1 else 0 then ;
: items.pos[]  items.pos [] ;
: items.taken[]  items.taken [] ;
: items.str[]  ( n -- 'items.str[n] )  
   str_size * items.str + ;
: blk[]  ( n n -- a , blk# i -- 'blk[i] )  
   swap blk@ blk( swap str_size * + ;
: blk@c  ( n n -- c , low byte )  blk[] @ l|m drop ;
: str_num->blkpos  ( n -- n n )
   dup str_per_blk / str_start +
   swap str_per_blk mod ;
: str_blk[] ( n -- 's )  str_num->blkpos blk[] ;
: str_check_blk  ( n -- n? , blk# -- last good offset or -1 )
   0 2dup 16 for       \\ blk i blk i
      tuck blk@c   \\ blk i i blk[i]
      SPC = if         \\ blk i i
      nip nip r~ exit  \\ i
      then
      str_total 1+ to str_total
      drop 1+ 2dup     \\ blk i+ blk i+
   next
   2drop 2drop -1 ;
: str_enum  ( -- , count strings and save info in globals ) 
   str_start dup dup begin
      2drop dup str_check_blk  \\ blk chk
      swap 1+ swap 2dup        \\ blk+ chk blk+ chk
   0>= until                   \\ blk+ chk blk+
   to str_end_blk  to str_end_i    drop ;
: copy_bytes  ( a2 a1 u -- , copy u bytes from a1 to a2 )
   for c@+ rot c!+ swap next 2drop ;
: copy_str  ( a2 a1 -- , copy str from a1 to a2 )
   str_size copy_bytes ;
: quit  ( -- , cleanup arrays for repeat init and abort )
   0 item_total for
      dup 0 swap items.pos[] !
      dup 0 swap items.taken[] !
      dup items.str[] empty_str copy_str
   1+ next drop clear abort ;
: str_taken?  ( str# -- f )
   0 item_total for
      2dup items.taken =[] if 
         2drop 1 r~ exit 
      then 1+
   next 2drop 0 ;
: wall?  ( n -- f , f=1 if wall ) 
   dup COLS < if 1  \\ y=0
   else dup COLS y_max-1 * 1- > if 1  \\ y too big
   else dup COLS mod 0 = if 1  \\ x=0
   else dup COLS mod x_max = if 1  \\ x too big
   else 0
   then then then then nip ;
: item@?  ( pos -- a? , address of item's string or -1 )
   0 item_total for
      2dup items.pos =[] if 
         nip items.str[] r~ exit 
      then 1+ 
   next 2drop -1 ;
: safe?  ( pos -- f , f=1 if no collision )  
   dup wall? swap item@? 0>= or if 0 else 1 then ;
: free_space  ( -- n , get empty pos ) 
   begin 
      0 grid_len rand 
      dup safe? if exit then 
   drop again ;
free_space value #pos
: #offset  ( n -- n , #pos + offset )  #pos @ + ;  
: #pos!  ( n -- , set # with absolute value )  #pos ! ;
: #erase  ( -- , we are the space robots )  SPC #pos @ cell! ;
: #cursor  ( -- reset cursor under # )  #pos @ xypos! ;
: #draw  ( -- )  '#' #pos @ cell! #cursor ;
: #update_empty!  ( pos -- , you are alone in the void )  
   #erase #pos! #draw ;
: item_char  ( -- c , get random ascii char )  
   33 126 rand dup '#' = if drop '~' ( ascii 126 ) then ;
: item_place  ( -- pos , print and return pos )
   item_char free_space tuck cell! ;
: sayln ( a -- )
   bottom-left str_size cells!  1 to text_state  #cursor ;
: say_nothing  ( -- , erase text )  
   text_state if empty_str sayln then 0 to text_state ;
: kitten?  ( pos -- f )  0 items.pos[] @ = ;
: delay ( n -- , n=delay iterations ) 
   for $7FFF for random drop next next ;
\\ : center  ( -- n , middle of screen ) 
\\   LINES 2 /    COLS * 
\\   COLS 2 / +    COLS - 1 - ;
: kitten_init  ( generate kitten ) 
   item_place 0 items.pos[] !
   0 items.str[] win_str copy_str ;
: find_str  ( -- a , selected string )  
   begin
      0 str_total rand  dup str_taken?
      not if str_blk[] exit
      then drop
   again ;
: pick_str! ( i -- , get str and store in items.str[i] )
      items.str[] find_str copy_str ;
: item_init  ( -- , generate all items )
   kitten_init
   1 item_total 1- for
      dup item_place swap items.pos[] !
      dup pick_str!
   1+ next drop ;
: title_input ( -- ) key 'q' = if drop quit then drop ;
: title ( -- , show title screen ) 
   clear 510 xypos! title_str emitln 
   745 xypos! title_str2 emitln
   980 xypos! controls_str emitln
   525 xypos!   title_input ;
0 value init_done
: mem_init  ( -- , init strings stored in blocks after rfk )  
   init_done not if  blk> 1+ to str_start  str_enum  then ;
: init  ( -- ) 
   title clear border random_init 
   init_done not if mem_init then item_init #draw
   1 to init_done ;
: reunion_anim  ( -- ) ;
: reunion  ( -- ) 
   reunion_anim 0 items.str[] sayln delay_count delay init ;
: move  ( n -- , offset #pos by n and update screen )
   #offset dup wall? if
         drop else
         dup kitten? if
         drop reunion else
         dup item@? dup 0>= if
         sayln drop else 
         drop say_nothing #update_empty!
  then then then ;
3 consts COLS down -1 left 1 right
COLS negate value up
: input  ( -- ) 
   key dup 'q' = if drop quit
   then dup 'w' = if up move
   else dup 'k' = if up move
   else dup 'a' = if left move
   else dup 'h' = if left move
   else dup 's' = if down move
   else dup 'j' = if down move
   else dup 'd' = if right move
   else dup 'l' = if right move
   then then then then
   then then then then drop ;
: rfk  ( blk? -- , block number where NKIs are stored )  
( blkno needs to be specified on first run if: )
( - rfk not invoked immediately after loading )
( - NKIs not stored in block immediately following rfk )
    scnt 0 = not if 1- blk@ then init begin input again ;