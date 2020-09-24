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
mov eax, 1
ret
main ENDP
str_Func PROC
mov eax, "test"
ret
str_Func ENDP
END _start
