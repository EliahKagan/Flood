// help.js - JavaScript customization for the full help file.
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

    // Highlights the element specified in the query string, if any, with a
    // colored vertical bar running along it, to the left of the body. Links in
    // tips.html use this to open the help with the relevant part highlighted.
    function applyHighlighting() {
        const idToMark = new URLSearchParams(location.search).get('highlight');
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
            // before *or* after the scrollIntoView call, is also sufficient.
            // The unwanted alert makes that unsuitable as a solution, but the
            // fact that it also works may be relevant to future debugging.)
            setTimeout(function () {
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
