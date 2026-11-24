mergeInto(LibraryManager.library, {
  OpenInSameTab: function (url) {
    window.top.location.href = UTF8ToString(url);
  }
});
