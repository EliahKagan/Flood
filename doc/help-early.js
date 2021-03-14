(function () {
    'use strict';

    // Configuraton for Hyphenopoly.
    window.Hyphenopoly = {
        require: {
            'en-us': 'Pseudopseudohypoparathyroidism'
        },
        setup: {
            hide: 'element',
            keepAlive: false,
            selectors: {
                'p, li': {
                    'orphanControl': '1'
                }
            }
        }
    };
})();
