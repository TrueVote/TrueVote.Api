// sign-service.js
const express = require('express');
const { 
    getPublicKey, 
    getEventHash, 
    nip19, 
    finalizeEvent 
} = require('nostr-tools');
const cors = require('cors');
const app = express();

app.use(cors());
app.use(express.json());

app.post('/sign', (req, res) => {
    try {
        const { nsec } = req.body;
        
        if (!nsec) {
            return res.status(400).json({ error: 'nsec is required' });
        }

        console.log('Received nsec:', nsec);
        
        // Clean and validate the nsec
        const cleanNsec = nsec.trim().toLowerCase();
        if (!cleanNsec.startsWith('nsec1')) {
            return res.status(400).json({ error: 'Invalid nsec format. Must start with nsec1' });
        }

        console.log('Cleaned nsec:', cleanNsec);

        // Decode nsec to get the private key
        const decoded = nip19.decode(cleanNsec);
        console.log('Decoded type:', decoded.type);
        const privateKey = decoded.data;
        console.log('Private key length:', privateKey.length);
        
        // Get npub from private key
        const publicKey = getPublicKey(privateKey);
        const npub = nip19.npubEncode(publicKey);
        console.log('Generated npub:', npub);
        
        // Create the content object
        const content = {
            Email: "user@example.com",
            FullName: "Test User",
            NostrPubKey: npub
        };

        // Get current timestamp
        const dt = Math.floor(Date.now() / 1000);
        
        // Convert content to string
        const contentStr = JSON.stringify(content);
        
        // Create unsigned event
        const event = {
            kind: 1,
            created_at: dt,
            tags: [],
            content: contentStr,
            pubkey: publicKey
        };

        console.log('Event to sign:', event);

        // Sign the event
        const signedEvent = finalizeEvent(event, privateKey);
        console.log('Generated signature:', signedEvent.sig);

        // Create final payload
        const signInEventModel = {
            Kind: 1,
            CreatedAt: new Date(dt * 1000).toISOString(),
            PubKey: npub,
            Signature: signedEvent.sig,
            Content: contentStr
        };

        console.log('Final payload created');
        res.json(signInEventModel);

    } catch (error) {
        console.error('Server error with details:', {
            message: error.message,
            stack: error.stack,
            name: error.name
        });
        res.status(500).json({ 
            error: error.message,
            details: 'There was an error processing the nsec key. Check the server logs for more details.'
        });
    }
});

const PORT = 3456;
app.listen(PORT, () => {
    console.log(`Signing service running on http://localhost:${PORT}`);
    console.log('Waiting for requests...');
});