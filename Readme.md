Deliveroo - no relation

Small utility to run at logon (or on demand) to read a gpo directly and parse the xml preferences to add printers based on those preferences.

Intended to map a printer based on Security groups containing computer names.

Default options are for our network :) but can be overriden with command arguments 
domain=Yourdomain.internal
gpo=GUID of gpo to process

ie: .\Deliveroo.exe GPO="{F111111B-11C1-1D11-E111-1F411GH111IJ}" domain="mydomain.internal"

Flow:
checks for and opens \\domain\sysvol\guid\user\preferences\Printers.xml

Log is created in %appdata%\Deliveroo