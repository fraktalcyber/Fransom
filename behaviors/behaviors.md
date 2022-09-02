| Ransomware / Group | Attack Phase | Description | MITRE |
| ------------------ | ------------ | ----------- | ----- | 
| Maze, Ryuk, FinFisher, FIN7 | Persistence | Add executable to the run registry key | T1547.001: Boot or Logon Autostart Execution - Registry Run Keys / Startup Folder |
| Carabanak, FIN7 | Persistence | Create Scheduled Task | T1053.005: Scheduled Task/Job - Scheduled Task |
| Carbanak | Persistence | Create Local Account | T1136.001: Create Account - Local Account |
| Conti/Ryuk, Pysa, Clop, Hive, RagnarLocker | Persistence | Create Service | Create or Modify System Process: Windows Service. T1543.003 |
| Maze, FinFisher | Defense Evasion | Inject pre-compiled .NET assembly into memory | T1055.001: Process Injection - Dynamic-link Library Injection |
| Carbanak, FIN7 | Defense Evasion | Process Hollowing | T1055.012: Process Injection - Process Hollowing |
| | Defense Evasion | Inject a thread inside a running process | T1055.003: Process Injection - Thread Execution Hijacking |
| | Defense Evasion | Inject code through Asynchronous Procedure Call | T1055.004: Process Injection - Asynchronous Procedure Call | 
| FinFisher | Defense Evasion | Delete windows event logs from local machine | T1070.001: Clear Windows Event Logs |
| | Credential Access | Dump LSASS process memory | T1003.001: OS Credential Dumping - LSASS Memory |
| Maze, Ryuk, FinFisher | Discovery | Enumerate running office processes | T1057: Process Discovery | 
| Ryuk | Discovery | Enumerate user profile folders | T1083: File and Directory Discovery | 	
| | Discovery | Enumerate mounted network shares | T1135: Network Share Discovery |
| | Discovery | Enumerate domain users | T1087.002: Account Discovery - Domain Account | 
| | Discovery | Enumerate domain groups | T1069.002: Permission Groups Discovery - Domain Groups | 
| | Discovery | Enumerate domain computers | T1018: Remote System Discovery |
| | Discovery | Enumerate domain trusts	 | T1482: Domain Trust Discovery | 
| Maze, Ryuk | Impact | Encrypt files inside the user profile folders | T1486: Data Encrypted for Impact |
| Maze, Ryuk | Impact | Encrypt files inside mounted network shares | T1486: Data Encrypted for Impact |
| Maze, Ryuk, Ragnar Locker | Impact | Delete shadow copies | T1490: Inhibit System Recovery |
| Maze, Ryuk, Ragnar Locker | Impact | Kill running office processes | T1489: Service Stop |
| Conti/Ryuk, Hive, LockBit 2.0, RagnarLocker, BlackByte, BlackCat | Impact | Exfiltrate data | Exfiltration over Web service. Exfiltration to Cloud Storage. T1567.002 |
