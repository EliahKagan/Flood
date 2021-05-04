(function () {
    'use strict';

    // Whole-row items should navigate when clicked anywhere, not just in a
    // cell. We'll find the first link inside each and make clicking go there.
    const wholeRowItems = document.querySelectorAll('tr.item');

    for (let i = 0; i < wholeRowItems.length; ++i) {
        const row = wholeRowItems[i];
        const url = row.querySelector('a').getAttribute('href');
        row.addEventListener('click', function () {
            location.href = url;
        });
        row.classList.add('made-clickable');
    }
})();
