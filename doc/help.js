(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        const params = new URLSearchParams(window.location.search);
        const idToMark = params.get('highlight');
        if (idToMark !== null) {
            document.getElementById(idToMark).classList.add('marked');
        }
    });
})();
