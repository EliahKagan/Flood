(function () {
    'use strict';

    // Highlights the element specified in the query string, if any, with a
    // colored vertical bar running along it, to the left of the body. Links in
    // tips.html use this to open the help with the relevant part highlighted.
    function applyHighlighting() {
        const params = new URLSearchParams(location.search);
        const idToMark = params.get('highlight');
        if (idToMark === null) return;

        document.getElementById(idToMark).classList.add('highlighted');
        const pagelink = document.getElementById('pagelink');
        pagelink.title = 'Clear highlighting\u2009/\u2009' + pagelink.title;
    }

    function browserIsInternetExplorer() {
        return document.documentMode !== undefined;
    }

    function scrollToFragment() {
        if (location.hash.length > 1) {
            // Even after waiting for all assets to load, scrolling often
            // doesn't work, on initial page load in IE. It seems some task is
            // interfering with it, but I don't know what. Waiting until the
            // next cycle of the event loop seems to work. (Raising an alert,
            // either before *or* after the scrollIntoView call, also works.
            // The unwanted alert makes that unsuitable as a solution, but this
            // information may be relevant to future debugging.)
            setTimeout(function() {
                document.querySelector(location.hash).scrollIntoView(true);
            });
        }
    }

    // Works around how IE doesn't reliably scroll to a fragment on first load.
    function applyScrollingFix() {
        if (browserIsInternetExplorer()) {
            addEventListener('load', scrollToFragment);
        }
    }

    applyHighlighting();
    applyScrollingFix();
})();
