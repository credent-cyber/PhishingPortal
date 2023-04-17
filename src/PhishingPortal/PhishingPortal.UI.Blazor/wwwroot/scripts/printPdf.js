
function BlazorDownloadFile(filename, data) {
    const link = document.createElement("a");
    link.href = data;
    link.download = filename;
    link.click();
}

