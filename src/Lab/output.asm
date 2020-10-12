.386
.model flat,stdcall
option casemap:none

_main        PROTO

main PROTO
str_Func PROTO

.data
.code
_start:
	invoke  _main
_main PROC

	call main
	call str_Func

	ret

_main ENDP

main PROC
push 1
pop eax
ret
main ENDP
str_Func PROC
push 2
pop eax
ret
str_Func ENDP
END _start
