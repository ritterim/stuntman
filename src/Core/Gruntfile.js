'use strict';

module.exports = function(grunt) {
    grunt.initConfig({
        css_selectors: {
            options: {
                mutations: [
                    { search: /\.pure/g, replace: '.stuntman' }, 
                    { search: /"pure-u"/g, replace: '"stuntman-u"' }
                ]
            },
            default: {
                files: {
                    'resources/stuntman.pure.css': ['assets/pure/pure.css'],
                }
            }
        }
    });

    grunt.loadNpmTasks('grunt-css-selectors');

    grunt.registerTask('default', ['css_selectors']);
};
