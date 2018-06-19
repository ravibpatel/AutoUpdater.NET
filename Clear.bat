@echo clear...
for /r "../AutoUpdater.NET" %%f in (.) Do If %%~nf == bin rd /s/q "%%f"
for /r "../AutoUpdater.NET" %%f in (.) Do If %%~nf == obj rd /s/q "%%f"

@pause