// Post-response Script
try {
    const response = pm.response.json();

    // Log full response for debugging
    console.log('API Response:', response);

    // Check if we have a Token in the response
    if (response.Token) {
        // Set token directly in environment
        pm.environment.set('authToken', response.Token);
        console.log('Auth token saved to environment');
    } else {
        console.log('No Token found in response');
    }

    // Test response status
    pm.test('Status code is 200', () => {
        pm.response.to.have.status(200);
    });

    // Test response structure
    pm.test('Response has expected structure', () => {
        pm.expect(response).to.be.an('object');
        pm.expect(response.Token).to.exist;
    });

} catch (error) {
    console.error('Error in test script:', error);
}
