mergeInto(LibraryManager.library, {
  // Shows a real link for the player to click: browsers can block a download the page starts by itself.
  DownloadTextFile: function (fileNamePointer, textPointer) {
    var old = document.getElementById("notes-save");
    if (old) old.remove();

    var box = document.createElement("div");
    box.id = "notes-save";
    box.style = "position:fixed;left:50%;top:50%;transform:translate(-50%,-50%);display:flex;gap:16px;align-items:center;" +
      "padding:24px 28px;border-radius:18px;background:#0B2242;border:2px solid #FF9418;font:700 20px system-ui,sans-serif;z-index:10";

    var link = document.createElement("a");
    link.href = URL.createObjectURL(new Blob([UTF8ToString(textPointer)], { type: "text/plain" }));
    link.download = UTF8ToString(fileNamePointer);
    link.textContent = "Save the notes file";
    link.style = "color:#07142B;background:#FF9418;padding:12px 20px;border-radius:12px;text-decoration:none";

    var close = document.createElement("button");
    close.textContent = "Close";
    close.style = "color:#B7CAE0;background:none;border:none;font:inherit;cursor:pointer";
    close.onclick = function () { box.remove(); };

    box.appendChild(link);
    box.appendChild(close);
    document.body.appendChild(box);
  },
});
