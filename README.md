# NCW-DNS
New Cyber Web (NCW) - DNS Project.
This repo is part of a series of repo's that form my NCW project, the goal of this project is to recreate the base elements the internet is based on.
Only for learning and entertainment (mostly myself) purposes.

## Technical information
Written below is information on how this DNS server is going to function.

This server is going to work using UDP Datagram messages on port `51`, this number can change in the future.

### Structure
Each DNS packet has a specific structure, but can be of any length from 4 to 258

The first byte is for the packet-type:
```
0x1A: Request  
0x1B: Response
```

The second byte is for the dns lookup type:
```
0x2A: Name lookup  
0x2B: IP lookup
```

The third byte contains the length of the provided resource to use for performing the lookup (1-255)
```
0x01: 1 character
0x02: 2 characters
0x80: 128 characters
0xFF: 225 characters
```
The rest of the data has a variable length decided by the third byte.

### DNS Storage
The DNS Records will be stored in a database that can be accessed and interpreted by the system.
