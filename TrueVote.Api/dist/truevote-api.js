console.info('truevote-api.js');

// Munge the Favicon in after loading
document.addEventListener("DOMContentLoaded", function() {
    document.querySelector('link[rel="icon"]').href = '/dist/favicon.ico';
});
