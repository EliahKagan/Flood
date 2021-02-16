(function () {
    'use strict';

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
            // document.querySelector(location.hash).scrollIntoView({
            //     behavior: 'smooth',
            //     block: 'start'
            // });

            setTimeout(function() {
                document.querySelector(location.hash).scrollIntoView(true);
            });

            //document.querySelector(location.hash).scrollIntoView(true);

            // const fragment = document.querySelector(location.hash);
            // scroll(0, fragment.getBoundingClientRect().top);

            //console.log(location.hash);

            //alert(location.hash);

            // document.getElementById(location.hash.slice(1))
            //         .scrollIntoView(true);
        }
    }

    applyHighlighting();

    if (browserIsInternetExplorer()) {
        // Scroll to the right place. IE doesn't do it reliably on first load.
        addEventListener('load', scrollToFragment);
    }
})();
