.386
.model flat,stdcall
option casemap:none

include     G:\masm32\include\windows.inc
include     G:\masm32\include\kernel32.inc
include     G:\masm32\include\masm32.inc
includelib  G:\masm32\lib\kernel32.lib
includelib  G:\masm32\lib\masm32.lib

_main        PROTO

main PROTO
strFunc PROTO

.data
buff        db 11 dup(?)

.code
_start:
	invoke  _main
	invoke  _NumbToStr, ebx, ADDR buff
	invoke  StdOut,eax
	invoke  ExitProcess,0

_main PROC

	call main
call strFunc

	ret

_main ENDP

main PROC
mov eax, 1
ret
main ENDP
strFunc PROC
mov eax, "asdf"
ret
strFunc ENDP
END _start
