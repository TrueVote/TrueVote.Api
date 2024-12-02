# Nostr Sign-In Service

Local service that handles Nostr key signing for TrueVote.Api authentication.

## Prerequisites
- Node.js installed
- TrueVote [Postman collection](https://www.postman.com/truevote/truevote-api) running
- A valid Nostr private key (nsec)

## Setup

```bash
cd sign-service
npm install
```

Required dependencies:
- express
- cors
- nostr-tools

## Running the Service

```bash
node sign-service.js
```

The service runs on `http://localhost:3456` by default.

## How It Works

1. **Postman Pre-request Script** sends your nsec to the local signing service
2. **Local Service** uses nostr-tools to:
   - Generate public key from nsec
   - Create and sign Nostr event
   - Return signed payload
3. **Postman** sends signed payload to TrueVote API
4. **API** verifies signature and returns auth token
5. **Postman Test Script** saves token to environment

## Environment Setup

In Postman, create an environment variable for either local or prod. Mark is as "secret" (not default):
- Name: `nsec`
- Value: Your Nostr private key (starts with nsec1...)

## Security Notes

- Service runs locally - your nsec never leaves your machine
- No keys are stored in the service
- Each request generates a new signature
- Environment variables stay local to your Postman instance

## Troubleshooting

If sign-in fails:
1. Verify service is running
2. Check nsec environment variable is set
3. Ensure nsec starts with "nsec1"
4. Check service logs for detailed errors