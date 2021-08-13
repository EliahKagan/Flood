// tips.js - JavaScript customization for the tips drawer contents.
// This file is part of Flood, an interactive flood-fill visualizer.
//
// Copyright (C) 2021 Eliah Kagan <degeneracypressure@gmail.com>
//
// Permission to use, copy, modify, and/or distribute this software for any
// purpose with or without fee is hereby granted.
//
// THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
// WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
// MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY
// SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
// WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION
// OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN
// CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.

(function () {
    'use strict';

    // Set a body font size, if requested in the URL query string.
    const size = new URLSearchParams(location.search).get('size');
    if (size !== null) {
        document.body.style.fontSize = size;
    }

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
