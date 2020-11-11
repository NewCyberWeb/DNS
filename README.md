# NCW-DNS
New Cyber Web (NCW) - DNS Project.
This repo is part of a series of repo's that form my NCW project, the goal of this project is to recreate the base elements the internet is based on.
Only for learning and entertainment (mostly myself) purposes.

## Project structure
This repo consists of three Visual Studio projects.\
One of these projects is the actual DNS Server, for now this is an executable that can receive the DNS requests. This could eventually change into a Windows service.\
The other project is the DNS Library, this can be used to make DNS requests, or create your own implementation of this special DNS Protocol.\
The third project is for testing the DNS server and can be used to perform multiple query's in the starting arguments.

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

### DNS Table
DNS Records will be stored in c# objects using the structure of going per domain level.\
That means there is one Root object, this object contains a list of top-level domains. And each domain record contains a list of sub domain records, and so on.

This object can be stored using the object to binary functions.
#### Domain lookup
When given a domain name, for example: `test.ncw-dns.example.com`, the domain is splitted on the `.`.\
and it starts searching from the back. That means it starts by looking for the domain `com`, once found, it starts looking for `example`.\
If a domain part cannot be found the program will return a dns error.

#### IP lookup (reverse DNS)
In order to do this, an existing method was used, and implemented in a similar fashion to [arpa](https://en.wikipedia.org/wiki/.arpa).\
There will be a reserved top level domain name called arpa. in here there will be a domain name ip4v, and under this domain all possible ip adresses that have been assigned will be added.
