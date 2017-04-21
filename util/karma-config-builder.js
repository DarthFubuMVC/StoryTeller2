var wp = require('webpack');
var defaultConfig = require('../webpack.config');
var _ = require('lodash');
var webpackConfig = _.extend({}, defaultConfig, {
  entry: undefined
});

var PROD = process.env.NODE_ENV === 'production'

module.exports = function(entryFile) {
  var preprocessors = {};
  preprocessors[entryFile] = ['webpack'];

  return function(config) {
    config.set({

      // base path that will be used to resolve all patterns (eg. files, exclude)
      basePath: '',

      // frameworks to use
      // available frameworks: https://npmjs.org/browse/keyword/karma-adapter
      frameworks: ['mocha'],

      plugins: [
        'karma-growl-reporter',
        'karma-mocha',
        'karma-mocha-reporter',
        'karma-osx-reporter',
        'karma-phantomjs-launcher',
        'karma-chrome-launcher',
        'karma-webpack'
      ],

      // list of files / patterns to load in the browser
      files: [ entryFile ],

      // list of files to exclude
      exclude: [
      ],

      // preprocess matching files before serving them to the browser
      // available preprocessors: https://npmjs.org/browse/keyword/karma-preprocessor
      preprocessors: preprocessors,

      //webpack.config
      webpack: webpackConfig,

      //options for webpack server
      webpackServer: {
        stats: {
          colors: true
        },
        //suppresses output of webpack log
        quiet: PROD
      },

      // test results reporter to use
      // possible values: 'dots', 'progress'
      // available reporters: https://npmjs.org/browse/keyword/karma-reporter
      reporters: ['growl', 'osx', 'mocha'],

      // web server port
      port: 9876,

      // enable / disable colors in the output (reporters and logs)
      colors: true,

      // level of logging
      // possible values: config.LOG_DISABLE || config.LOG_ERROR || config.LOG_WARN || config.LOG_INFO || config.LOG_DEBUG
      logLevel: config.LOG_INFO,

      // start these browsers
      // available browser launchers: https://npmjs.org/browse/keyword/karma-launcher
      browsers: ['Chrome'],
    });
  };
};
