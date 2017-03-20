# Kvpbase CryptoServer

The Kvpbase cryptoserver is a RESTful API platform for encryption and decryption using generated one-time use keys generated using a private passphrase, salt, and initialization vector.

![alt tag](https://github.com/maraudersoftware/CryptoServer/blob/master/assets/diagram.png)

## Setup
Run the app with ```setup``` in the command line arguments to run the setup script.

## Under the Hood
When encrypting clear data:
- a one-time use key is generated using the private passphrase, salt, and initialization vector, which is identified by a key sequence number
- the clear data is encrypted using this key, and a JSON object containing the encrypted data ```Cipher``` and the key sequence number ```Ksn``` are returned
- the key sequence number can only be used to decrypt this particular block of cipher data
- the key sequence number must be persisted by the caller to be later submitted during decryption
- the key sequence number does not compromise the integrity of the private passphrase

When decrypting data:
- a JSON object containing ```Cipher``` and ```Ksn``` must be submitted.
- the key sequence number is used to derive the one-time use key
- the cipher data is then decrypted to the clear data
- the clear data is returned directly in the HTTP response
 
Accessing the API:
- the HTTP header where the API key is supplied in the request is defined under ```Auth.ApiKeyHeader```
- the ```AdminApiKey``` is used for administrative APIs
- the ```CryptoApiKey``` is used for performing encrypt/decrypt operations
- headers may be attached as headers or as querystring key-value-pairs

A sample encrypt and decrypt operation are as follows:
```
POST /encrypt?x-api-key=user
Data: 
foo
Response:
{
  "Cipher": "cxUy+dEUdEgvfEiVmJhlrQ==",
  "Ksn": "Fxx8GCurX4nTARauzeMkVtQnObgYP1WlJ3MqntwL1/A=",
  "StartTime": "2017-03-20T19:24:22.2700012Z",
  "EndTime": "2017-03-20T19:24:22.2740111Z",
  "TotalTimeMs": 4.01
}

POST /decrypt?x-api-key=user
Data:
{
  "Cipher": "cxUy+dEUdEgvfEiVmJhlrQ==",
  "Ksn": "Fxx8GCurX4nTARauzeMkVtQnObgYP1WlJ3MqntwL1/A="
}
Response:
foo
```

## Sample Configuration
```
{
  "EnableConsole": 1,
  "Server": {
    "DnsHostname": "localhost",
    "Port": 9000,
    "Ssl": 0
  },
  "Crypto": {
    "Passphrase": "6D8B18A82CAD7534",
    "Salt": "68417BB6B11D41ED",
    "InitVector": "E01A5C88D8B17D47"
  },
  "Auth": {
    "ApiKeyHeader": "x-api-key",
    "AdminApiKey": "admin",
    "CryptoApiKey": "user"
  },
  "Syslog": {
    "SyslogServerIp": "127.0.0.1",
    "SyslogServerPort": 514,
    "MinimumSeverityLevel": 0,
    "LogRequests": 0,
    "LogResponses": 0,
    "ConsoleLogging": 1
  },
  "Rest": {
    "UseWebProxy": 0,
    "WebProxyUrl": "",
    "AcceptInvalidCerts": 1
  }
}

```

## Admin APIs
Using the admin API key, a set of RESTful APIs can be used to gather visibility into the loadbalancer during runtime.  The admin API key header defined in the ```Auth``` section of the config can be included as a header or as a querystring key-value pair.
```
GET /_cryptoserver/config?x-api-key=admin
GET /_cryptoserver/connections?x-api-key=admin
```

## Running under Mono
Watson works well in Mono environments to the extent that we have tested it. It is recommended that when running under Mono, you execute the containing EXE using --server and after using the Mono Ahead-of-Time Compiler (AOT).

NOTE: Windows accepts '0.0.0.0' and '+' as an IP address representing any interface.  On Mac and Linux with Mono you must supply a specific IP address ('127.0.0.1' is also acceptable, but '0.0.0.0' is NOT).

```
mono --aot=nrgctx-trampolines=8096,nimt-trampolines=8096,ntrampolines=4048 --server LoadBalancer.exe
mono --server myapp.exe
```
