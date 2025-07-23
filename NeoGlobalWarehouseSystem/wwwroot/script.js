window.BlazorDownloadFile = (fileName, contentType, byteBase64) => {
    const link = document.createElement('a');
    link.download = fileName;
    link.href = "data:" + contentType + ";base64," + btoa(String.fromCharCode.apply(null, new Uint8Array(byteBase64)));
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};