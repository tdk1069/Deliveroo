Deliveroo - _Any similarity to actual companies, persons, or actual events, is purely coincidental_

Small utility to run at logon (or on demand) to read a gpo directly and parse the xml preferences to add printers based on those preferences.

Intended to map a printer based on Security groups containing computer names.

Default options are for our network :) but can be overriden with command arguments 
domain=Yourdomain.internal
gpo=GUID of gpo to process

ie: .\Deliveroo.exe GPO="{F111111B-11C1-1D11-E111-1F411GH111IJ}" domain="mydomain.internal"

Flow:
checks for and opens \\domain\sysvol\guid\user\preferences\Printers.xml

Log is created in %appdata%\Deliveroo

Log example:
2022-04-28 15:14:20.2211|INFO|Reading GPP from bkhs.internal {F927136B-03C3-4B63-A262-8F4E1AB240DA}|
2022-04-28 15:14:22.2418|INFO|Adding computer: KH-PC-TESTPC -> \\printServer.mydomain.internal\Printer|
2022-04-28 15:14:26.9089|INFO|Successfully connected printer.|

2022-04-29 11:21:07.5885|INFO|Reading GPP from bkhs.internal11 {F927136B-03C3-4B63-A262-8F4E1AB240DA}|
2022-04-29 11:21:07.6199|INFO|GPO Not found|

