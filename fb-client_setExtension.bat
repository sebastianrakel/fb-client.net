REG ADD "HKCR\*\shell\Paste to Filebin\command" /t REG_SZ /d """"%~dp0fb.exe""" """%%1"""