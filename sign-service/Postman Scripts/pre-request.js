// Pre-request Script
const nsec = pm.environment.get("nsec");
if (!nsec) {
    throw new Error("nsec environment variable is not set. Please set this directly in your environment (not as a vault reference).");
}

if (!nsec.startsWith('nsec1')) {
    throw new Error("Invalid nsec format. Must start with nsec1");
}

const signRequest = {
    url: 'http://localhost:3456/sign',
    method: 'POST',
    header: {
        'Content-Type': 'application/json'
    },
    body: {
        mode: 'raw',
        raw: JSON.stringify({ nsec })
    }
};

pm.sendRequest(signRequest, (err, res) => {
    if (err) {
        console.error('Error calling signing service:', err);
        throw err;
    }

    try {
        const signedPayload = res.json();
        
        if (signedPayload.error) {
            console.error('Error from signing service:', signedPayload.error);
            throw new Error(signedPayload.error);
        }

        // Set request headers
        pm.request.headers.upsert({
            key: 'Content-Type',
            value: 'application/json'
        });

        // Set request body
        pm.request.body.raw = JSON.stringify(signedPayload);
        console.log('Final payload:', signedPayload);

    } catch (error) {
        console.error('Error processing signed payload:', error);
        throw error;
    }
});
