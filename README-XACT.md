This is a potential way to get a standalone Xact setup for those who don't want to bother with installing outdated crap like VS 2010.

For the absolute paths you see in the commands, replace them with your own paths:
1. Download XNA Game Studio 4.0: https://www.microsoft.com/en-ca/download/details.aspx?id=23714
2. From CMD / Powershell run the installer with `XNAGS40_setup.exe /x` and unpack it in a new empty folder
3. Find redists.msi, then run in CMD / Powershell `msiexec /a redists.msi /qn TARGETDIR=C:\Users\Alex\Downloads\redists_unpacked`
4. Grab the folder generated, and get `xnags_shared.msi`. Repeat step 3 for the installer.
    - `msiexec /a xnags_shared.msi /qn TARGETDIR=C:\Users\Alex\Downloads\xnags_shared`
5. Navigate to `xnags_shared\Program Files\Microsoft XNA\XNA Game Studio\v4.0\Tools` and move the files from there in a dedicated folder.