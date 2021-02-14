(function () {
    'use strict';

    window.tryReshowFragment = function () {
        document.querySelector(window.location.hash).scrollIntoView(true);
    };

    function applyHighlighting() {
        const params = new URLSearchParams(window.location.search);
        const idToMark = params.get('highlight');
        if (idToMark === null) return;

        document.getElementById(idToMark).classList.add('marked');
        const pagelink = document.getElementById('pagelink');
        pagelink.title = 'Clear highlighting\u2009/\u2009' + pagelink.title;
    }

    applyHighlighting();
})();
