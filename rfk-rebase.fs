\\ --------------------- robotfindskitten -----------------------
\\ rebased on pre-git version. for loops not working
ALIAS CLRSCR clear
2 VALUE cells
: negate  ( n -- -n , 2's complement )  $FFFF XOR 1+ ;
 COLS 1- VALUE x_max
LINES 1- VALUE y_max
LINES 2 - VALUE y_max-1
y_max COLS * VALUE bottom-left
COLS y_max-1 * VALUE grid_len
grid_len LINES 3 * / VALUE item_total  \\ <-- tweakable 
42069 VALUE seed0                       \\ <-- values
666 VALUE seed1                          \\ <-- here
2 CONSTS 2 delay_count 64 str_size        \\
5 VALUES text_state str_start str_total str_end_blk str_end_i
1024 str_size / VALUE str_per_blk
CREATE items.pos item_total cells ALLOT0
CREATE items.taken item_total cells ALLOT0
CREATE items.str item_total str_size * ALLOT0
CREATE title_str
   ," You are robot (#)." COLS ALLOT0
CREATE title_str2
   ," Your job is to find kitten." COLS ALLOT0
CREATE controls_str 
   ," move - wasd/hjkl   quit - q   any - start" COLS ALLOT0
CREATE win_str 
   ," You found kitten! Way to go, robot!" COLS ALLOT0
CREATE empty_str str_size ALLOT0
\\ drawing to last cell in grid scrolls screen
: y->abs  ( n -- n )  COLS * ;
: print!  ( pos c -- )  SWAP CELL! ;
: hr  ( y -- , horizontal rule ) 
   y->abs  COLS >R BEGIN 
      DUP '-' print! 
   1 + NEXT DROP ;
: vr  ( x -- , vertical rule ) 
   COLS +  y_max-1 >R BEGIN 
      DUP '|' print! 
   COLS + NEXT DROP ;
: corners  ( -- , print corners ) 
   '+' 0 CELL! '+' x_max CELL!
   '+' y_max-1 y->abs CELL!
   '+' y_max-1 y->abs x_max + CELL! ;
: border  ( -- , print border )
   0 vr x_max vr 
   y_max hr 0 hr 
   corners ;
\\ https://wiki.forth-ev.de/doku.php/
\\ en:pfw:random_generators_xorshift
: random  ( -- n )
  seed0  seed1 TO seed0
  DUP 7 LSHIFT XOR
  DUP 9 RSHIFT XOR
  DUP 13 LSHIFT XOR
  DUP seed1 XOR TO seed1 ;
: random_init  ( -- )  8 >R BEGIN random DROP NEXT ;
: rand  ( floor ceil -- n , floor <= n < ceil )  
   OVER -  random SWAP MOD  + ;
: []  ( n a -- 'a[n] )  SWAP cells + ;
: =[]  ( val n a -- f , f=1 if a[n]=val ) 
   [] @ = IF 1 ELSE 0 THEN ;
: items.pos[]  items.pos [] ;
: items.taken[]  items.taken [] ;
: items.str[]  ( n -- 'items.str[n] )  
   str_size * items.str + ;
: blk[]  ( n n -- a , blk# i -- 'blk[i] )  
   SWAP BLK@ BLK( SWAP str_size * + ;
: blk@c  ( n n -- c , low byte )  blk[] @ L|M DROP ;
: str_num->blkpos  ( n -- n n )
   DUP str_per_blk / str_start +
   SWAP str_per_blk MOD ;
: str_blk[] ( n -- 's )  str_num->blkpos blk[] ;
: str_check_blk  ( n -- n? , blk# -- last good offset or -1 )
   0 2DUP 16 >R BEGIN       \\ blk i blk i
      TUCK blk@c   \\ blk i i blk[i]
      SPC = IF         \\ blk i i
      NIP NIP R~ EXIT  \\ i
      THEN
      str_total 1+ TO str_total
      DROP 1+ 2DUP     \\ blk i+ blk i+
   NEXT
   2DROP 2DROP -1 ;
: str_enum  ( -- , count strings and save info in globals ) 
   str_start DUP DUP BEGIN
      2DROP DUP str_check_blk  \\ blk chk
      SWAP 1+ SWAP 2DUP        \\ blk+ chk blk+ chk
   0>= UNTIL                   \\ blk+ chk blk+
   TO str_end_blk  TO str_end_i    DROP ;
: copy_bytes  ( a2 a1 u -- , copy u bytes from a1 to a2 )
   >R BEGIN C@+ ROT C!+ SWAP NEXT 2DROP ;
: copy_str  ( a2 a1 -- , copy str from a1 to a2 )
   str_size copy_bytes ;
: quit  ( -- , cleanup arrays for repeat init and abort )
   0 item_total >R BEGIN
      DUP 0 SWAP items.pos[] !
      DUP 0 SWAP items.taken[] !
      DUP items.str[] empty_str copy_str
   1+ NEXT DROP clear ABORT ;
: str_taken?  ( str# -- f )
   0 item_total >R BEGIN
      2DUP items.taken =[] IF 
         2DROP 1 R~ EXIT 
      THEN 1+
   NEXT 2DROP 0 ;
: wall?  ( n -- f , f=1 if wall ) 
   DUP COLS < IF 1  \\ y=0
   ELSE DUP COLS y_max-1 * 1- > IF 1  \\ y too big
   ELSE DUP COLS MOD 0 = IF 1  \\ x=0
   ELSE DUP COLS MOD x_max = IF 1  \\ x too big
   ELSE 0
   THEN THEN THEN THEN NIP ;
: item@?  ( pos -- a? , address of item's string or -1 )
   0 item_total >R BEGIN
      2DUP items.pos =[] IF 
         NIP items.str[] R~ EXIT 
      THEN 1+ 
   NEXT 2DROP -1 ;
: safe?  ( pos -- f , f=1 if no collision )  
   DUP wall? SWAP item@? 0>= OR IF 0 ELSE 1 THEN ;
: free_space  ( -- n , get empty pos ) 
   BEGIN 
      0 grid_len rand 
      DUP safe? IF EXIT THEN 
   DROP AGAIN ;
free_space VALUE #pos
: #offset  ( n -- n , #pos + offset )  #pos @ + ;  
: #pos!  ( n -- , set # with absolute value )  #pos ! ;
: #erase  ( -- , we are the space robots )  SPC #pos @ CELL! ;
: #cursor  ( -- reset cursor under # )  #pos @ XYPOS! ;
: #draw  ( -- )  '#' #pos @ CELL! #cursor ;
: #update_empty!  ( pos -- , you are alone in the void )  
   #erase #pos! #draw ;
: item_char  ( -- c , get random ascii char )  
   33 126 rand DUP '#' = IF DROP '~' ( ascii 126 ) THEN ;
: item_place  ( -- pos , print and return pos )
   item_char free_space TUCK CELL! ;
: sayln ( a -- )
   bottom-left str_size CELLS!  1 TO text_state  #cursor ;
: say_nothing  ( -- , erase text )  
   text_state IF empty_str sayln THEN 0 TO text_state ;
: kitten?  ( pos -- f )  0 items.pos[] @ = ;
: delay ( n -- , n=delay iterations ) 
   >R BEGIN $7FFF >R BEGIN random DROP NEXT NEXT ;
\\ : center  ( -- n , middle of screen ) 
\\   LINES 2 /    COLS * 
\\   COLS 2 / +    COLS - 1 - ;
: kitten_init  ( generate kitten ) 
   item_place 0 items.pos[] !
   0 items.str[] win_str copy_str ;
: find_str  ( -- a , selected string )  
   BEGIN
      0 str_total rand  DUP str_taken?
      NOT IF str_blk[] EXIT
      THEN DROP
   AGAIN ;
: pick_str! ( i -- , get str and store in items.str[i] )
      items.str[] find_str copy_str ;
: item_init  ( -- , generate all items )
   kitten_init
   1 item_total 1- >R BEGIN
      DUP item_place SWAP items.pos[] !
      DUP pick_str!
   1+ NEXT DROP ;
: title_input ( -- ) key 'q' = IF DROP quit THEN DROP ;
: title ( -- , show title screen ) 
   clear 510 XYPOS! title_str EMITLN 
   745 XYPOS! title_str2 EMITLN
   980 XYPOS! controls_str EMITLN
   525 XYPOS!   title_input ;
0 VALUE init_done
: mem_init  ( -- , init strings stored in blocks after rfk )  
   init_done NOT IF  BLK> 1+ TO str_start  str_enum  THEN ;
: init  ( -- ) 
   title clear border random_init 
   init_done NOT IF mem_init THEN item_init #draw
   1 TO init_done ;
: reunion_anim  ( -- ) ;
: reunion  ( -- ) 
   reunion_anim 0 items.str[] sayln delay_count delay init ;
: move  ( n -- , offset #pos by n and update screen )
   #offset DUP wall? IF
         DROP ELSE
         DUP kitten? IF
         DROP reunion ELSE
         DUP item@? DUP 0>= IF
         sayln DROP ELSE 
         DROP say_nothing #update_empty!
  THEN THEN THEN ;
3 consts COLS down -1 left 1 right
COLS negate VALUE up
: input  ( -- ) 
   key DUP 'q' = IF DROP quit
   THEN DUP 'w' = IF up move
   ELSE DUP 'k' = IF up move
   ELSE DUP 'a' = IF left move
   ELSE DUP 'h' = IF left move
   ELSE DUP 's' = IF down move
   ELSE DUP 'j' = IF down move
   ELSE DUP 'd' = IF right move
   ELSE DUP 'l' = IF right move
   THEN THEN THEN THEN
   THEN THEN THEN THEN DROP ;
: rfk  ( blk? -- , block number where NKIs are stored )  
\\ blkno needs to be specified on first run if:
\\ - rfk not invoked immediately after loading
\\ - NKIs not stored in block immediately following rfk
    SCNT 0 = NOT IF 1- BLK@ THEN init BEGIN input AGAIN ;