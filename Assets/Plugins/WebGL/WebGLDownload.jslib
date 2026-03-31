mergeInto(LibraryManager.library, {
  WebGLDownloadTextFile: function (filenamePtr, textPtr) {
    var filename = UTF8ToString(filenamePtr);
    var text = UTF8ToString(textPtr);
    var blob = new Blob([text], { type: "text/csv;charset=utf-8" });
    var url = URL.createObjectURL(blob);
    var a = document.createElement("a");
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  }
});
