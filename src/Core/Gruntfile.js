'use strict';

module.exports = function(grunt) {
    grunt.initConfig({
        less: {
            default: {
                files: {
                    "resources/stuntman.css": "assets/stuntman/stuntman.less"
                }
            }
        },
        watch: {
            styles: {
                files: ['assets/stuntman/*.less'],
                tasks: ['less'],
                options: {
                    nospawn: true
                }
            }
        }
    });

    grunt.loadNpmTasks('grunt-contrib-less');
    grunt.loadNpmTasks('grunt-contrib-watch');

    grunt.registerTask('default', ['less']);
};
