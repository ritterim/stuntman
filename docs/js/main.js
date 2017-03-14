$(function() {
  // catch anchors in url
  $('a[href^="#"]')
  .on('click', function(e) {
    var target = $(this.hash);

    if (target.length) {
      e.preventDefault();
      $('html,body').animate({
        scrollTop: (target.offset().top)
      }, 'slow');
      return false;
    }
  });

  // fix menu when passed
  $('.masthead')
  .visibility({
    once: false,
    onBottomPassed: function() {
      $('.fixed.menu').transition('fade in');
    },
    onBottomPassedReverse: function() {
      $('.fixed.menu').transition('fade out');
    }
  });

  // create sidebar and attach to menu open
  $('.ui.sidebar')
  .sidebar('attach events', '.toc.item');
});
