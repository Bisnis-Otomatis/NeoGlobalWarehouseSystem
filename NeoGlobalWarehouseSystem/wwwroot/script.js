window.BlazorDownloadFile = (fileName, contentType, byteBase64) => {
    const link = document.createElement('a');
    link.download = fileName;
    link.href = "data:" + contentType + ";base64," + btoa(String.fromCharCode.apply(null, new Uint8Array(byteBase64)));
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

window.startQuoteFade = function (quotes) {
    let idx = 0;
    const quoteDiv = document.getElementById('quote-fade');
    if (!quoteDiv) return;
    quoteDiv.textContent = quotes[0];
    quoteDiv.classList.add('fade-in');
    setInterval(() => {
        quoteDiv.classList.remove('fade-in');
        quoteDiv.classList.add('fade-out');
        setTimeout(() => {
            idx = (idx + 1) % quotes.length;
            quoteDiv.textContent = quotes[idx];
            quoteDiv.classList.remove('fade-out');
            quoteDiv.classList.add('fade-in');
        }, 2000); // match fade duration
    }, 8000);
};